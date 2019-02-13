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

        private MediaElement mMediaElement;
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

        private void _HandleMediaEnded(object sender, RoutedEventArgs e)
        {
            var element = ((MediaElement) sender);
            element.Position = TimeSpan.FromSeconds(1);
            element.Play();
        }

        private void _HandleMediaOpened(object sender, RoutedEventArgs e)
        {
            var elem = (MediaElement) sender;
            var tag = (Tuple<TaskCompletionSource<Tuple<Brush, MediaElement, Size>>, Size>) elem.Tag;
            tag.Item1.SetResult(new Tuple<Brush, MediaElement, Size>(
                new VisualBrush(elem), elem, tag.Item2));
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
                mMediaElement = null;
                return;
            }

            mLoadingBar.Visibility = Visibility.Visible;

            var result = await _Load(file);
            var oldMediaElement = mMediaElement;

            mBrush = result.Item1;
            mMediaElement = result.Item2;
            mImageSize = result.Item3;

            if (oldMediaElement != null)
            {
                oldMediaElement.MediaOpened -= _HandleMediaOpened;
                oldMediaElement.MediaEnded -= _HandleMediaEnded;
                oldMediaElement.Stop();
                oldMediaElement.Source = null;
                mMediaElementHolder.Children.Remove(oldMediaElement);
                GC.Collect();
            }

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

        private Task<Tuple<Brush, MediaElement, Size>> _Load(FileData file)
        {
            if (file.IsAnimated)
            {
                var dimensions = file.Dimensions;
                var element = new MediaElement
                {
                    Stretch = Stretch.Uniform,
                    LoadedBehavior = MediaState.Manual
                };
                mMediaElementHolder.Children.Add(element);
                var tcs = new TaskCompletionSource<Tuple<Brush, MediaElement, Size>>();
                element.Tag = Tuple.Create(tcs, dimensions);
                element.MediaOpened += _HandleMediaOpened;
                element.MediaEnded += _HandleMediaEnded;
                element.Source = file.Uri;
                element.Play();
                return tcs.Task;
            }

            var image = new BitmapImage(file.Uri);
            var brush = new ImageBrush(image);
            return Task.FromResult(new Tuple<Brush, MediaElement, Size>(
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
