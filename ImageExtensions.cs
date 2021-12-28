using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacebookFixDates
{
    public enum MetaProperty
    {
        Title = 40091,
        Comment = 40092,
        Author = 40093,
        Keywords = 40094,
        Subject = 40095,
        Copyright = 33432,
        Software = 11,
        DateTime = 36867
    }

    internal static class ImageExtensions
    {
        public static string GetMetaValue(this Bitmap sourceBitmap, MetaProperty property)
        {
            PropertyItem[] propItems = sourceBitmap.PropertyItems;
            var prop = propItems.FirstOrDefault(p => p.Id == (int)property);
            if (prop != null)
            {
                return Encoding.UTF8.GetString(prop.Value);
            }
            else
            {
                return null;
            }
        }
    }
}
