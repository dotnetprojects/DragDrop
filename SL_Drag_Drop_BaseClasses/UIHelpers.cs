using System;
using System.Linq;
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

        public static ScaleTransform GetCombinedScaleTransform(this UIElement element)
        {
            double scaleX = 1;
            double scaleY = 1;
            while (element != null)
            {
                var t1 = element.RenderTransform as ScaleTransform;

                if (t1 != null)
                {
                    scaleX *= t1.ScaleX;
                    scaleY *= t1.ScaleY;
                }
#if SILVERLIGHT
                var t2 = element.RenderTransform as CompositeTransform;

                if (t2 != null)
                {
                    scaleX *= t2.ScaleX;
                    scaleY *= t2.ScaleY;
                }
#else
                var t2 = element.RenderTransform as MatrixTransform;
                if (t2 != null)
                {
                    
                    scaleX *= t2.Matrix.M11;
                    scaleY *= t2.Matrix.M22;
                }

                var t3 = element.RenderTransform as TransformGroup;
                if (t3 != null)
                {
                    var scaleT = t3.Children.FirstOrDefault(x => x is ScaleTransform) as ScaleTransform;
                    if (scaleT != null)
                    {
                        scaleX *= scaleT.ScaleX;
                        scaleY *= scaleT.ScaleY;
                    }
                }
#endif
                element = VisualTreeHelper.GetParent(element) as UIElement;
            }

            return new ScaleTransform() {ScaleX = scaleX, ScaleY = scaleY};
        }
    }
}
