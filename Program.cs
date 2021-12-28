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
            //const string FB_ALBUMS_FOLDER_NAME = "album";

            var facebook_base_path = Console.ReadLine();

            facebook_base_path = "C:\\fb"; //"/home/lluisfranco/Pictures/Fb";//"C:\\fb";

            if (string.IsNullOrWhiteSpace(facebook_base_path))
            {
                Console.WriteLine("Facebook base path cannot be null.");
                return;
            }
            FacebookParser.BaseFolderPath = facebook_base_path;
            var facebook_folder = new DirectoryInfo(facebook_base_path);
            if (facebook_folder.Exists)
            {
                var facebook_photos_path = Path.GetFullPath(Path.Combine(facebook_base_path, FB_PHOTOS_FOLDER_NAME));
                var facebook_photos_folder = new DirectoryInfo(facebook_photos_path);
                if (facebook_photos_folder.Exists)
                {
                    FacebookParser.PhotosFolderPath = facebook_folder.FullName; 
                    var facebook_photos_index_page_path = Path.GetFullPath(Path.Combine(facebook_photos_path, FB_PHOTOS_INDEX_PAGE_NAME));
                    if (File.Exists(facebook_photos_index_page_path))
                    {
                        FacebookParser.PhotosIndexPage = facebook_photos_index_page_path;
                        var doc = new HtmlDocument();
                        doc.Load(facebook_photos_index_page_path);
                        var node = doc.DocumentNode.SelectSingleNode("//body");

                        var divNodesAlbums = node.SelectNodes("//div").
                            Where(p => p.Attributes["class"].Value == "_3-96 _2let");
                        foreach (var divNodesAlbum in divNodesAlbums)
                        {
                            var album = new AlbumNode();

                            var prev = divNodesAlbum.PreviousSibling;
                            var next = divNodesAlbum.NextSibling;

                            album.Title = next.InnerText;

                            var nodeLink = divNodesAlbum.FirstChild;
                            var nodeLinkImage = nodeLink.FirstChild;

                            album.Name = nodeLink.Attributes["href"].Value;
                            album.URL = Path.GetFullPath(Path.Combine(facebook_base_path, album.Name));

                            if (File.Exists(album.URL))
                            {

                            }
                            album.CoverImageURL = Path.Combine(facebook_photos_path, nodeLinkImage.Attributes["src"].Value);
                            FacebookParser.Albums.Add(album);
                        }
                    }
                    else
                    {
                        PrintItemDontExistsMessage(facebook_photos_index_page_path);
                    }
                }
                else
                {
                    PrintItemDontExistsMessage(facebook_photos_path);
                }
            }
            else
            {
                PrintItemDontExistsMessage(facebook_base_path);
            }
        }

        private static void PrintItemDontExistsMessage(string item_path)
        {
            Console.WriteLine($"OOps! '{item_path}' doesn't exists.");
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
    }
}
