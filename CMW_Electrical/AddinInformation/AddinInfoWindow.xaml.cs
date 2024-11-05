using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CMW_Electrical.AddinInformation
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class AddinInfoWindow : Window
    {
        private readonly string _reportBugURL;
        private readonly string _sharePointURL;
        public AddinInfoWindow()
        {
            InitializeComponent();

            _reportBugURL = "https://wearelegence.sharepoint.com/sites/CMTAMidwestBIM/SitePages/Support.aspx";

            _sharePointURL = "https://wearelegence.sharepoint.com/sites/CMTAMidwestBIM";

            tbVerNumVal.Text = CMW_Electrical_Ribbon.versionNumber;

            tbReleaseDateVal.Text = CMW_Electrical_Ribbon.releaseDate;
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            if (btn.Name == btnReport.Name)
            {
                Process.Start(_reportBugURL);
            }
            else if (btn.Name == btnSharePoint.Name)
            {
                Process.Start(_sharePointURL);
            }
        }
    }
}
