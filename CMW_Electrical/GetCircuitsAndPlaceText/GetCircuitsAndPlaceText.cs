using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMW_Electrical.GetCircuitsAndPlaceText;

namespace GetCircuitsAndPlaceText
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GetCircuitsAndPlaceText : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            #region AutodeskInfo
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            #endregion //AutodeskInfo

            //collect BuiltInCategories for tool
            BuiltInCategory bicElecSys = BuiltInCategory.OST_ElectricalCircuit;
            BuiltInCategory bicSpaces = BuiltInCategory.OST_MEPSpaces;

            //collect ActiveView
            View activeView = doc.ActiveView;

            #region ViewType check
            //check if activeView is not a FloorPlan
            if (activeView.ViewType != ViewType.FloorPlan)
            {
                errorReport = "Incorrect View Type. Change the active view to a FloorPlan view and rerun the tool.";

                return Result.Cancelled;
            }
            #endregion //ViewType check

            //collect ElectricalSystems in active document
            List<ElectricalSystem> all_circuits = 
                new FilteredElementCollector(doc)
                .OfCategory(bicElecSys)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<ElectricalSystem>()
                .ToList();

            #region ElectricalSystem check
            //cancel if no ElectricalSystems in active document
            if (!all_circuits.Any())
            {
                errorReport = "No Electrical Circuits in active document. The tool will now cancel.";

                return Result.Cancelled;
            }
            #endregion //ElectricalSystem check

            //get current view level name
            string lvlName = activeView.LookupParameter("Associated Level").AsString();

            //collect spaces created on the activeView level
            List<Element> lvl_spaces = 
                new FilteredElementCollector(doc)
                .OfCategory(bicSpaces)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(x=>x.get_Parameter(BuiltInParameter.LEVEL_NAME).AsString() == lvlName)
                .ToList();

            #region Check Spaces on Associated Level
            //check if any Spaces exist on current view level
            if (!lvl_spaces.Any())
            {
                errorReport = "No Spaces have been created on the current level. The tool will now cancel.";

                return Result.Cancelled;
            }
            #endregion //Check Spaces on Associated Level

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMW-Elec - Place Circuit Text of Selected Spaces");

                    //sort list of Spaces
                    lvl_spaces = lvl_spaces.OrderBy(x => x.get_Parameter(BuiltInParameter.ROOM_NAME).AsString()).ToList();

                    //create new instance and launch EnterSpaceNameForm
                    EnterSpaceNameForm form = new EnterSpaceNameForm(lvl_spaces);
                    form.ShowDialog();

                    #region check form result
                    //check result of form
                    if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    {
                        errorReport = "User canceled operation.";

                        return Result.Cancelled;
                    }
                    #endregion //check form result

                    List<ElementId> filSpaceIds = (from sp 
                                                    in lvl_spaces 
                                                    where sp.get_Parameter(BuiltInParameter.ROOM_NAME).AsString().ToLower().Contains(form.textBox1.Text.ToLower()) 
                                                    select sp.Id).ToList();

                    List<FamilyInstance> elecFixtures = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_ElectricalFixtures)
                        .OfClass(typeof(FamilyInstance))
                        .Cast<FamilyInstance>()
                        .Where(x => x.Space != null && filSpaceIds.Contains(x.Space.Id))
                        .ToList();

                    #region check for ElectricalFixtures
                    //check if any elements were collected in elecFixtures
                    if (!elecFixtures.Any())
                    {
                        errorReport = "There are no Electrical Fixtures that match the search criteria. The tool will now cancel.";

                        return Result.Cancelled;
                    }
                    #endregion //check for ElectricalFixtures

                    #region Create TextNote Information
                    //create TextNote information
                    ElementId tnType = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
                    TextNoteOptions tnOptions = new TextNoteOptions()
                    {
                        HorizontalAlignment = HorizontalTextAlignment.Center,
                        TypeId = tnType
                    };
                    #endregion //Create TextNote Information

                    #region Create TextNotes at each Space
                    //iterate through list of spaces and create TextNotes from associated ElectricalFixtures
                    foreach (ElementId spId in filSpaceIds)
                    {
                        //collect Space info for TextNote creation
                        Element sp = doc.GetElement(spId);
                        XYZ loc = (sp.Location as LocationPoint).Point;

                        //create blank list of strings to be sorted and joined
                        List<string> cctValues = new List<string>();

                        //iterate through ElectricalFixtures for space circuit information
                        foreach (FamilyInstance ef in elecFixtures)
                        {
                            if (ef.Space.Id == spId && ef.get_Parameter(BuiltInParameter.RBS_ELEC_CIRCUIT_PANEL_PARAM).AsString() != "")
                            {
                                cctValues.Add(
                                    ef.get_Parameter(BuiltInParameter.RBS_ELEC_CIRCUIT_PANEL_PARAM).AsString() 
                                    + "-" 
                                    + ef.get_Parameter(BuiltInParameter.RBS_ELEC_CIRCUIT_NUMBER).AsString());
                            }
                        }

                        //sort Circuit info from ElectricalFixtures
                        cctValues.Sort();

                        string output = string.Join("\n", cctValues);

                        //create TextNote
                        TextNote note = TextNote.Create(doc, activeView.Id, loc, output, tnOptions);
                    }
                    #endregion //Create TextNotes at each Space

                    trac.Commit();
                }
                catch (Exception ex)
                {
                    errorReport = ex.Message;

                    return Result.Failed;
                }
            }

            return Result.Succeeded;
        }
    }
}
