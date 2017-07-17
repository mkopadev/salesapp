using System.IO;
using Android.Content;
using Android.Content.Res;
using SalesApp.Core.Services.Settings;
using SalesApp.Core.ViewModels;

namespace SalesApp.Droid.UI
{
    /// <summary>
    /// Android specific implementation of <see cref="IDeviceResource" class. />
    /// </summary>
    public class AndroidDeviceResource : IDeviceResource
    {
        private Context _context;

        public AndroidDeviceResource(Context context)
        {
            this._context = context;
        }

        public int TicketClosedResourceId
        {
            get
            {
                return Resource.Drawable.ticket_closed;
            }
        }

        public int TicketOpenResourceId
        {
            get
            {
                return Resource.Drawable.ticket_open;
            }
        }

        public int VideosModuleIcon
        {
            get
            {
                return Resource.Drawable.icon_videos_green;
            }
        }

        public int FactsModuleIcon
        {
            get
            {
                return Resource.Drawable.icon_facts_green;
            }
        }

        public int CalculatorModuleIcon
        {
            get
            {
                return Resource.Drawable.icon_calc_green;
            }
        }

        public string StatsToday
        {
            get
            {
                return this._context.GetString(Resource.String.stats_today);
            }
        }

        public string StatsThisWeek
        {
            get
            {
                return this._context.GetString(Resource.String.stats_this_week);
            }
        }

        public string StatsThisMonth
        {
            get
            {
                return this._context.GetString(Resource.String.stats_this_month);
            }
        }

        /// <summary>
        /// Stats name header = Rank
        /// </summary>
        public string StatsRank
        {
            get
            {
                return this._context.GetString(Resource.String.stats_rank);
            }
        }

        /// <summary>
        /// Stats name header = NAME
        /// </summary>
        public string StatsNameHeader
        {
            get
            {
                return this._context.GetString(Resource.String.stats_name_header);
            }
        }

        /// <summary>
        /// Column sales = SALES
        /// </summary>
        public string ColumnSales
        {
            get
            {
                return this._context.GetString(Resource.String.column_sales);
            }
        }

        /// <summary>
        /// Column prospects  = PROSPECTS
        /// </summary>
        public string ColumnProspects
        {
            get
            {
                return this._context.GetString(Resource.String.column_prospects);
            }
        }

        /// <summary>
        /// Column day = DAY
        /// </summary>
        public string ColumnDay
        {
            get
            {
                return this._context.GetString(Resource.String.column_day);
            }
        }

        /// <summary>
        /// Column week = WEEK
        /// </summary>
        public string ColumnWeek
        {
            get
            {
                return this._context.GetString(Resource.String.column_week);
            }
        }

        /// <summary>
        /// Column month = MONTH
        /// </summary>
        public string ColumnMonth
        {
            get
            {
                return this._context.GetString(Resource.String.column_month);
            }
        }

        /// <summary>
        /// Stats Dsr = DSR
        /// </summary>
        public string StatsDsr
        {
            get
            {
                return this._context.GetString(Resource.String.stats_dsr);
            }
        }

        /// <summary>
        /// Stats Sales  = SALES
        /// </summary>
        public string StatsSales
        {
            get
            {
                return this._context.GetString(Resource.String.stats_sales);
            }
        }

        public int CustomerPhotoSyncFailedIcon
        {
            get
            {
                return Resource.Drawable.customer_photo_sync_error;
            }
        }

        public int CustomerPhotoSyncSuccessIcon
        {
            get
            {
                return Resource.Drawable.customer_photo_sync_success;
            }
        }

        public int CustomerPhotoSyncPendingIcon
        {
            get
            {
                return Resource.Drawable.customer_photo_sync_pending;
            }
        }

        public string PhotoDeleteConfirmationMessage
        {
            get
            {
                return this._context.GetString(Resource.String.cancel_and_delete);
            }
        }

        public string Cancel
        {
            get
            {
                return this._context.GetString(Resource.String.cancel);
            }
        }

        public string DontCancel
        {
            get
            {
                return this._context.GetString(Resource.String.dont_cancel);
            }
        }

        public string PhoneNumberInvalidFormat
        {
            get
            {
                return this._context.GetString(Resource.String.unified_phone_validation_invalid_format);
            }
        }

        public string PhoneNumberInvalidCharacters
        {
            get
            {
                return this._context.GetString(Resource.String.unified_phone_validation_bad_chars);
            }
        }

        public string PhoneNumberEmpty
        {
            get
            {
                return this._context.GetString(Resource.String.unified_phone_validation_null);
            }
        }

        public string PhoneNumberTooShort
        {
            get
            {
                return this._context.GetString(Resource.String.unified_phone_validation_short);
            }
        }

        public string PhoneNumberTooLong
        {
            get
            {
                return this._context.GetString(Resource.String.unified_phone_validation_long);
            }
        }

        public string DsrNotFound
        {
            get
            {
                return this._context.GetString(Resource.String.stock_dsr_not_found);
            }
        }

        public string ManageStock
        {
            get
            {
                return this._context.GetString(Resource.String.manage_stock);
            }
        }

        public string Home
        {
            get
            {
                return this._context.GetString(Resource.String.home);
            }
        }

        public int CheckMarkIcon
        {
            get
            {
                return Resource.Drawable.checkmark;
            }
        }

        public string TryAgain
        {
            get
            {
                return this._context.GetString(Resource.String.try_again);
            }
        }

        public int ErrorNewIcon
        {
            get
            {
                return Resource.Drawable.errornew;
            }
        }

        public string PleaseWait
        {
            get
            {
                return this._context.GetString(Resource.String.please_wait);
            }
        }

        public string CheckInternetConnection
        {
            get
            {
                return this._context.GetString(Resource.String.check_internet_connection);
            }
        }

        public string MoreThanMaxUnitsSelected
        {
            get
            {
                return this._context.GetString(Resource.String.more_than_max_units_selected);
            }
        }

        public string UnitsCouldNotBeAllocated
        {
            get
            {
                return this._context.GetString(Resource.String.units_could_no_be_allocated);
            }
        }
        
        public string UnitsAllocatedKey
        {
            get
            {
                return this._context.GetString(Resource.String.units_allocated_key);
            }
        }
        
        public string MaxUnitsAllowed
        {
            get
            {
                return this._context.GetString(Resource.String.max_units_allowed);
            }
        }

        public string SelectUnitsBeingReturned
        {
            get
            {
                return this._context.GetString(Resource.String.select_units_being_returned);
            }
        }

        public string ReasonForReturn
        {
            get
            {
                return this._context.GetString(Resource.String.reason_for_return);
            }
        }

        public string UpdatingListPleaseWait
        {
            get
            {
                return this._context.GetString(Resource.String.updating_list_please_wait);
            }
        }

        public string InitialGroupList
        {
            get
            {
                string content;
                AssetManager assets = _context.Assets;

                string filePath = string.Format("Groups/{0}/groups.json", Settings.Instance.DsrCountryCode);

                using (StreamReader sr = new StreamReader(assets.Open(filePath)))
                {
                    content = sr.ReadToEnd();
                }

                return content;
            }
        }
    }
}