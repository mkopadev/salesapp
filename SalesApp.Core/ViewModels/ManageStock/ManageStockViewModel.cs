using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using Newtonsoft.Json;
using SalesApp.Core.Api.ManageStock;
using SalesApp.Core.BL;
using SalesApp.Core.BL.Models.ManageStock;
using SalesApp.Core.Enums.ManageStock;
using SalesApp.Core.Enums.Validation;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.ManageStock;
using SalesApp.Core.Services.Product;
using SalesApp.Core.Services.Settings;
using SalesApp.Core.Validation;
using SalesApp.Core.ViewModels.Home;

namespace SalesApp.Core.ViewModels.ManageStock
{
    public class ManageStockViewModel : BaseViewModel
    {
        private IDeviceResource _deviceResource = Resolver.Instance.Get<IDeviceResource>();
        private IConnectivityService _connectivityService = Resolver.Instance.Get<IConnectivityService>();

        private DsrStockServerResponseObject _dsrStock;
        private PeopleDetailsValidater _validater;
        private ScmStock _selectedProduct;

        private ObservableCollection<DeviceAllocationItem> _dsrStockUnits;
        private ObservableCollection<DeviceAllocationItem> _selectedUnits;
        private ObservableCollection<DeviceAllocationItem> _selectedProductUnits;
        private ObservableCollection<KeyValue> _dsrDetails;
        private ObservableCollection<ScmStock> _scmUnitsStock;

        private string _selectUnitsBeingReturned;
        private string _phoneNumberErrorMessage;
        private string _progressDialogMessage;
        private string _positiveButtonText;
        private string _negativeButtonText;
        private string _apiStatusMessage;
        private string _apiStatusDescription;
        private string _dsrPhoneNumber;
        private string _nextButtonText;
        private string _errorMessage;

        private int _statusIcon;
        private int _retryCount;

        private bool _dsrHasUnits;
        private bool _scmHasStock = true;
        private bool _nextButtonEnabled = true;
        private bool _showGreyDivider = true;
        private bool _showNoInternetAlert;
        private bool _isBusy;

        private bool _nextButtonVisible;
        private bool _previousButtonVisible;
        private bool _apiSuccess;

        public bool CanProcessSelectedItems { get; set; }

        private MvxCommand<DeviceAllocationItem> _onItemSelected;
        private MvxCommand _positiveButtonCommand;
        private MvxCommand _negativeButtonCommand;

        private ManageStockAction _stockAction;

        // Receive stock
        private ObservableCollection<ReasonForReturning> _reasons;
        private ReasonForReturning _selectedReason;
        private string _selectMoreOrReasonTitle;
        private string _previousPhoneNumber;

        public bool ShowNoInternetAlert
        {
            get
            {
                return this._showNoInternetAlert;
            }

            set
            {
                this.SetProperty(ref this._showNoInternetAlert, value, () => this.ShowNoInternetAlert);
            }
        }

        public bool ShowGreyDivider
        {
            get
            {
                return this._showGreyDivider;
            }

            set
            {
                this.SetProperty(ref this._showGreyDivider, value, () => this.ShowGreyDivider);
            }
        }

        public bool IsBusy
        {
            get
            {
                return this._isBusy;
            }

            set
            {
                this.SetProperty(ref this._isBusy, value, () => this.IsBusy);
            }
        }

        public string NextButtonText
        {
            get
            {
                return this._nextButtonText;
            }

            set
            {
                this.SetProperty(ref this._nextButtonText, value, () => this.NextButtonText);
            }
        }

        public bool NextButtonVisible
        {
            get
            {
                return this._nextButtonVisible;
            }

            set
            {
                this.SetProperty(ref this._nextButtonVisible, value, () => this.NextButtonVisible);
            }
        }

        public bool PreviousButtonVisible
        {
            get
            {
                return this._previousButtonVisible;
            }

            set
            {
                this.SetProperty(ref this._previousButtonVisible, value, () => this.PreviousButtonVisible);
            }
        }

        public bool ApiSuccess
        {
            get
            {
                return this._apiSuccess;
            }

            set
            {
                this.SetProperty(ref this._apiSuccess, value, () => this.ApiSuccess);
            }
        }

