using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.People;
using SalesApp.Core.Services.Person.Customer;

namespace SalesApp.Core.ViewModels.Person.Customer
{
    public class CustomerPhotoViewModel : BaseViewModel
    {
        private ObservableCollection<CustomerPhoto> _customerPhotos = new ObservableCollection<CustomerPhoto>();
        private CustomerPhotoService _service;
        private MvxCommand _addPhotoCommand;
        private string _nationalId;

        public CustomerPhotoViewModel(string nationalId, CustomerPhotoService service)
        {
            this._service = service;
            this._nationalId = nationalId;
        }

        public ObservableCollection<CustomerPhoto> CustomerPhotos
        {
            get
            {
                return this._customerPhotos;
            }

            set
            {
                this.SetProperty(ref this._customerPhotos, value, () => this.CustomerPhotos);
            }
        }

        public async void PhotoUpdated(int position, PhotoUploadStatus newStatus = PhotoUploadStatus.Pending)
        {
            CustomerPhotoController controller = new CustomerPhotoController();
            CustomerPhoto photo = this.CustomerPhotos[position];
            photo.PhotoUploadStatus = newStatus;

            await controller.SaveAsync(photo);

            CustomerPhoto backUpPhoto = new CustomerPhoto
            {
                CustomerIdentifier = this.CustomerPhotos[position].CustomerIdentifier,
                FilePath = this.CustomerPhotos[position].FilePath,
                Phone = this.CustomerPhotos[position].Phone,
                PhotoStatus = this.CustomerPhotos[position].PhotoStatus,
                PhotoUploadStatus = this.CustomerPhotos[position].PhotoUploadStatus,
                TypeOfPhoto = this.CustomerPhotos[position].TypeOfPhoto,
                Id = this.CustomerPhotos[position].Id,
                Created = this.CustomerPhotos[position].Created,
                Modified = this.CustomerPhotos[position].Modified
            };

            this.CustomerPhotos.RemoveAt(position);
            this.CustomerPhotos.Insert(position, backUpPhoto);

            this.RaisePropertyChanged(() => this.CustomerPhotos);
        }

        public void PhotoUpdated(CustomerPhoto newPhoto)
        {
            int position = -1;

            for (int i = 0; i < this.CustomerPhotos.Count; i++)
            {
                if (this.CustomerPhotos[i].Id == newPhoto.Id)
                {
                    position = i;
                    break;
                }
            }

            this.PhotoUpdated(position, newPhoto.PhotoUploadStatus);
        }

        public void UpdatePhotos(List<CustomerPhoto> newPhotos)
        {
            foreach (var cp in newPhotos)
            {
                if (this.CustomerPhotos.Contains(cp))
                {
                    continue;
                }

                cp.PhotoUploadStatus = PhotoUploadStatus.Pending;
                this.CustomerPhotos.Add(cp);
            }

            this.RaisePropertyChanged(() => this.CustomerPhotos);
        }

        public async Task GetCustomerPhotos()
        {
            List<CustomerPhoto> photos = await this._service.GetCustomerPhotos(this._nationalId);
            this.CustomerPhotos = new ObservableCollection<CustomerPhoto>(photos);
        }

        public ICommand AddPhotoCommand
        {
            get
            {
                this._addPhotoCommand = this._addPhotoCommand ?? new MvxCommand(() =>
                {
                    ShowViewModel<CustomerPhotoSyncViewModel>(new
                    {
                        nationalId = this._nationalId
                    });
                });

                return this._addPhotoCommand;
            }
        }
    }
}