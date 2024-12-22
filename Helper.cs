using System.Windows;
using System.Windows.Media;

namespace ZembryoAnalyser;

public static class Helper
{
    public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(obj, i);

            if (child is not null and T item)
            {
                return item;
            }
            else
            {
                T childOfChild = FindVisualChild<T>(child);

                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
        }

        return null;
    }
}
