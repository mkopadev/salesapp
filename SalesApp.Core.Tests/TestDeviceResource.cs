using SalesApp.Core.ViewModels;

namespace SalesApp.Core.Tests
{
    public class TestDeviceResource : IDeviceResource
    {
        public int TicketClosedResourceId
        {
            get
            {
                return 0;
            }
        }

        public int TicketOpenResourceId
        {
            get
            {
                return 0;
            }
        }

        public int VideosModuleIcon
        {
            get
            {
                return 0;
            }
        }

        public int FactsModuleIcon
        {
            get { return 0; }
        }

        public int CalculatorModuleIcon
        {
            get { return 0; }
        }

        public string StatsToday
        {
            get { return string.Empty; }
        }

        public string StatsThisWeek
        {
            get { return string.Empty; }
        }

        public string StatsThisMonth
        {
            get { return string.Empty; }
        }

        public string StatsRank
        {
            get { return string.Empty; }
        }

        public string StatsNameHeader
        {
            get { return string.Empty; }
        }

        public string ColumnSales
        {
            get { return string.Empty; }
        }

        public string ColumnProspects
        {
            get { return string.Empty; }
        }

        public string ColumnDay
        {
            get { return string.Empty; }
        }

        public string ColumnWeek
        {
            get { return string.Empty; }
        }

        public string ColumnMonth
        {
            get { return string.Empty; }
        }

        public string StatsDsr
        {
            get { return string.Empty; }
        }

        public string StatsSales
        {
            get { return string.Empty; }
        }

        public int CustomerPhotoSyncFailedIcon
        {
            get { return 0; }
        }

        public int CustomerPhotoSyncSuccessIcon
        {
            get { return 0; }
        }

        public int CustomerPhotoSyncPendingIcon
        {
            get { return 0; }
        }

        public string PhotoDeleteConfirmationMessage
        {
            get { return string.Empty; }
        }

        public string Cancel
        {
            get
            {
                return "Cancel";
            }
        }

        public string DontCancel
        {
            get { return string.Empty; }
        }

        public string PhoneNumberInvalidFormat
        {
            get
            {
                return "The entered number\'s format is incorrect.";
            }
        }

        public string PhoneNumberInvalidCharacters
        {
            get
            {
                return "Phone number can only be numeric.";
            }
        }

        public string PhoneNumberEmpty
        {
            get
            {
                return "Please enter a phone number";
            }
        }

        public string PhoneNumberTooShort
        {
            get
            {
                return "Phone number entered was too short.";
            }
        }

        public string PhoneNumberTooLong
        {
            get
            {
                return "Phone number entered was too long.";
            }
        }

        public string DsrNotFound
        {
            get
            {
                return "DSR could not be found, please re-enter telephone no";
            }
        }

        public string ManageStock
        {
            get
            {
                return "Manage Stock";
            }
        }

        public string Home
        {
            get
            {
                return "Home";
            }
        }

        public string TryAgain
        {
            get
            {
                return "Try Again";
            }
        }

        public int ErrorNewIcon
        {
            get
            {
                return 2130837664;
            }
        }

        public int CheckMarkIcon
        {
            get
            {
                return 2130837629;
            }
        }

        public string PleaseWait
        {
            get { return string.Empty; }
        }

        public string CheckInternetConnection
        {
            get { return string.Empty; }
        }

        public string MoreThanMaxUnitsSelected
        {
            get
            {
                return "No more than {0} units can be selected for this DSR";
            }
        }

        public string UnitsCouldNotBeAllocated
        {
            get
            {
                return string.Empty;
            }
        }

        public string UnitsAllocatedKey
        {
            get
            {
                return string.Empty;
            }
        }

        public string MaxUnitsAllowed
        {
            get
            {
                return string.Empty;
            }
        }

        public string SelectUnitsBeingReturned
        {
            get
            {
                return "Please select the units that are being returned by tapping on the serial no.";
            }
        }

        public string ReasonForReturn
        {
            get
            {
                return string.Empty;
            }
        }

        public string UpdatingListPleaseWait
        {
            get
            {
                return string.Empty;
            }
        }

        public string InitialGroupList
        {
            get
            {
                return string.Empty;
            }
        }
    }
}