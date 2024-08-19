using OneLine_FindDisconnected;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace CMW_Electrical.OneLineTools.OneLine_FindDisconnected
{
    public partial class FindDisconnectedElementForm : Form
    {
        public Autodesk.Revit.DB.ElementId _output;
        public FindDisconnectedElementForm(List<DisconnectedElement> discElemList)
        {
            InitializeComponent();


            dataGridView1.AutoGenerateColumns = false;

            dataGridView1.DataSource = discElemList;

            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;

            Add_Columns();

            //subscribe to CellClick event
            dataGridView1.CellClick += dataGridView_CellClicked;
        }

        private void Add_Columns()
        {
            DataGridViewCellStyle centerAlignStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            //create column information
            DataGridViewColumn infoColumn = new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "InstanceInfo",
                Name = "Element Info",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            };

            DataGridViewColumn nameColumn = new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Name",
                Name = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                ReadOnly = true,
                DefaultCellStyle = centerAlignStyle
            };

            DataGridViewColumn idColumn = new DataGridViewButtonColumn()
            {
                DataPropertyName = "Id",
                Name = "Id",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                ReadOnly = true,
                DefaultCellStyle = centerAlignStyle
            };

            DataGridViewColumn clearDupColumn = new DataGridViewCheckBoxColumn()
            {
                DataPropertyName = "ClearEqConId",
                Name = "Clear?",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                DefaultCellStyle = centerAlignStyle
            };

            //add columns to dataGridView1
            dataGridView1.Columns.Add(infoColumn);
            dataGridView1.Columns.Add(nameColumn);
            dataGridView1.Columns.Add(idColumn);
            dataGridView1.Columns.Add(clearDupColumn);

            dataGridView1.EnableHeadersVisualStyles = false;

            //format column headers
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                col.HeaderCell.Style.BackColor = Color.LightBlue;
            }
        }

        internal void Btn_Clicked(object sender, EventArgs eventArgs)
        {
            this.Close();
        }

        internal void CheckAll_Clicked(object sender, EventArgs eventArgs)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DataGridViewCheckBoxCell chk = (DataGridViewCheckBoxCell)row.Cells[3]; //fix selection issue
                Button clickedButton = (Button)sender;

                if (clickedButton.Name == btnCheckAll.Name)
                {
                    chk.Value = true;
                }
                else
                {
                    chk.Value = false;
                }
            }
        }

        private void dataGridView_CellClicked(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                DataGridViewButtonCell cell =
                    (DataGridViewButtonCell)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];

                _output = cell.Value as Autodesk.Revit.DB.ElementId;

                this.DialogResult = DialogResult.OK;

                this.Close();
            }
        }
    }
}
