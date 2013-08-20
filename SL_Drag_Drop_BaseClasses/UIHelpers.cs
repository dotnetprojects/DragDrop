using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DragDropLibrary
{
    public static class UIHelpers
    {
        public static FrameworkElement RootUI
        {
            get
            {
#if SILVERLIGHT
                return Application.Current.RootVisual as FrameworkElement;
#else
                return Application.Current.MainWindow;
#endif
            }
        }
    }
}
