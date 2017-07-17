using System;
using System.Collections.Generic;
using SalesApp.Core.Enums.Modules.Videos;

namespace SalesApp.Core.BL.Models.Modules.Videos
{
    public abstract class VideoComponent
    {
        protected VideoComponent(string filePath, string fileName)
        {
            this.FilePath = filePath;
            this.Name = fileName;
        }

        public virtual NodeType NodeType
        {
            get
            {
                return NodeType.NonLeafNode;
            }
        }

        public virtual void Add(VideoComponent videoComponent)
        {
            throw new NotImplementedException();
        }

        public virtual void Remove(VideoComponent videoComponent)
        {
            throw new NotImplementedException();
        }

        public virtual VideoComponent Get(int index)
        {
            throw new NotImplementedException();
        }

        public virtual List<VideoComponent> Children()
        {
            throw new NotImplementedException();
        }

        public string Name { get; set; }

        public virtual string Description
        {
            get { return null; }
        }

        public string FilePath { get; private set; }

        public string ThumbNailUrl { get; set; }
    }
}