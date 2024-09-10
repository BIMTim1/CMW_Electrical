using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI.Selection;
using CMW_Electrical;

namespace NestedFontUpdate
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class UpdateNestedFamilyFont : IExternalCommand
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

            View activeView = doc.ActiveView;
            Reference selItem;
            string font = "ARchitxt";

            #region ActiveView check
            //active view check
            if (activeView.ViewType != ViewType.FloorPlan)
            {
                errorReport = "Incorrect view type. Change the active view to a FloorPlan and rerun the tool.";

                return Result.Cancelled;
            }
            #endregion //ActiveView check

            #region Element Selection
            //prompt user to select an element
            try
            {
                ISelectionFilter selFilter = new CMWElecSelectionFilter.ElectricalCategoryFilter();

                selItem = uidoc.Selection.PickObject(
                    ObjectType.Element,
                    selFilter,
                    "Select an Electrical element to update the nested font.");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
            {
                errorReport = "User canceled operation";

                return Result.Cancelled;
            }
            #endregion //Element Selection

            try
            {
                //get Family of selected element
                Family selFam = (doc.GetElement(selItem) as FamilyInstance).Symbol.Family;

                IFamilyLoadOptions famLoadOptions = new FamilyOption();

                //Edit selected family
                Document famDoc = doc.EditFamily(selFam);

                List<Family> colFams = new FilteredElementCollector(famDoc)
                    .OfClass(typeof(Family))
                    .ToElements()
                    .Cast<Family>()
                    .Where(x => x.IsEditable && !x.Name.Contains("Section"))
                    .ToList();

                #region Nested Annotation Editing
                foreach (Family fam in colFams)
                {
                    Document annoFamDoc = famDoc.EditFamily(fam);

                    using (Transaction trac = new Transaction(annoFamDoc))
                    {
                        trac.Start("Update nested Annotation Family Font");

                        string bip = "Text Font";
                        IList<Element> textTypes = new FilteredElementCollector(annoFamDoc).OfClass(typeof(TextNoteType)).ToElements();

                        foreach (Element tType in textTypes)
                        {
                            string tempFont = "Arial Narrow";
                            tType.LookupParameter(bip).Set(tempFont);
                            tType.LookupParameter(bip).Set(font);
                        }

                        trac.Commit();
                    }

                    annoFamDoc.LoadFamily(famDoc, famLoadOptions);
                    annoFamDoc.Close(false);
                }
                #endregion //Nested Annotation Editing

                famDoc.LoadFamily(doc, famLoadOptions);
                famDoc.Close(false);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                errorReport = ex.Message;

                return Result.Failed;
            }
        }
    }

    public class FamilyOption : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            return true;
        }
        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource familySrc, out bool overwriteParameterValues)
        {
            familySrc = FamilySource.Family;
            overwriteParameterValues = true;
            return true;
        }
    }
}
