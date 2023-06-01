using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BerryBoard2
{
    public static class TreeViewItemProperties
    {
        public static readonly DependencyProperty ImagePathProperty = DependencyProperty.RegisterAttached(
            "ImagePath", typeof(string), typeof(TreeViewItemProperties), new FrameworkPropertyMetadata(string.Empty));

        public static string GetImagePath(DependencyObject obj)
        {
            return (string)obj.GetValue(ImagePathProperty);
        }

        public static void SetImagePath(DependencyObject obj, string value)
        {
            obj.SetValue(ImagePathProperty, value);
        }
    }
}
