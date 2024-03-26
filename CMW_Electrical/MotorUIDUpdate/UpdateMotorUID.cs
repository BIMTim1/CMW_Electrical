using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Electrical;
using System.Security.Cryptography;

namespace MotorUIDUpdate
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class UpdateMotorUID : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            BuiltInCategory bic = BuiltInCategory.OST_ElectricalFixtures;

            //collect E_EF_Motor FamilyInstances that are hosted to a same model MechanicalEquipment family
            List<FamilyInstance> allMotors = 
                new FilteredElementCollector(doc)
                .OfCategory(bic)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Where(x => x.Host.Category.Name == "Mechanical Equipment")
                .ToList();

            //cancel tool if no motors to update
            if (allMotors.Count() == 0)
            {
                TaskDialog.Show("No Motor Elements to Update", "The tool could not find any Motor elements to update.");
                return Result.Failed;
            }

            Transaction trac = new Transaction(doc);

            try
            {
                trac.Start("Update Motor UIDs and Circuit Load Names");

                foreach (FamilyInstance motor in allMotors)
                {
                    UpdateMotorInfo(motor);
                }

                trac.Commit();

                TaskDialog.Show("Motors Updated", $"{allMotors.Count} Motors have been updated based on their hosted equipment Identity Mark value.");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Operation Cancelled", "An error occurred. Contact the BIM Team for assistance.");
                return Result.Failed;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateMotorInfo(FamilyInstance motor)
        {
            //collect Motor information
            Parameter mUID = motor.LookupParameter("UID");
            ISet<ElectricalSystem> mCct = motor.MEPModel.GetElectricalSystems();

            //collect host Mechanical Equipment Identity Mark value
            string equipMark = motor.Host.LookupParameter("Identity Mark").AsString(); //determine when Identity Mark or Identity Type Mark should be used.

            if (mCct.Any())
            {
                ElectricalSystem circuit = mCct.First();

                Parameter circuitLoadName = circuit.LookupParameter("Load Name");
                //update Motor Circuit Load DIName value
                string loadNameVal = circuitLoadName.AsString().ToUpper();

                //update circuit Load DIName if never updated
                if (loadNameVal.Contains("MOTOR/HVAC/MECH"))
                {
                    loadNameVal = loadNameVal.Replace("MOTOR/HVAC/MECH", equipMark);
                }
                //update circuit Load DIName if current UID value was used
                else
                {
                    loadNameVal = loadNameVal.Replace(mUID.AsString(), equipMark);
                }

                circuitLoadName.Set(loadNameVal);
            }

            //update Motor UID parameter value
            mUID.Set(equipMark);
        }
    }
}
