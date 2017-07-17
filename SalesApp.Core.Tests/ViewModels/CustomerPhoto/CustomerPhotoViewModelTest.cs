using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using SalesApp.Core.Enums.People;
using SalesApp.Core.Services.Person.Customer;
using SalesApp.Core.ViewModels.Person.Customer;

namespace SalesApp.Core.Tests.ViewModels.CustomerPhoto
{
    public class CustomerPhotoViewModelTest : TestsBase
    {
        private CustomerPhotoViewModel _model;
        private List<SalesApp.Core.BL.Models.People.CustomerPhoto> _photos;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            var photo1 = new SalesApp.Core.BL.Models.People.CustomerPhoto
            {
                CustomerIdentifier = "273194961",
                PhotoStatus = PhotoSaveStatus.Successful,
                PhotoUploadStatus = PhotoUploadStatus.Pending,
                FilePath = "bla bla bla bla"
            };

            var photo2 = new SalesApp.Core.BL.Models.People.CustomerPhoto
            {
                CustomerIdentifier = "273194961",
                PhotoStatus = PhotoSaveStatus.Successful,
                PhotoUploadStatus = PhotoUploadStatus.Pending,
                FilePath = "bla bla bla bla"
            };

            this._photos = new List<SalesApp.Core.BL.Models.People.CustomerPhoto>
            {
                photo1, photo2
            };

            string nationalId = "273194961";
            var service = Substitute.For<CustomerPhotoService>();
            service.GetCustomerPhotos(nationalId).Returns(async a => await this.GetPhotos());

            this._model = new CustomerPhotoViewModel(nationalId, service);
        }

        private async Task<List<SalesApp.Core.BL.Models.People.CustomerPhoto>> GetPhotos()
        {
            return await Task.Run(() => this._photos);
        }

        [Test]
        public async void TestAllPhotosAreFetched()
        {
            await this._model.GetCustomerPhotos();

            Assert.That(2, Is.EqualTo(this._model.CustomerPhotos.Count));
        }
    }
}