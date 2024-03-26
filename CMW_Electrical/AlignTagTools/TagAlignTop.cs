using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Windows.Media.Animation;

namespace AlignTagTools
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class TagAlignTop : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            //tagAlign Identifier
            string tagAlignId = "TagAlignTop";

            try
            {
                TagAlign ta = new TagAlign();
                ta.SelectAndAlignTags(uidoc, doc, tagAlignId);

                return Result.Succeeded;
            }

            catch (OperationCanceledException ex)
            {
                TaskDialog.Show("User Canceled", "Tool canceled.");
                return Result.Cancelled;
            }

            catch (Exception ex)
            {
                return Result.Failed;
            }
        }
    }
}
