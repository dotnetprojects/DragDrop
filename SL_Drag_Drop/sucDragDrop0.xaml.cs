﻿using System;
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
    public partial class sucDragDrop0 : UserControl
    {
        public sucDragDrop0()
        {
            InitializeComponent();

            InitControl();
        }


        private void InitControl()
        {
            // add droptargets to wrappanel

            DropTarget dropTarget1 = new DropTarget() { Ghost = new DropTargetGhost() };
            DropTarget dropTarget2 = new DropTarget() { Ghost = new DropTargetGhost() };
            DropTarget dropTarget3 = new DropTarget() { Ghost = new DropTargetGhost() };
            DropTarget dropTarget4 = new DropTarget() { Ghost = new DropTargetGhost(), Name = "a"};
            DropTarget dropTarget5 = new DropTarget() { Ghost = new DropTargetGhost() };
            DropTarget dropTarget6 = new DropTarget() { Ghost = new DropTargetGhost() };

            PanelDropTargets.Children.Add(dropTarget1);
            PanelDropTargets.Children.Add(dropTarget2);
            PanelDropTargets.Children.Add(dropTarget3);
            PanelDragSources.Children.Add(dropTarget4);
            PanelDragSources.Children.Add(dropTarget5);
            PanelDragSources.Children.Add(dropTarget6);

            // create list of droptargets to pass to the dragsources

            List<DropTarget> dropTargets = new List<DropTarget>() { dropTarget1, dropTarget2, dropTarget3,
            dropTarget4, dropTarget5, dropTarget6};

              // add dragsources to wrappanel

            DragSource dragSource1 = new DragSource()
            {
                Content = new DragSourceContent() { DataContext = new Dummy() { DummyText = "1" } },
                Ghost = new DragSourceGhost(),
                DropTargets = dropTargets
            };

            DragSource dragSource2 = new DragSource()
            {
                Content = new DragSourceContent() { DataContext = new Dummy() { DummyText = "2" } },
                Ghost = new DragSourceGhost(),
                DropTargets = dropTargets
            };

            DragSource dragSource3 = new DragSource()
            {
                Content = new DragSourceContent() { DataContext = new Dummy() { DummyText = "3" } },
                Ghost = new DragSourceGhost(),
                DropTargets = dropTargets
            };

            // add dragsources as content to droptargets
            dropTarget4.Content = dragSource1;
            dropTarget5.Content = dragSource2;
            dropTarget6.Content = dragSource3;

        }
    }
}
