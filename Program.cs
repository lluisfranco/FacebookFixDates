using System;
using System.IO;

namespace FacebookFixDates
{
    class Program
    {
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
            Console.WriteLine("Enter your Facebook backup base path ('C:\\FB' (Win) or '/home/<user>/FB' (Linux)");

            var facebook_base_path = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(facebook_base_path))
            {
                Console.WriteLine("Facebook base path cannot be null.");
                return;
            }
            var facebook_folder = new DirectoryInfo(facebook_base_path);
            if (facebook_folder.Exists)
            {
                Console.WriteLine("NICE!");
            }
            else
            {
                Console.WriteLine($"OOps! '{facebook_base_path}' doesn't exists.");
            }
        }
    }
}
