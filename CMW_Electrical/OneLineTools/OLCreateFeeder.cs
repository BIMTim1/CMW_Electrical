using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OneLineTools
{
    public class OLCreateFeeder
    {
        public List<FamilyInstance> CreateFeeder(FamilyInstance selectedDetailItem, FamilyInstance newDetailItem, XYZ selectedPoint, View activeView, Document document, FamilySymbol feederLine)
        {
            XYZ startPoint;
            XYZ endPoint = selectedPoint;

            XYZ firstPoint;
            XYZ midTopPoint;
            XYZ midBottomPoint;
            XYZ secondPoint;

            double topY;
            double bottomY;

            bool polyLine = true;

            List<FamilyInstance> feederLines = new List<FamilyInstance>();

            //create initial offsets and BoundingBoxXYZ elements
            BoundingBoxXYZ selBB = selectedDetailItem.Symbol.get_BoundingBox(activeView);
            double selYOffset;
            double selCenterX;

            double placedYOffset = newDetailItem.Symbol.get_BoundingBox(activeView).Max[1];

            FamilySymbol cbSymbol = new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .OfClass(typeof(FamilySymbol))
                .Where(x => x.Name.Contains("Circuit"))
                .Cast<FamilySymbol>()
                .First();

            if (selectedDetailItem.Name.Contains("Bus"))
            {
                XYZ busPoint = (selectedDetailItem.Location as LocationPoint).Point;

                double selBBCenterX = selBB.Max.X - ((selBB.Max.X - selBB.Min.X) / 2);
                selCenterX = busPoint.X + selBBCenterX;

                //FamilySymbol cbSymbol = new FilteredElementCollector(document)
                //    .OfCategory(BuiltInCategory.OST_DetailComponents)
                //    .OfClass(typeof(FamilySymbol))
                //    .Where(x => x.Name.Contains("Circuit"))
                //    .Cast<FamilySymbol>()
                //    .First();

                XYZ cbPlacePoint = new XYZ(selCenterX, busPoint.Y, busPoint.Z);

                //create E_DI_OL_Circuit Breaker FamilyInstance
                FamilyInstance cbInst = document.Create.NewFamilyInstance(cbPlacePoint, cbSymbol, activeView);
                feederLines.Add(cbInst);

                document.Regenerate();

                //collect location and bounding box information of Circuit Breaker FamilyInstance
                XYZ cbInstLoc = (cbInst.Location as LocationPoint).Point;

                BoundingBoxXYZ cbInstBB = cbInst.get_BoundingBox(activeView);

                //update startPoint and selYOffset to reference new Circuit Breaker FamilyInstance
                startPoint = new XYZ(cbInstLoc.X, cbInstBB.Min.Y, cbInstLoc.Z);
                selYOffset = 0;
            }
            //else if (selectedDetailItem.Name.Contains("CT"))
            //{
            //    XYZ ctPoint = (selectedDetailItem.Location as LocationPoint).Point;


            //}
            else
            {
                startPoint = (selectedDetailItem.Location as LocationPoint).Point;
                selYOffset = selBB.Max.Y;
            }

            //determine if multiple lines need to be created
            if (startPoint.X == endPoint.X)
            {
                polyLine = false;
            }

            //determine offsets from uppermost point and bottom-most point
            if (startPoint.Y > endPoint.Y)
            {
                topY = startPoint.Y - selYOffset;
                bottomY = endPoint.Y + placedYOffset;

                firstPoint = new XYZ(startPoint.X, topY, startPoint.Z);
                secondPoint = new XYZ(endPoint.X, bottomY, startPoint.Z);
            }
            else
            {
                topY = endPoint.Y - placedYOffset;
                bottomY = startPoint.Y + selYOffset;

                firstPoint = new XYZ(endPoint.X, topY, endPoint.Z);
                secondPoint = new XYZ(startPoint.X, bottomY, endPoint.Z);
            }

            //create line work based on position of selected and placed elements
            if (!polyLine)
            {
                Line curve = Line.CreateBound(firstPoint, secondPoint);

                FamilyInstance newFeeder = document.Create.NewFamilyInstance(curve, feederLine, activeView);

                feederLines.Add(newFeeder);
            }
            else
            {
                double midY = topY - (Line.CreateBound(firstPoint, new XYZ(firstPoint.X, bottomY, secondPoint.Z)).Length / 2);
                midTopPoint = new XYZ(firstPoint.X, midY, firstPoint.Z);
                midBottomPoint = new XYZ(secondPoint.X, midY, secondPoint.Z);

                Line curve1 = Line.CreateBound(firstPoint, midTopPoint);
                Line curve2 = Line.CreateBound(midTopPoint, midBottomPoint);
                Line curve3 = Line.CreateBound(midBottomPoint, secondPoint);
                List<Line> curveList = new List<Line>()
                {
                    curve1,
                    curve2,
                    curve3
                };

                foreach (Line line in curveList)
                {
                    FamilyInstance newFeeder = document.Create.NewFamilyInstance(line, feederLine, activeView);

                    feederLines.Add(newFeeder);
                }
            }

            return feederLines;
        }
    }
}
