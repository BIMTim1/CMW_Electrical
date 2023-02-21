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

            //create new Transaction
            Transaction trac = new Transaction(doc);

            try
            {
                //start and name Transaction
                trac.Start("Rotate Electrical Device Symbols");

                foreach (Element elem in DeviceCollection(doc))
                {
                    if (elem.LookupParameter("Symbol U_D") != null)
                    {
                        double locrot = (elem.Location as LocationPoint).Rotation;
                        locrot *= (180 / 3.14);

                        if (locrot >= 45 & locrot <= 89)
                        {
                            elem.LookupParameter("Symbol U_D").Set(0);
                        }
                        else if (locrot >= 91 & locrot <= 179)
                        {
                            elem.LookupParameter("Symbol U_D").Set(0);
                        }
                        else if (locrot >= 225 & locrot <= 314)
                        {
                            elem.LookupParameter("Symbol U_D").Set(0);
                        }
                        else
                        {
                            elem.LookupParameter("Symbol U_D").Set(1);
                        }
                    }
                }

                trac.Commit();
                TaskDialog.Show("Element Symbols Rotated",
                    "All Electrical Device Symbols have been rotated.");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                trac.RollBack();
                errorReport = ex.Message;
                TaskDialog.Show("Element Symbols Failed to Rotate",
                    "An error occured while attempting to rotate Electrical Device Symbols. Contact the BIM Team for support.");
                return Result.Failed;
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
                .ToList();

            return elems;
        }
    }
}
