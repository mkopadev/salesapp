using Android.Content;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;
using SalesApp.Core.BL.Models.Modules.Videos;
using SalesApp.Core.Enums.Modules.Videos;

namespace SalesApp.Droid.Views.Modules.Videos
{
    public class VideoComponentAdapter : MvxAdapter
    {
        public VideoComponentAdapter(Context context, IMvxAndroidBindingContext bindingContext) : base(context, bindingContext)
        {
        }

        protected override View GetBindableView(View convertView, object source, int templateId)
        {
            VideoComponent item = source as VideoComponent;

            if (item == null)
            {
                return null;
            }

            if (item.NodeType == NodeType.NonLeafNode)
            {
                templateId = Resource.Layout.video_category_list_item;
            }
            else if(item.NodeType == NodeType.LeafNode)
            {
                templateId = Resource.Layout.video_list_item;
            }

            return base.GetBindableView(convertView, source, templateId);
        }
    }
}