using System.IO;

namespace FacebookFixDates
{
    //var file = ImageFile.FromFile(photoURL);

    //var isoTag = file.Properties.Get<ExifUShort>(ExifTag.ISOSpeedRatings);
    //var dateTag = file.Properties.Get<ExifDateTime>(ExifTag.DateTimeOriginal);
    //var photoExifDate = dateTag?.Value ?? DateTime.Today;
    //var newdate = photoExifDate.AddDays(1);
    //file.Properties.Set(ExifTag.DateTimeOriginal, newdate);
    //file.Save(photoURL);
    public static class StringExtensions
    {
        public static string ReplaceInvalidCharsInFileName(this string filename)
        {
            var invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
            {
                filename = filename.Replace(c.ToString(), "_");
            }
            return filename;
        }
    }
    }
