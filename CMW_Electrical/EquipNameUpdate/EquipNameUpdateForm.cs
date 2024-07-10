using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMW_Electrical.EquipNameUpdate
{
    public partial class EquipNameUpdateForm : System.Windows.Forms.Form
    {
        public EquipNameUpdateForm(List<Autodesk.Revit.DB.FamilyInstance> panels)
        {
            InitializeComponent();

            //set column information for ListView
            listViewPanels.View = System.Windows.Forms.View.Details;
            listViewPanels.OwnerDraw = true;
            listViewPanels.HeaderStyle = ColumnHeaderStyle.Nonclickable;

            //create listView columns
            listViewPanels.Columns.Add("Panel Name", -2, HorizontalAlignment.Left);
            listViewPanels.Columns.Add("Type", -2, HorizontalAlignment.Center);
            listViewPanels.Columns.Add("Part Type", -2, HorizontalAlignment.Center);
            listViewPanels.Columns.Add("Element Id", -2, HorizontalAlignment.Center);

            //add information to listView
            foreach (FamilyInstance panel in panels)
            {
                var colItem = new ListViewItem(new[]
                { panel.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString(),
                    panel.Symbol.Name,
                    panel.Symbol.Family.LookupParameter("Part Type").AsValueString(),
                    panel.Id.ToString() }); ;

                listViewPanels.Items.Add(colItem);
            }
        }

        public void Button_Click(object sender, EventArgs eventArgs)
        {
            this.Close();
        }

        private void listView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
            if ((e.ItemIndex % 2) == 1)
            {
                e.Item.BackColor = System.Drawing.Color.FromArgb(230, 230, 255);
                e.Item.UseItemStyleForSubItems = true;
            }
        }

        private void listView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            StringFormat align = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);

            System.Drawing.Rectangle rect = e.Bounds; rect.X += 2;
            e.Graphics.DrawString(e.Header.Text, e.Font, Brushes.Black, rect, align);
            //e.DrawDefault = true;
        }
    }
}
