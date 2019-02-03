using System;
using System.Windows;
using System.Windows.Controls;
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


        public ImageFit ImageFit
        {
            get => (ImageFit) GetValue(ImageFitProperty);
            set => SetValue(ImageFitProperty, value);
        }

        public FileData File
        {
            get => (FileData) GetValue(FileProperty);
            set => SetValue(FileProperty, value);
        }

        private static void _HandleImageFitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ImageViewer viewer))
            {
                return;
            }

            viewer._FitImage();
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

        private void _InitFromFile(FileData file)
        {
            AnimationBehavior.SetSourceUri(mImage, file?.Uri);
            if (file == null)
            {
                return;
            }

            var dims = file.GetDimensions();
            mImage.Width = dims.Width;
            mImage.Height = dims.Height;

            if (ImageFit == ImageFit.Custom)
            {
                ImageFit = ImageFit.FitAll;
            }
            else
            {
                _FitImage();
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

        private void _HandleSizeChanged(object sender, SizeChangedEventArgs e) => _FitImage();
    }
}
