using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneLineTools
{
    public class OLCreateFeeder
    {
        public List<FamilyInstance> CreateFeederCoordinates(FamilyInstance selectedDetailItem, FamilyInstance newDetailItem, XYZ selectedPoint, View activeView, Document document, FamilySymbol feederLine)
        {
            XYZ startPoint = (selectedDetailItem.Location as LocationPoint).Point;
            XYZ endPoint = selectedPoint;

            XYZ firstPoint;
            XYZ midTopPoint;
            XYZ midBottomPoint;
            XYZ secondPoint;

            double topY;
            double bottomY;

            bool polyLine = true;

            //find Y offsets of selected and placed Detail Items
            //double selYOffset = selectedDetailItem.Symbol.get_BoundingBox(activeView).Max[1];
            BoundingBoxXYZ selBB = selectedDetailItem.Symbol.get_BoundingBox(activeView);
            double selYOffset;
            double selCenterX;

            double placedYOffset = newDetailItem.Symbol.get_BoundingBox(activeView).Max[1];

            if (selectedDetailItem.Name.Contains("Bus"))
            {
                //selYOffset = selBB.Max.Y - ((selBB.Max.Y - selBB.Min.Y) / 2);
                double selBBCenterX = selBB.Max.X - ((selBB.Max.X - selBB.Min.X) / 2);
                selCenterX = startPoint.X + selBBCenterX;

                //create location data to place E_DI_OL_Circuit Breaker
                FamilySymbol cbSymbol = new FilteredElementCollector(document)
                    .OfCategory(BuiltInCategory.OST_DetailComponents)
                    .Where(x => x.LookupParameter("Family Name").AsString() == "E_DI_OL_Circuit Breaker")
                    .Cast<FamilySymbol>()
                    .First();

                BoundingBoxXYZ cbBB = cbSymbol.get_BoundingBox(activeView);

                double cbYOffset = cbBB.Max.Y - ((cbBB.Max.Y + cbBB.Min.Y) / 2);

                XYZ cbPlacePoint = new XYZ(selCenterX, startPoint.Y - cbYOffset, startPoint.Z);

                //create E_DI_OL_Circuit Breaker FamilyInstance
                FamilyInstance cbInst = document.Create.NewFamilyInstance(cbPlacePoint, cbSymbol, activeView);

                //collect location and bounding box information of Circuit Breaker FamilyInstance
                XYZ cbInstLoc = (cbInst.Location as LocationPoint).Point;

                BoundingBoxXYZ cbInstBB = cbInst.get_BoundingBox(activeView);

                //update startPoint and selYOffset to reference new Circuit Breaker FamilyInstance
                startPoint = new XYZ(cbInstLoc.X, cbInstBB.Min.Y, cbInstLoc.Z);
                selYOffset = cbInstBB.Max.Y;
            }
            else
            {
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

            List<FamilyInstance> feederLines = new List<FamilyInstance>();

            //create line work based on position of selected and placed elements
            if (!polyLine)
            {
                Line curve = Line.CreateBound(firstPoint, secondPoint);
                //doc.Create.NewDetailCurve(activeView, curve);
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
