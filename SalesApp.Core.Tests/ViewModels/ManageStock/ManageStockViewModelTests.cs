using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using SalesApp.Core.Api;
using SalesApp.Core.Api.ManageStock;
using SalesApp.Core.BL;
using SalesApp.Core.BL.Models.ManageStock;
using SalesApp.Core.Enums.ManageStock;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.ManageStock;
using SalesApp.Core.Services.Product;
using SalesApp.Core.ViewModels;
using SalesApp.Core.ViewModels.Home;
using SalesApp.Core.ViewModels.ManageStock;

namespace SalesApp.Core.Tests.ViewModels.ManageStock
{
    public class ManageStockViewModelTests : MvxTestBase
    {
        private ManageStockViewModel _viewModel;
        private RemoteProductService _remoteProductService;
        private DsrStockAllocationService _dsrStockAllocationService;
        private IDeviceResource _deviceResource = Resolver.Instance.Get<IDeviceResource>();
        private string _falseStatusJson = "{\"UnitsAllocated\":4,\"MaxAllowedUnits\":7,\"PersonId\":\"8adaf5aa-dfac-4e25-9be7-3eb877aea93c\",\"PersonRoleId\":\"9bdaf5aa-dfac-4e25-9be7-3eb877aea93c\",\"Status\":{\"Status\":false,\"Message\":\"DSR could not be found, please re-enter the telephone no\"},\"DsrDetails\":[{\"Key\":\"Name:\",\"Value\":\"DSR Test\"},{\"Key\":\"Phone no:\",\"Value\":\"0726363373\"},{\"Key\":\"Maximum no of units allowed:\",\"Value\":\"7\"}],\"Products\":[{\"Name\":\"Product 4\",\"Units\":[{\"SerialNumber\":\"894651453216451348651\",\"DateAllocated\":\"01/02/2016\"},{\"SerialNumber\":\"894651453216451348652\",\"DateAllocated\":\"01/02/2016\"}]},{\"Name\":\"Product 5\",\"Units\":[{\"SerialNumber\":\"894651453216451348653\",\"DateAllocated\":\"01/02/2016\"},{\"SerialNumber\":\"894651453216451348654\",\"DateAllocated\":\"01/02/2016\"}]}]}";
        private ServerResponse<DsrStockServerResponseObject> _falseStatusDsrStockServerResponseObject;

        string _successJson = "{\"UnitsAllocated\":4,\"MaxAllowedUnits\":7,\"PersonId\":\"8adaf5aa-dfac-4e25-9be7-3eb877aea93c\",\"PersonRoleId\":\"9bdaf5aa-dfac-4e25-9be7-3eb877aea93c\",\"Status\":{\"Status\":true,\"Message\":\"DSR could not be found, please re-enter the telephone no\"},\"DsrDetails\":[{\"Key\":\"Name:\",\"Value\":\"DSR Test\"},{\"Key\":\"Phone no:\",\"Value\":\"0726363373\"},{\"Key\":\"Maximum no of units allowed:\",\"Value\":\"7\"}],\"Products\":[{\"Name\":\"Product 4\",\"Units\":[{\"SerialNumber\":\"894651453216451348651\",\"DateAllocated\":\"01/02/2016\"},{\"SerialNumber\":\"894651453216451348652\",\"DateAllocated\":\"01/02/2016\"}]},{\"Name\":\"Product 5\",\"Units\":[{\"SerialNumber\":\"894651453216451348653\",\"DateAllocated\":\"01/02/2016\"},{\"SerialNumber\":\"894651453216451348654\",\"DateAllocated\":\"01/02/2016\"}]}]}";
        private ServerResponse<DsrStockServerResponseObject> _successDsrStockServerResponseObject;

        private string _noProductsJson = "{\"UnitsAllocated\":4,\"MaxAllowedUnits\":7,\"PersonId\":\"8adaf5aa-dfac-4e25-9be7-3eb877aea93c\",\"PersonRoleId\":\"9bdaf5aa-dfac-4e25-9be7-3eb877aea93c\",\"Status\":{\"Status\":true,\"Message\":\"All fine, please proceed.\"},\"DsrDetails\":[{\"Key\":\"Name:\",\"Value\":\"DSR Test\"},{\"Key\":\"Phone no:\",\"Value\":\"0726363373\"},{\"Key\":\"Maximum no of units allowed:\",\"Value\":\"7\"}],\"Products\":[]}";
        private ServerResponse<DsrStockServerResponseObject> _noPoductsDsrStockServerResponseObject;

        private string _nullProductsJson = "{\"UnitsAllocated\":4,\"MaxAllowedUnits\":7,\"PersonId\":\"8adaf5aa-dfac-4e25-9be7-3eb877aea93c\",\"PersonRoleId\":\"9bdaf5aa-dfac-4e25-9be7-3eb877aea93c\",\"Status\":{\"Status\":true,\"Message\":\"All fine, please proceed.\"},\"DsrDetails\":[{\"Key\":\"Name:\",\"Value\":\"DSR Test\"},{\"Key\":\"Phone no:\",\"Value\":\"0726363373\"},{\"Key\":\"Maximum no of units allowed:\",\"Value\":\"7\"}]}";

