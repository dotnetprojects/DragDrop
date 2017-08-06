﻿/* Kevin Dockx
 * 
 * Base class for dragsources.  Implements the dragging behaviour.
 * Any instance of this control will be draggable.  
 * 
 * You have to include the corresponding template WITH CORRESPONDING NAMES
 * in the resource dictionary, and you have to have a content presenter to make
 * sure you can put something in this control (eg, a usercontrol you want to 
 * make draggable) when you create it :-)
 * 
 * For this example, we use a gripbar for dragging our item around.  If that's not
 * necessary, you might as well leave it out of the template alltogether and add 
 * the mouse handlers to the control itself instead of to the gripbar.  This way,
 * you will be able to drag the control around no matter where you click.
 * 
 * Project@CodePlex: http://www.codeplex.com/silverlightdragdrop/
 * Author site: http://kevindockx.blogspot.com/
 * 
 */


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DragDropLibrary
{
    /// <summary>
    /// Class defining a drag source
    /// </summary>
    public class DragSource : ContentControl, IDisposable
    {

        /// <summary>
        /// Enumeration with the draghandlemodes
        /// </summary>
        public enum DragHandleModeType
        {
            /// <summary>
            /// DragSource has a draghandle
            /// </summary>
            Handle,
            /// <summary>
            /// Full drag source is draggable
            /// </summary>
            FullDragSource
        }

        /// <summary>
        /// Enumeration with the dropmodes
        /// </summary>
        public enum DropModeType
        {
            /// <summary>
            /// Dragsource will be dropped/moved if possible when dropped
            /// </summary>
            DropDragSource,
            /// <summary>
            /// Dragsource will return when dropped
            /// </summary>
            ReturnDragSource,
            /// <summary>
            /// Dragsource will return when dropped
            /// </summary>
            ReturnDragSourceWithoutAnimation,
        }

        /// <summary>
        /// Is dragging flag.
        /// </summary>
        private bool dragging = false;

        /// <summary>
        /// Am I in a droptarget?
        /// </summary>
        private bool isInDropTarget = false;

        private ScaleTransform combinedScaleTransform;


        /// <summary>
        /// Stores the last drag position.
        /// </summary>
        private Point lastDragPosition;

        /// <summary>
        /// Saves the current canvas.left and canvas.top-properties
        /// </summary>
        private Point currentCanvasPosition;


        #region DraggingEnabled (DependencyProperty)

        /// <summary>
        /// Determines wether or not to dragging is enabled (default: true)
        /// </summary>
        public bool DraggingEnabled
        {
            get { return (bool)GetValue(DraggingEnabledProperty); }
            set { SetValue(DraggingEnabledProperty, value); }
        }
        /// <summary>
        /// Determines wether or not to dragging is enabled (default: true)
        /// </summary>
        public static readonly DependencyProperty DraggingEnabledProperty =
            DependencyProperty.Register("DraggingEnabled", typeof(bool), typeof(DragSource),
              new PropertyMetadata(true));

        #endregion



        #region Ghost (DependencyProperty)

        /// <summary>
        /// Contains the ghost control, shown on original position when dragging
        /// </summary>
        public FrameworkElement Ghost
        {
            get { return (FrameworkElement)GetValue(GhostProperty); }
            set { SetValue(GhostProperty, value); }
        }
        /// <summary>
        /// Contains the ghost control, shown on original position when dragging
        /// </summary>
        public static readonly DependencyProperty GhostProperty =
            DependencyProperty.Register("Ghost", typeof(FrameworkElement), typeof(DragSource),
              new PropertyMetadata(null));

        #endregion


        #region GhostVisibility (DependencyProperty)

        /// <summary>
        /// Determines wether or not to show the ghost (default: Visible)
        /// </summary>
        public Visibility GhostVisibility
        {
            get { return (Visibility)GetValue(GhostVisibilityProperty); }
            set { SetValue(GhostVisibilityProperty, value); }
        }
        /// <summary>
        /// Determines wether or not to show the ghost (default: Visible)
        /// </summary>
        public static readonly DependencyProperty GhostVisibilityProperty =
            DependencyProperty.Register("GhostVisibility", typeof(Visibility), typeof(DragSource),
              new PropertyMetadata(Visibility.Visible));

        #endregion


        #region ShowReturnToOriginalPositionAnimation (DependencyProperty)

        /// <summary>
        /// Determines wether or not to show an animation when returning to the original position (default: true)
        /// </summary>
        public bool ShowReturnToOriginalPositionAnimation
        {
            get { return (bool)GetValue(ShowReturnToOriginalPositionAnimationProperty); }
            set { SetValue(ShowReturnToOriginalPositionAnimationProperty, value); }
        }
        /// <summary>
        /// Determines wether or not to show an animation when returning to the original position (default: true)
        /// </summary>
        public static readonly DependencyProperty ShowReturnToOriginalPositionAnimationProperty =
            DependencyProperty.Register("ShowReturnToOriginalPositionAnimation", typeof(bool), typeof(DragSource),
              new PropertyMetadata(true));

        #endregion


        #region ShowSwitchReplaceAnimation (DependencyProperty)

        /// <summary>
        ///  Determines wether or not to show an animation when switching/replacing 2 dragsources (default: true)
        /// </summary>
        public bool ShowSwitchReplaceAnimation
        {
            get { return (bool)GetValue(ShowSwitchReplaceAnimationProperty); }
            set { SetValue(ShowSwitchReplaceAnimationProperty, value); }
        }
        /// <summary>
        ///  Determines wether or not to show an animation when switching/replacing 2 dragsources (default: true)
        /// </summary>
        public static readonly DependencyProperty ShowSwitchReplaceAnimationProperty =
            DependencyProperty.Register("ShowSwitchReplaceAnimation", typeof(bool), typeof(DragSource),
              new PropertyMetadata(true));

        #endregion


        #region AutoFitGhost (DependencyProperty)

        /// <summary>
        /// Determines wether or not to auto-fit the ghost control.  It this is true, the ghost (if available)
        /// will be resized to match the content (default: false)
        /// </summary>
        public bool AutoFitGhost
        {
            get { return (bool)GetValue(AutoFitGhostProperty); }
            set { SetValue(AutoFitGhostProperty, value); }
        }
        /// <summary>
        /// Determines wether or not to auto-fit the ghost control.  It this is true, the ghost (if available)
        /// will be resized to match the content (default: false)
        /// </summary>
        public static readonly DependencyProperty AutoFitGhostProperty =
            DependencyProperty.Register("AutoFitGhost", typeof(bool), typeof(DragSource),
              new PropertyMetadata(false));

        #endregion


        #region DropTargets (DependencyProperty)

        /// <summary>
        /// A list of valid droptargets for this dragsource
        /// </summary>
        public List<DropTarget> DropTargets
        {
            get { return (List<DropTarget>)GetValue(DropTargetsProperty); }
            set { SetValue(DropTargetsProperty, value); }
        }
        /// <summary>
        /// A list of valid droptargets for this dragsource
        /// </summary>
        public static readonly DependencyProperty DropTargetsProperty =
            DependencyProperty.Register("DropTargets", typeof(List<DropTarget>), typeof(DragSource),
              new PropertyMetadata(null));

        #endregion


        #region InternalDropTargets (DependencyProperty)

        /// <summary>
        /// A list of ALL droptargets available, used with AllDropTargetsValid==true
        /// </summary>
        private List<DropTarget> InternalDropTargets
        {
            get { return (List<DropTarget>)GetValue(InternalDropTargetsProperty); }
            set { SetValue(InternalDropTargetsProperty, value); }
        }
        /// <summary>
        /// A list of ALL droptargets available, used with AllDropTargetsValid==true
        /// </summary>
        private static readonly DependencyProperty InternalDropTargetsProperty =
            DependencyProperty.Register("InternalDropTargets", typeof(List<DropTarget>), typeof(DragSource),
              new PropertyMetadata(null));

        #endregion


        #region AllDropTargetsValid (DependencyProperty)

        /// <summary>
        /// When this is set to true, all droptargets on the page will be valid for this dragsource.  
        /// The droptarget-list-property will effectively be ignored when this is set to true (default: false)
        /// </summary>
        public bool AllDropTargetsValid
        {
            get { return (bool)GetValue(AllDropTargetsValidProperty); }
            set { SetValue(AllDropTargetsValidProperty, value); }
        }
        /// <summary>
        /// When this is set to true, all droptargets on the page will be valid for this dragsource.  
        /// The droptarget-list-property will effectively be ignored when this is set to true (default: false)
        /// </summary>
        public static readonly DependencyProperty AllDropTargetsValidProperty =
            DependencyProperty.Register("AllDropTargetsValid", typeof(bool), typeof(DragSource),
              new PropertyMetadata(false));

        #endregion


        #region DragHandleMode (DependencyProperty)

        /// <summary>
        /// This property determines how you can drag your dragsource around: by using a draghandle (a bar
        /// on top of your dragsource), or without one, meaning you can drag by clicking anywhere in the dragsource
        /// </summary>
        public DragHandleModeType DragHandleMode
        {
            get { return (DragHandleModeType)GetValue(DragHandleModeProperty); }
            set { SetValue(DragHandleModeProperty, value); }
        }
        /// <summary>
        /// This property determines how you can drag your dragsource around: by using a draghandle (a bar
        /// on top of your dragsource), or without one, meaning you can drag by clicking anywhere in the dragsource
        /// </summary>
        public static readonly DependencyProperty DragHandleModeProperty =
            DependencyProperty.Register("DragHandleMode", typeof(DragHandleModeType), typeof(DragSource),
              new PropertyMetadata(DragHandleModeType.Handle));

        #endregion


        #region DropMode (DependencyProperty)

        /// <summary>
        /// Determines the behaviour when a dragsource is dropped, and the drop is valid: by default, the dragsource is 
        /// dropped on the droptarget.  If you set DropMode to "ReturnDragSource", the dragsource is returned to where it
        /// originated from.  This mode can be useful if you just want to execute some code on dropping (dragsourcedropped event
        /// on droptarget is triggered and can be handled if you add a handler to it), but you do not want
        /// the dragsource to be removed from the originating collection.
        /// </summary>
        public DropModeType DropMode
        {
            get { return (DropModeType)GetValue(DropModeProperty); }
            set { SetValue(DropModeProperty, value); }
        }

        /// <summary>
        /// Determines the behaviour when a dragsource is dropped, and the drop is valid: by default, the dragsource is 
        /// dropped on the droptarget.  If you set DropMode to "ReturnDragSource", the dragsource is returned to where it
        /// originated from.  This mode can be useful if you just want to execute some code on dropping (dragsourcedropped event
        /// on droptarget is triggered and can be handled if you add a handler to it), but you do not want
        /// the dragsource to be removed from the originating collection.
        /// </summary>
        public static readonly DependencyProperty DropModeProperty =
            DependencyProperty.Register("DropMode", typeof(DropModeType), typeof(DragSource),
              new PropertyMetadata(DropModeType.DropDragSource));

        #endregion

        #region ReturnAnimationDuration (DependencyProperty)

        /// <summary>
        /// Determines the length of the animation shown when returning a DragSource to its original position (default: 0.2)
        /// </summary>
        public double ReturnAnimationDuration
        {
            get { return (double)GetValue(ReturnAnimationDurationProperty); }
            set { SetValue(ReturnAnimationDurationProperty, value); }
        }
        /// <summary>
        /// Determines the length of the animation shown when returning a DragSource to its original position (default: 0.2)
        /// </summary>
        public static readonly DependencyProperty ReturnAnimationDurationProperty =
            DependencyProperty.Register("ReturnAnimationDuration", typeof(double), typeof(DragSource),
              new PropertyMetadata(0.2));

        #endregion

        #region SwitchAnimationDuration (DependencyProperty)

        /// <summary>
        /// Determines the length of the animation shown when switching 2 DragSources (default: 0.2)
        /// </summary>
        public double SwitchAnimationDuration
        {
            get { return (double)GetValue(SwitchAnimationDurationProperty); }
            set { SetValue(SwitchAnimationDurationProperty, value); }
        }
        /// <summary>
        /// Determines the length of the animation shown when switching 2 DragSources (default: 0.2)
        /// </summary>
        public static readonly DependencyProperty SwitchAnimationDurationProperty =
            DependencyProperty.Register("SwitchAnimationDuration", typeof(double), typeof(DragSource),
              new PropertyMetadata(0.2));

        #endregion

        internal Canvas MainControlHost;
        internal Grid MainDraggableControl;
        private ContentPresenter GhostContentControl;
        internal Point OriginalOffset = new Point();

        /// <summary>
        /// The before drag started event
        /// </summary>
        public event DragEventHandler BeforeDragStarted;

        /// <summary>
        /// The drag started event
        /// </summary>
        public event DragEventHandler DragStarted;

        /// <summary>
        /// The drag moved event
        /// </summary>
        public event DragEventHandler DragMoved;

        /// <summary>
        /// The drag finished event
        /// </summary>
        public event DragEventHandler DragFinished;


        private Canvas DummyOverlay;
        private Panel OriginalParent;
        private FrameworkElement DragBar;
        private FrameworkElement MainContentPresenter;

#if !SILVERLIGHT
        static DragSource()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragSource), new FrameworkPropertyMetadata(typeof(DragSource)));
        }
