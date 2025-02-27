﻿using Autodesk.Revit.DB;
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
using CreatePanelSchedules;

namespace CMW_Electrical.CreatePanelSchedules
{
    public partial class PhaseSelectWindow : Window
    {
        public PhaseSelectWindow(List<Phase> phase_list)//List<PhaseInformation> phaseInformation)
        {
            InitializeComponent();

            cboxPhaseSelect.ItemsSource = phase_list.OrderBy(x => x.Name);
        }

        public void Button_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            DialogResult = true;
            this.Close();
        }

        public void Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            btnOK.IsEnabled = true;
        }
    }
}
