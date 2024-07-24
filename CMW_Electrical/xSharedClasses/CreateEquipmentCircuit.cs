using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB.Electrical;
using OneLineTools;
using CMW_Electrical;

namespace CMW_Electrical
{
    internal class CreateEquipmentCircuit
    {
        public ElectricalSystem CreateEquipCircuit(FamilyInstance sourceEquipment, FamilyInstance fedToEquipment)
        {
            ElectricalSystem createdCircuit = CreateCircuit(sourceEquipment, fedToEquipment);

            return createdCircuit;
        }

        public ElectricalSystem CreateEquipCircuit(Document document, string sourceEquipmentName, string fedToEquipmentName)
        {
            ElectricalSystem createdCircuit = null;

            //collect the Electrical Equipment FamilyInstance
            List<Element> sourceEquipmentList = new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                .WhereElementIsNotElementType()
                .Where(x => x.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString() == sourceEquipmentName)
                .ToList();

            if (sourceEquipmentList != null)
            {
                //collect the Electrical Equipment FamilyInstance to be circuited
                FamilyInstance fedToEquipment = new FilteredElementCollector(document)
                    .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                    .WhereElementIsNotElementType()
                    .Where(x => x.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString() == fedToEquipmentName)
                    .First() as FamilyInstance;

                createdCircuit = CreateCircuit(sourceEquipmentList.First() as FamilyInstance, fedToEquipment);
            }

            return createdCircuit;
        }

        public ElectricalSystem CreateEquipCircuit(Document document, FamilyInstance sourceEquipmentInput, FamilyInstance fedToEquipmentInput)
        {
            ElectricalSystem createdCircuit = null;

            FamilyInstance sourceEquipment = null;
            FamilyInstance fedToEquipment = null;

            //set sourceEquipment to input instance or collect from model
            if (sourceEquipmentInput.Category.Name == "Detail Items")
            {
                sourceEquipment = new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                .OfClass(typeof(FamilyInstance))
                .ToElements()
                .Cast<FamilyInstance>()
                .Where(x => x.LookupParameter("EqConId").AsString() == sourceEquipmentInput.LookupParameter("EqConId").AsString())
                .First();
            }
            else
            {
                sourceEquipment = sourceEquipmentInput;
            }

            //set fedToEquipment to input instance or collect from model
            if (fedToEquipmentInput.Category.Name == "Detail Items")
            {
                fedToEquipment = new FilteredElementCollector(document)
                    .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                    .OfClass(typeof(FamilyInstance))
                    .ToElements()
                    .Cast<FamilyInstance>()
                    .Where(x => x.LookupParameter("EqConId").AsString() == fedToEquipmentInput.LookupParameter("EqConId").AsString())
                    .First();
            }

            //update method to accept detail items or equipment

            if (sourceEquipment != null && fedToEquipment != null)
            {
                createdCircuit = CreateCircuit(sourceEquipment, fedToEquipment);
            }

            return createdCircuit;
        }

        private ElectricalSystem CreateCircuit(FamilyInstance sourceEquipment, FamilyInstance fedToEquipment)
        {
            ElectricalSystem createdCircuit = null;

            ConnectorSet connectorSet = fedToEquipment.MEPModel.ConnectorManager.UnusedConnectors;

            foreach (Connector connector in connectorSet)
            {
                ElectricalSystemType elecSysType = connector.ElectricalSystemType;
                createdCircuit = ElectricalSystem.Create(connector, elecSysType);
                createdCircuit.SelectPanel(sourceEquipment);
            }

            return createdCircuit;
        }
    }
}
