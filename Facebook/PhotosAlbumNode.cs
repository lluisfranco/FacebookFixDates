using System;
using System.Collections.Generic;

namespace FacebookFixDates
{
    public class PhotosAlbumNode
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        public string CoverImageURL { get; set; }
        public DateTime Date { get; set; }
        public List<PhotoNode> Photos { get; set; } = new List<PhotoNode>();
    }
}
