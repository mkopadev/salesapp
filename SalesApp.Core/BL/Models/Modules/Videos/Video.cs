using SalesApp.Core.Enums.Modules.Videos;

namespace SalesApp.Core.BL.Models.Modules.Videos
{
    public class Video : VideoComponent
    {
        public Video(string videoName, string filePath) : base(filePath + videoName, videoName)
        {
        }

        public override NodeType NodeType
        {
            get
            {
                return NodeType.LeafNode;
            }
        }
    }
}