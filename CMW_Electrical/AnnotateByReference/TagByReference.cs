using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMW_Electrical;
using Autodesk.Revit.UI.Selection;

namespace AnnotateByReference
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class TagByReference : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            View activeView = doc.ActiveView;

            //check ActiveView ViewType
            if (activeView.ViewType != ViewType.FloorPlan)
            {
                errorReport = "Incorrect View Type. Change your active view to a Floor Plan and rerun the tool.";
                elementSet.Insert(activeView);

                return Result.Cancelled;
            }

            IList<Element> selTags;
            IList<Reference> selObjReferences;

            //begin selection process
            try
            {
                //prompt user to select collection of tags
                ISelectionFilter selTagFilter = new CMWElecSelectionFilter.TagSelectionFilter();
                selTags = uidoc.Selection.PickElementsByRectangle(
                    selTagFilter,
                    "Select multiple tags to assign to other similar elements by rectangle.");

                //selTags = uidoc.Selection.PickElementsByRectangle(
                //    "Select multiple tags to assign to other similar elements by rectangle."); //debug only

                //prompt user to select objects based on selected tags
                string tagCat = selTags.First().Category.Name;
                tagCat = tagCat.Replace(" Tags", "s");

                //collect ISelectionFilter from method, cancel if null
                ISelectionFilter elemSelFilter = GetSelectionFilter(tagCat) ?? throw new OperationCanceledException();

                //prompt user to select collection of elements
                selObjReferences = uidoc.Selection.PickObjects(
                    ObjectType.Element,
                    elemSelFilter,
                    "Select multiple elements to tag with selected tags.");
                //selObjReferences = uidoc.Selection.PickObjects(
                //    ObjectType.Element, 
                //    "Select multiple elements to tag with selected tags."); //debug only
            }
            catch (OperationCanceledException ex)
            {
                errorReport = "User canceled operation.";

                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                errorReport = ex.Message;

                return Result.Failed;
            }

            //begin Transaction information
            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMWElec-Tag by Reference");

                    foreach (Element tagElem in selTags)
                    {
                        IndependentTag tag = tagElem as IndependentTag;

                        ElementId tagType = tag.LookupParameter("Type").AsElementId();
                        XYZ tagHeadPosition = tag.TagHeadPosition;
                        XYZ elementLoc = (tag.GetTaggedLocalElements().First().Location as LocationPoint).Point;
                        //bool hasLeader = tag.HasLeader;
                        //bool tagLeaderLanding = tag.HasLeaderElbow(tag.GetTaggedReferences().First());

                        //GetTagHeadOffset
                        XYZ tagHeadOffset = GetTagHeadOffset(tagHeadPosition, elementLoc);

                        //calculate new tag position
                        foreach (Reference selRef in selObjReferences)
                        {
                            Element selElem = doc.GetElement(selRef);
                            XYZ selElemLoc = (selElem.Location as LocationPoint).Point;

                            //CreateNewTagHeadPosition
                            XYZ newTagHeadLoc = CreateNewTagHeadPosition(tagHeadOffset, selElemLoc);

                            TagOrientation tagOrient = new TagOrientation();

                            IndependentTag newTag = IndependentTag.Create(
                                doc, 
                                tagType, 
                                activeView.Id, 
                                selRef, 
                                false, 
                                tagOrient, 
                                newTagHeadLoc);
                        }
                    }

                    trac.Commit();

                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    errorReport = ex.Message;

                    return Result.Failed;
                }
            }
        }

        public XYZ GetTagHeadOffset(XYZ refTagHeadPos, XYZ refElemPos)
        {
            //calculate offsetX value
            double offsetX = refTagHeadPos.X - refElemPos.X;

            //calculate offsetY value
            double offsetY = refTagHeadPos.Y - refElemPos.Y;

            //update XYZ value
            XYZ tagHeadOffset = new XYZ(offsetX, offsetY, 0);

            return tagHeadOffset;
        }

        public XYZ CreateNewTagHeadPosition(XYZ tagOffset, XYZ currentElemLoc)
        {
            double newX = currentElemLoc.X + tagOffset.X;
            double newY = currentElemLoc.Y + tagOffset.Y;

            XYZ newTagHeadPos = new XYZ(newX, newY, currentElemLoc.Z);

            return newTagHeadPos;
        }

        #region GetSelectionFilters
        /// <summary>
        /// Method for collecting the variable ISelectionFilter based on user tag selection.
        /// </summary>
        /// <param name="categoryRef"></param>
        /// <returns>ISelectionFilter based on text input.</returns>
        public ISelectionFilter GetSelectionFilter(string categoryRef)
        {
            ISelectionFilter filter;

            switch (categoryRef)
            {
                case "Communication Devices":
                    filter = new CMWElecSelectionFilter.CommunicationDeviceSelectionFilter();
                    break;
                case "Data Devices":
                    filter = new CMWElecSelectionFilter.DataDeviceSelectionFilter();
                    break;
                case "Fire Alarm Devices":
                    filter = new CMWElecSelectionFilter.FireAlarmDeviceSelectionFilter();
                    break;
                case "Electrical Equipment":
                    filter = new CMWElecSelectionFilter.EquipmentSelectionFilter();
                    break;
                case "Electrical Fixtures":
                    filter = new CMWElecSelectionFilter.ElecFixtureSelectionFilter();
                    break;
                case "Lighting Devices":
                    filter = new CMWElecSelectionFilter.LightingDeviceSelectionFilter();
                    break;
                case "Lighting Fixtures":
                    filter = new CMWElecSelectionFilter.LightingSelectionFilter();
                    break;
                case "Nurse Call Devices":
                    filter = new CMWElecSelectionFilter.NurseCallDeviceSelectionFilter();
                    break;
                case "Telephone Devices":
                    filter = new CMWElecSelectionFilter.TelephoneDeviceSelectionFilter();
                    break;
                case "Security Devices":
                    filter = new CMWElecSelectionFilter.SecurityDeviceSelectionFilter();
                    break;
                default:
                    filter = null;
                    break;
            }

            return filter;
        }
        #endregion ///GetSelectionFilter
    }
}
