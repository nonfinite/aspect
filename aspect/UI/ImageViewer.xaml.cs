using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

using Aspect.Models;
using Aspect.Properties;
using Aspect.Services.Win32;
using Aspect.Utility;

using XamlAnimatedGif;

namespace Aspect.UI
{
    public enum ImageFit
    {
        FitAll,
        FitWidth,
        FitHeight,
        Custom,
    }

    public partial class ImageViewer : UserControl
    {
        public ImageViewer()
        {
            InitializeComponent();
            AnimationBehavior.SetAutoStart(mImage, true);
            AnimationBehavior.SetRepeatBehavior(mImage, RepeatBehavior.Forever);
        }

        public static readonly DependencyProperty ImageFitProperty = DependencyProperty.Register(
            "ImageFit", typeof(ImageFit), typeof(ImageViewer),
            new PropertyMetadata(default(ImageFit), _HandleImageFitChanged));

        public static readonly DependencyProperty FileProperty = DependencyProperty.Register(
            "File", typeof(FileData), typeof(ImageViewer),
            new PropertyMetadata(default(FileData), _HandleFileChanged));

        private Point mMouseStart = new Point(0, 0);


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
                scale = Math.Min(ActualWidth / mImage.ActualWidth, ActualHeight / mImage.ActualHeight);
            }
            else if (fit == ImageFit.FitHeight)
            {
                scale = ActualHeight / mImage.ActualHeight;
            }
            else if (fit == ImageFit.FitWidth)
            {
                scale = ActualWidth / mImage.ActualWidth;
            }

            var matrix = new Matrix();
            matrix.Scale(scale, scale);

            var imageWidth = mImage.ActualWidth * scale;
            if (!(ActualWidth < imageWidth))
            {
                matrix.Translate((ActualWidth - imageWidth) / 2.0, 0);
            }

            var imageHeight = mImage.ActualHeight * scale;
            if (!(ActualHeight < imageHeight))
            {
                matrix.Translate(0, (ActualHeight - imageHeight) / 2.0);
            }

            this.Log().Information("Setting image to {Fit} scale of {Scale} at {OffsetX},{OffsetY}",
                fit, scale, matrix.OffsetX, matrix.OffsetY);

            _SetMatrix(matrix);
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

            viewer._FitImage();
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
                mMouseStart = position;
                var matrix = mMatrixTransform.Matrix;
                matrix.Translate(v.X, v.Y);
                // MatrixTransform needs Matrix reset in order to notice the translate
                _SetMatrix(matrix);

                ImageFit = ImageFit.Custom;
            }
        }

        private void _HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            var input = (IInputElement) sender;
            input.ReleaseMouseCapture();
        }

        private void _HandleMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var matrix = mMatrixTransform.Matrix;
            var scale = e.Delta > 0 ? 1.1 : (1.0 / 1.1);

            var point = e.GetPosition(mImage);

            this.Log().Verbose("Scaling image by {Scale} as {Point}", scale, point);
            matrix.ScaleAtPrepend(scale, scale, point.X, point.Y);
            _SetMatrix(matrix);

            ImageFit = ImageFit.Custom;
        }

        private void _HandleSizeChanged(object sender, SizeChangedEventArgs e) => _FitImage();

        private void _InitFromFile(FileData file)
        {
            this.Log().Information("Loading {Uri}", file?.Uri);

            AnimationBehavior.SetSourceUri(mImage, file?.Uri);
            if (file == null)
            {
                return;
            }

            mImage.Width = file.Dimensions.Width;
            mImage.Height = file.Dimensions.Height;
            this.Log().Information("Setting image size to {Width}x{Height}", mImage.Width, mImage.Height);

            if (ImageFit == ImageFit.Custom)
            {
                ImageFit = ImageFit.FitAll;
            }
            else
            {
                _FitImage();
            }
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

        private void _SetMatrix(Matrix matrix)
        {
            this.Log().Verbose("Setting matrix to {Matrix} at {OffsetX},{OffsetY}",
                matrix, matrix.OffsetX, matrix.OffsetY);

            if (Settings.Default.KeepImageOnScreen)
            {
                var topLeft = matrix.Transform(new Point(0, 0));
                var bottomRight = matrix.Transform(new Point(mImage.Width, mImage.Height));

                matrix.OffsetX = _LockBounds(topLeft.X, bottomRight.X, 0, ActualWidth, matrix.OffsetX);
                matrix.OffsetY = _LockBounds(topLeft.Y, bottomRight.Y, 0, ActualHeight, matrix.OffsetY);
            }

            mMatrixTransform.Matrix = matrix;
        }

        private void _ShowProperties(object sender, RoutedEventArgs e)
        {
            var filePath = File?.Uri.LocalPath;
            if (filePath != null)
            {
                Shell32.ShowFileProperties(filePath);
            }
        }
    }
}
