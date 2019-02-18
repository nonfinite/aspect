using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Aspect.Models;
using Aspect.Services;
using Aspect.Services.Win32;
using Aspect.Utility;

namespace Aspect.UI
{
    public enum ImageFit
    {
        FitAll,
        FitWidth,
        FitHeight,
        FullSize,
        Custom,
    }

    public partial class ImageViewer : UserControl
    {
        public ImageViewer()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MediaControlsProperty = DependencyProperty.Register(
            "MediaControls", typeof(MediaElementControls), typeof(ImageViewer),
            new PropertyMetadata(default(MediaElementControls), _HandleMediaControlsChanged));

        public static readonly DependencyProperty ImageFitProperty = DependencyProperty.Register(
            "ImageFit", typeof(ImageFit), typeof(ImageViewer),
            new PropertyMetadata(default(ImageFit), _HandleImageFitChanged));

        public static readonly DependencyProperty FileProperty = DependencyProperty.Register(
            "File", typeof(FileData), typeof(ImageViewer),
            new PropertyMetadata(default(FileData), _HandleFileChanged));

        private Brush mBrush;
        private Size mImageSize;
        private bool mIsDragStarted = false;
        private Matrix mMatrix;
        private Point mMouseStart = new Point(0, 0);
        private ImageFit mRestoreToImageFit = ImageFit.FitAll;

        public FileData File
        {
            get => (FileData) GetValue(FileProperty);
            set => SetValue(FileProperty, value);
        }

        public ImageFit ImageFit
        {
            get => (ImageFit) GetValue(ImageFitProperty);
            set => SetValue(ImageFitProperty, value);
        }

        public MediaElementControls MediaControls
        {
            get => (MediaElementControls) GetValue(MediaControlsProperty);
            set => SetValue(MediaControlsProperty, value);
        }

        private void _FitImage()
        {
            var fit = ImageFit;
            if (fit == ImageFit.Custom)
            {
                return;
            }

            var scale = 1.0;
            if (fit == ImageFit.FitAll)
            {
                scale = Math.Min(ActualWidth / mImageSize.Width, ActualHeight / mImageSize.Height);
            }
            else if (fit == ImageFit.FitHeight)
            {
                scale = ActualHeight / mImageSize.Height;
            }
            else if (fit == ImageFit.FitWidth)
            {
                scale = ActualWidth / mImageSize.Width;
            }
            else if (fit == ImageFit.FullSize)
            {
                scale = 1.0;
            }

            mMatrix = new Matrix();
            mMatrix.Scale(scale, scale);

            var imageWidth = mImageSize.Width * scale;
            if (!(ActualWidth < imageWidth))
            {
                mMatrix.Translate((ActualWidth - imageWidth) / 2.0, 0);
            }

            var imageHeight = mImageSize.Height * scale;
            if (!(ActualHeight < imageHeight))
            {
                mMatrix.Translate(0, (ActualHeight - imageHeight) / 2.0);
            }

            this.Log().Information("Setting image to {Fit} scale of {Scale} at {OffsetX},{OffsetY}",
                fit, scale, mMatrix.OffsetX, mMatrix.OffsetY);

            _UpdateMatrix();
        }

