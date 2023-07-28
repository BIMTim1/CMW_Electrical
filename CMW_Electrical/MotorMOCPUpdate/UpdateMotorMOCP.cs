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

            //get ActiveDocument RevitVersion
            int rev_version = Int32.Parse(app.VersionNumber);

            //define BuiltInCategory to collect
            BuiltInCategory bic = BuiltInCategory.OST_ElectricalFixtures;

            //collect all Motors in model
            List<Element> all_motors = new FilteredElementCollector(doc)
                .OfCategory(bic)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(x=>x.LookupParameter("Family").AsValueString().Contains("Motor"))
                .ToList();

            //check if the tool collected any Motor Elements
            //if no, close the tool.
            if (all_motors.Count == 0)
            {
                TaskDialog.Show("No Motors in Project", "There are no Motor Families placed in the Active Project. The tool will now close.");
                return Result.Failed;
            }

            Transaction trac = new Transaction(doc);

            try
            {
                trac.Start("Update Motor Circuit Load Name from MOCP");

                foreach (FamilyInstance motor in all_motors)
                {
                    //collect motor MOCP
                    //verify if can be converted to number
                    string motor_mocp_str = motor.LookupParameter("MES_(MFS) MOCP").AsString();
                    bool isNumber = Int32.TryParse("1234", out int motor_mocp);

                    if (isNumber)
                    {
                        
                    }

                }

                trac.Commit();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("An Error Occurred", "Contact the BIM Team for Assistance.");
                return Result.Failed;
            }
        }

        //verify this method works
        public ElectricalSystem GetElectricalCircuit(int revitVersion, Document document, FamilyInstance mtr)
        {
            ElectricalSystem mtrCct;

            if (revitVersion < 2021)
            {
                ElectricalSystemSet mtrCctSet = mtr.MEPModel.ElectricalSystems;

                foreach (ElectricalSystem elecSys in mtrCctSet)
                {
                    mtrCct = elecSys;

                    return mtrCct;
                }
            }

            mtrCct = mtr.MEPModel.GetElectricalSystems().First();

            return mtrCct;
        }

        public void CorrectMOCP(Element mtrCct, int mocp)
        {
            mtrCct.LookupParameter("Rating").Set(mocp);
        }
    }
}