        public ObservableCollection<DeviceAllocationItem> DsrStockUnits
        {
            get
            {
                return this._dsrStockUnits;
            }

            set
            {
                if (value == null || value.Count == 0)
                {
                    this.DsrHasUnits = false;
                    return;
                }

                this.DsrHasUnits = true;
                this.SetProperty(ref this._dsrStockUnits, value, () => this.DsrStockUnits);
            }
        }

        public ObservableCollection<KeyValue> DsrDetails
        {
            get
            {
                return this._dsrDetails;
            }

            set
            {
                this.SetProperty(ref this._dsrDetails, value, () => this.DsrDetails);
            }
        }

        public bool DsrHasUnits
        {
            get
            {
                return this._dsrHasUnits;
            }

            set
            {
                this.SetProperty(ref this._dsrHasUnits, value, () => this.DsrHasUnits);
            }
        }

        public bool ScmHasStock
        {
            get
            {
                return this._scmHasStock;
            }

            set
            {
                this.SetProperty(ref this._scmHasStock, value, () => this.ScmHasStock);
            }
        }

        public void SectionizeSelectedUnitsList()
        {
            List<DeviceAllocationItem> devices = new List<DeviceAllocationItem>();
            List<List<DeviceAllocationItem>> groupedSelectedUnitList = this.SelectedUnits.GroupBy(u => u.ProductTypeId).Select(grp => grp.ToList()).ToList();

            foreach (List<DeviceAllocationItem> selectedUnits in groupedSelectedUnitList)
            {
                int i = 0;
                foreach (var item in selectedUnits)
                {
                    DeviceAllocationItem serials = new DeviceAllocationItem();
                    serials.ProductTypeId = item.ProductTypeId;
                    serials.Name = item.Name;

                    if (i == 0)
                    {
                        if (this.StockAction == ManageStockAction.Issue)
                        {
                            serials.HeaderText = this.SelectedProduct.Name;
                        }
                        else
                        {
                            serials.HeaderText = item.Name;
                        }
                    }

                    serials.SerialNumber = item.SerialNumber;
                    devices.Add(serials);

                    i++;
                }
            }

            this.SelectedUnits = new ObservableCollection<DeviceAllocationItem>(devices);
        }

        public void SectionizeDsrAllocationList(List<DsrProductAllocationItem> products)
        {
            if (products == null || products.Count == 0)
            {
                return;
            }

            List<DeviceAllocationItem> devices = new List<DeviceAllocationItem>();

            for (int i = 0; i < products.Count; i++)
            {
                var product = products[i];
                var units = product.Units;

                for (int j = 0; j < units.Count; j++)
                {
                    var device = units[j];
                    device.ProductTypeId = product.ProductTypeId;
                    device.Name = product.ProductName;

                    if (j == 0)
                    {
                        device.HeaderText = product.ProductName;
                    }

                    if (j == units.Count - 1)
                    {
                        device.IsFooterItem = true;
                    }

                    // choose the screen to display on
                    if (this.StockAction == ManageStockAction.Receive)
                    {
                        device.IsSelectable = true;
                    }
                    else
                    {
                        device.IsSelectable = false;
                    }

                    devices.Add(device);
                }
            }

            this.DsrStockUnits = new ObservableCollection<DeviceAllocationItem>(devices);

            if (this.StockAction == ManageStockAction.Receive)
            {
                this.SelectUnitsBeingReturned = this._deviceResource.SelectUnitsBeingReturned;
            }
        }

        public ManageStockViewModel()
        {
            this._validater = new PeopleDetailsValidater();
        }

        public RemoteProductService RemoteProductService { get; set; }

        public DsrStockAllocationService DsrStockAllocationService { get; set; }

        public int StatusIcon
        {
            get
            {
                return this._statusIcon;
            }

            set
            {
                this.SetProperty(ref this._statusIcon, value, () => this.StatusIcon);
            }
        }

        public DsrStockServerResponseObject DsrStock
        {
            get
            {
                return this._dsrStock;
            }

            set
            {
                this.SetProperty(ref this._dsrStock, value, () => this.DsrStock);
            }
        }

        public string ErrorMessage
        {
            get
            {
                return this._errorMessage;
            }

            set
            {
                this.SetProperty(ref this._errorMessage, value, () => this.ErrorMessage);
            }
        }

