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

namespace UpdateMultiLegSwitchIds
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class MutliLegSwitchLegUpdate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;

            //get lighting devices to modify
            List<Element> ltg_devices = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_LightingDevices)
                .WhereElementIsNotElementType()
                .Where(x => x.LookupParameter("E_Switch Id (Read-Only)") != null)
                .ToList();

            //create Transaction
            Transaction trac = new Transaction(doc);

            try
            {
                //start and name transaction
                trac.Start("Update Multi-Leg Switch Ids");

                foreach (Element sw in ltg_devices)
                {
                    //get Switch Id value to change to
                    string swIdTemp = sw.LookupParameter("E_Switch Id (Read-Only)").AsString();

                    //get Switch Id parameter of Lighting Device
                    Parameter sw_id = sw.get_Parameter(BuiltInParameter.RBS_ELEC_SWITCH_ID_PARAM);

                    //update Switch Id
                    sw_id.Set(swIdTemp);
                }

                trac.Commit();
                TaskDialog.Show("Multi-Leg Switches Update Succeeded", "All Multi-Leg Switch Id Parameters have been updated.");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                trac.RollBack();
                errorReport = ex.Message;
                TaskDialog.Show("Multi-Leg Switch Id Update Failed", "This tool failed to complete. Contact the BIM Team for assistance.");
                return Result.Failed;
            }
        }
    }
}
