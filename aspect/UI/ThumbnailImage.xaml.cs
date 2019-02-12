using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Aspect.Services;
using Aspect.Utility;

namespace Aspect.UI
{
    public partial class ThumbnailImage : UserControl
    {
        public ThumbnailImage()
        {
            InitializeComponent();
            IsVisibleChanged += _HandleVisibilityChanged;
        }

        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(
            "FilePath", typeof(Uri), typeof(ThumbnailImage),
            new PropertyMetadata(default(Uri), _HandleFilePathChanged));

        public Uri FilePath
        {
            get => (Uri) GetValue(FilePathProperty);
            set => SetValue(FilePathProperty, value);
        }

        private static void _HandleFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var image = (ThumbnailImage) d;
            image._Reload();
        }

        private void _HandleVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Equals(e.NewValue, Visibility.Visible))
            {
                _Reload();
            }
        }

        private void _Reload()
        {
            if (Visibility != Visibility.Visible)
            {
                this.Log().Information("Not loading thumbnail due to control being hidden");
                return;
            }

            var file = FilePath?.LocalPath;

            this.Log().Information("Reloading thumbnail image from {File}", file);

            mProgress.IsActive = true;
            mImage.Source = null;

            if (!string.IsNullOrWhiteSpace(file))
            {
                var width = (int) Width.IfNaN(ActualWidth);
                var height = (int) Height.IfNaN(ActualHeight);
                ThumbnailService
                    .GetThumbnail(file, width, height)
                    .ContinueWith(t =>
                    {
                        mProgress.IsActive = false;

                        if (t.Exception != null)
                        {
                            this.Log().Error("Failed to load thumbnail for {File}", file);
                            return;
                        }

                        mImage.Source = t.Result;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                mProgress.IsActive = false;
            }
        }
    }
}
