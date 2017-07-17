using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Fragment = Android.Support.V4.App.Fragment;

using ImageViews.Rounded;

namespace RoundedImageViewSample
{
    public class PicassoFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.Rounded, container, false);

            PicassoAdapter adapter = new PicassoAdapter(Activity);
            view.FindViewById<ListView>(Resource.Id.main_list).Adapter = adapter;

            adapter.Add(new PicassoItem("http://24.media.tumblr.com/2176464a507f8a34f09d58ee7fcf105a/tumblr_mzgzd79XMY1st5lhmo1_1280.jpg", ImageView.ScaleType.Center));
            adapter.Add(new PicassoItem("http://24.media.tumblr.com/af50758346e388e6e69f4c378c4f264f/tumblr_mzgzcdEDTL1st5lhmo1_1280.jpg", ImageView.ScaleType.CenterCrop));
            adapter.Add(new PicassoItem("http://24.media.tumblr.com/5f97f94756bf706bf41ac0dd37b585cf/tumblr_mzgzbdYBht1st5lhmo1_1280.jpg", ImageView.ScaleType.CenterInside));
            adapter.Add(new PicassoItem("http://24.media.tumblr.com/6ddffd6a6036f61a1f2b1744bad77730/tumblr_mzgz9vJ1CK1st5lhmo1_1280.jpg", ImageView.ScaleType.FitCenter));
            adapter.Add(new PicassoItem("http://24.media.tumblr.com/2176464a507f8a34f09d58ee7fcf105a/tumblr_mzgzd79XMY1st5lhmo1_1280.jpg", ImageView.ScaleType.FitEnd));
            adapter.Add(new PicassoItem("http://24.media.tumblr.com/af50758346e388e6e69f4c378c4f264f/tumblr_mzgzcdEDTL1st5lhmo1_1280.jpg", ImageView.ScaleType.FitStart));
            adapter.Add(new PicassoItem("http://24.media.tumblr.com/5f97f94756bf706bf41ac0dd37b585cf/tumblr_mzgzbdYBht1st5lhmo1_1280.jpg", ImageView.ScaleType.FitXy));

            return view;
        }

        public class PicassoItem
        {
            public string mUrl { get; set; }

            public ImageView.ScaleType mScaleType { get; set; }

            public PicassoItem(string url, ImageView.ScaleType scaleType)
            {
                mUrl = url;
                mScaleType = scaleType;
            }
        }

        public class PicassoAdapter : ArrayAdapter<PicassoItem>
        {
            private LayoutInflater mInflater;
            private ITransformation mTransformation;

            public PicassoAdapter(Context context)
                : base(context, 0)
            {
                mInflater = LayoutInflater.From(Context);
                mTransformation = new RoundedTransformationBuilder()
                    .CornerRadiusDp(30)
                    .BorderColor(Color.Black)
                    .BorderWidthDp(3)
                    .Oval(false)
                    .Build();
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                ViewGroup view;
                if (convertView == null)
                {
                    view = (ViewGroup)mInflater.Inflate(Resource.Layout.PicassoItem, parent, false);
                }
                else
                {
                    view = (ViewGroup)convertView;
                }

                PicassoItem item = GetItem(position);

                ImageView imageView = view.FindViewById<ImageView>(Resource.Id.imageView1);
                imageView.SetScaleType(item.mScaleType);

                Picasso.With(Context)
                    .Load(item.mUrl)
                    .Fit()
                    .Transform(mTransformation)
                    .Into(imageView);

                view.FindViewById<TextView>(Resource.Id.textView3).Text = item.mScaleType.ToString();
                return view;
            }
        }
    }
}