        public string SelectUnitsBeingReturned
        {
            get
            {
                return this._selectUnitsBeingReturned;
            }

            set
            {
                this.SetProperty(ref this._selectUnitsBeingReturned, value, () => this.SelectUnitsBeingReturned);
            }
        }

        public string PhoneNumberErrorMessage
        {
            get
            {
                return this._phoneNumberErrorMessage;
            }

            set
            {
                this.SetProperty(ref this._phoneNumberErrorMessage, value, () => this.PhoneNumberErrorMessage);
            }
        }

        public string ProgressDialogMessage
        {
            get
            {
                return this._progressDialogMessage;
            }

            set
            {
                this.SetProperty(ref this._progressDialogMessage, value, () => this.ProgressDialogMessage);
            }
        }

        public string DsrPhoneNumber
        {
            get
            {
                return this._dsrPhoneNumber;
            }

            set
            {
                this.SetProperty(ref this._dsrPhoneNumber, value, () => this.DsrPhoneNumber);
            }
        }

        private bool PhoneNumberChanged
        {
            get
            {
                return this.DsrPhoneNumber != this._previousPhoneNumber;
            }
        }

        private bool ValidatePhoneNumber()
        {
            var result = this._validater.ValidatePhoneNumber(this.DsrPhoneNumber);

            switch (result)
            {
                case PhoneValidationResultEnum.NullEntry:
                    this.PhoneNumberErrorMessage = this._deviceResource.PhoneNumberEmpty;
                    return false;
                case PhoneValidationResultEnum.InvalidFormat:
                    this.PhoneNumberErrorMessage = this._deviceResource.PhoneNumberInvalidFormat;
                    return false;
                case PhoneValidationResultEnum.InvalidCharacters:
                    this.PhoneNumberErrorMessage = this._deviceResource.PhoneNumberInvalidCharacters;
                    return false;
                case PhoneValidationResultEnum.NumberTooLong:
                    this.PhoneNumberErrorMessage = this._deviceResource.PhoneNumberTooLong;
                    return false;
                case PhoneValidationResultEnum.NumberTooShort:
                    this.PhoneNumberErrorMessage = this._deviceResource.PhoneNumberTooShort;
                    return false;
                case PhoneValidationResultEnum.NumberOk:
                    this.PhoneNumberErrorMessage = null;
                    return true;
                default:
                    this.PhoneNumberErrorMessage = "Unkown Error in phone number.";
                    return false;
            }
        }

        public async Task FindDsrAsync()
        {
            if (!this._connectivityService.HasConnection())
            {
                return;
            }

            bool isValid = this.ValidatePhoneNumber();

            if (!isValid)
            {
                return;
            }

            this.NextButtonEnabled = false;
            this.IsBusy = true;

            string queryParams = string.Format("?dsrPhone={0}", this.DsrPhoneNumber);

            try
            {
                var response = await this.DsrStockAllocationService.GetCurrentDsrStock(queryParams);

                if (response == null || response.RequestException != null)
                {
                    return;
                }

                var result = response.GetObject();

                if (result == null)
                {
                    if (this._connectivityService.HasConnection())
                    {
                        this.PhoneNumberErrorMessage = this._deviceResource.DsrNotFound;
                    }

                    return;
                }

                if (!result.Status.Status)
                {
                    this.PhoneNumberErrorMessage = result.Status.Message;
                    return;
                }

                this.DsrStock = result;

                this.DsrDetails = new ObservableCollection<KeyValue>(result.DsrDetails);
                this.SectionizeDsrAllocationList(result.Products);

                if (this.StockAction == ManageStockAction.Receive)
                {
                    this.AddKeyToDsrDetails(this._deviceResource.UnitsAllocatedKey, this.DsrStock.UnitsAllocated.ToString());
                    this.RemoveKeyFromDsrDetails(this._deviceResource.MaxUnitsAllowed);
                }
            }
            finally
            {
                this.NextButtonEnabled = true;
                this.IsBusy = false;
            }
        }

