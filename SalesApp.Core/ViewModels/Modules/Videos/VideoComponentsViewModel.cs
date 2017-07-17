using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using SalesApp.Core.BL.Models.Modules.Videos;
using SalesApp.Core.Enums.Modules.Videos;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;

namespace SalesApp.Core.ViewModels.Modules.Videos
{
    public class VideoComponentsViewModel : BaseViewModel
    {
        private Stack<ObservableCollection<VideoComponent>> _historyStack;
        private ILog _logger = LogManager.Get(typeof(VideoComponentsViewModel));
        private ObservableCollection<VideoComponent> _videoComponents;
        private MvxCommand<VideoComponent> _itemClickedCommand;
        private string _title;
        private string _screenTitle;
        private bool _playing;
        private string _currentVideoPath;

        public VideoComponentsViewModel(IFolderEnumerator enumerator)
        {
            List<VideoComponent> categories = enumerator.Enumerate();
            this.VideoComponents = new ObservableCollection<VideoComponent>(categories);
        }

        public bool NavigateUp()
        {
            if (this.HistoryStack.Count == 0)
            {
                return false;
            }

            if (this.Playing)
            {
                this.Playing = false;
                return true;
            }

            var previousComponents = this.HistoryStack.Pop();

            if (previousComponents == null)
            {
                return false;
            }

            this.VideoComponents = previousComponents;
            this.Title = "Please select a category";
            return true;
        }

        public Stack<ObservableCollection<VideoComponent>> HistoryStack
        {
            get
            {
                if (this._historyStack == null)
                {
                    this._historyStack = new Stack<ObservableCollection<VideoComponent>>();
                }

                return this._historyStack;
            }

            set
            {
                this._historyStack = value;
            }
        }

        public ObservableCollection<VideoComponent> VideoComponents
        {
            get
            {
                return this._videoComponents;
            }

            set
            {
                this.SetProperty(ref this._videoComponents, value, () => this.VideoComponents);
                this.Title = value.Count > 0 ? "Please select a category" : "There are no videos to view";
            }
        }

        public string CurrentVideoPath
        {
            get
            {
                return this._currentVideoPath;
            }

            set
            {
                this.SetProperty(ref this._currentVideoPath, value, () => this.CurrentVideoPath);
            }
        }

        public bool Playing
        {
            get
            {
                return this._playing;
            }

            set
            {
                this.SetProperty(ref this._playing, value, () => this.Playing);
            }
        }

        public string Title
        {
            get
            {
                return this._title;
            }

            set
            {
                this.SetProperty(ref this._title, value, () => this.Title);
            }
        }

        public string ScreenTitle
        {
            get
            {
                return this._screenTitle;
            }

            set
            {
                this.SetProperty(ref this._screenTitle, value, () => this.ScreenTitle);
            }
        }

        public ICommand ItemClickCommand
        {
            get
            {
                this._itemClickedCommand = this._itemClickedCommand ?? new MvxCommand<VideoComponent>(this.VideoComponentClicked);
                return this._itemClickedCommand;
            }
        }

        public void VideoComponentClicked(VideoComponent videoComponent)
        {
            try
            {
                if (videoComponent.NodeType == NodeType.NonLeafNode)
                {
                    this.HistoryStack.Push(this.VideoComponents);

                    List<VideoComponent> newVideos = videoComponent.Children();
                    this.VideoComponents = new ObservableCollection<VideoComponent>(newVideos);

                    this.Title = videoComponent.Name;
                }
                else
                {
                    this._logger.Debug("Playing video " + videoComponent.Name);

                    this.CurrentVideoPath = videoComponent.FilePath;
                    this.ScreenTitle = videoComponent.Name.StripFileExtension();
                    this.Playing = true;
                }
            }
            catch (NotImplementedException niex)
            {
                this._logger.Info(videoComponent.GetType() + niex.Message);
            }
        }
    }
}