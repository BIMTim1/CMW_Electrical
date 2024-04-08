using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI.Selection;


namespace OneLine_HalftoneExisting
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OneLineHalftoneExisting
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            View activeView = doc.ActiveView;

            if (activeView.ViewType != ViewType.DraftingView)
            {
                return Result.Cancelled;
            }
            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    //collect DetailItems and Lines to override
                    List<Element> detailItems = new FilteredElementCollector(doc, activeView.Id)
                        .OfCategory(BuiltInCategory.OST_DetailComponents)
                        .ToElements()
                        .Where(x => x.LookupParameter("New, Existing, Demo (1,2,3)").AsInteger() == 2)
                        .ToList();

                    List<Element> thinLines = new FilteredElementCollector(doc, activeView.Id)
                        .OfCategory(BuiltInCategory.OST_Lines)
                        .ToElements()
                        .ToList();

                    if (!detailItems.Any() && !thinLines.Any())
                    {
                        TaskDialog.Show("Tool Canceled", "No Elements are created in the active view to be overridden. The tool will now cancel.");

                        return Result.Failed;
                    }
                    else
                    {
                        trac.Start("Update Existing OneLine Elements to be Halftoned");

                        OverrideGraphicSettings overrideSettings = new OverrideGraphicSettings();
                        overrideSettings.SetHalftone(true);

                        if (detailItems.Any())
                        {
                            foreach (Element di in detailItems)
                            {
                                activeView.SetElementOverrides(di.Id, overrideSettings);
                            }
                        }

                        if (thinLines.Any())
                        {
                            foreach (Element tl in thinLines)
                            {
                                activeView.SetElementOverrides(tl.Id, overrideSettings);
                            }
                        }

                        trac.Commit();

                        return Result.Succeeded;
                    }
                }
                catch (Exception ex)
                {
                    errorReport = ex.Message;

                    return Result.Failed;
                }
            }
        }
    }
}
