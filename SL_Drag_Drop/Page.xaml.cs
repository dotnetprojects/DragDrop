/*
 * Kevin Dockx, 02/2009, update: 04/2009
 * 
 * Sample implementations of the drag drop manager classes.  All controls 
 * that are created in codebehind can just as easily be created in XAML if you wish.
 * 
 * Project@CodePlex: http://www.codeplex.com/silverlightdragdrop/
 * Author site: http://kevindockx.blogspot.com/

 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DragDropLibrary;


namespace SL_Drag_Drop
{

  
    public partial class Page : UserControl
    {
        public Page()
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

        private void btnExample5_Click(object sender, RoutedEventArgs e)
        {
            grdWrapper.Children.Clear();
            grdWrapper.Children.Add(new sucDragDrop5());
        }





    }
}