        public ObservableCollection<ScmStock> ScmUnitsInStock
        {
            get
            {
                if (this._scmUnitsStock == null)
                {
                    this._scmUnitsStock = new ObservableCollection<ScmStock>();
                }

                return this._scmUnitsStock;
            }

            set
            {
                if (value == null || value.Count == 0)
                {
                    this.ScmHasStock = false;
                    return;
                }

                this.ScmHasStock = true;

                this.SetProperty(ref this._scmUnitsStock, value, () => this.ScmUnitsInStock);
            }
        }

        public async Task FetchCurrentScmStock()
        {
            if (this.PhoneNumberChanged)
            {
                this.ScmUnitsInStock.Clear();
            }

            if (this.ScmUnitsInStock.Count > 0)
            {
                return;
            }

            this.NextButtonEnabled = false;
            this.IsBusy = true;
            this.ScmHasStock = true; // though we dont know yet

            try
            {
                this._previousPhoneNumber = this.DsrPhoneNumber;
                string urlParams = string.Format("?dsrPersonId={0}&productTypeId={1}", this.DsrStock.PersonId, string.Empty);

                List<Product> productsListResponse = await this.RemoteProductService.GetProducts(urlParams);

                List<List<Product>> groupedProductList = productsListResponse.GroupBy(u => u.ProductTypeId).Select(grp => grp.ToList()).ToList();

                ObservableCollection<ScmStock> productList = new ObservableCollection<ScmStock>();

                foreach (List<Product> products in groupedProductList)
                {
                    ScmStock scmStock = new ScmStock();
                    List<string> units = new List<string>();
                    foreach (var product in products)
                    {
                        scmStock.ProductTypeId = product.ProductTypeId;
                        scmStock.Name = product.ProductName;
                        units.Add(product.SerialNumber);
                    }

                    scmStock.SerialNumbers = units;
                    productList.Add(scmStock);
                }

                this.ScmUnitsInStock = productList;
            }
            finally
            {
                this.NextButtonEnabled = true;
                this.IsBusy = false;
            }
        }

        public void UpdateSelectedUnitsCountStatus()
        {
            int count = this.SelectedUnits.Count + this.DsrStock.UnitsAllocated;
            bool notAllowed = count > this.DsrStock.MaxAllowedUnits;
            if (notAllowed)
            {
                string message = string.Format(this._deviceResource.MoreThanMaxUnitsSelected, this.DsrStock.MaxAllowedUnits - this.DsrStock.UnitsAllocated);

                this.ErrorMessage = message;
                this.NextButtonEnabled = false;
            }
            else
            {
                this.ErrorMessage = null;
                this.NextButtonEnabled = true;

                if (this.SelectedUnits.Count == 0)
                {
                    this.NextButtonEnabled = false;
                }
            }
        }

        public bool NextButtonEnabled
        {
            get
            {
                return this._nextButtonEnabled;
            }

            set
            {
                this.SetProperty(ref this._nextButtonEnabled, value, () => this.NextButtonEnabled);
            }
        }

        public ObservableCollection<DeviceAllocationItem> SelectedProductUnits
        {
            get
            {
                if (this._selectedProductUnits == null)
                {
                    this._selectedProductUnits = new ObservableCollection<DeviceAllocationItem>();
                }

                return this._selectedProductUnits;
            }

            set
            {
                this.SetProperty(ref this._selectedProductUnits, value, () => this.SelectedProductUnits);
            }
        }

        public ScmStock SelectedProduct
        {
            get
            {
                return this._selectedProduct;
            }

            set
            {
                this.SetProperty(ref this._selectedProduct, value, () => this.SelectedProduct);

                if (value == null)
                {
                    return;
                }

                ObservableCollection<DeviceAllocationItem> localUnits = new ObservableCollection<DeviceAllocationItem>();
                foreach (var unit in value.SerialNumbers)
                {
                    localUnits.Add(new DeviceAllocationItem { SerialNumber = unit });
                }

                this.SelectedProductUnits = localUnits;
            }
        }

        public ObservableCollection<DeviceAllocationItem> SelectedUnits
        {
            get
            {
                if (this._selectedUnits == null)
                {
                    this._selectedUnits = new ObservableCollection<DeviceAllocationItem>();
                }

                return this._selectedUnits;
            }

            set
            {
                this.SetProperty(ref this._selectedUnits, value, () => this.SelectedUnits);
            }
        }

