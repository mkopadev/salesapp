using System.Collections.Generic;
using System.Collections.ObjectModel;
using NSubstitute;
using NUnit.Framework;
using SalesApp.Core.BL.Models.Modules.Videos;
using SalesApp.Core.ViewModels.Modules.Videos;

namespace SalesApp.Core.Tests.ViewModels.Modules.Video
{
    [TestFixture]
    public class VideoComponentViewModelTest : TestsBase
    {
        private VideoComponentsViewModel _model;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            List<VideoComponent> videos = new List<VideoComponent>();
            var folderEnumerator = Substitute.For<IFolderEnumerator>();

            folderEnumerator.Enumerate().Returns(videos);

            this._model = new VideoComponentsViewModel(folderEnumerator);
            Assert.That("There are no videos to view", Is.EqualTo(this._model.Title));
        }

        [Test]
        public void TestAddingItemsChangesTitle()
        {
            List<VideoComponent> videos = new List<VideoComponent>
            {
                new SalesApp.Core.BL.Models.Modules.Videos.Video("Video 1", "Path to video file")
            };

            this._model.VideoComponents = new ObservableCollection<VideoComponent>(videos);
            Assert.That("Please select a category", Is.EqualTo(this._model.Title));
        }

        [Test]
        public void TestNavigateUpWithZeroItemsInHistory()
        {
            this._model.HistoryStack = new Stack<ObservableCollection<VideoComponent>>();

            bool navigated = this._model.NavigateUp();
            Assert.That(navigated, Is.False);
        }

        [Test]
        public void TestNavigateUpWithItemsInHistory()
        {
            this._model.HistoryStack = new Stack<ObservableCollection<VideoComponent>>();

            ObservableCollection<VideoComponent> videos = new ObservableCollection<VideoComponent>
            {
                new SalesApp.Core.BL.Models.Modules.Videos.Video("Video 1.mp4", "SalesApp/Videos/Category 1/"),
                new SalesApp.Core.BL.Models.Modules.Videos.Video("Video 2.mp4", "SalesApp/Videos/Category 2/")
            };

            this._model.HistoryStack.Push(videos);

            bool navigated = this._model.NavigateUp();
            Assert.That(navigated, Is.True);
            Assert.That(this._model.VideoComponents, Is.EqualTo(videos));
            Assert.That(this._model.Title, Is.EqualTo("Please select a category"));
        }

        [Test]
        public void TestNavigateUpWithNullInHistory()
        {
            this._model.HistoryStack = new Stack<ObservableCollection<VideoComponent>>();

            this._model.HistoryStack.Push(null);

            bool navigated = this._model.NavigateUp();
            Assert.That(navigated, Is.False);
        }

        [Test]
        public void TestClickingAVideoCategory()
        {
            this._model.HistoryStack = new Stack<ObservableCollection<VideoComponent>>();

            VideoComponent category = new VideoCategory("User Testimonials", "/storage/sdcard0/SalesApp/Videos/User Testimonials/");

            ObservableCollection<VideoComponent> categories = new ObservableCollection<VideoComponent>()
            {
                category
            };

            this._model.VideoComponents = categories;

            VideoComponent video1 = new SalesApp.Core.BL.Models.Modules.Videos.Video("Video 1", "/storage/sdcard0/SalesApp/Videos/User Testimonials/Video 1.MP4");
            VideoComponent video2 = new SalesApp.Core.BL.Models.Modules.Videos.Video("Video 2", "/storage/sdcard0/SalesApp/Videos/User Testimonials/Video 2.MP4");

            category.Add(video1);
            category.Add(video2);

            // Play a video
            this._model.VideoComponentClicked(category);

            ObservableCollection<VideoComponent> videos = new ObservableCollection<VideoComponent>()
            {
                video1,
                video2
            };

            // Test expected model states
            Assert.That(this._model.Title, Is.EqualTo("User Testimonials"));
            Assert.That(this._model.VideoComponents, Is.EqualTo(videos));
            Assert.That(this._model.HistoryStack.Peek(), Is.EqualTo(categories));
        }

        [Test]
        public void NavigateUpWhenPlayingVideo()
        {
            this._model.HistoryStack = new Stack<ObservableCollection<VideoComponent>>();

            ObservableCollection<VideoComponent> videos = new ObservableCollection<VideoComponent>
            {
                new SalesApp.Core.BL.Models.Modules.Videos.Video("Video 1.mp4", "Path to video file/")
            };

            this._model.HistoryStack.Push(videos);

            // Play a video
            this._model.VideoComponentClicked(new SalesApp.Core.BL.Models.Modules.Videos.Video("Video 1.mp4", "Path to video file/"));

            // Test expected model states
            Assert.That(this._model.Playing, Is.True);
            Assert.That(this._model.CurrentVideoPath, Is.EqualTo("Path to video file/Video 1.mp4"));
            Assert.That(this._model.ScreenTitle, Is.EqualTo("Video 1"));

            // Navigate Up
            bool navigated = this._model.NavigateUp();

            // Expect that video stopped playing
            Assert.That(this._model.Playing, Is.False);
            Assert.That(navigated, Is.True);
        }
    }
}