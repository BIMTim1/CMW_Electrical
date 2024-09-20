using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMW_Electrical.ChangePanelToSinglePhase
{
    public partial class ReconnectForm : Form
    {
        public ReconnectForm(string formName, string userPrompt)
        {
            InitializeComponent();

            this.Text = formName;

            lblReconnect.Text = userPrompt;
        }

        public void Button_Click(object sender, EventArgs eventArgs)
        {
            this.Close();
        }
    }
}
