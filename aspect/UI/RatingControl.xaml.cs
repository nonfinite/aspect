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

        public Rating?[] AvailableRatings { get; } =
        {
            null,
            new Rating(1),
            new Rating(2),
            new Rating(3),
            new Rating(4),
            new Rating(5),
        };

        public Rating? Rating
        {
            get => (Rating) GetValue(RatingProperty);
            set => SetValue(RatingProperty, value);
        }
    }
}
