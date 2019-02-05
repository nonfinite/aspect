using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

using Aspect.Models;

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

        private Point mTransformStart;

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

            if (fit == ImageFit.FitAll)
            {
                mScaleTransform.ScaleX = mScaleTransform.ScaleY =
                    Math.Min(ActualWidth / mImage.ActualWidth, ActualHeight / mImage.ActualHeight);
            }
            else if (fit == ImageFit.FitHeight)
            {
                mScaleTransform.ScaleX = mScaleTransform.ScaleY = ActualHeight / mImage.ActualHeight;
            }
            else if (fit == ImageFit.FitWidth)
            {
                mScaleTransform.ScaleX = mScaleTransform.ScaleY = ActualWidth / mImage.ActualWidth;
            }


            var imageWidth = mImage.ActualWidth * mScaleTransform.ScaleX;
            if (ActualWidth < imageWidth)
            {
                mTranslateTransform.X = 0;
            }
            else
            {
                mTranslateTransform.X = (ActualWidth - imageWidth) / 2.0;
            }

            var imageHeight = mImage.ActualHeight * mScaleTransform.ScaleY;
            if (ActualHeight < imageHeight)
            {
                mTranslateTransform.Y = 0;
            }
            else
            {
                mTranslateTransform.Y = (ActualHeight - imageHeight) / 2.0;
            }
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
            mTransformStart = new Point(mTranslateTransform.X, mTranslateTransform.Y);
            input.CaptureMouse();
        }

        private void _HandleMouseMove(object sender, MouseEventArgs e)
        {
            var input = (IInputElement) sender;
            if (input.IsMouseCaptured)
            {
                var position = e.GetPosition(input);
                var v = mMouseStart - position;
                mTranslateTransform.X = mTransformStart.X - v.X;
                mTranslateTransform.Y = mTransformStart.Y - v.Y;
                ImageFit = ImageFit.Custom;
            }
        }

        private void _HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            var input = (IInputElement) sender;
            input.ReleaseMouseCapture();
        }

        private void _HandleSizeChanged(object sender, SizeChangedEventArgs e) => _FitImage();

        private void _InitFromFile(FileData file)
        {
            AnimationBehavior.SetSourceUri(mImage, file?.Uri);
            if (file == null)
            {
                return;
            }

            mImage.Width = file.Dimensions.Width;
            mImage.Height = file.Dimensions.Height;

            if (ImageFit == ImageFit.Custom)
            {
                ImageFit = ImageFit.FitAll;
            }
            else
            {
                _FitImage();
            }
        }
    }
}
