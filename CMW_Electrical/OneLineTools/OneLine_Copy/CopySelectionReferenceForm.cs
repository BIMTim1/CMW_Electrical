using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMW_Electrical.OneLineTools.OneLine_Copy
{
    public partial class CopySelectionReferenceForm : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
            (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
            );
        public CopySelectionReferenceForm(List<string> equipNames)
        {
            InitializeComponent();

            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            cBoxEquipSelect.Items.AddRange(equipNames.ToArray());
        }

        public void BtnClick(object sender, EventArgs eventArgs)
        {
            this.Close();
        }

        public void SelectionChanged(object sender, EventArgs eventArgs)
        {
            btnOK.Enabled = true;
        }
    }
}