        private string Query
        {
            get
            {
                if (this._viewModel == null)
                {
                    return string.Empty;
                }

                return string.Format("?dsrPhone={0}", this._viewModel.DsrPhoneNumber);
            }
        }

        [SetUp]
        public void Initialize()
        {
            this._remoteProductService = Substitute.For<RemoteProductService>("StockAllocation/Products");
            this._dsrStockAllocationService = Substitute.For<DsrStockAllocationService>();

            var connectivityService = Substitute.For<IConnectivityService>();
            connectivityService.HasConnection().Returns(true);
            Resolver.Instance.RegisterSingleton(connectivityService);

            this._viewModel = new ManageStockViewModel
            {
                RemoteProductService = this._remoteProductService,
                DsrStockAllocationService = this._dsrStockAllocationService,
                DsrPhoneNumber = "0711111111"
            };

            this._falseStatusDsrStockServerResponseObject = new ServerResponse<DsrStockServerResponseObject>
            {
                IsSuccessStatus = true,
                RawResponse = this._falseStatusJson
            };

            this._successDsrStockServerResponseObject = new ServerResponse<DsrStockServerResponseObject>
            {
                IsSuccessStatus = true,
                RawResponse = this._successJson
            };

            this._noPoductsDsrStockServerResponseObject = new ServerResponse<DsrStockServerResponseObject>
            {
                IsSuccessStatus = true,
                RawResponse = this._noProductsJson
            };
        }

        [Test]
        public async Task TestFindDsrReturnsNull()
        {
            this._dsrStockAllocationService.GetCurrentDsrStock(this.Query).Returns(a => this.ReturnNullDsrDetails());

            await this._viewModel.FindDsrAsync();

            Assert.Null(this._viewModel.PhoneNumberErrorMessage);
            Assert.Null(this._viewModel.DsrStock);

            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);
        }

        [Test]
        public async Task TestFindDsrReturnsNoProducts()
        {
            this._dsrStockAllocationService.GetCurrentDsrStock(this.Query).Returns(a => this.ReturnNoProducts());

            await this._viewModel.FindDsrAsync();

            Assert.Null(this._viewModel.SelectUnitsBeingReturned);
            Assert.False(this._viewModel.DsrHasUnits);
        }

        [Test]
        public async Task TestFindDsrReturnsNullProducts()
        {
            this._dsrStockAllocationService.GetCurrentDsrStock(this.Query).Returns(a => this.ReturnNullProducts());

            await this._viewModel.FindDsrAsync();

            Assert.Null(this._viewModel.SelectUnitsBeingReturned);
            Assert.False(this._viewModel.DsrHasUnits);
        }

