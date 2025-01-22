using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

namespace CMW_Electrical.ArrayCopy
{
    /// <summary>
    /// Interaction logic for ArrayCopy.xaml
    /// </summary>
    public partial class ArrayInfoWindow : Window
    {
        public ObservableCollection<string> _comboBoxCollection;
        public ArrayData ArrayInformation { get { return _arrayData; } }
        private ArrayData _arrayData;
        public ArrayInfoWindow(ArrayData arrayData)
        {
            InitializeComponent();

            _arrayData = arrayData;
            DataContext = _arrayData;

            _comboBoxCollection = new ObservableCollection<string>()
            {
                "Total Distance", "Spacing"
            };

            cbox_XDistList.ItemsSource = _comboBoxCollection;
            cbox_YDistList.ItemsSource = _comboBoxCollection;
        }

        private void TextBox_PreviewIntegerInput(object sender, TextCompositionEventArgs tb)
        {
            //check if the input is not an integer
            string tbText = tb.Text;
            if (!int.TryParse(tbText, out int result))
            {
                tb.Handled = true;
            }
        }

        private void TextBox_IntegerLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string tbText = tb.Text;

            if (string.IsNullOrEmpty(tbText))
            {
                MessageBox.Show("Field cannot be empty.");
                tb.Undo();
            }

            if (int.Parse(tbText) > 35)
            {
                MessageBox.Show("Value must be an integer between 0 and 35.");
                tb.Undo();
            }
        }

        private void TextBox_DoubleLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string tbText = tb.Text;

            if (string.IsNullOrEmpty(tbText))
            {
                MessageBox.Show("Field cannot be empty.");
                tb.Undo();
            }
            else
            {
                if (!double.TryParse(tbText, out double result))
                {
                    MessageBox.Show("Value must be a number.");
                    tb.Undo();
                }
            }
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }
}
