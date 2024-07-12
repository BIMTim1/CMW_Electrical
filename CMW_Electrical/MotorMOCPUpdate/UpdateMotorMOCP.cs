using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace MotorMOCPUpdate
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class UpdateMotorMOCP: IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            int count = 0;

            //get ActiveDocument RevitVersion
            int rev_version = Int32.Parse(app.VersionNumber);

            //define BuiltInCategory to collect
            BuiltInCategory bic = BuiltInCategory.OST_ElectricalFixtures;

            //collect all Motors in model
            List<FamilyInstance> all_motors = new FilteredElementCollector(doc)
                .OfCategory(bic)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(x=>x.LookupParameter("Family").AsValueString().Contains("Motor"))
                .Cast<FamilyInstance>()
                .ToList();

            //check if the tool collected any Motor Elements
            //if no, close the tool.
            if (all_motors.Count == 0)
            {
                TaskDialog.Show("No Motors in Project", "There are no Motor Families placed in the Active Project. The tool will now close.");
                return Result.Cancelled;
            }

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("Update Motor Circuit Load Name from MOCP");

                    foreach (FamilyInstance motor in all_motors)
                    {
                        //collect ElectricalSystem element from motor FamilyInstance
                        ISet<ElectricalSystem> motorCircuits = motor.MEPModel.GetElectricalSystems();

                        if (motorCircuits.Any())
                        {
                            ElectricalSystem motorCircuit = motorCircuits.First();

                            //collect motor MOCP
                            string motor_mocp_str = motor.LookupParameter("MES_(MFS) MOCP").AsString();
                            //verify if can be converted to number
                            bool isNumber = Int32.TryParse(motor_mocp_str, out int motor_mocp);

                            if (!isNumber)
                            {
                                continue;
                            }

                            motorCircuit.LookupParameter("Rating").Set(motor_mocp);
                            count++;
                        }
                    }

                    TaskDialog.Show("Motor Circuit Ratings Updated", $"{count} Motor Circuits have been updated to display the most up to date MOCP information.");

                    trac.Commit();

                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    //TaskDialog.Show("An Error Occurred", "Contact the BIM Team for Assistance.");
                    errorReport = ex.Message;

                    foreach (Element motor in all_motors)
                    {
                        elementSet.Insert(motor);
                    }

                    return Result.Failed;
                }
            }
        }
    }
}
