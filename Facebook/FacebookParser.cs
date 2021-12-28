using System.Collections.Generic;

namespace FacebookFixDates
{
    public class FacebookParser
    {
        public string BaseFolderPath { get; set; }
        public string PhotosFolderPath { get; set; }
        public string PhotosIndexPage { get; set; }
        public List<PhotosAlbumNode> PhotoAlbums { get; set; } = new List<PhotosAlbumNode>();
    }
}
