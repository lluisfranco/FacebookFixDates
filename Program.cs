using System;
using System.IO;

namespace FacebookFixDates
{
    class Program
    {
        public static FacebookParserService FacebookParserService { get; set; } 
        static void Main(string[] args)
        {
            try
            {            
                PrintHeader();
                var facebook_base_path = Console.ReadLine();

                facebook_base_path = "C:\\fb";// "/home/lluisfranco/Pictures/Fb";//"C:\\fb";

                FacebookParserService = new FacebookParserService(facebook_base_path);
                FacebookParserService.Log += (s, e) => { Console.WriteLine(e.LogMessage); };
                FacebookParserService.Initialize();
                FacebookParserService.ReadPhotosInformationFromFileSystem();
                FacebookParserService.ExportInformationToFileSystem();
                PrintSummary();
            }
            catch (DirectoryNotFoundException dex)
            {
                Console.WriteLine($"** ERROR : Folder '{dex.Message}' not found **");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"** ERROR : {ex.Message} **");
            }
        }

        private static void PrintHeader()
        {
            Console.WriteLine($"*****************************************************************************************");
            Console.WriteLine($"** FACEBOOK BACKUP UTILITY - PHOTOS/VIDEOS DATES FIXER                                 **");
            Console.WriteLine($"** When you backup your Facebook profile, a ZIP file is generated with all your info.  **");
            Console.WriteLine($"** Once unzipped, there is a folder named 'photos_and_videos which contains all your   **");
            Console.WriteLine($"** albums, profile and timeline photos, organized in folders.                          **");
            Console.WriteLine($"** These folders contains your photos but some metadata has been removed (like date)   **");
            Console.WriteLine($"** This script reads the content of your profile, reading from the HTML pages,         **");
            Console.WriteLine($"** and fixing the photo metadata. After finishing, all then photos are copied          **");
            Console.WriteLine($"** to a new folder called '_Export'                                                    **");
            Console.WriteLine($"*****************************************************************************************");
            Console.WriteLine($"");
            Console.WriteLine($"Enter your Facebook backup base path ('C:\\FB' (Win) or '/home/<user>/FB' (Linux or Mac)");
        }

        private static void PrintSummary()
        {
            Console.WriteLine($"*****************************************************************************************");
            Console.WriteLine($"** EXPORT SUMMARY");
            Console.WriteLine($"** TOTAL ALBUMS: {FacebookParserService.TotalAlbumsExported}");
            Console.WriteLine($"** TOTAL PHOTOS: {FacebookParserService.TotalPhotosExported}");
            Console.WriteLine($"** TOTAL ERRORS: {FacebookParserService.TotalErrors}");
            Console.WriteLine($"** Elapsed Time: {FacebookParserService.Clock.ElapsedMilliseconds / 1000:n0}s.");
            Console.WriteLine($"*****************************************************************************************");
        }
    }
}
