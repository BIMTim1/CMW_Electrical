using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            return Result.Succeeded;
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateMotorInfo(Document document, FamilyInstance motor)
        {
            Parameter mUID = motor.LookupParameter("UID");
        }
    }
}
