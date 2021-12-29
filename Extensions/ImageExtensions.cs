using ExifLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacebookFixDates
{
    public static class ImageExtensions
    {
        public static void SaveDateMetadata(string photoURL, DateTime date)
        {
            var file = ImageFile.FromFile(photoURL);
            //var dateTag = file.Properties.Get<ExifDateTime>(ExifTag.DateTimeOriginal);
            //var photoExifDate = dateTag?.Value ?? DateTime.Today;
            //var newdate = photoExifDate.AddDays(1);
            file.Properties.Set(ExifTag.DateTimeOriginal, date);
            file.Save(photoURL);
        }
    }
}
