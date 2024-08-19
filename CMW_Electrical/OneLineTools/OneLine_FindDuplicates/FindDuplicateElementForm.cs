using OneLine_FindDuplicates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMW_Electrical.OneLineTools.OneLine_FindDuplicates
{
    public partial class FindDuplicateElementForm : Form
    {
        public string _output;
        public FindDuplicateElementForm(List<DuplicateElementData> duplicateElements)
        {
            InitializeComponent();

            //disable auto-generation of Columns from DataSource
            dataGridView1.AutoGenerateColumns = false;

            dataGridView1.DataSource = duplicateElements;

            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;

            Add_Columns();

            //subscribe to CellClick event
            dataGridView1.CellClick += dataGridView_CellClicked;
        }

        public void Button_Clicked(object sender, EventArgs eventArgs)
        {
            this.Close();
        }

        private void Add_Columns()
        {
            DataGridViewCellStyle centerAlignStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            DataGridViewColumn dupNameColumn = new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "GetDuplicateInfo",
                Name = "Is Duplicate",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            };

            DataGridViewColumn EqConIdColumn = new DataGridViewButtonColumn()
            {
                DataPropertyName = "GetEqConId",
                Name = "EqConId",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                ReadOnly = true,
                DefaultCellStyle = centerAlignStyle
            };

            //add columns to dataGridView1
            dataGridView1.Columns.Add(dupNameColumn);
            dataGridView1.Columns.Add(EqConIdColumn);

            dataGridView1.EnableHeadersVisualStyles = false;

            //format column headers
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                col.HeaderCell.Style.BackColor = Color.LightBlue;
            }
        }

        private void dataGridView_CellClicked(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                DataGridViewButtonCell cell = 
                    (DataGridViewButtonCell)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];

                _output = cell.Value.ToString();

                this.DialogResult = DialogResult.OK;

                this.Close();
            }
        }
    }
}
