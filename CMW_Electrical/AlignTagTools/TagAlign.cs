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
using CMW_Electrical;

namespace AlignTagTools
{
    public class TagAlign
    {
        public void SelectAndAlignTags(UIDocument uidoc, Document doc, string alignTagId)
        {
            ISelectionFilter selFilter = new CMWElecSelectionFilter.TagSelectionFilter();
            //Reference selRefTag = uidoc.Selection.PickObject(ObjectType.Element, selFilter, "Select Reference Tag to Align to");
            Reference selRefTag = uidoc.Selection.PickObject(ObjectType.Element, "Select Reference Tag to Align to"); //debug only

            Element refTag = doc.GetElement(selRefTag);

            //IList<Reference> selAdjustTags = uidoc.Selection.PickObjects(ObjectType.Element, tagFilter, "Select Tags to be Aligned");
            IList<Reference> selAdjustTags = uidoc.Selection.PickObjects(ObjectType.Element, "Select Tags to be Aligned"); //debug only

            List<Element> adjustTags = (from x in selAdjustTags select doc.GetElement(x)).ToList();

            //collect ActiveView to be used for tag references / movement
            View activeView = doc.ActiveView;

            BoundingBoxXYZ refTagBB = refTag.get_BoundingBox(activeView);

            using (Transaction trac = new Transaction(doc))
            {
                trac.Start("Tag Align");

                foreach (IndependentTag t in adjustTags)
                {
                    UpdateTagLocation(t, refTagBB, activeView, alignTagId);
                }

                trac.Commit();
            }
        }

        public void UpdateTagLocation(IndependentTag tag, BoundingBoxXYZ referenceTagBB, View docActiveView, string tagAlignId)
        {
            BoundingBoxXYZ tagBB = tag.get_BoundingBox(docActiveView);
            XYZ tagMax = tagBB.Max;
            XYZ tagMin = tagBB.Min;

            double y;
            double x;
            double centerRefX = referenceTagBB.Max.X - ((referenceTagBB.Max.X - referenceTagBB.Min.X) / 2);
            double centerTagX = tagMax.X - ((tagMax.X - tagMin.X) / 2);
            XYZ newXYZ;

            switch (tagAlignId)
            {
                case "TagAlignTop":
                    y = referenceTagBB.Max.Y;

                    newXYZ = new XYZ(tagMin.X, y + 0.125, tagMin.Z);

                    break;

                case "TagAlignBottom":
                    y = referenceTagBB.Min.Y;
                    XYZ bottomOffset = new XYZ(0, tagMax.Y - tagMin.Y, 0);

                    newXYZ = new XYZ(tagMin.X, y + 0.125, tagMin.Z);
                    newXYZ -= bottomOffset;

                    break;

                case "TagAlignLeft":
                    x = referenceTagBB.Min.X;

                    newXYZ = new XYZ(x, tagMin.Y, tagMin.Z);

                    break;

                case "TagAlignRight":
                    x = referenceTagBB.Max.X;
                    XYZ offset = new XYZ(tagMax.X - tagMin.X, 0, 0);

                    newXYZ = new XYZ(x, tagMin.Y, tagMin.Z);
                    newXYZ -= offset;
                    
                    break;

                case "TagAlignTopAndCenter":
                    x = tagMax.X - (tagMax.X - centerRefX) - (tagMax.X - centerTagX);

                    newXYZ = new XYZ(x, referenceTagBB.Max.Y + 0.125, tagMin.Z);
                    
                    break;

                case "TagAlignBottomAndCenter":
                    XYZ offsetY = new XYZ(0, tagMax.Y - tagMin.Y, 0);

                    x = tagMax.X - (tagMax.X - centerRefX) - (tagMax.X - centerTagX);

                    newXYZ = new XYZ(x, referenceTagBB.Min.Y + 0.125, tagMin.Z);
                    newXYZ -= offsetY;

                    break;

                case "TagAlignCenter":
                    x = tagMax.X - (tagMax.X - centerRefX) - (tagMax.X - centerTagX);

                    newXYZ = new XYZ(x, tagMin.Y + 0.125, tagMin.Z);
                    
                    break;
                default:
                    newXYZ = null;
                    break;

            }

            newXYZ -= tag.TagHeadPosition;
            tag.Location.Move(newXYZ);
        }

        public class MySelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element.Category.Name.Contains("Tags"))
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
    }
}
