using System.Windows;
using System.Windows.Controls;

using Aspect.Models;

namespace Aspect.UI
{
    public partial class RatingControl : UserControl
    {
        public RatingControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty RatingProperty = DependencyProperty.Register(
            "Rating", typeof(Rating?), typeof(RatingControl),
            new PropertyMetadata(default(Rating?)));

        public byte[] AvailableRatings { get; } = {0, 1, 2, 3, 4, 5};

        public Rating? Rating
        {
            get => (Rating?) GetValue(RatingProperty);
            set => SetValue(RatingProperty, value);
        }
    }
}
