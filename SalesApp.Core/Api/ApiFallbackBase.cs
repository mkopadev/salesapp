using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Settings;

namespace SalesApp.Core.Api
{
    public interface IApiFallbackBase
    {
        FallbackResponse PostString(string message);
    }

    public abstract class ApiFallbackBase : IApiFallbackBase
    {
        protected ISmsService SMSService;

        // Test fall back number
        private readonly string _smsProviderNumber = Settings.Instance.SMSShortCode;

        public ApiFallbackBase()
        {
            SMSService = Resolver.Instance.Get<ISmsService>();
        }

        public ApiFallbackBase(ISmsService smsService)
        {
            SMSService = smsService;
        }

        public FallbackResponse PostString(string message)
        {
            // send the SMS and wait for the result
            bool success = SMSService.SendSms(_smsProviderNumber, message);
            return new FallbackResponse
            {
                Success = success
            };
        }
    }
}
