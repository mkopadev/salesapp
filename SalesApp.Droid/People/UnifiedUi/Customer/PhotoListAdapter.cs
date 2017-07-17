using System;
using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.People;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database;
using SalesApp.Droid.Components.UIComponents.Image;
using SalesApp.Droid.UI.Utils;
using Exception = System.Exception;

namespace SalesApp.Droid.People.UnifiedUi.Customer
{
    public class PhotoListAdapter : BaseAdapter<CustomerPhoto>, View.IOnClickListener
    {
        private static readonly ILog Log = LogManager.Get(typeof(PhotoListAdapter));
        private Activity _context;
        private List<CustomerPhoto> _photos;
        private CustomerPhotoFragment _customerPhotoFragment;
        private bool _inWizard;

        // Initialize the dataset of the Adapter
        public PhotoListAdapter(bool inWizard, List<CustomerPhoto> photos, Activity context, CustomerPhotoFragment customerPhotoFragment)
        {
            this._photos = new List<CustomerPhoto>();

            foreach (var photo in photos)
            {
                this._photos.Add(photo);
            }

            this._customerPhotoFragment = customerPhotoFragment;
            this._context = context;
            this._inWizard = inWizard;
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        public override CustomerPhoto this[int position]
        {
            get { return _photos[position]; }
        }
        public override int Count
        {
            get { return _photos.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ViewHolder viewHolder;

            if (convertView == null)
            {
                convertView = this._context.LayoutInflater.Inflate(Resource.Layout.layout_registration_customer_photo_item, parent, false);

                viewHolder = new ViewHolder
                {
                    PhotoImageView = convertView.FindViewById<CircularImageView>(Resource.Id.imageView_photo),
                    PhotoMessage = convertView.FindViewById<TextView>(Resource.Id.textView_customer_photo_message),
                    DeleteImageView = convertView.FindViewById<ImageView>(Resource.Id.imageView_delete)
                };

                convertView.Tag = viewHolder;
            }
            else
            {
                viewHolder = (ViewHolder)convertView.Tag;
            }

            CustomerPhoto photo = _photos[position];
            Log.Verbose("Photo details filepath: " + photo.FilePath + " status: " + photo.PhotoStatus + " TypeOfPhoto: " + photo.TypeOfPhoto);

            int photoStatus;
            int defaultPhoto = Resource.Drawable.salesapp_logo;

            if (photo.TypeOfPhoto == PhotoType.Customer)
            {
                if (photo.PhotoStatus == PhotoSaveStatus.Successful)
                {
                    photoStatus = Resource.String.customer_photo_status_saved;
                }
                else
                {
                    photoStatus = Resource.String.customer_photo_status_failed;
                }
            }
            else
            {
                if (photo.PhotoStatus == PhotoSaveStatus.Successful)
                {
                    photoStatus = Resource.String.document_photo_status_saved;
                }
                else
                {
                    photoStatus = Resource.String.document_photo_status_failed;
                    defaultPhoto = Resource.Drawable.ic_doc_photo;
                }
            }

            viewHolder.PhotoMessage.Text = this._context.GetString(photoStatus);

            try
            {
                if (photo.FilePath != null)
                {
                    Bitmap photoBitMap = BitmapFactory.DecodeFile(photo.FilePath);
                    if (photoBitMap != null)
                    {
                        Log.Verbose("Bitmap created successfully :)");
                        viewHolder.PhotoImageView.SetImageBitmap(photoBitMap);
                    }
                    else
                    {
                        Log.Verbose("Oh crap! failed to create the bitmap");
                        //set a default image
                        viewHolder.PhotoImageView.SetImageResource(defaultPhoto);
                    }
                }
                else
                {
                    Log.Verbose("Oh crap! failed to create the bitmap");
                    //set a default image
                    viewHolder.PhotoImageView.SetImageResource(defaultPhoto);
                }
            }
            catch (OutOfMemoryError e)
            {
                GC.Collect();
                Log.Verbose("Oops, Got an OutOfMemoryError when loading image");
                Log.Error(e);

                //set a default image
                viewHolder.PhotoImageView.SetImageResource(defaultPhoto);
            }
            catch (Exception e)
            {
                Log.Verbose("Exception " + e.Message);
                //set a default image
                viewHolder.PhotoImageView.SetImageResource(defaultPhoto);
            }

            viewHolder.DeleteImageView.Tag = position;
            viewHolder.DeleteImageView.SetOnClickListener(this);

            return convertView;
        }

        public void OnClick(View view)
        {
            var position = (int)view.Tag;

            CustomerPhoto photo = this._photos[position];

            int positiveButtonText = this._inWizard ? Resource.String.yes : Resource.String.delete_photo;
            int negativeButtonText = this._inWizard ? Resource.String.no : Resource.String.keep_photo;

            AlertDialogBuilder.Instance
                .AddButton(positiveButtonText, () => this.DeletePhoto(photo))
                .AddButton(negativeButtonText, () => { })
                .SetText(0, Resource.String.delete_photo_msg)
                .Show(this._context, false, true);
        }

        private async void DeletePhoto(CustomerPhoto photo)
        {
            File file = new File(photo.FilePath);

            if (file.Exists())
            {
                file.Delete();
            }

            await DataAccess.Instance.Connection.DeleteAsync(photo);
            _photos.Remove(photo);
            _customerPhotoFragment.RemovePhoto(photo);
            this.NotifyDataSetChanged();

            _customerPhotoFragment.RefreshList();
        }

        public class ViewHolder : Java.Lang.Object
        {
            public CircularImageView PhotoImageView { get; set; }

            public TextView PhotoMessage { get; set; }

            public ImageView DeleteImageView { get; set; }
        }

        public void AddItem(CustomerPhoto newPhoto)
        {
            this._photos.Add(newPhoto);
            this.NotifyDataSetChanged();
        }
    }
}