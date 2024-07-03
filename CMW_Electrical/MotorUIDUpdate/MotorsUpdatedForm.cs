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

namespace CMW_Electrical.MotorUIDUpdate
{
    public partial class MotorsUpdatedForm : System.Windows.Forms.Form
    {
        public MotorsUpdatedForm(List<Autodesk.Revit.DB.FamilyInstance> motors)
        {
            InitializeComponent();

            //set column information for ListView
            listViewMotors.View = System.Windows.Forms.View.Details;
            listViewMotors.OwnerDraw = true;
            listViewMotors.HeaderStyle = ColumnHeaderStyle.Nonclickable;

            //ColumnHeader columnHeaderID = new ColumnHeader()
            //{
            //    Text = "Element Id",
            //    TextAlign = HorizontalAlignment.Left,
            //    Width = -2
            //};
            listViewMotors.Columns.Add("Element Id", -2, HorizontalAlignment.Left);
            listViewMotors.Columns.Add("UID", -2, HorizontalAlignment.Left);
            listViewMotors.Columns.Add("Type", -2, HorizontalAlignment.Center);
            listViewMotors.Columns.Add("Level", -2, HorizontalAlignment.Center);

            //from motors list, collect information for form
            foreach (FamilyInstance motor in motors)
            {
                var colItem = new ListViewItem(new[]
                { motor.Id.ToString(), 
                    motor.LookupParameter("UID").AsString(),
                    motor.Name,
                    motor.LookupParameter("Level").AsValueString() }); ;

                listViewMotors.Items.Add(colItem);
            }
        }

        private void Btn_Click(object sender, EventArgs eventArgs)
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
            StringFormat align = new StringFormat();
            align.Alignment = StringAlignment.Center;
            align.LineAlignment = StringAlignment.Center;

            e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);

            System.Drawing.Rectangle rect = e.Bounds; rect.X += 2;
            e.Graphics.DrawString(e.Header.Text, e.Font, Brushes.Black, rect, align);
            //e.DrawDefault = true;
        }
    }
}