        public async Task<ManageStockPostApiResponse> AllocateSelectedUnits()
        {
            this.ProgressDialogMessage = this._deviceResource.PleaseWait;
            this.IsBusy = true;

            SelectedProduct product = new SelectedProduct { DsrPhone = this.DsrPhoneNumber, PersonId = this.DsrStock.PersonId, PersonRoleId = this.DsrStock.PersonRoleId };

            List<string> units = new List<string>();
            foreach (var unit in this.SelectedUnits)
            {
                units.Add(unit.SerialNumber);
            }

            ScmStock scmStock = new ScmStock { ProductTypeId = this.SelectedProduct.ProductTypeId, SerialNumbers = units };

            product.Units = new List<ScmStock> { scmStock };

            ManageStockPostApiResponse response = await this.DsrStockAllocationService.AllocateUnitsToDsr(product);

            this.IsBusy = false;

            if (response == null)
            {
                return new ManageStockPostApiResponse { Success = false, Text = this._deviceResource.UnitsCouldNotBeAllocated };
            }

            return response;
        }

        public string ApiStatusMessage
        {
            get
            {
                return this._apiStatusMessage;
            }

            set
            {
                this.SetProperty(ref this._apiStatusMessage, value, () => this.ApiStatusMessage);
            }
        }

        public string ApiStatusDescription
        {
            get
            {
                return this._apiStatusDescription;
            }

            set
            {
                this.SetProperty(ref this._apiStatusDescription, value, () => this.ApiStatusDescription);
            }
        }

        public string NegativeButtonText
        {
            get
            {
                return this._negativeButtonText;
            }

            set
            {
                this.SetProperty(ref this._negativeButtonText, value, () => this.NegativeButtonText);
            }
        }

        public string PositiveButtonText
        {
            get
            {
                return this._positiveButtonText;
            }

            set
            {
                this.SetProperty(ref this._positiveButtonText, value, () => this.PositiveButtonText);
            }
        }

        public void ProcessApiResponse(ManageStockPostApiResponse response)
        {
            this.ApiStatusMessage = response.Text;
            this.ApiSuccess = response.Success;

            if (this.ApiSuccess || this._retryCount >= 3)
            {
                int statusIcon = response.Success ? this._deviceResource.CheckMarkIcon : this._deviceResource.ErrorNewIcon;

                this.PositiveButtonText = this._deviceResource.ManageStock;
                this.ApiStatusDescription = string.Empty;
                this.StatusIcon = statusIcon;
            }
            else
            {
                this.PositiveButtonText = this._deviceResource.TryAgain;
                this.StatusIcon = this._deviceResource.ErrorNewIcon;
            }

            this.ApiStatusDescription = !this._connectivityService.HasConnection() ? this._deviceResource.CheckInternetConnection : string.Empty;
        }

        [JsonIgnore]
        public MvxAsyncCommand FindDsrCommand
        {
            get
            {
                return new MvxAsyncCommand(this.FindDsrAsync);
            }
        }

        [JsonIgnore]
        public ICommand PositiveButtonCommand
        {
            get
            {
                this._positiveButtonCommand = this._positiveButtonCommand ?? new MvxCommand(async () =>
                {
                    if (this.ApiSuccess || this._retryCount >= 3)
                    {
                        this.ShowViewModel<ManageStockViewModel>();
                    }
                    else
                    {
                        this._retryCount++;

                        ManageStockPostApiResponse response;

                        if (this.StockAction == ManageStockAction.Issue)
                        {
                            response = await this.AllocateSelectedUnits();
                        }
                        else
                        {
                            response = await this.ReceiveStock();
                        }

                        this.ProcessApiResponse(response);
                    }
                });

                return this._positiveButtonCommand;
            }
        }

        [JsonIgnore]
        public ICommand NegativeButtonCommand
        {
            get
            {
                this._negativeButtonCommand = this._negativeButtonCommand ?? new MvxCommand(() =>
                {
                        this.ShowViewModel<HomeViewModel>();
                });

                return this._negativeButtonCommand;
            }
        }

        [JsonIgnore]
        public ICommand OnItemSelected
        {
            get
            {
                this._onItemSelected = this._onItemSelected ?? new MvxCommand<DeviceAllocationItem>(this.ProcessSelectedItem);

                return this._onItemSelected;
            }
        }

