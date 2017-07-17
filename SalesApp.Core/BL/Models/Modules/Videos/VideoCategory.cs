using System.Collections.Generic;
using System.Linq;

namespace SalesApp.Core.BL.Models.Modules.Videos
{
    public class VideoCategory : VideoComponent
    {
        private List<VideoComponent> _videoList = new List<VideoComponent>();

        public VideoCategory(string groupName, string filePath) : base(filePath, groupName)
        {
        }

        public override void Add(VideoComponent videoComponent)
        {
            this._videoList.Add(videoComponent);
        }

        public override void Remove(VideoComponent videoComponent)
        {
            this._videoList.Remove(videoComponent);
        }

        public override VideoComponent Get(int index)
        {
            return this._videoList[index];
        }

        public override List<VideoComponent> Children()
        {
            return this._videoList;
        }

        public List<VideoComponent> VideoList
        {
            get
            {
                return this._videoList;
            }

            set
            {
                this._videoList = value;
            }
        }

        public override string Description
        {
            get
            {
                int count = this.VideoList.Count(x => x is Video);

                return count == 1 ? count + " Video" : count + " Videos";
            }
        }
    }
}