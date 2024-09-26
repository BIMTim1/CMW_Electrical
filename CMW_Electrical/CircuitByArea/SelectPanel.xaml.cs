using Autodesk.Revit.DB;
using CircuitByArea;
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

namespace CMW_Electrical.CircuitByArea
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class SelectPanel : Window
    {
        private ObservableCollection<EquipmentSelectionData> formInput;
        public SelectPanel(List<EquipmentSelectionData> equipList)
        {
            InitializeComponent();

            formInput = new ObservableCollection<EquipmentSelectionData>(equipList);

            cboxPanels.ItemsSource = formInput.OrderBy(x => x.PanelInformation);
        }

        public void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        public void Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            btnOK.IsEnabled = true;
        }

        public void cbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            cboxPanels.IsDropDownOpen = true;
        }

        //public void cbox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(cboxPanels.Text))
        //    {
        //        cboxPanels.ItemsSource = formInput;
        //    }
        //    else
        //    {

        //        //var search = cboxPanels.Text.ToLower();
        //        //cboxPanels.ItemsSource = formInput.Where(x => x.ToLower().Contains(search));
        //    }
        //}
    }
}
