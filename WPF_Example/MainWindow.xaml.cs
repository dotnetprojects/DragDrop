using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DragDropLibrary;
using SL_Drag_Drop;

namespace WPF_Example
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitialValues.ContainingLayoutPanel = this.LayoutRoot;
        }

        private void btnExample1_Click(object sender, RoutedEventArgs e)
        {
            grdWrapper.Children.Clear();
            grdWrapper.Children.Add(new sucDragDrop1());
        }

        private void btnExample2_Click(object sender, RoutedEventArgs e)
        {
            grdWrapper.Children.Clear();
            grdWrapper.Children.Add(new sucDragDrop2());
        }

        private void btnExample0_Click(object sender, RoutedEventArgs e)
        {
            grdWrapper.Children.Clear();
            grdWrapper.Children.Add(new sucDragDrop0());
        }

        private void btnExample3_Click(object sender, RoutedEventArgs e)
        {
            grdWrapper.Children.Clear();
            grdWrapper.Children.Add(new sucDragDrop3());
        }

        private void btnExample4_Click(object sender, RoutedEventArgs e)
        {
            grdWrapper.Children.Clear();
            grdWrapper.Children.Add(new sucDragDrop4());
        }

    }
}
