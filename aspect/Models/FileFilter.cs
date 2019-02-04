using Aspect.Utility;

namespace Aspect.Models
{
    public sealed class FileFilter : NotifyPropertyChanged
    {
        private Rating? mRating;

        public Rating? Rating
        {
            get => mRating;
            set => Set(ref mRating, value);
        }

        public bool IsMatch(FileData file)
        {
            if (file == null)
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

            return true;
        }
    }
}