        [Test]
        public async Task TestFindDsrReturnsFalseStatus()
        {
            this._dsrStockAllocationService.GetCurrentDsrStock(this.Query).Returns(a => this.ReturnFalseStatus());

            await this._viewModel.FindDsrAsync();

            string message = this._falseStatusDsrStockServerResponseObject.GetObject().Status.Message;

            Assert.That(this._viewModel.PhoneNumberErrorMessage, Is.EqualTo(message));
            Assert.Null(this._viewModel.DsrStock);

            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);
        }

        [Test]
        public async Task TestFindDsrReturnsNonNull()
        {
            this._dsrStockAllocationService.GetCurrentDsrStock(this.Query).Returns(a => this.ReturnNonNullDsrDetails());
            this._viewModel.StockAction = ManageStockAction.Issue;

            await this._viewModel.FindDsrAsync();
            Assert.Null(this._viewModel.PhoneNumberErrorMessage);
            Assert.NotNull(this._viewModel.DsrStock);
            Assert.That(this._viewModel.DsrDetails.Count, Is.EqualTo(3));
            Assert.That(this._viewModel.DsrStockUnits.Count, Is.EqualTo(4));

            // Check that sectioning of items works ok
            string product4 = this._viewModel.DsrStock.Products[0].ProductName;

            Assert.That(this._viewModel.DsrStockUnits[0].HeaderText, Is.EqualTo(product4));
            Assert.That(this._viewModel.DsrStockUnits[1].HeaderText, Is.Null);
            Assert.True(this._viewModel.DsrStockUnits[1].IsFooterItem);

            string product5 = this._viewModel.DsrStock.Products[1].ProductName;

            Assert.That(this._viewModel.DsrStockUnits[2].HeaderText, Is.EqualTo(product5));
            Assert.That(this._viewModel.DsrStockUnits[3].HeaderText, Is.Null);
            Assert.True(this._viewModel.DsrStockUnits[3].IsFooterItem);

            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);

            Assert.Null(this._viewModel.SelectUnitsBeingReturned);
        }

        [Test]
        public async Task TestFindDsrReturnsNonNullWhileInReceiveStock()
        {
            this._dsrStockAllocationService.GetCurrentDsrStock(this.Query).Returns(a => this.ReturnNonNullDsrDetails());
            this._viewModel.StockAction = ManageStockAction.Receive;

            await this._viewModel.FindDsrAsync();

            Assert.Null(this._viewModel.PhoneNumberErrorMessage);
            Assert.NotNull(this._viewModel.DsrStock);
            Assert.That(this._viewModel.DsrDetails.Count, Is.EqualTo(3));
            Assert.That(this._viewModel.DsrStockUnits.Count, Is.EqualTo(4));

            // Check that sectioning of items works ok
            string product4 = this._viewModel.DsrStock.Products[0].ProductName;

            Assert.That(this._viewModel.DsrStockUnits[0].HeaderText, Is.EqualTo(product4));
            Assert.That(this._viewModel.DsrStockUnits[1].HeaderText, Is.Null);
            Assert.True(this._viewModel.DsrStockUnits[1].IsFooterItem);

            string product5 = this._viewModel.DsrStock.Products[1].ProductName;

            Assert.That(this._viewModel.DsrStockUnits[2].HeaderText, Is.EqualTo(product5));
            Assert.That(this._viewModel.DsrStockUnits[3].HeaderText, Is.Null);
            Assert.True(this._viewModel.DsrStockUnits[3].IsFooterItem);

            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);

            // check selection prompt
            string message = this._deviceResource.SelectUnitsBeingReturned;
            Assert.That(this._viewModel.SelectUnitsBeingReturned, Is.EqualTo(message));
        }

        [Test]
        public async Task TestFindWithNullPhoneNumber()
        {
            this._viewModel.DsrPhoneNumber = null;

            this._dsrStockAllocationService.GetCurrentDsrStock(this.Query).Returns(a => this.ReturnNullDsrDetails());

            await this._viewModel.FindDsrAsync();

            Assert.That(this._viewModel.PhoneNumberErrorMessage, Is.EqualTo(this._deviceResource.PhoneNumberEmpty));
            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);
        }

        [Test]
        public async Task TestFindWithEmptyPhoneNumber()
        {
            this._viewModel.DsrPhoneNumber = string.Empty;

            this._dsrStockAllocationService.GetCurrentDsrStock(this.Query).Returns(a => this.ReturnNullDsrDetails());

            await this._viewModel.FindDsrAsync();

            Assert.That(this._viewModel.PhoneNumberErrorMessage, Is.EqualTo(this._deviceResource.PhoneNumberEmpty));
            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);
        }

        [Test]
        public async Task TestFindWithTooShortPhoneNumber()
        {
            this._viewModel.DsrPhoneNumber = "0721";

            this._dsrStockAllocationService.GetCurrentDsrStock(this.Query).Returns(a => this.ReturnNullDsrDetails());

            await this._viewModel.FindDsrAsync();

            Assert.That(this._viewModel.PhoneNumberErrorMessage, Is.EqualTo(this._deviceResource.PhoneNumberTooShort));
            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);
        }

        [Test]
        public async Task TestFindWithTooLongPhoneNumber()
        {
            this._viewModel.DsrPhoneNumber = "07211254563";

            this._dsrStockAllocationService.GetCurrentDsrStock(this.Query).Returns(a => this.ReturnNullDsrDetails());

            await this._viewModel.FindDsrAsync();

            Assert.That(this._viewModel.PhoneNumberErrorMessage, Is.EqualTo(this._deviceResource.PhoneNumberTooLong));
            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);
        }

        [Test]
        public async Task TestFindWithInvalidPhoneNumber()
        {
            this._viewModel.DsrPhoneNumber = "0821125456";

            this._dsrStockAllocationService.GetCurrentDsrStock(this.Query).Returns(a => this.ReturnNullDsrDetails());

            await this._viewModel.FindDsrAsync();

            Assert.That(this._viewModel.PhoneNumberErrorMessage, Is.EqualTo(this._deviceResource.PhoneNumberInvalidFormat));
            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);
        }

        [Test]
        public async Task TestFindWithBadFormatPhoneNumber()
        {
            this._viewModel.DsrPhoneNumber = "0721+254563";

            this._dsrStockAllocationService.GetCurrentDsrStock(this.Query).Returns(a => this.ReturnNullDsrDetails());

            await this._viewModel.FindDsrAsync();

            Assert.That(this._viewModel.PhoneNumberErrorMessage, Is.EqualTo(this._deviceResource.PhoneNumberInvalidCharacters));
            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);
        }

        [Test]
        public void TestFindPhoneNumberThrowsUnexpectedException()
        {
            this._viewModel.DsrPhoneNumber = "0721254563";

            var exception = new Exception("Damn, something went wrong!");
            this._dsrStockAllocationService.GetCurrentDsrStock(this.Query).Throws(exception);

            Assert.Throws<Exception>(async () => await this._viewModel.FindDsrAsync());

            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);
        }

        [Test]
        public async void TestAllocateSelectedUnitsReturnsNull()
        {
            this._dsrStockAllocationService.AllocateUnitsToDsr(new SelectedProduct()).ReturnsForAnyArgs(a => this.ReturnAllocateReturnsNull());
            this._viewModel.DsrStock = new DsrStockServerResponseObject();

            this._viewModel.SelectedProduct = new ScmStock
            {
                Name = "Product 1",
                SerialNumbers = new List<string>()
            };

            this._viewModel.SelectedUnits = new ObservableCollection<DeviceAllocationItem>();

            await this._viewModel.AllocateSelectedUnits();

            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);
        }

        [Test]
        public async void TestReceiveStockReturnsNull()
        {
            this._dsrStockAllocationService.ReceiveStockFromDsr(new SelectedProduct()).ReturnsForAnyArgs(a => this.ReturnAllocateReturnsNull());
            this._viewModel.DsrStock = new DsrStockServerResponseObject();

            this._viewModel.SelectedProduct = new ScmStock
            {
                Name = "Product 1",
                SerialNumbers = new List<string>()
            };

            this._viewModel.SelectedUnits = new ObservableCollection<DeviceAllocationItem>
            {
                new DeviceAllocationItem(),
                new DeviceAllocationItem()
            };

            this._viewModel.SelectedReason = new ReasonForReturning { Reason = "Broken lamp" };

            await this._viewModel.ReceiveStock();

            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);
        }

        [Test]
        public async void TestAllocateSelectedUnits()
        {
            this._dsrStockAllocationService.AllocateUnitsToDsr(new SelectedProduct()).ReturnsForAnyArgs(a => this.ReturnAllocateSuccess());
            this._viewModel.DsrStock = new DsrStockServerResponseObject();

            this._viewModel.SelectedProduct = new ScmStock
            {
                Name = "Product 1",
                SerialNumbers = new List<string>()
            };

            this._viewModel.SelectedUnits = new ObservableCollection<DeviceAllocationItem>()
            {
                new DeviceAllocationItem(),
                new DeviceAllocationItem()
            };

            await this._viewModel.AllocateSelectedUnits();

            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);
        }

        [Test]
        public async void TestReceiveStock()
        {
            this._dsrStockAllocationService.AllocateUnitsToDsr(new SelectedProduct()).ReturnsForAnyArgs(a => this.ReturnAllocateSuccess());
            this._viewModel.DsrStock = new DsrStockServerResponseObject();

            this._viewModel.SelectedProduct = new ScmStock
            {
                Name = "Product 1",
                SerialNumbers = new List<string>()
            };

            this._viewModel.SelectedUnits = new ObservableCollection<DeviceAllocationItem>()
            {
                new DeviceAllocationItem(),
                new DeviceAllocationItem()
            };

            this._viewModel.SelectedReason = new ReasonForReturning { Reason = "Broken lamp" };

            await this._viewModel.ReceiveStock();

            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);
        }

        [Test]
        public async void TestFetchCurrentScmStockWhenDsrPhoneHasChanged()
        {
            string json =
                "[{\"ProductTypeId\":\"91311660-50FD-E411-BEB2-0009DC09884A\",\"DisplayName\":\"Product III\",\"SerialNumber\":\"894651453216451348647\",\"DateAcquired\":\"2015-08-31\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e60e\",\"DisplayName\":\"Product 4\",\"SerialNumber\":\"894651453216451348648\",\"DateAcquired\":\"2015-08-31\"},{\"ProductTypeId\":\"91311660-50FD-E411-BEB2-0009DC09884A\",\"DisplayName\":\"Product III\",\"SerialNumber\":\"894651453216451348649\",\"DateAcquired\":\"2015-07-01\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e60e\",\"DisplayName\":\"Product 4\",\"SerialNumber\":\"894651453216451348650\",\"DateAcquired\":\"2015-08-21\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e60e\",\"DisplayName\":\"Product 4\",\"SerialNumber\":\"894651453216451348651\",\"DateAcquired\":\"2015-08-21\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e60e\",\"DisplayName\":\"Product 4\",\"SerialNumber\":\"894651453216451348652\",\"DateAcquired\":\"2015-08-21\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e69e\",\"DisplayName\":\"Product 5\",\"SerialNumber\":\"894651453216451348653\",\"DateAcquired\":\"2015-08-21\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e69e\",\"DisplayName\":\"Product 5\",\"SerialNumber\":\"894651453216451348654\",\"DateAcquired\":\"2015-08-21\"}]";

            this._remoteProductService.GetProducts().ReturnsForAnyArgs(a => this.ReturnScmStock(json));
            this._viewModel.DsrStock = new DsrStockServerResponseObject();
            await this._viewModel.FetchCurrentScmStock();

            json = "[]";

            // change the phone number
            this._viewModel.DsrPhoneNumber = "0726789852";
            this._remoteProductService.GetProducts().ReturnsForAnyArgs(a => this.ReturnScmStock(json));

            // fetch again
            await this._viewModel.FetchCurrentScmStock();

            // make sure when phone number changes, we hit the api
            Assert.That(this._viewModel.ScmUnitsInStock.Count, Is.EqualTo(0));

            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);
        }

        [Test]
        public async void TestFetchCurrentScmStockWhenItsEmpty()
        {
            string json =
                "[{\"ProductTypeId\":\"91311660-50FD-E411-BEB2-0009DC09884A\",\"DisplayName\":\"Product III\",\"SerialNumber\":\"894651453216451348647\",\"DateAcquired\":\"2015-08-31\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e60e\",\"DisplayName\":\"Product 4\",\"SerialNumber\":\"894651453216451348648\",\"DateAcquired\":\"2015-08-31\"},{\"ProductTypeId\":\"91311660-50FD-E411-BEB2-0009DC09884A\",\"DisplayName\":\"Product III\",\"SerialNumber\":\"894651453216451348649\",\"DateAcquired\":\"2015-07-01\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e60e\",\"DisplayName\":\"Product 4\",\"SerialNumber\":\"894651453216451348650\",\"DateAcquired\":\"2015-08-21\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e60e\",\"DisplayName\":\"Product 4\",\"SerialNumber\":\"894651453216451348651\",\"DateAcquired\":\"2015-08-21\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e60e\",\"DisplayName\":\"Product 4\",\"SerialNumber\":\"894651453216451348652\",\"DateAcquired\":\"2015-08-21\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e69e\",\"DisplayName\":\"Product 5\",\"SerialNumber\":\"894651453216451348653\",\"DateAcquired\":\"2015-08-21\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e69e\",\"DisplayName\":\"Product 5\",\"SerialNumber\":\"894651453216451348654\",\"DateAcquired\":\"2015-08-21\"}]";

            this._viewModel.ScmUnitsInStock = new ObservableCollection<ScmStock>();

            this._remoteProductService.GetProducts().ReturnsForAnyArgs(a => this.ReturnScmStock(json));
            this._viewModel.DsrStock = new DsrStockServerResponseObject();
            await this._viewModel.FetchCurrentScmStock();

            Assert.That(this._viewModel.ScmUnitsInStock.Count, Is.EqualTo(3));

            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);
        }

        [Test]
        public async void TestFetchCurrentScmStockWhenDsrPhonHasNotChanged()
        {
            string json =
                 "[{\"ProductTypeId\":\"91311660-50FD-E411-BEB2-0009DC09884A\",\"DisplayName\":\"Product III\",\"SerialNumber\":\"894651453216451348647\",\"DateAcquired\":\"2015-08-31\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e60e\",\"DisplayName\":\"Product 4\",\"SerialNumber\":\"894651453216451348648\",\"DateAcquired\":\"2015-08-31\"},{\"ProductTypeId\":\"91311660-50FD-E411-BEB2-0009DC09884A\",\"DisplayName\":\"Product III\",\"SerialNumber\":\"894651453216451348649\",\"DateAcquired\":\"2015-07-01\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e60e\",\"DisplayName\":\"Product 4\",\"SerialNumber\":\"894651453216451348650\",\"DateAcquired\":\"2015-08-21\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e60e\",\"DisplayName\":\"Product 4\",\"SerialNumber\":\"894651453216451348651\",\"DateAcquired\":\"2015-08-21\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e60e\",\"DisplayName\":\"Product 4\",\"SerialNumber\":\"894651453216451348652\",\"DateAcquired\":\"2015-08-21\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e69e\",\"DisplayName\":\"Product 5\",\"SerialNumber\":\"894651453216451348653\",\"DateAcquired\":\"2015-08-21\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e69e\",\"DisplayName\":\"Product 5\",\"SerialNumber\":\"894651453216451348654\",\"DateAcquired\":\"2015-08-21\"}]";

            this._viewModel.DsrStock = new DsrStockServerResponseObject();

            this._remoteProductService.GetProducts().ReturnsForAnyArgs(a => this.ReturnScmStock(json));

            // Fetch for the first time
            await this._viewModel.FetchCurrentScmStock();
            var stockBefore = this._viewModel.ScmUnitsInStock;

            json =
                 "[{\"ProductTypeId\":\"91311660-50FD-E411-BEB2-0009DC09884A\",\"DisplayName\":\"Product II\",\"SerialNumber\":\"894651453256451348647\",\"DateAcquired\":\"2015-08-31\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e60e\",\"DisplayName\":\"Product 4\",\"SerialNumber\":\"894651453216451348648\",\"DateAcquired\":\"2015-08-31\"},{\"ProductTypeId\":\"91311660-50FD-E411-BEB2-0009DC09884A\",\"DisplayName\":\"Product III\",\"SerialNumber\":\"894651453216451348649\",\"DateAcquired\":\"2015-07-01\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e60e\",\"DisplayName\":\"Product 4\",\"SerialNumber\":\"894651453216451348650\",\"DateAcquired\":\"2015-08-21\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e60e\",\"DisplayName\":\"Product 4\",\"SerialNumber\":\"894651453216451348651\",\"DateAcquired\":\"2015-08-21\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e60e\",\"DisplayName\":\"Product 4\",\"SerialNumber\":\"894651453216451348652\",\"DateAcquired\":\"2015-08-21\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e69e\",\"DisplayName\":\"Product 5\",\"SerialNumber\":\"894651453216451348653\",\"DateAcquired\":\"2015-08-21\"},{\"ProductTypeId\":\"bccee7a0-e895-4051-ba40-4818e059e69e\",\"DisplayName\":\"Product 5\",\"SerialNumber\":\"894651453216451348654\",\"DateAcquired\":\"2015-08-21\"}]";
            this._remoteProductService.GetProducts().ReturnsForAnyArgs(a => this.ReturnScmStock(json));

            // Fetch again
            await this._viewModel.FetchCurrentScmStock();

            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.False(this._viewModel.IsBusy);

            // assert that existing scm stock is not changed
            Assert.That(this._viewModel.ScmUnitsInStock, Is.EqualTo(stockBefore));
            Assert.That(this._viewModel.ScmUnitsInStock.Count, Is.EqualTo(stockBefore.Count));
        }

        [Test]
        public void TestGetSelectedUnitsWhenNullReturnsNonNull()
        {
            this._viewModel.SelectedUnits = null;

            Assert.NotNull(this._viewModel.SelectedUnits);
            Assert.That(this._viewModel.SelectedUnits.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestGetScmUnitsInStockWhenNullReturnsNonNull()
        {
            this._viewModel.ScmUnitsInStock = null;

            Assert.NotNull(this._viewModel.ScmUnitsInStock);
            Assert.That(this._viewModel.ScmUnitsInStock.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestSetSelectedProductToNull()
        {
            this._viewModel.SelectedProduct = null;
            Assert.Null(this._viewModel.SelectedProduct);
        }

        [Test]
        public void TestSetSelectedProductAlsoPopulatesSelectedProductUnits()
        {
            this._viewModel.SelectedProduct = new ScmStock()
            {
                Name = "Product 4",
                SerialNumbers = new List<string>
                {
                    "gdfgdfghfhfhsdfds",
                    "fdfgdfhfhfghfghf5"
                }
            };

            Assert.NotNull(this._viewModel.SelectedProductUnits);
            Assert.That(this._viewModel.SelectedProductUnits.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestGetSelectedProductUnitsWhenNullReturnsNonNull()
        {
            this._viewModel.SelectedProductUnits = null;

            Assert.NotNull(this._viewModel.SelectedProductUnits);
            Assert.That(this._viewModel.SelectedProductUnits.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestGetProgressDialogMessage()
        {
            string pleaseWait = "Please wait...";
            this._viewModel.ProgressDialogMessage = pleaseWait;

            Assert.That(this._viewModel.ProgressDialogMessage, Is.EqualTo(pleaseWait));
        }

        [Test]
        public void TestUpdateSelectedUnitsCountStatus()
        {
            this._viewModel.DsrStock = new DsrStockServerResponseObject { MaxAllowedUnits = 10, UnitsAllocated = 5 };

            // Case for when UnitsSelected.Count = 0, UnitsAllocated + UnitsSelected.Count < MaxAllowedUnits
            this._viewModel.UpdateSelectedUnitsCountStatus();

            Assert.Null(this._viewModel.ErrorMessage);
            Assert.False(this._viewModel.NextButtonEnabled);

            // Case for when UnitsSelected.Count > 0, UnitsAllocated + UnitsSelected.Count < MaxAllowedUnits
            string errorMessage = string.Format(this._deviceResource.MoreThanMaxUnitsSelected, this._viewModel.DsrStock.MaxAllowedUnits - this._viewModel.DsrStock.UnitsAllocated);
            this._viewModel.SelectedUnits = new ObservableCollection<DeviceAllocationItem>
            {
                new DeviceAllocationItem(),
                new DeviceAllocationItem()
            };

            this._viewModel.UpdateSelectedUnitsCountStatus();

            Assert.True(this._viewModel.NextButtonEnabled);

            // Case for when UnitsSelected.Count > 0, UnitsAllocated + UnitsSelected.Count > MaxAllowedUnits
            this._viewModel.SelectedUnits = new ObservableCollection<DeviceAllocationItem>
            {
                new DeviceAllocationItem(),
                new DeviceAllocationItem(),
                new DeviceAllocationItem(),
                new DeviceAllocationItem(),
                new DeviceAllocationItem(),
                new DeviceAllocationItem()
            };

            this._viewModel.UpdateSelectedUnitsCountStatus();
            Assert.That(this._viewModel.ErrorMessage, Is.EqualTo(errorMessage));
            Assert.False(this._viewModel.NextButtonEnabled);
        }

        [Test]
        public void TestProcessApiResponseSuccess()
        {
            var mockResponse = new ManageStockPostApiResponse { Success = true, Text = "Allocation Successfull." };
            this._viewModel.ProcessApiResponse(mockResponse);

            Assert.That(this._viewModel.ApiStatusMessage, Is.EqualTo(mockResponse.Text));
            Assert.That(this._viewModel.ApiSuccess, Is.EqualTo(mockResponse.Success));
            Assert.That(this._viewModel.PositiveButtonText, Is.EqualTo(this._deviceResource.ManageStock));
            Assert.IsEmpty(this._viewModel.ApiStatusDescription);
            Assert.That(this._viewModel.StatusIcon, Is.EqualTo(this._deviceResource.CheckMarkIcon));
        }

        [Test]
        public void TestProcessApiResponseFailure()
        {
            var mockResponse = new ManageStockPostApiResponse { Success = false, Text = "Allocation Failed." };
            this._viewModel.ProcessApiResponse(mockResponse);

            Assert.That(this._viewModel.ApiStatusMessage, Is.EqualTo(mockResponse.Text));
            Assert.That(this._viewModel.ApiSuccess, Is.EqualTo(mockResponse.Success));
            Assert.That(this._viewModel.PositiveButtonText, Is.EqualTo(this._deviceResource.TryAgain));
            Assert.That(this._viewModel.StatusIcon, Is.EqualTo(this._deviceResource.ErrorNewIcon));
        }

        [Test]
        public void TestSectionizeSelectedUnitsListWhenIssuingStock()
        {
            this._viewModel.StockAction = ManageStockAction.Issue;
            this._viewModel.SelectedProduct = new ScmStock
            {
                Name = "Product 4",
                SerialNumbers = new List<string>
                {
                    "fdfgdgg", "sfdgg"
                }
            };

            this._viewModel.SelectedUnits = new ObservableCollection<DeviceAllocationItem>
            {
                new DeviceAllocationItem
                {
                    SerialNumber = "8448-44654-84564-6465-454dgdfg"
                },
                new DeviceAllocationItem
                {
                    SerialNumber = "644654-654894-fsdf-gghfh"
                }
            };

            this._viewModel.SectionizeSelectedUnitsList();
            Assert.NotNull(this._viewModel.SelectedUnits);
            Assert.That(this._viewModel.SelectedUnits.Count, Is.EqualTo(2));
            Assert.That(this._viewModel.SelectedUnits[0].HeaderText, Is.EqualTo(this._viewModel.SelectedProduct.Name));
        }

        [Test]
        public void TestSectionizeSelectedUnitsListWhenReceivingStock()
        {
            this._viewModel.StockAction = ManageStockAction.Receive;
            this._viewModel.SelectedProduct = new ScmStock
            {
                Name = "Product 4",
                SerialNumbers = new List<string>
                {
                    "fdfgdgg", "sfdgg"
                }
            };

            this._viewModel.SelectedUnits = new ObservableCollection<DeviceAllocationItem>
            {
                new DeviceAllocationItem
                {
                    Name = "Product 5",
                    SerialNumber = "8448-44654-84564-6465-454dgdfg"
                },
                new DeviceAllocationItem
                {
                    Name = "Product 6",
                    SerialNumber = "644654-654894-fsdf-gghfh"
                }
            };

            this._viewModel.SectionizeSelectedUnitsList();
            Assert.NotNull(this._viewModel.SelectedUnits);
            Assert.That(this._viewModel.SelectedUnits.Count, Is.EqualTo(2));
            Assert.That(this._viewModel.SelectedUnits[0].HeaderText, Is.EqualTo(this._viewModel.SelectedUnits[0].Name));
        }

        [Test]
        public void TestNegativeButtonToHomeView()
        {
            this.ClearAll();
            this._viewModel.NegativeButtonCommand.Execute(null);

            // check that we moved to the HomeViewModel
            Assert.That(this.MockDispatcher.Requests.Count, Is.EqualTo(1));
            Assert.That(this.MockDispatcher.Requests.First().ViewModelType, Is.EqualTo(typeof(HomeViewModel)));
        }

        [Test]
        public void TestProcessSelectingAnItemForTheFirstTime()
        {
            this.ClearAll();

            DeviceAllocationItem selectedItem = new DeviceAllocationItem
            {
                SerialNumber = "123456789"
            };

            this._viewModel.CanProcessSelectedItems = true;

            this._viewModel.OnItemSelected.Execute(selectedItem);
            Assert.AreEqual(1, this._viewModel.SelectedUnits.Count);
            Assert.AreEqual(selectedItem, this._viewModel.SelectedUnits[0]);
        }

        [Test]
        public void TestProcessAlreadySelectedItemBeingSelectedAgain()
        {
            this.ClearAll();

            DeviceAllocationItem selectedItem = new DeviceAllocationItem
            {
                SerialNumber = "1234567891"
            };

            this._viewModel.CanProcessSelectedItems = true;

            this._viewModel.SelectedUnits.Add(selectedItem);

            this._viewModel.OnItemSelected.Execute(selectedItem);
            Assert.AreEqual(0, this._viewModel.SelectedUnits.Count);
        }

        [Test]
        public void TestProcessSelectingMoreItemsThatMaxAllowed()
        {
            this.ClearAll();

            DeviceAllocationItem selectedItem1 = new DeviceAllocationItem
            {
                SerialNumber = "1234567891"
            };

            DeviceAllocationItem selectedItem2 = new DeviceAllocationItem
            {
                SerialNumber = "646446544"
            };

            this._viewModel.StockAction = ManageStockAction.Issue;
            this._viewModel.DsrStock = new DsrStockServerResponseObject { MaxAllowedUnits = 6, UnitsAllocated = 5 };

            this._viewModel.CanProcessSelectedItems = true;

            this._viewModel.OnItemSelected.Execute(selectedItem1);
            Assert.True(this._viewModel.NextButtonEnabled);
            Assert.AreEqual(1, this._viewModel.SelectedUnits.Count);

            this._viewModel.OnItemSelected.Execute(selectedItem2);
            Assert.AreEqual(2, this._viewModel.SelectedUnits.Count);
            Assert.False(this._viewModel.NextButtonEnabled);
            string errorMessage = string.Format(this._deviceResource.MoreThanMaxUnitsSelected, this._viewModel.DsrStock.MaxAllowedUnits - this._viewModel.DsrStock.UnitsAllocated);
            Assert.That(errorMessage, Is.EqualTo(this._viewModel.ErrorMessage));
        }

        [Test]
        public void TestCanProcessSelectedItemsIsDisabled()
        {
            this.ClearAll();

            DeviceAllocationItem selectedItem = new DeviceAllocationItem
            {
                SerialNumber = "1234567891"
            };

            this._viewModel.CanProcessSelectedItems = false;

            this._viewModel.OnItemSelected.Execute(selectedItem);
            Assert.AreEqual(0, this._viewModel.SelectedUnits.Count);
        }

        [Test]
        public void TestProcessSelectedItemWhenIssuingStock()
        {
            this.ClearAll();
            this._viewModel.StockAction = ManageStockAction.Receive;

            DeviceAllocationItem selectedItem = new DeviceAllocationItem
            {
                SerialNumber = "1234567891"
            };

            this._viewModel.OnItemSelected.Execute(selectedItem);
            Assert.AreEqual(0, this._viewModel.SelectedUnits.Count);
        }

        public void TestWhetherAkeywasRemovedFromKeyValueObject()
        {
            int currentObjectSize = this._viewModel.DsrDetails.Count;
            foreach (var item in this._viewModel.DsrDetails)
            {
                if (!item.Key.Equals("UNITS ALLOCATED:"))
                {
                    // dsrDetails.Add(new KeyValue { Key = item.Key, Value = item.Value });
                }
            }
        }

        private async Task<ServerResponse<DsrStockServerResponseObject>> ReturnNullDsrDetails()
        {
            ServerResponse<DsrStockServerResponseObject> result = null;
            return await Task.Run(() => result);
        }

        private async Task<ServerResponse<DsrStockServerResponseObject>> ReturnNonNullDsrDetails()
        {
            return await Task.Run(() => this._successDsrStockServerResponseObject);
        }

        private async Task<ServerResponse<DsrStockServerResponseObject>> ReturnFalseStatus()
        {
            return await Task.Run(() => this._falseStatusDsrStockServerResponseObject);
        }

        private async Task<ServerResponse<DsrStockServerResponseObject>> ReturnNoProducts()
        {
            return await Task.Run(() => this._noPoductsDsrStockServerResponseObject);
        }

        private async Task<ServerResponse<DsrStockServerResponseObject>> ReturnNullProducts()
        {
            var result = new ServerResponse<DsrStockServerResponseObject>
            {
                IsSuccessStatus = true,
                RawResponse = this._nullProductsJson
            };

            return await Task.Run(() => result);
        }

        private async Task<List<Product>> ReturnScmStock(string rawJson)
        {
            var result = JsonConvert.DeserializeObject<List<Product>>(rawJson);
            return await Task.Run(() => result);
        }

        private async Task<ManageStockPostApiResponse> ReturnAllocateSuccess()
        {
            var response = new ManageStockPostApiResponse
            {
                Success = true,
                Text = "Allocation successfull"
            };

            return await Task.Run(() => response);
        }

        private async Task<ManageStockPostApiResponse> ReturnAllocateReturnsNull()
        {
            ManageStockPostApiResponse response = null;

            return await Task.Run(() => response);
        }
    }
}