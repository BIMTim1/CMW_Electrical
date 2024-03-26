using CMW_Electrical.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CMW_Electrical.AddinInformation
{
    public partial class cmwElecAddinInfo : Form
    {
        public cmwElecAddinInfo()
        {
            InitializeComponent();

            this.lblVersionNumberDisplay.Text = CMW_Electrical_Ribbon.versionNumber;
            this.lblReleaseDateDisplay.Text = CMW_Electrical_Ribbon.releaseDate;

            //BitmapImage logoSource = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/CMTALogo.jpg"));
            
            //Bitmap cmtaLogo = new Bitmap(logoSource.StreamSource);
            //this.picBoxLogo.Image = cmtaLogo;
        }

        public void BtnClicked(object sender, EventArgs eventArgs)
        {
            Button btn = (Button)sender;

            if (btn.Name == btnReportBug.Name)
            {
                Process.Start("https://therma123.sharepoint.com/:u:/r/sites/ObernelBIM/SitePages/Support.aspx?csf=1&web=1&e=VNHdtj");
            }

            else if (btn.Name == btnBIMSite.Name)
            {
                Process.Start("https://therma123.sharepoint.com/sites/ObernelBIM");
            }
        }
    }
}
