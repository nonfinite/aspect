using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Aspect.UI.Behaviors
{
    public class KeepCurrentItemInViewBehavior : Behavior<ListBox>
    {
        private static void _HandleIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var listBox = (ListBox) sender;
            if (Equals(e.NewValue, true))
            {
                foreach (var item in listBox.SelectedItems)
                {
                    listBox.ScrollIntoView(item);
                }
            }
        }

        private static void _HandleSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox) sender;

            if (e.AddedItems != null)
            {
                foreach (var item in e.AddedItems)
                {
                    listBox.ScrollIntoView(item);
                }
            }
        }

        protected override void OnAttached()
        {
            AssociatedObject.SelectionChanged += _HandleSelectionChanged;
            AssociatedObject.IsVisibleChanged += _HandleIsVisibleChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= _HandleSelectionChanged;
            AssociatedObject.IsVisibleChanged -= _HandleIsVisibleChanged;
        }
    }
}
