using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq.Expressions;

namespace RotateDeviceSymbols
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class DeviceSymbolsRotate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    //start and name Transaction
                    trac.Start("Rotate Electrical Device Symbols");

                    foreach (Element elem in DeviceCollection(doc))
                    {
                        ///collect family information
                        FamilyInstance famInst = elem as FamilyInstance;
                        Parameter param = famInst.LookupParameter("Symbol U_D");

                        double facingOrientation = Math.Round(famInst.FacingOrientation[0]);
                        double handOrientation = Math.Round(famInst.HandOrientation[1]);

                        ///<summary>
                        ///Ceiling/workplane hosted content will have a FacingOrienation[0] of -1 or 1 if needs to be rotated;
                        ///Wall hosted content will have a HandOrientation[1] of -1 or 1 if needs to be rotated
                        /// </summary>
                        if (facingOrientation != 0 || handOrientation != 0)
                        {
                            param.Set(0);
                        }
                        else
                        {
                            param.Set(1);
                        }
                    }

                    trac.Commit();

                    SimpleDialog("Element Symbols Rotated", 
                        "All Electrical Device Symbols have been rotated.");

                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    trac.RollBack();
                    errorReport = ex.Message;

                    SimpleDialog("Element Symbols Failed to Rotate",
                        "An error occured while attempting to rotate Electrical Device Symbols. Contact the BIM Team for support.");

                    return Result.Failed;
                }
            }
        }

        public List<Element> DeviceCollection(Document document)
        {
            //create list of BuiltInCategory type
            List<BuiltInCategory> catList = new List<BuiltInCategory> {BuiltInCategory.OST_CommunicationDevices, BuiltInCategory.OST_DataDevices,
                BuiltInCategory.OST_ElectricalFixtures, BuiltInCategory.OST_FireAlarmDevices, BuiltInCategory.OST_LightingDevices,
                BuiltInCategory.OST_SecurityDevices};

            ElementMulticategoryFilter filter = new ElementMulticategoryFilter(catList);

            List<Element> elems = new FilteredElementCollector(document)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .Where(x=>x.LookupParameter("Symbol U_D") != null)
                .ToList();

            return elems;
        }

        internal static void SimpleDialog(string header, string content)
        {
            TaskDialog mainDialog = new TaskDialog("CMW Elec Dialog")
            {
                TitleAutoPrefix = false,
                MainInstruction = header,
                MainContent = content
            };
            mainDialog.Show();
        }
    }
}
