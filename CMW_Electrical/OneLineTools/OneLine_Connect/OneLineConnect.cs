﻿using Autodesk.Revit.Attributes;
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

namespace OneLineConnect
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OneLineConnect : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            View activeView = doc.ActiveView;

            //check for EqConId Current Value parameter
            EqConIdCheck eqConIdCheck = new EqConIdCheck();
            bool eqConIdExists = eqConIdCheck.EqConIdCurrentValueExists(doc);

            if (!eqConIdExists)
            {
                errorReport = "The EqConId Current Value parameter does not exist in the current Document. Contact the BIM team for assistance.";
                elementSet.Insert(activeView);

                return Result.Cancelled;
            }

            //stop tool if activeView is not a Drafting View
            if (activeView.ViewType != ViewType.DraftingView)
            {
                TaskDialog.Show("Incorrect View Type", "Open a Drafting View that contains your One-Line Diagram and rerun the tool.");
                return Result.Cancelled;
            }

            Element sourceDetailItem;
            Element fedToDetailItem;
            //prompt user to select (2) DetailComponents
            try
            {
                ISelectionFilter selFilter = new CMWElecSelectionFilter.DetailItemSelectionFilter();
                Reference sourceDetailItemSelection = uidoc.Selection.PickObject(ObjectType.Element, selFilter, "Select the Source Detail Item.");
                //Reference sourceDetailItemSelection = uidoc.Selection.PickObject(ObjectType.Element, "Select the Source Detail Item."); //debug only

                sourceDetailItem = doc.GetElement(sourceDetailItemSelection);

                Reference fedToDetailItemSelection = uidoc.Selection.PickObject(ObjectType.Element, selFilter, "Select the Fed To Detail Item.");
                //Reference fedToDetailItemSelection = uidoc.Selection.PickObject(ObjectType.Element, "Select the Fed To Detail Item."); //debug only

                fedToDetailItem = doc.GetElement(fedToDetailItemSelection);
            }
            catch (OperationCanceledException ex)
            {
                errorReport = "User canceled operation.";
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                errorReport = ex.Message;
                elementSet.Insert(activeView);
                
                return Result.Failed;
            }

            //start updating process
            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMWElec-Power");

                    DetailItemInfo detItemInfo = new DetailItemInfo(fedToDetailItem)
                    {
                        EqConIdConnectedSource = sourceDetailItem.LookupParameter("EqConId").AsString() //set value of EqConIdConnectedSource
                    };

                    //verify if feeder lines already exist
                    List<Element> createdFeederLines = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_DetailComponents)
                        .WhereElementIsNotElementType()
                        .ToElements()
                        .Where(x => x.LookupParameter("Family").AsValueString() == "E_DI_Feeder-Line Based" && x.LookupParameter("EqConId").AsString() == detItemInfo.EqConId)
                        .ToList();

                    if (!createdFeederLines.Any())
                    {
                        //collect E_DI_Feeder-Line Based Detail Item
                        FamilySymbol feederLine = (new FilteredElementCollector(doc)
                            .OfCategory(BuiltInCategory.OST_DetailComponents)
                            .WhereElementIsElementType()
                            .ToElements()
                            .Where(x => x.LookupParameter("Family Name").AsString() == "E_DI_Feeder-Line Based")
                            .ToList())
                            .First()
                            as FamilySymbol;

                        //create feeder lines
                        List<FamilyInstance> feederLines = new OLCreateFeeder().CreateFeeder(
                            sourceDetailItem as FamilyInstance,
                            fedToDetailItem as FamilyInstance,
                            (fedToDetailItem.Location as LocationPoint).Point,
                            activeView,
                            doc,
                            feederLine);

                        foreach (FamilyInstance feeder in feederLines)
                        {
                            feeder.LookupParameter("EqConId").Set(detItemInfo.EqConId);
                        }
                    }

                    //create ElectricalSystem of selected DetailItems
                    ElectricalSystem createdCircuit = 
                        new CreateEquipmentCircuit().CreateEquipCircuit(
                            doc, 
                            sourceDetailItem as FamilyInstance, 
                            fedToDetailItem as FamilyInstance);

                    trac.Commit();

                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    errorReport = "An error occurred that has prevented the tool from running. Contact the BIM team for assistance.";
                    elementSet.Insert(sourceDetailItem);

                    return Result.Failed;
                }
            }
        }
    }
}
