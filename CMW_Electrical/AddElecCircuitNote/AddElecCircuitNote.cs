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
using System.Linq.Expressions;

namespace AddNoteToElectricalCircuit
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class AddElecCircuitNote : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            //Application app = uiapp.Application;

            //create transaction to modify active document
            Transaction trans = new Transaction(doc, "Update Electrical Circuits with Circuit Notes");

            BuiltInCategory bic = BuiltInCategory.OST_ElectricalCircuit;
            List<Element> allCircuits = new FilteredElementCollector(doc).OfCategory(bic).WhereElementIsNotElementType().ToElements().ToList();

            if (allCircuits.Count != 0)
            {
                try
                {
                    trans.Start();

                    foreach (Element cct in allCircuits)
                    {
                        SetLoadName(cct);
                    }
                    trans.Commit();
                    return Result.Succeeded;
                }

                catch (Exception ex)
                {
                    trans.RollBack();
                    errorReport = ex.Message;
                    TaskDialog.Show("Update Circuit Load Name Failed",
                        "Something went wrong while trying to set the Electrical Circuit Load Name values. Contact the BIM Team for assistance.");
                    return Result.Failed;
                }
            }
            else
            {
                TaskDialog.Show("Circuit Notes to Load Name Failed",
                    "There are no circuits in the project. Once circuits have been created, this tool can then be run.");
                return Result.Failed;
            }
        }

        public void SetLoadName(Element cct)
        {
            //collect circuit parameters
            string frontNote = cct.LookupParameter("E_Circuit Note-Front").AsString();
            string endNote = cct.LookupParameter("E_Circuit Note-Back").AsString();
            string currentLoadName = cct.LookupParameter("Load Name").AsString();
            Parameter cctLoadName = cct.LookupParameter("Load Name");
            //check if frontNote or endNote are blank
            if (frontNote != null) //only need to compare null as parameter = Project Parameter
            {
                if (!currentLoadName.StartsWith(frontNote))
                {
                    string updatedName = frontNote + " " + currentLoadName;
                    cctLoadName.Set(updatedName);
                    currentLoadName = updatedName;
                }
            }

            if (endNote != null) //only need to compare null as parameter = Project Parameter
            {
                if (!currentLoadName.EndsWith(endNote))
                {
                    cctLoadName.Set(currentLoadName + " " + endNote);
                }
            }
        }
    }
}
