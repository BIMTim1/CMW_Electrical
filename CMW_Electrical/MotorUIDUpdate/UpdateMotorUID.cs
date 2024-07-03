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
using Autodesk.Revit.UI.Selection;
using CMW_Electrical;
using CMW_Electrical.MotorUIDUpdate;

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
            View activeView = doc.ActiveView;

            //cancel tool if incorrect ViewType
            if (activeView.ViewType != ViewType.FloorPlan)
            {
                TaskDialog.Show("Incorrect View Type", 
                    "The current View must be a Floor Plan. Change your active view to a Floor Plan and then rerun the tool.");

                return Result.Cancelled;
            }

            //check for already selected elements
            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();

            //user selection if no elements preselected
            if (selectedIds.Count() == 0)
            {
                try
                {
                    //ISelectionFilter selFilter = new CMWElecSelectionFilter.ElecFixtureSelectionFilter();
                    ISelectionFilter selFilter = new MotorSelectionFilter();
                    IList<Element> selectedElems = uidoc.Selection.PickElementsByRectangle(selFilter, "Select Motors by Rectangle to Update");

                    //test if list is empty
                    if (selectedElems.Count() == 0)
                    {
                        return Result.Cancelled;
                        throw new NoElementSelectionException("No elements selected");
                    }

                    foreach (Element elem in selectedElems)
                    {
                        selectedIds.Add(elem.Id);
                    }
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
                {
                    return Result.Cancelled;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("An Error Occurred", 
                        "An error occurred that has prevented the tool from running. Contact the BIM team for assistance.");

                    return Result.Failed;
                }
            }

            //wrap rest of code in Transaction
            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("Update Motor UIDs and Circuit Load Names");

                    //filter selected ElementIds
                    List<FamilyInstance> motors = new FilteredElementCollector(doc, selectedIds)
                        .OfCategory(bic)
                        .ToElements()
                        .Cast<FamilyInstance>()
                        .Where(x => x.Host.Category.Name == "Mechanical Equipment")
                        .ToList();

                    //check if selected elements can be modified
                    if (motors.Count() == 0)
                    {
                        TaskDialog.Show("No Motors to Update", 
                            "The selected elements do not meet the update criteria. The tool will now cancel.");

                        return Result.Cancelled;
                    }

                    foreach (FamilyInstance motor in motors)
                    {
                        UpdateMotorInfo(motor);
                    }

                    ///regenerate the model so that the updated UID appears in form
                    doc.Regenerate();

                    //create form instance to display which elements were updated
                    MotorsUpdatedForm form = new MotorsUpdatedForm(motors);
                    form.ShowDialog();

                    ///verify if Transaction needs to be committed after form
                    trac.Commit();

                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("An Error Occurred",
                        "An error occurred that has prevented the tool from running. Contact the BIM team for assistance.");
                    
                    return Result.Failed;
                }
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

        public class MotorSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element.Category.Name == "Electrical Fixtures" && element.LookupParameter("Family").AsValueString() == "E_EF_Motor")
                {
                    return true;
                }
                return false;
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
            }
        }

        public class NoElementSelectionException : Exception
        {
            public NoElementSelectionException(string message) : base(message)
            {
            }
        }
    }
}
