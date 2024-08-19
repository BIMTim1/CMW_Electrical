using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneLine_FindDisconnected
{
    public class DisconnectedElement
    {
        private readonly FamilyInstance DEFamilyInstance;
        private readonly FamilySymbol DEFamilySymbol;
        private readonly string DEFamilyName;
        private readonly ElementId DEElementId;
        private readonly string DEPanelName;
        private bool DEClearEqConId;

        public DisconnectedElement(Element element)
        {
            DEFamilyInstance = element as FamilyInstance;

            DEFamilySymbol = DEFamilyInstance.Symbol;
            DEFamilyName = DEFamilySymbol.Family.Name;

            DEElementId = DEFamilyInstance.Id;

            //check if Electrical Equipment or Detail Item
            if (DEFamilyInstance.Category.Name == "Electrical Equipment")
            {
                DEPanelName = DEFamilyInstance.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString();
            }
            else
            {
                DEPanelName = DEFamilyInstance.LookupParameter("Panel Name - Detail").AsString();
            }

            DEClearEqConId = false;
        }

        /// <summary>
        /// Get the Family Instance of the element.
        /// </summary>
        public FamilyInstance GetFamilyInstance
        {
            get { return DEFamilyInstance; }
        }

        /// <summary>
        /// Get combined string of Family Name and FamilySymbol Name
        /// </summary>
        public string InstanceInfo
        {
            get { return DEFamilyName + ", " + DEFamilySymbol.Name; }
        }

        /// <summary>
        /// Get Panel Name or Panel Name - Detail based on referenced element type.
        /// </summary>
        public string Name
        {
            get { return DEPanelName; }
        }

        /// <summary>
        /// Get ElementId of element.
        /// </summary>
        public ElementId Id
        {
            get { return DEElementId; }
        }

        /// <summary>
        /// Get the boolean associated for FindDisconnected form. Default of false.
        /// </summary>
        public bool ClearEqConId
        {
            get { return DEClearEqConId; }
            set { DEClearEqConId = value; }
        }
    }
}
