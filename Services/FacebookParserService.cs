﻿using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace FacebookFixDates
{
    public class FacebookParserService
    {
        readonly Stopwatch Clock = new();
        public FacebookParser FacebookParser { get; private set; } = new FacebookParser();
        
        const string FB_PHOTOS_FOLDER_NAME = "photos_and_videos";
        const string FB_PHOTOS_INDEX_PAGE_NAME = "your_photos.html";
        const string FB_VIDEOS_INDEX_PAGE_NAME = "your_videos.html";
        const string EXPORT_FOLDER_NAME = "_Export";
        const bool EXPORT_USE_ALBUM_FOLDER_NAME = true;
        const string EXPORT_PHOTOS_FOLDER_NAME = "Photos";

        public LogDetailEnum LogDetailMode { get; set; } = LogDetailEnum.Normal;
        public event EventHandler<LogEventArgs> Log;
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
                throw new Exception(FacebookParser.BaseFolderPath);
            var facebook_photos_path = Path.GetFullPath(
                Path.Combine(FacebookParser.BaseFolderPath, FB_PHOTOS_FOLDER_NAME));
            var facebook_photos_folder = new DirectoryInfo(facebook_photos_path);
            FacebookParser.PhotosFolderPath = facebook_photos_path;
            if (!facebook_photos_folder.Exists)
                throw new Exception(FacebookParser.PhotosFolderPath);
            var facebook_photos_index_page_path = Path.GetFullPath(
                Path.Combine(FacebookParser.PhotosFolderPath, FB_PHOTOS_INDEX_PAGE_NAME));
            FacebookParser.PhotosIndexPage = facebook_photos_index_page_path;
            if (!File.Exists(FacebookParser.PhotosIndexPage))
                throw new Exception(FacebookParser.PhotosIndexPage);
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
                FacebookParser.PhotoAlbums.Add(album);
            }
            RaiseEventLog($"Reading Info - OK ({Clock.ElapsedMilliseconds:n2}ms.)");
        }

        private PhotosAlbumNode GetPhotoAlbumFromNode(HtmlNode albumNode)
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
                    album.Photos.Add(photo);
                }
            }
            RaiseEventLog($"{album.Photos.Count} photos in '{albumName}'", LogDetailEnum.Verbose);
            RaiseEventLog($"End - Reading Album Info '{albumName}'", LogDetailEnum.Verbose);
            return album;
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
                var albumName = photoAlbum.Title.ReplaceInvalidCharsInFileName();
                var albumFolder = exportPhotosMainFolder.CreateSubdirectory(albumName);

            }
        }
    }
}