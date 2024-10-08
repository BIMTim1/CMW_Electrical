﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Events;
using System.Windows.Input;
using Autodesk.Revit.UI.Selection;
using CMW_Electrical;
using OneLineTools;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace OneLine_Associate
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OneLineAssociate: IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            #region Autodesk Info
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            #endregion //Autodesk Info

            View activeView = doc.ActiveView;

            #region EqConId check
            //check for EqConId Current Value parameter
            EqConIdCheck eqConIdCheck = new EqConIdCheck();
            bool eqConIdExists = eqConIdCheck.EqConIdCurrentValueExists(doc);

            //cancel tool if EqConId Current Value parameter does not exist in project
            if (!eqConIdExists)
            {
                errorReport = "The EqConId Current Value parameter does not exist in the current Document. Contact the BIM team for assistance.";
                elementSet.Insert(activeView);

                return Result.Cancelled;
            }
            #endregion //EqConId check

            //collect names in array to add to selection form
            List<Element> allRefElements = null;
            BuiltInCategory selBic;
            string paramRef = "";

            ISelectionFilter selFilter;
            string selectionStatus = "";

            if (activeView.ViewType == ViewType.FloorPlan)
            {
                selBic = BuiltInCategory.OST_DetailComponents;

                //collect all Electrical Equipment families in document
                allRefElements = new FilteredElementCollector(doc)
                    .OfCategory(selBic)
                    .OfClass(typeof(FamilyInstance))
                    .ToElements()
                    .Where(x => x.LookupParameter("EqConId").AsString() == null || x.LookupParameter("EqConId").AsString() == "")
                    .Where(x=>x.LookupParameter("Panel Name - Detail") != null)
                    .ToList();

                paramRef = "Panel Name - Detail";
                selFilter = new CMWElecSelectionFilter.EquipmentSelectionFilter();
                selectionStatus = "Select an Electrical Equipment instance to reference";
            }
            else if(activeView.ViewType == ViewType.DraftingView)
            {
                selBic = BuiltInCategory.OST_ElectricalEquipment;

                //collect all Detail Item families in document
                allRefElements = new FilteredElementCollector(doc)
                    .OfCategory(selBic)
                    .OfClass(typeof(FamilyInstance))
                    .ToElements()
                    .Where(x => x.LookupParameter("EqConId").AsString() == null || x.LookupParameter("EqConId").AsString() == "")
                    .ToList();

                paramRef = "Panel Name";
                selFilter = new CMWElecSelectionFilter.DetailItemSelectionFilter();
                selectionStatus = "Select a Schematic Detail Item to reference.";
            }
            else //cancel tool if activeView is not a FloorPlan or DraftingView
            {
                errorReport = "Change the active view to a Floor Plan or Drafting View then rerun the tool.";
                elementSet.Insert(activeView);

                return Result.Cancelled;
            }

            //cancel tool if not items can be found
            if (allRefElements == null || !allRefElements.Any())
            {
                errorReport = "There are no elements in the model that can be referenced to. The tool will now cancel.";

                return Result.Cancelled;
            }

            Reference userSelection;

            try
            {
                //prompt user to select DetailComponent to use for association
                userSelection = uidoc.Selection.PickObject(ObjectType.Element, selFilter, selectionStatus);
                //userSelection = uidoc.Selection.PickObject(ObjectType.Element, selectionStatus); //debug only
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
            {
                //user canceled
                errorReport = "User canceled tool.";

                return Result.Cancelled;
            }

            //sort collected elements
            allRefElements = allRefElements.OrderBy(x => x.LookupParameter(paramRef).AsString()).ToList();

            //launch form to select Equipment Family
            OneLineAssociateForm form = new OneLineAssociateForm(allRefElements, paramRef);
            form.ShowDialog();

            if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                return Result.Cancelled;
            }

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMWElec-Associate Electrical Equipment and Schematic Component");

                    ElecEquipInfo elecEquipInfo;
                    DetailItemInfo detItemInfo;

                    Element selFormElem = allRefElements[form.cBoxEquipSelection.SelectedIndex];

                    if (paramRef == "Panel Name")
                    {
                        elecEquipInfo = new ElecEquipInfo(selFormElem);

                        detItemInfo = new DetailItemInfo(doc.GetElement(userSelection));

                        detItemInfo.Name = elecEquipInfo.Name;
                    }
                    else
                    {
                        detItemInfo = new DetailItemInfo(selFormElem);

                        elecEquipInfo = new ElecEquipInfo(doc.GetElement(userSelection))
                        {
                            Name = detItemInfo.Name
                        };
                    }

                    OLEqConIdUpdateClass updateEqConId = new OLEqConIdUpdateClass();
                    updateEqConId.OneLineEqConIdValueUpdate(elecEquipInfo, detItemInfo, doc);

                    trac.Commit();

                    //TaskDialog.Show("Associate Complete", "Electrical Equipment and Schematic Component have been had their EqConId values updated.");
                    return Result.Succeeded;
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
