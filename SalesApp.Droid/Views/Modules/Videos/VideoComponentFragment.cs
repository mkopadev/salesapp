using System;
using System.Collections.ObjectModel;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.Modules.Videos;
using SalesApp.Core.ViewModels.Modules.Videos;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.Views.Modules.Videos
{
    public class VideoComponentFragment : ModuleFragmentBase, IPreviousNavigator
    {
        private const string HistoryStackBundleKey = "HistoryStackBundleKey";
        private const string CurrentTitleBundleKey = "CurrentTitleBundleKey";
        private const string CurrentScreenTitleBundleKey = "CurrentScreenTitleBundleKey";
        private const string CurrentVideosBundleKey = "CurrentVideosBundleKey";
        private const string CurrentVideoPositionBundleKey = "CurrentVideoPositionBundleKey";
        private const string VideoPlayStateBundleKey = "VideoPlayStateBundleKey";
        private const string VideoPathBundleKey = "VideoPathBundleKey";

        private MvxListView _listView;
        private VideoView _videoView;
        private VideoComponentsViewModel _viewModel;
        private BindableVideoPlayer _bindableVideoPlayer;
        private bool _videoWasPlaying;
        private int _videoPosition;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle inState)
        {
            base.OnCreateView(inflater, container, inState);
            View view = this.BindingInflate(Resource.Layout.fragment_video_components, null);
            this._listView = view.FindViewById<MvxListView>(Resource.Id.video_list);
            this._videoView = view.FindViewById<VideoView>(Resource.Id.video_view);
            this._videoView.Completion += WatchingVideoComplete;

            MediaController controller = new MediaController(this.Activity);
            this._videoView.SetMediaController(controller);

            this._viewModel = new VideoComponentsViewModel(new AndroidFolderEnumerator());

            this._bindableVideoPlayer = new BindableVideoPlayer(this._videoView);

            var set = this.CreateBindingSet<VideoComponentFragment, VideoComponentsViewModel>();

            set.Bind(this._bindableVideoPlayer).For(obj => obj.VideoPath).To(x => x.CurrentVideoPath);
            set.Bind(this._bindableVideoPlayer).For(obj => obj.IsPlaying).To(x => x.Playing);
            set.Bind(this._bindableVideoPlayer).For(obj => obj.ScreenTitle).To(x => x.ScreenTitle);
            set.Apply();

            this.ViewModel = this._viewModel;

            var adapter = new VideoComponentAdapter(this.Activity, (IMvxAndroidBindingContext)this.BindingContext);
            this._listView.Adapter = adapter;

            if (inState != null)
            {
                string json = inState.GetString(CurrentVideosBundleKey);
                string stackJson = inState.GetString(HistoryStackBundleKey);

                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };

                ObservableCollection<VideoComponent> savedList = JsonConvert.DeserializeObject<ObservableCollection<VideoComponent>>(json, settings);
                var previousStack = JsonConvert.DeserializeObject<ObservableCollection<VideoComponent>[]>(stackJson, settings);

                this.RestoreHistory(previousStack);
                this._viewModel.VideoComponents = savedList;
                string title = inState.GetString(CurrentTitleBundleKey);
                this._viewModel.Title = title;

                if (inState.ContainsKey(CurrentScreenTitleBundleKey))
                {
                    this._viewModel.ScreenTitle = inState.GetString(CurrentScreenTitleBundleKey);
                }

                if (inState.ContainsKey(VideoPlayStateBundleKey))
                {
                    this.RestoreVideState(inState);
                }

                string category = Activity.GetString(Resource.String.module_videos);
                string video = _bindableVideoPlayer.ScreenTitle;
                string watching = Activity.GetString(Resource.String.started_watching);
                StartedWatchingVideo(category, video, watching);
            }
        
            return view;
        }

        private void StartedWatchingVideo(string category, string video, string watching)
        {
            GoogleAnalyticService.Instance.TrackEvent(category, video, watching);
        }

        private void WatchingVideoComplete(object sender, EventArgs e)
        {
            string category = Activity.GetString(Resource.String.module_videos);
            string doneWatching = Activity.GetString(Resource.String.done_watching);
            string label = _bindableVideoPlayer.ScreenTitle;
            GoogleAnalyticService.Instance.TrackEvent(category, label, doneWatching);
        }

        private void RestoreHistory(ObservableCollection<VideoComponent>[] history)
        {
            foreach (var list in history)
            {
                this._viewModel.HistoryStack.Push(list);
            }
        }

        /// <summary>
        /// Restores the state of the vide from a bundle
        /// </summary>
        /// <param name="inState">The bundle containing state information</param>
        private void RestoreVideState(Bundle inState)
        {
            string videoPath = inState.GetString(VideoPathBundleKey);
            this._videoView.Visibility = ViewStates.Visible;
            this._listView.Visibility = ViewStates.Gone;
            this._videoView.SetVideoPath(videoPath);
            this._videoView.Tag = videoPath;
            this._videoView.Start();
            int position = inState.GetInt(CurrentVideoPositionBundleKey);
            this._videoView.SeekTo(position);
            this._viewModel.Playing = true;
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            outState.PutString(CurrentVideosBundleKey, JsonConvert.SerializeObject(this._viewModel.VideoComponents, settings));
            outState.PutString(HistoryStackBundleKey, JsonConvert.SerializeObject(this._viewModel.HistoryStack.ToArray(), settings));
            outState.PutString(CurrentTitleBundleKey, this._viewModel.Title);
            if (!string.IsNullOrEmpty(this._viewModel.ScreenTitle))
            {
                outState.PutString(CurrentScreenTitleBundleKey, this._viewModel.ScreenTitle);
            }
            
            if (this._videoWasPlaying)
            {
                // save video state here
                outState.PutBoolean(VideoPlayStateBundleKey, true);
                outState.PutInt(CurrentVideoPositionBundleKey, _videoPosition);
                outState.PutString(VideoPathBundleKey, this._videoView.Tag.ToString());
            }
        }

        public override void OnResume()
        {
            base.OnResume();
            this.FragmentLoadStateListener.RequestOrintation(ScreenOrientation.Sensor);
        }

        public override void OnPause()
        {
            base.OnPause();
            this._videoWasPlaying = this._videoView.IsPlaying;
            this._videoPosition = this._videoView.CurrentPosition;
        }

        public bool Previous()
        {
            VideoComponentsViewModel vm = this.ViewModel as VideoComponentsViewModel;

            if (vm == null)
            {
                return false;
            }

            string title = this.GetString(Resource.String.module_videos);
            this.FragmentLoadStateListener.TitleChanged(title);
            return vm.NavigateUp();
        }

        private class BindableVideoPlayer
        {
            private VideoView _videoView;
            private string _videoPath;
            private string _screenTitle;
            private ActivityBase _activity;

            public BindableVideoPlayer(VideoView videoView)
            {
                this._videoView = videoView;
                this._activity = this._videoView.Context as ActivityBase;
            }

            public string ScreenTitle
            {
                get
                {
                    return this._screenTitle;
                }

                set
                {
                    if (value == null || this._activity == null || this._activity.SupportActionBar == null)
                    {
                        return;
                    }

                    this._activity.SupportActionBar.Title = value;
                    this._screenTitle = value;
                }
            }

            public string VideoPath
            {
                get
                {
                    return this._videoPath;
                }

                set
                {
                    if (value == null)
                    {
                        return;
                    }

                    this._videoPath = value;
                    this._videoView.SetVideoPath(value);
                }
            }

            public bool IsPlaying
            {
                get
                {
                    return this._videoView.IsPlaying;
                }

                set
                {
                    if (value)
                    {
                        if (this._videoView.IsPlaying)
                        {
                            return;
                        }

                        this._videoView.SetVideoPath(this.VideoPath);
                        this._videoView.Tag = this.VideoPath;
                        this._videoView.Start();
                    }
                    else
                    {
                        if (this._videoView.IsPlaying)
                        {
                            this._videoView.StopPlayback();
                        }
                    }
                }
            }
        }
    }
}