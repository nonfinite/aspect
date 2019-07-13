using System;
using System.Text.RegularExpressions;

using Aspect.Services;
using Aspect.Utility;

namespace Aspect.Models
{
    public sealed class FileFilter : NotifyPropertyChanged
    {
        public FileFilter()
        {
            mTextRegex = new LazyEx<Regex>(_CreateTextRegex);
            Settings.Default.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(Settings.MultiImageSelection) && !Settings.Default.MultiImageSelection)
                {
                    ShowOnlyCheckedItems = false;
                }
            };
        }

        private readonly LazyEx<Regex> mTextRegex;
        private Rating? mRating;
        private bool mShowOnlyCheckedItems;
        private string mText;

        public Rating? Rating
        {
            get => mRating;
            set => Set(ref mRating, value);
        }

        public bool ShowOnlyCheckedItems
        {
            get => mShowOnlyCheckedItems;
            set => Set(ref mShowOnlyCheckedItems, value);
        }

        public string Text
        {
            get => mText;
            set
            {
                if (Set(ref mText, value))
                {
                    mTextRegex.Reset();
                }
            }
        }

        private Regex _CreateTextRegex()
        {
            var text = Text;
            if (string.IsNullOrWhiteSpace(text) || !text.Contains("*"))
            {
                return null;
            }

            var regex = Regex.Escape(text).Replace(@"\*", ".*");
            return new Regex($"^{regex}$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        public bool IsMatch(FileData file)
        {
            if (file == null)
            {
                return false;
            }

            if (ShowOnlyCheckedItems && !file.IsSelected)
            {
                return false;
            }

            var rating = Rating;
            if (rating.HasValue)
            {
                var fr = file.Rating;
                if (!fr.HasValue)
                {
                    return false;
                }

                if (fr.Value < rating.Value)
                {
                    return false;
                }
            }

            var text = Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                var regex = mTextRegex.Value;
                if (regex == null)
                {
                    if (file.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if (!regex.IsMatch(file.Name))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
