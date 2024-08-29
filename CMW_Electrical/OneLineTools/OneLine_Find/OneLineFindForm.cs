using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OneLineFind;

namespace CMW_Electrical.OneLineTools.OneLine_Find
{
    public partial class OneLineFindForm : Form
    {
        public Autodesk.Revit.DB.ElementId outputElementId;
        public OneLineFindForm(List<ElementData> elemDataList)
        {
            //start form
            InitializeComponent();

            //disable auto-generation of Columns from DataSource
            dataGridView1.AutoGenerateColumns = false;

            //create references for dataGridView1
            dataGridView1.DataSource = elemDataList;

            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;

            AddGridColumns();

            //subscribe to CellClick event
            dataGridView1.CellClick += dataGridView_CellClick;
        }

        private void AddGridColumns()
        {
            DataGridViewCellStyle centerAlignStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            DataGridViewColumn famTypeCol = new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "EFamilyType",
                Name = "Family and Type",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, //check visibility
                ReadOnly = true
            };

            DataGridViewColumn panNameCol = new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "EPanelName",
                Name = "Panel Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                ReadOnly = true,
                DefaultCellStyle = centerAlignStyle
            };

            DataGridViewColumn elemIdCol = new DataGridViewButtonColumn()
            {
                DataPropertyName = "EElementId",
                Name = "Element Id",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                ReadOnly = true,
                DefaultCellStyle = centerAlignStyle
            };

            //add columns to dataGridView1
            dataGridView1.Columns.Add(famTypeCol);
            dataGridView1.Columns.Add(panNameCol);
            dataGridView1.Columns.Add(elemIdCol);

            dataGridView1.EnableHeadersVisualStyles = false;

            //format column headers
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                col.HeaderCell.Style.BackColor = Color.LightBlue;
            }
        }

        private void Btn_Click(object sender, EventArgs eventArgs)
        {
            this.Close();
        }

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                DataGridViewButtonCell cell = 
                    (DataGridViewButtonCell)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];

                outputElementId = cell.Value as Autodesk.Revit.DB.ElementId;

                this.DialogResult = DialogResult.OK;

                this.Close();
            }
        }
    }
}
