using System.Windows.Input;
using System.Collections.Generic;
using System.Text;

namespace TRIM_Helper
{
    public static class HotKeys
    {
        public static RoutedCommand runTaskCmd = new RoutedCommand();
        public static RoutedCommand extractTaskCmd = new RoutedCommand();

        public static RoutedCommand addTaskCmd = new RoutedCommand();
        public static RoutedCommand editTaskCmd = new RoutedCommand();
        public static RoutedCommand deleteTaskCmd = new RoutedCommand();

        public static RoutedCommand cancelTaskWnd = new RoutedCommand();
    }
}
