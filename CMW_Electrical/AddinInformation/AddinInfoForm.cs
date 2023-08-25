using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
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
            //BitmapImage cmtaLogo = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/Info32x32.png"));
            Bitmap cmtaLogo = new Bitmap("pack://application:,,,/CMW_Electrical;component/Resources/Info32x32.png");
            this.picBoxLogo.Image = cmtaLogo;
        }

        public void BtnBugClicked(object sender, EventArgs e)
        {
            Process.Start("https://therma123.sharepoint.com/:u:/r/sites/ObernelBIM/SitePages/Support.aspx?csf=1&web=1&e=VNHdtj");
        }

        public void BtnBIMSiteClicked(object sender, EventArgs e)
        {
            Process.Start("https://therma123.sharepoint.com/sites/ObernelBIM");
        }
    }
}
