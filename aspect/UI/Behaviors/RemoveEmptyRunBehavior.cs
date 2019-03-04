using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interactivity;

namespace Aspect.UI.Behaviors
{
    public sealed class RemoveEmptyRunBehavior : Behavior<TextBlock>
    {
        private static void _HandleLoaded(object sender, RoutedEventArgs e)
        {
            var textBlock = (TextBlock) sender;
            textBlock.Loaded -= _HandleLoaded;

            var empty = textBlock.Inlines
                .OfType<Run>()
                .Where(run => run.Text == " " && run.GetBindingExpression(Run.TextProperty) == null)
                .ToArray();

            foreach (var run in empty)
            {
                textBlock.Inlines.Remove(run);
            }
        }

        protected override void OnAttached()
        {
            if (AssociatedObject.IsLoaded)
            {
                _HandleLoaded(AssociatedObject, new RoutedEventArgs());
            }
            else
            {
                AssociatedObject.Loaded += _HandleLoaded;
            }
        }

        protected override void OnDetaching() => AssociatedObject.Loaded -= _HandleLoaded;
    }
}
