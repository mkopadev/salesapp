namespace SalesApp.Core.ViewModels
{
    /// <summary>
    /// Interface to help push device specific resources to the core
    /// </summary>
    public interface IDeviceResource
    {
        /// <summary>
        /// closed ticket icon
        /// </summary>
        int TicketClosedResourceId { get; }

        /// <summary>
        /// open ticket icon
        /// </summary>
        int TicketOpenResourceId { get; }

        /// <summary>
        /// Icon for videos module
        /// </summary>
        int VideosModuleIcon { get; }

        /// <summary>
        /// Icon for facts module
        /// </summary>
        int FactsModuleIcon { get; }

        /// <summary>
        /// Icon for calculator module
        /// </summary>
        int CalculatorModuleIcon { get; }

        /// <summary>
        /// Stats today = TODAY
        /// </summary>
        string StatsToday { get; }

        /// <summary>
        /// Stats this week = THIS WEEK
        /// </summary>
        string StatsThisWeek { get; }

        /// <summary>
        /// Stats this month = THIS MONTH
        /// </summary>
        string StatsThisMonth { get; }

        /// <summary>
        /// Stats rank = Rank
        /// </summary>
        string StatsRank { get; }

        /// <summary>
        /// Stats name header = NAME
        /// </summary>
        string StatsNameHeader { get; }

        /// <summary>
        /// Column sales = SALES
        /// </summary>
        string ColumnSales { get; }

        /// <summary>
        /// Column prospects  = PROSPECTS
        /// </summary>
        string ColumnProspects { get; }

        /// <summary>
        /// Column day = DAY
        /// </summary>
        string ColumnDay { get; }

        /// <summary>
        /// Column week = WEEK
        /// </summary>
        string ColumnWeek { get; }

        /// <summary>
        /// Column month = MONTH
        /// </summary>
        string ColumnMonth { get; }

        /// <summary>
        /// Stats Dsr = DSR
        /// </summary>
        string StatsDsr { get; }

        /// <summary>
        /// Stats Sales  = SALES
        /// </summary>
        string StatsSales { get; }

        int CustomerPhotoSyncFailedIcon { get; }

        int CustomerPhotoSyncSuccessIcon { get; }

        int CustomerPhotoSyncPendingIcon { get; }

        string PhotoDeleteConfirmationMessage { get; }

        string Cancel { get; }

        string DontCancel { get; }

        string PhoneNumberInvalidFormat { get; }

        string PhoneNumberInvalidCharacters { get; }

        string PhoneNumberEmpty { get; }

        string PhoneNumberTooShort { get; }

        string PhoneNumberTooLong { get; }

        string DsrNotFound { get; }

        string ManageStock { get; }

        string Home { get; }

        int CheckMarkIcon { get; }

        string TryAgain { get; }

        int ErrorNewIcon { get; }

        string PleaseWait { get; }

        string CheckInternetConnection { get; }

        string MoreThanMaxUnitsSelected { get; }

        string UnitsCouldNotBeAllocated { get; }

        string UnitsAllocatedKey { get; }

        string MaxUnitsAllowed { get; }

        string SelectUnitsBeingReturned { get; }

        string ReasonForReturn { get; }

        string UpdatingListPleaseWait { get; }

        string InitialGroupList { get; }
    }
}