        private static void _HandleFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ImageViewer viewer))
            {
                return;
            }

            viewer._InitFromFile((FileData) e.NewValue);
        }

        private static void _HandleImageFitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ImageViewer viewer))
            {
                return;
            }

            var newFit = (ImageFit) e.NewValue;
            var oldFit = (ImageFit) e.OldValue;

            if (newFit == ImageFit.Custom && oldFit != ImageFit.Custom)
            {
                viewer.mRestoreToImageFit = oldFit;
            }

            viewer._FitImage();
        }

        private static void _HandleMediaControlsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is MediaElementControls controls)
            {
                var viewer = (ImageViewer) d;
                viewer.mMediaElementHolder.Children.Remove(controls.Element);
                controls.Dispose();
                GC.Collect();
            }
        }

        private void _HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            var input = (IInputElement) sender;
            mMouseStart = e.GetPosition(input);
            input.CaptureMouse();
        }

        private void _HandleMouseMove(object sender, MouseEventArgs e)
        {
            var input = (IInputElement) sender;
            if (input.IsMouseCaptured)
            {
                var position = e.GetPosition(input);
                var v = position - mMouseStart;
                if (mIsDragStarted ||
                    Math.Abs(v.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(v.Y) >= SystemParameters.MinimumVerticalDragDistance)
                {
                    mIsDragStarted = true;
                    mMouseStart = position;
                    mMatrix.Translate(v.X, v.Y);
                    _UpdateMatrix();

                    ImageFit = ImageFit.Custom;
                }
            }
        }

        private void _HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            var input = (IInputElement) sender;
            input.ReleaseMouseCapture();
            mIsDragStarted = false;
        }

        private void _HandleMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scale = e.Delta > 0 ? 1.1 : (1.0 / 1.1);

            var point = e.GetPosition(this);

            this.Log().Verbose("Scaling image by {Scale} at {Point}", scale, point);
            mMatrix.ScaleAtPrepend(scale, scale, point.X, point.Y);
            _UpdateMatrix();

            ImageFit = ImageFit.Custom;
        }

        private void _HandleSizeChanged(object sender, SizeChangedEventArgs e) => _FitImage();

        private async void _InitFromFile(FileData file)
        {
            this.Log().Information("Loading {Uri}", file?.Uri);

            if (file == null)
            {
                mBrush = null;
                MediaControls = null;
                return;
            }

            mLoadingBar.Visibility = Visibility.Visible;

            var result = await _Load(file);

            mBrush = result.Item1;
            MediaControls = result.Item2;
            mImageSize = result.Item3;

            mLoadingBar.Visibility = Visibility.Collapsed;

            if (ImageFit == ImageFit.Custom)
            {
                ImageFit = mRestoreToImageFit;
            }
            else
            {
                _FitImage();
            }
        }

        private Task<Tuple<Brush, MediaElementControls, Size>> _Load(FileData file)
        {
            if (file.IsAnimated)
            {
                var dimensions = file.Dimensions;
                var controls = new MediaElementControls(file);
                mMediaElementHolder.Children.Add(controls.Element);
                return controls.LoadedTask
                    .ContinueWith(t =>
                        new Tuple<Brush, MediaElementControls, Size>(t.Result.Brush, t.Result, dimensions));
            }

            var image = new BitmapImage(file.Uri);
            var brush = new ImageBrush(image);
            return Task.FromResult(new Tuple<Brush, MediaElementControls, Size>(
                brush, null, new Size(image.PixelWidth, image.PixelHeight)));
        }

        private double _LockBounds(double low, double high, double boundLow, double boundHigh, double current)
        {
            var size = high - low;
            var target = boundHigh - boundLow;
            if (size < target)
            {
                // if smaller, prevent pushing out sides
                if (low < 0)
                {
                    return 0;
                }

                if (high > target)
                {
                    return target - size;
                }
            }
            else
            {
                // if larger, prevent dragging past side
                if (low > 0)
                {
                    return 0;
                }

                if (high < target)
                {
                    return target - size;
                }
            }

            return current;
        }

        private void _ShowProperties(object sender, RoutedEventArgs e)
        {
            var filePath = File?.Uri.LocalPath;
            if (filePath != null)
            {
                Shell32.ShowFileProperties(filePath);
            }
        }

        private void _UpdateMatrix()
        {
            this.Log().Verbose("Setting matrix to {Matrix} at {OffsetX},{OffsetY}",
                mMatrix, mMatrix.OffsetX, mMatrix.OffsetY);

            if (Settings.Default.KeepImageOnScreen)
            {
                var topLeft = mMatrix.Transform(new Point(0, 0));
                var bottomRight = mMatrix.Transform(new Point(mImageSize.Width, mImageSize.Height));

                mMatrix.OffsetX = _LockBounds(topLeft.X, bottomRight.X, 0, ActualWidth, mMatrix.OffsetX);
                mMatrix.OffsetY = _LockBounds(topLeft.Y, bottomRight.Y, 0, ActualHeight, mMatrix.OffsetY);
            }

            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (mBrush == null)
            {
                return;
            }

            var dest = new Rect(
                mMatrix.Transform(new Point(0, 0)),
                mMatrix.Transform(new Point(mImageSize.Width, mImageSize.Height)));
            drawingContext.DrawRectangle(mBrush, null, dest);
        }
    }
}
