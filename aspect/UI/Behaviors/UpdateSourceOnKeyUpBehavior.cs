using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Aspect.UI.Behaviors
{
    public class UpdateSourceOnKeyUpBehavior : Behavior<FrameworkElement>
    {
        public Key Key { get; set; }
        public DependencyProperty Property { get; set; }

        private void _HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key)
            {
                var binding = AssociatedObject.GetBindingExpression(Property);
                binding?.UpdateSource();
            }
        }

        protected override void OnAttached()
        {
            AssociatedObject.KeyUp += _HandleKeyUp;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.KeyUp -= _HandleKeyUp;
            base.OnDetaching();
        }
    }
}
