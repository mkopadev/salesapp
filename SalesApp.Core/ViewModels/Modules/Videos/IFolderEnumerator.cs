using System.Collections.Generic;
using SalesApp.Core.BL.Models.Modules.Videos;

namespace SalesApp.Core.ViewModels.Modules.Videos
{
    public interface IFolderEnumerator
    {
        List<VideoComponent> Enumerate();
    }
}