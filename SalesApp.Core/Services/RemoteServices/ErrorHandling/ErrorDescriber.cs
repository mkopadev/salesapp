using System;
using System.Threading.Tasks;
using SalesApp.Core.Enums.Api;

namespace SalesApp.Core.Services.RemoteServices.ErrorHandling
{
    public class ErrorDescriber
    {
        public ErrorDescriber(Type exceptionType, Type uiNotifierType,ErrorFilterFlags filterFlag)
        {
            this.ExceptionType = exceptionType;
            this.UiNotifierType = uiNotifierType;
            this.FilterFlag = filterFlag;
        }


        public Type ExceptionType { get; private set; }

        public Type UiNotifierType { get; set; }

        public string ErrorCode { get; set; }

        public Func<Action<object>,Task> RetryAction { get; set; }

        public bool RetrySupported
        {
            get
            {
                return this.RetryAction != null && this.SuccessCallback != null;
            }
        }

        public Action<object> SuccessCallback { get; set; }

        public uint HttpStatusCode { get; set; }

        public ErrorFilterFlags FilterFlag { get; private set; }
    }
}