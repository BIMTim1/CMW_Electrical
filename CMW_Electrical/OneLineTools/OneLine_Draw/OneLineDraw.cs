using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Selection;
using OLUpdateInfo;
using Autodesk.Revit.DB.Electrical;
using OneLineTools;
using CMW_Electrical;

namespace OneLineDraw
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OneLineDraw : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            View activeView = doc.ActiveView;

            //collect E_DI_Feeder-Line Based Detail Item
            FamilySymbol feederLine = (new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .WhereElementIsElementType()
                .ToElements()
                .Where(x => x.LookupParameter("Family Name").AsString() == "E_DI_Feeder-Line Based")
                .ToList())
                .First()
                as FamilySymbol;

            if (feederLine == null)
            {
                return Result.Cancelled;
            }

            //create transaction and begin operation
            using (Transaction trac = new Transaction(doc))
            {
                trac.Start("Draw Feeder Lines");

                bool run = true;

                List<XYZ> points = new List<XYZ>();
                List<FamilyInstance> newFamInstances = new List<FamilyInstance>();

                while (run)
                {
                    try
                    {
                        XYZ point = uidoc.Selection.PickPoint(ObjectSnapTypes.Midpoints
                            | ObjectSnapTypes.Endpoints
                            | ObjectSnapTypes.Intersections
                            | ObjectSnapTypes.Nearest
                            | ObjectSnapTypes.Quadrants 
                            | ObjectSnapTypes.Perpendicular 
                            | ObjectSnapTypes.Points,
                            "Pick Next Point to Draw Feeder Lines");

                        points.Add(point);

                        if (points.Count() == 2)
                        {
                            Line curve = Line.CreateBound(points[0], points[1]);

                            FamilyInstance newFeederLine = doc.Create.NewFamilyInstance(curve, feederLine, activeView); //create feeder line

                            doc.Regenerate();

                            points.RemoveAt(0); //update list to only contain the last point
                        }
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
                    {
                        run = false;
                    }
                    catch (Exception ex)
                    {
                        return Result.Failed;
                    }
                }
                //code to prompt user to update assignment of feeders?

                trac.Commit();
                return Result.Succeeded;
            }
        }
    }
}
