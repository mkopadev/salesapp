using System.Collections.Generic;
using System.IO;
using Android.OS;
using SalesApp.Core.BL.Models.Modules.Videos;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Settings;
using SalesApp.Core.ViewModels.Modules.Videos;
using File = Java.IO.File;

namespace SalesApp.Droid.Views.Modules.Videos
{
    public class AndroidFolderEnumerator : IFolderEnumerator
    {
        private string _videosFolder;

        private List<string> PossibleSdCardLocations
        {
            get
            {
                return new List<string>
                {
                    "/storage/sdcard0",
                    "/storage/sdcard0/internalSD",
                    "/storage/sdcard1",
                    "/storage/extSdCard",
                    "/mnt/sdcard",
                    "/mnt/sdcard/ext_sd",
                    "/mnt/external",
                    "/mnt/extSdCard",
                    "/mnt/sdcard/external_sd"
                };
            }
        } 

        private string VideosFolder
        {
            get
            {
                if (this._videosFolder != null)
                {
                    return this._videosFolder;
                }

                foreach (var location in this.PossibleSdCardLocations)
                {
                    this._videosFolder = location + Settings.Instance.VideosDirectory;

                    if (Directory.Exists(this._videosFolder))
                    {
                        return this._videosFolder;
                    }
                }

                this._videosFolder = Environment.ExternalStorageDirectory.AbsolutePath + Settings.Instance.VideosDirectory;

                if (Directory.Exists(this._videosFolder))
                {
                    return this._videosFolder;
                }

                return null;
            }
        }

        public List<VideoComponent> Enumerate()
        {
            if (this.VideosFolder != null)
            {
                return this.EnumarateFolder(this.VideosFolder);
            }
            
            return new List<VideoComponent>();
        }

        private string GetVideoComponentIcon(VideoComponent component, File assetsFolder)
        {
            return assetsFolder.AbsolutePath + "/" + component.Name.StripFileExtension() + ".PNG";
        }

        private bool IsAssetsFolder(File folder)
        {
            return folder.IsDirectory && folder.Name.ToLowerInvariant() == "assets";
        }

        private List<VideoComponent> EnumarateFolder(string folderPath)
        {
            File baseFolder = new File(folderPath);

            File[] files = baseFolder.ListFiles();
            List<VideoComponent> list = new List<VideoComponent>();

            foreach (File file in files)
            {
                if (file.IsHidden || file.IsFile)
                {
                    continue;
                }

                File assetsFolder = new File(file.AbsolutePath + "/assets");
                if (!Directory.Exists(assetsFolder.AbsolutePath))
                {
                    Directory.CreateDirectory(assetsFolder.AbsolutePath);
                }

                if (file.IsDirectory)
                {
                    VideoComponent videoCategory = new VideoCategory(file.Name, folderPath);
                    string iconPath = GetVideoComponentIcon(videoCategory, assetsFolder);

                    if (new File(iconPath).Exists())
                    {
                        videoCategory.ThumbNailUrl = iconPath;
                    }

                    string innerFolderPath = this.VideosFolder + file.Name + "/";
                    File innerFoler = new File(innerFolderPath);

                    File[] innerFiles = innerFoler.ListFiles();
                    foreach (var innerFile in innerFiles)
                    {
                        if (innerFile.IsHidden || IsAssetsFolder(innerFile))
                        {
                            continue;
                        }

                        VideoComponent component;
                        if (innerFile.IsDirectory)
                        {
                            component = new VideoCategory(innerFile.Name, innerFolderPath);
                        }
                        else
                        {
                            component = new Video(innerFile.Name, innerFolderPath);
                        }

                        iconPath = GetVideoComponentIcon(component, assetsFolder);

                        if (new File(iconPath).Exists())
                        {
                            component.ThumbNailUrl = iconPath;
                        }
                        
                        videoCategory.Add(component);
                    }

                    list.Add(videoCategory);
                }
                else
                {
                    VideoComponent video = new Video(file.Name, folderPath);
                    list.Add(video);
                }
            }

            return list;
        }
    }
}