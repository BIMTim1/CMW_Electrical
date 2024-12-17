using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;

namespace CMW_Electrical.EquipmentTreeView
{
    public partial class EquipmentTreeForm : System.Windows.Forms.Form
    {
        public EquipmentTreeForm(List<FamilyInstance> elecEquipment)
        {
            InitializeComponent();

            BuiltInParameter bipPanName = BuiltInParameter.RBS_ELEC_PANEL_NAME;
            BuiltInParameter bipSupplyFrom = BuiltInParameter.RBS_ELEC_PANEL_SUPPLY_FROM_PARAM;

            List<FamilyInstance> source_equip = new List<FamilyInstance>();

            foreach (FamilyInstance famInst in elecEquipment)
            {
                string supplyFrom = famInst.get_Parameter(bipSupplyFrom).AsString();
                if (supplyFrom == "" || supplyFrom == null)
                {
                    source_equip.Add(famInst);
                }
            }

            //sort the source_equip list by Panel Name
            source_equip.OrderBy(x => x.get_Parameter(bipPanName).AsString());

            foreach (FamilyInstance seq in source_equip)
            {
                string eqName = seq.get_Parameter(bipPanName).AsString();

                treeView1.Nodes.Add(eqName);

                int equipmentCount = elecEquipment.Count();

                TraverseCircuit(elecEquipment, seq, eqName, equipmentCount);
            }
        }
        public void Button_Close(object sender, EventArgs eventArgs)
        {
            this.Close();
        }

        public void TraverseCircuit(List<FamilyInstance> equipmentInstances, FamilyInstance familyInstance, string equipmentName, int depth)
        {
            if (depth <= 0) return;

            foreach (FamilyInstance equipInstance in equipmentInstances)
            {
                string subEquipName = equipInstance.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString();
                string subSupplyFrom = equipInstance.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_SUPPLY_FROM_PARAM).AsString();

                if (subSupplyFrom == equipmentName)
                {
                    TreeNode parentNode = TraverseNodes(treeView1.Nodes, equipmentName);

                    parentNode?.Nodes.Add(subEquipName);

                    TraverseCircuit(equipmentInstances, equipInstance, subEquipName, depth - 1);
                }
            }
        }

        public TreeNode TraverseNodes(TreeNodeCollection nodes, string compString)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Text == compString)
                {
                    return node;
                }

                if (node.Nodes.Count > 0)
                {
                    TreeNode foundNode = TraverseNodes(node.Nodes, compString);
                    if (foundNode != null)
                    {
                        //Return the found node if it's not null
                        return foundNode;
                    }
                }
            }

            //return null if no matching node is found
            return null;
        }

        //public void TraverseCircuit(FamilyInstance familyInstance, string equipmentName)
        //{
        //    //if (traverseDepth <= 0) return;

        //    ISet<ElectricalSystem> electricalSystems = familyInstance.MEPModel.GetElectricalSystems();
        //    //traverseDepth = electricalSystems.Count();

        //    if (!electricalSystems.Any())
        //    {
        //        return;
        //    }
        //    else
        //    {
        //        foreach (ElectricalSystem electricalSystem in electricalSystems)
        //        {
        //            ElementSet elementSet = electricalSystem.Elements;

        //            FamilyInstance subFamilyInstance = null;

        //            if (elementSet.Size == 1)
        //            {
        //                subFamilyInstance = (from Element el in elementSet where el.Category.Name == "Electrical Equipment" select el).ToList().First() as FamilyInstance;
        //            }

        //            if (subFamilyInstance != null)
        //            {
        //                string sourcePanelName = subFamilyInstance.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_SUPPLY_FROM_PARAM).AsString();

        //                TreeNode parentNode = (from TreeNode tn in treeView1.Nodes where tn.Text == sourcePanelName select tn).ToList().First();

        //                string panelName = subFamilyInstance.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString();

        //                parentNode.Nodes.Add(panelName);

        //                TraverseCircuit(subFamilyInstance, panelName);
        //            }
        //        }
        //    }
        //}
    }
}
