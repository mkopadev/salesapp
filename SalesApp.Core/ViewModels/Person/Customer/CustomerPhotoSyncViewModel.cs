using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Person.Customer.Photo;
using SalesApp.Core.ViewModels.Dialog;

namespace SalesApp.Core.ViewModels.Person.Customer
{
    public class CustomerPhotoSyncViewModel : BaseViewModel
    {
        public string NationalId { get; set; }

        private string _message;
        private string _dontCancel;
        private string _cancel;

        private ICommand _cancelCommand;
        private ICommand _doneCommand;

        public CustomerPhotoSyncViewModel()
        {
            IDeviceResource deviceResource = Resolver.Instance.Get<IDeviceResource>();

            this._message = deviceResource.PhotoDeleteConfirmationMessage;
            this._dontCancel = deviceResource.DontCancel;
            this._cancel = deviceResource.Cancel;
        }

        public void Init(string nationalId)
        {
            this.NationalId = nationalId;
        }

        public ICommand CancelCommand
        {
            get
            {
                this._cancelCommand = this._cancelCommand ?? new MvxCommand(async () =>
                {
                    int numberOfPhotos = await new CustomerPhotoController().GetPhotos(this.NationalId);
                    if (numberOfPhotos > 0)
                    {
                        var result =
                            await Resolver.Instance.Get<IDialogService>().ShowAsync(this._message, this._cancel, this._dontCancel);
                        IPhotoService photoService = Resolver.Instance.Get<IPhotoService>();

                        if (result == true)
                        {
                            await photoService.DeletePhotos(this.NationalId);
                            this.Close(this);
                        }
                    }
                    else
                    {
                       this.Close(this);
                    }
                });

                return this._cancelCommand;
            }
        }

        public ICommand DoneCommand
        {
            get
            {
                this._doneCommand = this._doneCommand ?? new MvxCommand(async () =>
                {
                    await new CustomerPhotoController().UpdateStatus(this.NationalId);
                    this.Close(this);
                });

                return this._doneCommand;
            }
        }
    }
}