#endif

        public DragSource()
        {
#if SILVERLIGHT
            this.DefaultStyleKey = typeof(DragSource);
#endif
        }


        /// <summary>
        /// Method overrides OnApplyTemplate to add handlers / get references to control in the template
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


            // for dragging, handlers must be added to the mouse-events.  We 
            // add the handlers to the top bar from the template.  To do this,
            // make sure the correct name is available in the template of this control!
            DragBar = (FrameworkElement)this.GetTemplateChild("DragBar");

            if (DragHandleMode == DragHandleModeType.Handle)
            {

                if (DragBar != null)
                {
                    DragBar.MouseLeftButtonDown += new MouseButtonEventHandler(DragBar_MouseLeftButtonDown);
                    DragBar.MouseMove += new MouseEventHandler(DragBar_MouseMove);
                    DragBar.MouseLeftButtonUp += new MouseButtonEventHandler(DragBar_MouseLeftButtonUp);
                }
            }
            else
            {
                if (DragBar != null)
                {
                    DragBar.Visibility = Visibility.Collapsed;
                }
                this.Cursor = Cursors.Hand;
                this.MouseLeftButtonDown += new MouseButtonEventHandler(DragBar_MouseLeftButtonDown);
                this.MouseMove += new MouseEventHandler(DragBar_MouseMove);
                this.MouseLeftButtonUp += new MouseButtonEventHandler(DragBar_MouseLeftButtonUp);
            }

            if (DraggingEnabled == false)
            {
                if (DragBar != null)
                {
                    DragBar.Visibility = Visibility.Collapsed;
                }
                this.Cursor = Cursors.Arrow;
            }

            // all our controls are inside of a canvas control.  Because of this, it doesn't 
            // automatically resize.  We need to make sure the parent control is resized properly

            // get the main control host.  We need this to set the width off.
            MainControlHost = (Canvas)this.GetTemplateChild("MainControlHost");

            // add a listener to the sizechanged event
            MainContentPresenter = (FrameworkElement)this.GetTemplateChild("MainContentPresenter");
            if (MainContentPresenter != null)
                MainContentPresenter.SizeChanged += new SizeChangedEventHandler(MainContentPresenter_SizeChanged);

            // get the main draggable control (grid containing content & dragbar)
            MainDraggableControl = (Grid)this.GetTemplateChild("MainDraggableControl");

            // get the ghost content control
            GhostContentControl = (ContentPresenter)this.GetTemplateChild("GhostContentControl");



        }


        void MainContentPresenter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainControlHost.Width = e.NewSize.Width + BorderThickness.Left + BorderThickness.Right;
            MainControlHost.Height = e.NewSize.Height + BorderThickness.Top + BorderThickness.Bottom;

            // resize the ghostcontrol?

            if (AutoFitGhost)
            {
                if (MainDraggableControl != null && GhostContentControl != null)
                {
                    GhostContentControl.Width = e.NewSize.Width + BorderThickness.Left + BorderThickness.Right;
                    GhostContentControl.Height = e.NewSize.Height + BorderThickness.Top + BorderThickness.Bottom;
                }
            }
        }

        void DragBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.DraggingEnabled && this.dragging)
            {
                e.Handled = true;

                // Capture the mouse
                ((FrameworkElement)sender).ReleaseMouseCapture();

                Point position = e.GetPosition(sender as UIElement);
                //CheckIfIAmInDropTarget();

                // if I am in a droptarget, fire correct event, if not, return to original position

                if (isInDropTarget)
                {
                    DropTarget dropTarget = GetCorrectDropTarget(e.GetPosition(this.MainDraggableControl));
                    //DropTarget dropTarget = GetCorrectDropTarget();

                    if (dropTarget != null)
                    {
                        //// put back on the correct parent
                        DummyOverlay.Children.Remove(this);
                        OriginalParent.Children.Add(this);

                        if (DropMode == DropModeType.DropDragSource)
                        {
                            // trigger internal dragsourcedropped event to drop the dragsource
                            // (after the animations etc, so after the drop has been executed, 
                            // the external DragSourceDropped event will be triggered)
                            dropTarget.TriggerInternalDragSourceDropped(this);
                        }
                        else if (DropMode == DropModeType.ReturnDragSource || DropMode == DropModeType.ReturnDragSourceWithoutAnimation)
                        {
                            // return dragsource to original parent
                            ReturnToOriginalPosition(DropMode == DropModeType.ReturnDragSourceWithoutAnimation);

                            // trigger public event in droptarget (for external use)
                            dropTarget.TriggerDragSourceDropped(this);
                        }

                        //// remove canvas
                        InitialValues.ContainingLayoutPanel.Children.Remove(DummyOverlay);
                    }
                    else
                    {
                        ReturnToOriginalPosition();
                    }
                }
                else
                {
                    ReturnToOriginalPosition();
                }



                // Set dragging to false
                this.dragging = false;

                // Fire the drag finished event
                if (this.DragFinished != null)
                {
                    this.DragFinished(this, new DragEventArgs(position.X - this.lastDragPosition.X, position.Y - this.lastDragPosition.Y, e));
                }
            }
        }

        ///// <summary>
        ///// Starts the animation when switching 2 dragsources
        ///// </summary>
        ///// <param name="from"></param>
        //internal void AnimateOnSwitch(Point from)
        //{
        //    // start animation
        //    Storyboard sb = Animation.ReturnDragToOriginalPosition(MainDraggableControl, from, new Point(0, 0));
        //    sb.Begin();
        //}


        /// <summary>
        /// Returns the animation used when switching 2 dragsources
        /// </summary>
        /// <param name="offsetCurrentChild"></param>
        /// <param name="originalOffset"></param>
        /// <returns></returns>
        internal Storyboard ReturnAnimateOnSwitch(Point offsetCurrentChild, Point originalOffset)
        {

            // Move object to overlay for correct on-top positioning
            OriginalParent = ((Panel)VisualTreeHelper.GetParent(this));

            DummyOverlay = new Canvas();
            DummyOverlay.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            DummyOverlay.IsHitTestVisible = false;
            InitialValues.ContainingLayoutPanel.Children.Add(DummyOverlay);

            // save datacontext!
            object currentDC = this.DataContext;

            OriginalParent.Children.Remove(this);

            this.DataContext = currentDC;

            // set correct position
            Canvas.SetLeft(this, offsetCurrentChild.X);
            Canvas.SetTop(this, offsetCurrentChild.Y);

            Point p = new Point(-(offsetCurrentChild.X - originalOffset.X), -(offsetCurrentChild.Y - originalOffset.Y));

            DummyOverlay.Children.Add(this);

            // start animation
            Storyboard sb = Animation.ReturnDragToOriginalPosition(MainDraggableControl, new Point(0, 0), p, SwitchAnimationDuration);
            return sb;
        }

        /// <summary>
        /// Return the dragsource to its original position (on top of the ghost)
        /// </summary>
        internal void ResetMyPosition()
        {
            if (DummyOverlay.Children.Contains(this))
            {
                DummyOverlay.Children.Remove(this);
                OriginalParent.Children.Add(this);
            }

            // return to original position (so the real control is on top of the ghost)
            currentCanvasPosition.X = 0;
            currentCanvasPosition.Y = 0;

            Canvas.SetLeft(this.MainDraggableControl, currentCanvasPosition.X);
            Canvas.SetTop(this.MainDraggableControl, currentCanvasPosition.Y);

            // remove canvas
            InitialValues.ContainingLayoutPanel.Children.Remove(DummyOverlay);
        }


        /// <summary>
        /// Gets the currently hovered droptarget
        /// </summary>
        private DropTarget GetCorrectDropTarget(Point pt)
        {
            // get my absolute position
            Point offsetMine = this.MainDraggableControl.TransformToVisual(UIHelpers.RootUI).Transform(pt);

            if (AllDropTargetsValid)
            {
                foreach (var item in InternalDropTargets)
                {
                    // get the absolute position of this droptarget 
                    Point offsetDrop = item.TransformToVisual(UIHelpers.RootUI).Transform(new Point(0, 0));

                    // check its bounds against my absolute position
                    if (offsetMine.X > offsetDrop.X && offsetMine.X < offsetDrop.X + item.ActualWidth)
                    {
                        // X-coordinates are ok
                        if (offsetMine.Y > offsetDrop.Y && offsetMine.Y < offsetDrop.Y + item.ActualHeight)
                        {
                            return item;
                        }
                    }
                }
            }
            else
            {

                foreach (var item in DropTargets)
                {
                    // get the absolute position of this droptarget 
                    Point offsetDrop = item.TransformToVisual(UIHelpers.RootUI).Transform(new Point(0, 0));

                    // check its bounds against my absolute position
                    if (offsetMine.X > offsetDrop.X && offsetMine.X < offsetDrop.X + item.ActualWidth)
                    {
                        // X-coordinates are ok
                        if (offsetMine.Y > offsetDrop.Y && offsetMine.Y < offsetDrop.Y + item.ActualHeight)
                        {
                            return item;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the dragged element to its original position
        /// </summary>
        internal void ReturnToOriginalPosition(bool withoutAnimation = false)
        {
            if (!withoutAnimation && ShowReturnToOriginalPositionAnimation)
            {
                Storyboard sb = Animation.ReturnDragToOriginalPosition(MainDraggableControl, currentCanvasPosition, new Point(0, 0), ReturnAnimationDuration);



                EventHandler handler = null;
                handler = (send, args)
                    =>
                    {
                        sb.Completed -= handler;
                        sb.Stop();
                        ResetMyPosition();
                    };

                sb.Completed += handler;
                sb.Begin();
            }
            else
            {
                ResetMyPosition();
            }
        }


        void DragBar_MouseMove(object sender, MouseEventArgs e)
        {
            // only when we are dragging...
            if (this.dragging)
            {
                Point position = e.GetPosition(sender as UIElement);

                currentCanvasPosition.X = Canvas.GetLeft(this.MainDraggableControl) + position.X - this.lastDragPosition.X;
                currentCanvasPosition.Y = Canvas.GetTop(this.MainDraggableControl) + position.Y - this.lastDragPosition.Y;

                Debug.WriteLine(MainDraggableControl);
                Debug.WriteLine(currentCanvasPosition.Y);
                // Move the panel
                Canvas.SetLeft(this.MainDraggableControl, currentCanvasPosition.X);
                Canvas.SetTop(this.MainDraggableControl, currentCanvasPosition.Y);

                // if the absolute position of the draggable element is inside of one of 
                // the droptargets applying, fire the correct events

                CheckIfIAmInDropTarget(e.GetPosition(this.MainDraggableControl));

                // Fire the drag moved event
                if (this.DragMoved != null)
                {
                    this.DragMoved(this, new DragEventArgs(position.X - this.lastDragPosition.X, position.Y - this.lastDragPosition.Y, e));
                }

                // Update the last mouse position
                //this.lastDragPosition = e.GetPosition(sender as UIElement);
            }
        }


        /// <summary>
        /// Method to check if "I" am in one of my droptargets
        /// </summary>
        /// <returns></returns>
        private void CheckIfIAmInDropTarget(Point position)
        {
            // get my absolute position
            Point offsetMine =
                  this.MainDraggableControl
                  .TransformToVisual(UIHelpers.RootUI)
                  .Transform(position);

            isInDropTarget = false;

            if (AllDropTargetsValid)
            {

                foreach (var item in InternalDropTargets)
                {
                    // get the absolute position of this droptarget 
                    //Point offsetDrop;

                    if (item.AllowPositionSave)
                    {
                        if (item.PositionCalculated == false)
                        {
                            item.RecalculatePosition();
                            item.PositionCalculated = true;
                        }
                    }
                    else
                    {
                        item.RecalculatePosition();
                    }

                    // check its bounds against my absolute position

                    if (offsetMine.X > item.internalOffset.X && offsetMine.X < item.internalOffset.X + (item.ActualWidth * combinedScaleTransform.ScaleX))
                    {
                        // X-coordinates are ok
                        if (offsetMine.Y > item.internalOffset.Y && offsetMine.Y < item.internalOffset.Y + (item.ActualHeight * combinedScaleTransform.ScaleY))
                        {
                            // Y-coordinates are ok
                            // fire event on droptarget
                            item.TriggerDropTargetEntered(this);
                            isInDropTarget = true;

                            // Un-highight all other drop targets.
                            foreach (var dropTarget in InternalDropTargets)
                            {
                                if (item != dropTarget)
                                {
                                    dropTarget.TriggerDropTargetLeft(this);
                                }
                            }

                            break;
                        }
                        else
                        {
                            // X ok, Y not: not hovering anymore
                            item.TriggerDropTargetLeft(this);
                        }
                    }
                    else
                    {
                        // X not ok
                        item.TriggerDropTargetLeft(this);
                    }
                }

            }
            else
            {

                if (DropTargets != null)
                {
                    foreach (var item in DropTargets)
                    {
                        var itemCombinedScaleTransform = item.GetCombinedScaleTransform();

                        // get the absolute position of this droptarget 
                        Point offsetDrop = item.TransformToVisual(UIHelpers.RootUI).Transform(new Point(0, 0));

                        // check its bounds against my absolute position

                        if (offsetMine.X > offsetDrop.X && offsetMine.X < offsetDrop.X + (item.ActualWidth * itemCombinedScaleTransform.ScaleX))
                        {
                            // X-coordinates are ok
                            if (offsetMine.Y > offsetDrop.Y && offsetMine.Y < offsetDrop.Y + (item.ActualHeight * itemCombinedScaleTransform.ScaleY))
                            {
                                // Y-coordinates are ok
                                // fire event on droptarget
                                item.TriggerDropTargetEntered(this);
                                isInDropTarget = true;

                                // Un-highight all other drop targets.
                                foreach (var dropTarget in DropTargets)
                                {
                                    if (item != dropTarget)
                                    {
                                        dropTarget.TriggerDropTargetLeft(this);
                                    }
                                }

                                break;
                            }
                            else
                            {
                                // X ok, Y not: not hovering anymore
                                item.TriggerDropTargetLeft(this);
                            }
                        }
                        else
                        {
                            // X not ok
                            item.TriggerDropTargetLeft(this);
                        }
                    }
                }
            }
        }

        void DragBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.DraggingEnabled && !this.dragging)
            {
                e.Handled = true;

                combinedScaleTransform = this.GetCombinedScaleTransform();

                this.lastDragPosition = e.GetPosition(sender as UIElement);

                // Fire the drag started event
                if (this.BeforeDragStarted != null)
                {
                    this.BeforeDragStarted(this, new DragEventArgs(0, 0, e));
                }

                // Bring the control to the front

                if (InitialValues.ContainingLayoutPanel == null)
                {
                    throw new NullReferenceException("ContainingLayoutPanel is null. Please make sure that it is set.");
                }

                // fill droptarget-list with all droptargets on page?

                if (AllDropTargetsValid)
                {
                    var a = GetChildsOrderedRecursive(UIHelpers.RootUI);
                    var dropTargets = GetChildsOrderedRecursive(UIHelpers.RootUI).OfType<DropTarget>().ToList();
                    this.InternalDropTargets = dropTargets;
                }


                // create a dummy overlay (to make sure the dragsource we drag is always on top)
                // and add the dragsource to this overlayed canvas at the correct position.  To do 
                // this, the dragsource must also be removed from its original parent.

                OriginalParent = ((Panel)VisualTreeHelper.GetParent(this));

                DummyOverlay = new Canvas();
                DummyOverlay.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                DummyOverlay.IsHitTestVisible = false;
                InitialValues.ContainingLayoutPanel.Children.Add(DummyOverlay);

                Point offsetMine = this.MainDraggableControl.TransformToVisual(InitialValues.ContainingLayoutPanel).Transform(new Point(0, 0));

                OriginalOffset = offsetMine;

                // save datacontext!
                object currentDC = this.DataContext;

                OriginalParent.Children.Remove(this);

                this.DataContext = currentDC;

                // set correct position
                Canvas.SetLeft(this, offsetMine.X);
                Canvas.SetTop(this, offsetMine.Y);

                DummyOverlay.Children.Add(this);

                // Capture mouse & store position
                ((FrameworkElement)sender).CaptureMouse();
                //this.lastDragPosition = e.GetPosition(sender as UIElement);

                // Set dragging to true
                this.dragging = true;

                this.isInDropTarget = false;

                // Fire the drag started event
                if (this.DragStarted != null)
                {
                    this.DragStarted(this, new DragEventArgs(0, 0, e));
                }
            }
        }

        // Find all UIElements recursively of a specific type
        //private static IEnumerable<DependencyObject> GetChildsRecursive(DependencyObject root)
        //{
        //    var elts = new List<DependencyObject> { root };
        //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        //        elts.AddRange(GetChildsRecursive(VisualTreeHelper.GetChild(root, i)));

        //    return elts;
        //}

        // Find all UIElements recursively of a specific type
        private static IEnumerable<DependencyObject> GetChilds(DependencyObject root)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
                yield return VisualTreeHelper.GetChild(root, i);
        }

        // Find all UIElements recursively and Order by Deepest in Tree,
        private static IEnumerable<DependencyObject> GetChildsOrderedRecursive(DependencyObject root)
        {
            var childs = GetChilds(root).Reverse();

            if (root is Canvas)
                childs = childs.OrderByDescending(x => Canvas.GetZIndex((UIElement)x));

            foreach (var dependencyObject in childs)
            {
                foreach (var chld in GetChildsOrderedRecursive(dependencyObject))
                {
                    yield return chld;
                }
            }

            yield return root;
        }

        /// <summary>
        /// Gets the current position
        /// </summary>
        /// <returns></returns>
        internal Point getCurrentPosition()
        {
            return currentCanvasPosition;
        }

        /// <summary>
        /// Removes all drop borders
        /// </summary>
        internal void RemoveAllDropBorders()
        {
            if (AllDropTargetsValid)
            {

                foreach (var item in InternalDropTargets)
                {
                    item.RemoveBorder();
                }
            }
            else
            {
                foreach (var item in DropTargets)
                {
                    item.RemoveBorder();
                }
            }
        }

        #region IDisposable Members

        // reference IDisposable Pattern: http://msdn.microsoft.com/en-us/magazine/cc163392.aspx

        /// <summary>
        /// Destructor
        /// </summary>
        ~DragSource()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clean up all managed resources
                MainControlHost = null;
                MainDraggableControl = null;
                GhostContentControl = null;
                DummyOverlay = null;
                OriginalParent = null;

                if (DragHandleMode == DragHandleModeType.Handle)
                {
                    if (DragBar != null)
                    {
                        DragBar.MouseLeftButtonDown += new MouseButtonEventHandler(DragBar_MouseLeftButtonDown);
                        DragBar.MouseMove += new MouseEventHandler(DragBar_MouseMove);
                        DragBar.MouseLeftButtonUp += new MouseButtonEventHandler(DragBar_MouseLeftButtonUp);
                    }
                }
                else
                {
                    this.MouseLeftButtonDown += new MouseButtonEventHandler(DragBar_MouseLeftButtonDown);
                    this.MouseMove += new MouseEventHandler(DragBar_MouseMove);
                    this.MouseLeftButtonUp += new MouseButtonEventHandler(DragBar_MouseLeftButtonUp);
                }

                if (MainContentPresenter != null) MainContentPresenter.SizeChanged -= new SizeChangedEventHandler(MainContentPresenter_SizeChanged);


                DragBar = null;
                MainContentPresenter = null;

            }

            // Clean up all native resources
        }


        #endregion


    }
}