        private void ProcessSelectedItem(DeviceAllocationItem selectedItem)
        {
            if (!this.CanProcessSelectedItems)
            {
                return;
            }

            if (this.SelectedUnits.Contains(selectedItem))
            {
                // item is in the list, remove it.
                this.SelectedUnits.Remove(selectedItem);
            }
            else
            {
                // item is not in the list add it.
                this.SelectedUnits.Add(selectedItem);
            }

            if (this.StockAction == ManageStockAction.Issue)
            {
                this.UpdateSelectedUnitsCountStatus();
            }
            else if (this.StockAction == ManageStockAction.Receive)
            {
                this.NextButtonEnabled = this.SelectedUnits.Count > 0;
            }
        }

        // Receive stock
        public ManageStockAction StockAction
        {
            get { return this._stockAction; }

            set
            {
                this.SetProperty(ref this._stockAction, value, () => this.StockAction);
            }
        }

        public ObservableCollection<ReasonForReturning> Reasons
        {
            get { return this._reasons; }

            set
            {
                this.SetProperty(ref this._reasons, value, () => this.Reasons);
            }
        }

        public ReasonForReturning SelectedReason
        {
            get
            {
                return this._selectedReason;
            }

            set
            {
                this.SetProperty(ref this._selectedReason, value, () => this.SelectedReason);
            }
        }

        public bool ShowOnReceiveStockScreen
        {
            get
            {
                return this.StockAction == ManageStockAction.Receive;
            }
        }

        public string SelectMoreOrReasonTitle
        {
            get { return this._selectMoreOrReasonTitle; }
            set { this.SetProperty(ref this._selectMoreOrReasonTitle, value, () => this.SelectMoreOrReasonTitle); }
        }

        public void GetReasons()
        {
            string reasonsJson = Settings.Instance.ReasonForReturn;
            if (string.IsNullOrEmpty(reasonsJson))
            {
                reasonsJson = this._deviceResource.ReasonForReturn;
            }

            List<ReasonForReturning> reasons = JsonConvert.DeserializeObject<List<ReasonForReturning>>(reasonsJson);
            this.Reasons = new ObservableCollection<ReasonForReturning>(reasons);
        }

        public void ShowCheckBoxOnConfirmationScreen(bool isHidden)
        {
            foreach (var item in this.SelectedUnits)
            {
                item.IsSelectable = isHidden;
            }
        }

        public async Task<ManageStockPostApiResponse> ReceiveStock()
        {
            this.ProgressDialogMessage = this._deviceResource.PleaseWait;
            this.IsBusy = true;

            ReturnedProduct product = new ReturnedProduct { DsrPhone = this.DsrPhoneNumber, PersonId = this.DsrStock.PersonId, PersonRoleId = this.DsrStock.PersonRoleId };
            product.Reason = this.SelectedReason.Reason;

            List<List<DeviceAllocationItem>> groupedSelectedList = this.SelectedUnits.GroupBy(u => u.ProductTypeId).Select(grp => grp.ToList()).ToList();

            List<ScmStock> returnObj = new List<ScmStock>();

            foreach (List<DeviceAllocationItem> products in groupedSelectedList)
            {
                ScmStock stock = new ScmStock();
                stock.ProductTypeId = products[0].ProductTypeId;
                stock.Name = products[0].HeaderText;
                List<string> units = new List<string>();
                foreach (var item in products)
                {
                    units.Add(item.SerialNumber);
                }

                stock.SerialNumbers = units;
                returnObj.Add(stock);
            }

            product.Units = returnObj;

            ManageStockPostApiResponse response = await this.DsrStockAllocationService.ReceiveStockFromDsr(product);

            this.IsBusy = false;

            if (response == null)
            {
                return new ManageStockPostApiResponse { Success = false, Text = "Units could not be returned." };
            }

            return response;
        }

        /// <summary>
        /// Removing a key from keyvalue object
        /// </summary>
        public void RemoveKeyFromDsrDetails(string key)
        {
            foreach (var item in this.DsrDetails)
            {
                if (item.Key.Equals(key))
                {
                    this.DsrDetails.Remove(item);
                    break;
                }
            }
        }

        public void AddKeyToDsrDetails(string key, string value)
        {
            this.DsrDetails.Add(new KeyValue { Key = key, Value = value });
        }
    }
}