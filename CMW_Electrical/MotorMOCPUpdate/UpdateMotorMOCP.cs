﻿using System;
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
using CMW_Electrical.MotorMOCPUpdate;

namespace MotorMOCPUpdate
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class UpdateMotorMOCP: IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            #region Autodesk Info
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            #endregion //Autodesk Info

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

            #region Check Motors Any
            //check if the tool collected any Motor Elements
            if (!all_motors.Any())
            {
                //TaskDialog.Show("No Motors in Project", "There are no Motor Families placed in the Active Project. The tool will now close.");
                errorReport = "There are no Motor Families placed in the Active Project. The tool will now close.";

                return Result.Cancelled;
            }
            #endregion //Check Motors Any

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMWElec-Update Motor Circuit Load Name from MOCP");

                    List<MotorInfoData> motorInfoData = new List<MotorInfoData>();

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
                            
                            //add FamilyInstance to code-behind for WPF
                            motorInfoData.Add(new MotorInfoData(motor));
                        }
                    }

                    trac.Commit();

                    MotorResultsWindow resultsWindow = new MotorResultsWindow(motorInfoData);
                    resultsWindow.ShowDialog();

                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
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
