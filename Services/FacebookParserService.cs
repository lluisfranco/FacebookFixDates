using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace FacebookFixDates
{
    public class FacebookParserService
    {
        public Stopwatch Clock { get; private set; } = new();
        public FacebookParser FacebookParser { get; private set; } = new();
        public LogDetailEnum LogDetailMode { get; set; } = LogDetailEnum.Normal;
        public event EventHandler<LogEventArgs> Log;
        public int TotalAlbumsExported { get; private set; }
        public int TotalPhotosExported { get; private set; }
        public int TotalErrors { get; private set; }

        const string FB_PHOTOS_FOLDER_NAME = "posts";
        const string FB_PHOTOS_INDEX_PAGE_NAME = "your_photos.html";
        const string EXPORT_FOLDER_NAME = "_Export";
        const string EXPORT_PHOTOS_FOLDER_NAME = "Photos";
        const bool USE_ALBUM_NAME_IN_PHOTOS = true;

        public void RaiseEventLog(string message, LogDetailEnum detailMode = LogDetailEnum.Normal)
        {
            if (LogDetailMode == LogDetailEnum.Disabled) return;
            if (LogDetailMode >= detailMode)
                Log?.Invoke(this, new LogEventArgs() { LogMessage = message });
        }

        public FacebookParserService(string facebookBasePath)
        {
            FacebookParser.BaseFolderPath = facebookBasePath;
        }

        public void Initialize()
        {
            Clock.Start();
            if (string.IsNullOrWhiteSpace(FacebookParser.BaseFolderPath))
                throw new Exception("Facebook base path cannot be null.");
            var facebook_folder = new DirectoryInfo(FacebookParser.BaseFolderPath);
            if (!facebook_folder.Exists)
                throw new DirectoryNotFoundException(FacebookParser.BaseFolderPath);
            var facebook_photos_path = Path.GetFullPath(
                Path.Combine(FacebookParser.BaseFolderPath, FB_PHOTOS_FOLDER_NAME));
            var facebook_photos_folder = new DirectoryInfo(facebook_photos_path);
            FacebookParser.PhotosFolderPath = facebook_photos_path;
            if (!facebook_photos_folder.Exists)
                throw new DirectoryNotFoundException(FacebookParser.PhotosFolderPath);
            var facebook_photos_index_page_path = Path.GetFullPath(
                Path.Combine(FacebookParser.PhotosFolderPath, FB_PHOTOS_INDEX_PAGE_NAME));
            FacebookParser.PhotosIndexPage = facebook_photos_index_page_path;
            if (!File.Exists(FacebookParser.PhotosIndexPage))
                throw new DirectoryNotFoundException(FacebookParser.PhotosIndexPage);
            RaiseEventLog($"Initialize - OK ({Clock.ElapsedMilliseconds:n2}ms.)");
        }

        public void ReadPhotosInformationFromFileSystem()
        {
            var mainDocument = new HtmlDocument();
            mainDocument.Load(FacebookParser.PhotosIndexPage);
            var mainDocumentBodyNode = mainDocument.DocumentNode.SelectSingleNode("//body");
            var albumNodes = mainDocumentBodyNode.SelectNodes("//div").
                Where(p => p.Attributes["class"]?.Value == "_3-96 _2let");
            foreach (var albumNode in albumNodes)
            {
                var album = GetPhotoAlbumFromNode(albumNode);
                if (album != null) FacebookParser.PhotoAlbums.Add(album);
            }
            RaiseEventLog($"Reading Info - OK ({Clock.ElapsedMilliseconds:n2}ms.)");
        }

        private PhotosAlbumNode GetPhotoAlbumFromNode(HtmlNode albumNode)
        {
            try
            {
                var albumDateNode = albumNode.NextSibling;
                var albumLinkNode = albumNode.FirstChild;
                var albumLinkImageNode = albumLinkNode.FirstChild;
                var albumName = albumLinkNode.Attributes["href"]?.Value;
                var albumDate = Convert.ToDateTime(albumDateNode.InnerText);
                var albumCoverImageURL = albumLinkImageNode.Attributes["src"]?.Value;
                RaiseEventLog($"Start - Reading Album Info '{albumName}'", LogDetailEnum.Verbose);
                var album = new PhotosAlbumNode
                {
                    Name = albumName,
                    Date = albumDate,
                    URL = Path.GetFullPath(
                        Path.Combine(FacebookParser.BaseFolderPath, albumName)),
                    CoverImageURL = Path.GetFullPath(
                        Path.Combine(FacebookParser.BaseFolderPath, albumCoverImageURL))
                };
                if (File.Exists(album.URL))
                {
                    var albumDocument = new HtmlDocument();
                    albumDocument.Load(album.URL);
                    var albumDocumentBodyNode = albumDocument.DocumentNode.SelectSingleNode("//body");
                    var albumTitle = albumDocumentBodyNode.SelectNodes("//div").
                        FirstOrDefault(p => p.Attributes["class"]?.Value == "_3b0d");
                    album.Title = HttpUtility.HtmlDecode(albumTitle.InnerText);
                    RaiseEventLog($"Reading: {album.Title}");
                    var albumPhotosNodes = albumDocumentBodyNode.SelectNodes("//div").
                        Where(p => p.Attributes["class"]?.Value == "_3-96 _2let");
                    foreach (var albumPhotoNode in albumPhotosNodes)
                    {
                        var photo = GetPhotoFromNode(albumPhotoNode);
                        photo.AlbumNode = album;
                        album.Photos.Add(photo);
                    }
                }
                RaiseEventLog($"{album.Photos.Count} photos in '{albumName}'", LogDetailEnum.Verbose);
                RaiseEventLog($"End - Reading Album Info '{albumName}'", LogDetailEnum.Verbose);
                return album;
            }
            catch (Exception ex)
            {
                TotalErrors++;
                RaiseEventLog($"ERROR - {ex.Message}'");
                throw;
            }
        }

        private PhotoNode GetPhotoFromNode(HtmlNode albumPhotoNode)
        {
            var photoLinkNode = albumPhotoNode.FirstChild;
            var photoParentFrameNode = albumPhotoNode.ParentNode;
            var photoDateNode = photoParentFrameNode.LastChild;
            var photoDate = Convert.ToDateTime(photoDateNode.InnerText);
            var photoName = photoLinkNode.Attributes["href"]?.Value;
            var photoURL = Path.GetFullPath(
                Path.Combine(FacebookParser.BaseFolderPath, photoName));
            var photo = new PhotoNode
            {
                Name = photoName,
                URL = photoURL,
                Date = photoDate
            };
            return photo;
        }

        public void ExportInformationToFileSystem()
        {
            var exportFolderPath = Path.GetFullPath(
                Path.Combine(FacebookParser.BaseFolderPath, EXPORT_FOLDER_NAME));
            var exportFolder = new DirectoryInfo(exportFolderPath);
            if(exportFolder.Exists) exportFolder.Delete(true);
            exportFolder.Create();
            ExportPhotosInformationToFileSystem();
            Clock.Stop();
        }

        public void ExportPhotosInformationToFileSystem()
        {
            var exportPhotosFolderPath = Path.GetFullPath(
                Path.Combine(FacebookParser.BaseFolderPath, EXPORT_FOLDER_NAME, EXPORT_PHOTOS_FOLDER_NAME));
            var exportPhotosMainFolder = new DirectoryInfo(exportPhotosFolderPath);
            if (exportPhotosMainFolder.Exists) exportPhotosMainFolder.Delete(true);
            exportPhotosMainFolder.Create();
            foreach (var photoAlbum in FacebookParser.PhotoAlbums)
            {
                ExportAlbum(exportPhotosMainFolder, photoAlbum);
            }
        }

        private void ExportAlbum(DirectoryInfo exportPhotosMainFolder, PhotosAlbumNode photoAlbum)
        {
            int i = 0;            
            try
            {
                var albumName = photoAlbum.Title.ReplaceInvalidCharsInFileName();
                RaiseEventLog($"Start - Exporting Album '{albumName}'", LogDetailEnum.Verbose);
                var albumFolder = exportPhotosMainFolder.CreateSubdirectory(albumName);
                foreach (var photo in photoAlbum.Photos)
                {
                    i++;
                    ExportPhoto(albumFolder, photo, i);
                }
                RaiseEventLog($"Exported: {photoAlbum.Photos.Count} photos in album: '{albumName}'");
                RaiseEventLog($"End - Exporting Album '{albumName}'", LogDetailEnum.Verbose);
                TotalAlbumsExported++;
            }
            catch (Exception ex)
            {
                TotalErrors++;
                RaiseEventLog($"ERROR - {ex.Message}'");
                throw;
            }
        }

        private void ExportPhoto(DirectoryInfo albumFolder, PhotoNode photo, int i)
        {            
            var photoFile = new FileInfo(photo.URL);
            if (photoFile.Exists)
            {
                try
                {
                    var photoFileName = USE_ALBUM_NAME_IN_PHOTOS ? 
                        $"{albumFolder.Name}_{i}.{photoFile.Extension}" : 
                        photoFile.Name;
                    var newPhotoFile = Path.GetFullPath(
                        Path.Combine(albumFolder.FullName, photoFileName));
                    if (File.Exists(newPhotoFile))
                    {
                        newPhotoFile = GetFileNewName(newPhotoFile);
                    }
                    photoFile.CopyTo(newPhotoFile);
                    ImageExtensions.SaveDateMetadata(newPhotoFile, photo.Date);
                    RaiseEventLog($"Exported: '{photoFile.Name}' to '{albumFolder.Name}'", LogDetailEnum.Verbose);
                    TotalPhotosExported++;
                }
                catch (Exception ex)
                {
                    TotalErrors++;
                    RaiseEventLog($"ERROR - {ex.Message}'");
                    throw;
                }
            }
        }

        private string GetFileNewName(string filename)
        {
            var fi = new FileInfo(filename);
            if (fi.Exists)
            {
                var newname = $"{fi.Name.Replace("." + fi.Extension, null)}_1.{fi.Extension}";
                var newpath = Path.GetFullPath(Path.Combine(fi.DirectoryName, newname));
                return GetFileNewName(newpath);
            }
            else
            {
                return filename;
            }
        }
    }
}
