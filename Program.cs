using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FacebookFixDates
{
    class Program
    {
        public static FacebookParser FacebookParser { get; set; } = new FacebookParser();
        static void Main(string[] args)
        {
            Console.WriteLine("****************************************************************************************");
            Console.WriteLine("** FACEBOOK BACKUP - FIX PHOTOS DATES                                                 **");
            Console.WriteLine("** When you backup your Facebook profile a ZIP file is generated with all your info.  **");
            Console.WriteLine("** Once unzipped, there is a folder named 'photos_and_videos which contains all your  **");
            Console.WriteLine("** albums, profile and timeline photos, organized in folders.                         **");
            Console.WriteLine("** These folders contains your photos but some metadata has been removed (like date)  **");
            Console.WriteLine("** This script reads the content of this profile, reading from the HTML pages,        **");
            Console.WriteLine("** and fix the photo metadata. Then copy the fixed photos to a new folder.            **");
            Console.WriteLine("****************************************************************************************");
            Console.WriteLine();
            Console.WriteLine("Enter your Facebook backup base path ('C:\\FB' (Win) or '/home/<user>/FB' (Linux or Mac)");

            const string FB_PHOTOS_FOLDER_NAME = "photos_and_videos";
            const string FB_PHOTOS_INDEX_PAGE_NAME = "your_photos.html";
            
            const string EXPORT_ALBUMS_FOLDER_NAME = "_ExportedPhotos";
            const bool EXPORT_USE_ALBUM_FOLDER_NAME = true;

            var facebook_base_path = Console.ReadLine();

            facebook_base_path = "C:\\fb"; //"/home/lluisfranco/Pictures/Fb";//"C:\\fb";
            FacebookParser.BaseFolderPath = facebook_base_path;

            if (string.IsNullOrWhiteSpace(FacebookParser.BaseFolderPath))
            {
                Console.WriteLine("Facebook base path cannot be null.");
                return;
            }
            var facebook_folder = new DirectoryInfo(FacebookParser.BaseFolderPath);
            if (facebook_folder.Exists)
            {
                var facebook_photos_path = Path.GetFullPath(
                    Path.Combine(FacebookParser.BaseFolderPath, FB_PHOTOS_FOLDER_NAME));
                var facebook_photos_folder = new DirectoryInfo(facebook_photos_path);
                FacebookParser.PhotosFolderPath = facebook_photos_path; 
                if (facebook_photos_folder.Exists)
                {
                    var facebook_photos_index_page_path = Path.GetFullPath(
                        Path.Combine(FacebookParser.PhotosFolderPath, FB_PHOTOS_INDEX_PAGE_NAME));
                    FacebookParser.PhotosIndexPage = facebook_photos_index_page_path;
                    if (File.Exists(FacebookParser.PhotosIndexPage))
                    {
                        var mainDocument = new HtmlDocument();
                        mainDocument.Load(FacebookParser.PhotosIndexPage);
                        var mainDocumentBodyNode = mainDocument.DocumentNode.SelectSingleNode("//body");
                        var albumNodes = mainDocumentBodyNode.SelectNodes("//div").
                            Where(p => p.Attributes["class"]?.Value == "_3-96 _2let");
                        foreach (var albumNode in albumNodes)
                        {
                            var album = new AlbumNode();
                            var prevNode = albumNode.PreviousSibling;
                            var nextNode = albumNode.NextSibling;
                            var linkNode = albumNode.FirstChild;
                            var linkImageNode = linkNode.FirstChild;
                            //album.Title = nextNode.InnerText;                            
                            album.Name = linkNode.Attributes["href"]?.Value;
                            album.Date = Convert.ToDateTime(nextNode.InnerText);
                            album.URL = Path.GetFullPath(
                                Path.Combine(FacebookParser.BaseFolderPath, album.Name));
                            album.CoverImageURL = Path.GetFullPath(
                                Path.Combine(FacebookParser.BaseFolderPath, linkImageNode.Attributes["src"]?.Value));

                            if (File.Exists(album.URL))
                            {
                                var albumDocument = new HtmlDocument();
                                albumDocument.Load(album.URL);
                                var albumDocumentBodyNode = albumDocument.DocumentNode.SelectSingleNode("//body");

                                var albumTitle = albumDocumentBodyNode.SelectNodes("//div").
                                    FirstOrDefault(p => p.Attributes["class"]?.Value == "_3b0d");
                                album.Title = albumTitle.InnerText;

                                var albumPhotosNodes = albumDocumentBodyNode.SelectNodes("//div").
                                    Where(p => p.Attributes["class"]?.Value == "_3-96 _2let");
                                foreach (var albumPhotoNode in albumPhotosNodes)
                                {
                                    var photoLinkNode = albumPhotoNode.FirstChild;
                                    var photoParentFrameNode = albumPhotoNode.ParentNode;
                                    var dateNode = photoParentFrameNode.LastChild;
                                    var photoDate = Convert.ToDateTime(dateNode.InnerText);
                                    var photoName = photoLinkNode.Attributes["href"]?.Value;
                                    var photoURL = Path.GetFullPath(
                                        Path.Combine(FacebookParser.BaseFolderPath, photoName));
                                    var photo = new PhotoNode();
                                    photo.Name = photoName;
                                    photo.URL = photoURL;
                                    photo.Date = photoDate;
                                    album.Photos.Add(photo);

                                    
                                }
                            }
                            FacebookParser.Albums.Add(album);
                        }
                    }
                    else
                    {
                        PrintItemDontExistsMessage(FacebookParser.PhotosIndexPage);
                    }
                }
                else
                {
                    PrintItemDontExistsMessage(FacebookParser.PhotosFolderPath);
                }
            }
            else
            {
                PrintItemDontExistsMessage(FacebookParser.BaseFolderPath);
            }
        }

        private static void PrintItemDontExistsMessage(string item_path)
        {
            Console.WriteLine($"OOps! '{item_path}' doesn't exists.");
        }

        private static string ReplaceInvalidCharsInFileName(string filename)
        {
            var invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
            {
                filename = filename.Replace(c.ToString(), "_");
            }
            return filename;
        }
    }

    public class FacebookParser
    {
        public string BaseFolderPath { get; set; }
        public string PhotosFolderPath { get; set; }
        public string PhotosIndexPage { get; set; }
        public List<AlbumNode> Albums { get; set; } = new List<AlbumNode>();
    }

    public class AlbumNode
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        public string CoverImageURL { get; set; }
        public DateTime Date { get; set; }
        public List<PhotoNode> Photos { get; set; } = new List<PhotoNode>();
    }

    public class PhotoNode
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public DateTime Date { get; set; }
    }
}
