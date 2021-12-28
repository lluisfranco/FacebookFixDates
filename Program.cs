using ExifLibrary;
using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;

namespace FacebookFixDates
{
    class Program
    {
        public static FacebookParserService FacebookParserService { get; set; } 
        static void Main(string[] args)
        {
            //const string FB_PHOTOS_FOLDER_NAME = "photos_and_videos";
            //const string FB_PHOTOS_INDEX_PAGE_NAME = "your_photos.html";
            //const string FB_VIDEOS_INDEX_PAGE_NAME = "your_videos.html";
            //const string EXPORT_ALBUMS_FOLDER_NAME = "_Export";
            //const bool EXPORT_USE_ALBUM_FOLDER_NAME = true;

            try
            {            
                PrintHeader();
                var facebook_base_path = Console.ReadLine();

                facebook_base_path = "C:\\fb";// "/home/lluisfranco/Pictures/Fb";//"C:\\fb";

                FacebookParserService = new FacebookParserService(facebook_base_path);
                FacebookParserService.Initialize();
                FacebookParserService.ReadFacebookInformationFromFileSystem();

            }
            catch (Exception)
            {

                throw;
            }


            // FacebookParser.BaseFolderPath = facebook_base_path;

            //if (string.IsNullOrWhiteSpace(FacebookParser.BaseFolderPath))
            //{
            //    Console.WriteLine("Facebook base path cannot be null.");
            //    return;
            //}
            //var facebook_folder = new DirectoryInfo(FacebookParser.BaseFolderPath);
            //if (!facebook_folder.Exists)
            //{
            //    PrintItemDontExistsMessage(FacebookParser.BaseFolderPath);
            //    return;
            //}
            //var facebook_photos_path = Path.GetFullPath(
            //    Path.Combine(FacebookParser.BaseFolderPath, FB_PHOTOS_FOLDER_NAME));
            //var facebook_photos_folder = new DirectoryInfo(facebook_photos_path);
            //FacebookParser.PhotosFolderPath = facebook_photos_path;

            //if (!facebook_photos_folder.Exists)
            //{
            //    PrintItemDontExistsMessage(FacebookParser.PhotosFolderPath);
            //    return;
            //}
            //var facebook_photos_index_page_path = Path.GetFullPath(
            //    Path.Combine(FacebookParser.PhotosFolderPath, FB_PHOTOS_INDEX_PAGE_NAME));
            //FacebookParser.PhotosIndexPage = facebook_photos_index_page_path;
            //if (!File.Exists(FacebookParser.PhotosIndexPage))
            //{
            //    PrintItemDontExistsMessage(FacebookParser.PhotosIndexPage);
            //    return;
            //}

        }

        private static void PrintHeader()
        {
            Console.WriteLine("*****************************************************************************************");
            Console.WriteLine("** FACEBOOK BACKUP UTILITY - PHOTOS/VIDEOS DATES FIXER                                        **");
            Console.WriteLine("** When you backup your Facebook profile, a ZIP file is generated with all your info.  **");
            Console.WriteLine("** Once unzipped, there is a folder named 'photos_and_videos which contains all your   **");
            Console.WriteLine("** albums, profile and timeline photos, organized in folders.                          **");
            Console.WriteLine("** These folders contains your photos but some metadata has been removed (like date)   **");
            Console.WriteLine("** This script reads the content of your profile, reading from the HTML pages,         **");
            Console.WriteLine("** and fixing the photo metadata. After finishing, all then photos are copied          **");
            Console.WriteLine("** to a new folder called '_Export'                                                    **");
            Console.WriteLine("*****************************************************************************************");
            Console.WriteLine();
            Console.WriteLine("Enter your Facebook backup base path ('C:\\FB' (Win) or '/home/<user>/FB' (Linux or Mac)");
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
}
