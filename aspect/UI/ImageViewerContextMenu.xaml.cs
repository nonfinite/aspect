using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Aspect.Models;
using Aspect.Services.Win32;

namespace Aspect.UI
{
    public partial class ImageViewerContextMenu : ContextMenu
    {
        public ImageViewerContextMenu()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty FileProperty = DependencyProperty.Register(
            "File", typeof(FileData), typeof(ImageViewerContextMenu), new PropertyMetadata(default(FileData)));

        public FileData File
        {
            get => (FileData) GetValue(FileProperty);
            set => SetValue(FileProperty, value);
        }

        private void _CopyFile(object sender, RoutedEventArgs e) =>
            _WithFile(file => Clipboard.SetFileDropList(new StringCollection {file.Uri.LocalPath}));

        private void _CopyImage(object sender, RoutedEventArgs e) =>
            _WithFile(file => Clipboard.SetImage(new BitmapImage(file.Uri)));

        private void _CopyPath(object sender, RoutedEventArgs e) =>
            _WithFile(file => Clipboard.SetText(file.Uri.LocalPath));

        private void _ShowProperties(object sender, RoutedEventArgs e) =>
            _WithFile(file =>
            {
                var filePath = file.Uri.LocalPath;
                Shell32.ShowFileProperties(filePath);
            });

        private void _WithFile(Action<FileData> action)
        {
            var file = File;
            if (file != null)
            {
                action(file);
            }
        }
    }
}
