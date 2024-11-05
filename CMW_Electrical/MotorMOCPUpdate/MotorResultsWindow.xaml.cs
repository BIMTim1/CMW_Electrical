using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using MotorMOCPUpdate;

namespace CMW_Electrical.MotorMOCPUpdate
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MotorResultsWindow : Window
    {
        public ObservableCollection<MotorInfoData> _motorInfo;
        public MotorResultsWindow(List<MotorInfoData> motorInfoData)
        {
            InitializeComponent();

            _motorInfo = new ObservableCollection<MotorInfoData>(motorInfoData);

            dataGridMotors.ItemsSource = _motorInfo.OrderBy(x => x.GetMotorUID);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
