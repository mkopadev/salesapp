using SalesApp.Core.Services.Device;

namespace SalesApp.Core.Api
{
    public class LoginDto
    {
        public bool IsFirstLogin { get; set; }
        public string DeviceId { get; set; }
        public string Hash { get; set; }

        public IInformation DeviceInformation { get; set; }
    }
}