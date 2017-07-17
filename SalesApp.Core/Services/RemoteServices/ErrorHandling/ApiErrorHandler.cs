using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.RemoteServices.ErrorHandling.UiNotificationTypes;

namespace SalesApp.Core.Services.RemoteServices.ErrorHandling
{
    // TODO Evaluate Static class use...
    public static class ApiErrorHandler
    {
        private static ILog _logger;
        private static Action<ErrorDescriber> _errorOccuredCallback;
        private static Func<object, Task<object>> _retryCallback;

        public static void SetErrorOccuredCallback(Action<ErrorDescriber> errorOccuredCallback)
        {
            _errorOccuredCallback = default(Action<ErrorDescriber>);
            _errorOccuredCallback = errorOccuredCallback;
        }

        public static void UnsetCallbacks()
        {
            _errorOccuredCallback = default(Action<ErrorDescriber>);
            _retryCallback = default(Func<object, Task<object>>);
        }

        public static void SetRetryCallback(Func<object, Task<object>> retryCallback)
        {
            _retryCallback = null;
            _retryCallback = retryCallback;
        }

        private static ILog Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = Resolver.Instance.Get<ILog>();
                }

                return _logger;
            }
        }

        private static List<ErrorDescriber> _knownErrors = new List<ErrorDescriber>
        {
            new ErrorDescriber(typeof(TaskCanceledException), typeof(Flash), ErrorFilterFlags.IgnoreTimeOut),
            new ErrorDescriber(typeof(JsonReaderException), typeof(Flash), ErrorFilterFlags.IgnoreJsonParseError),
            new ErrorDescriber(typeof(NotConnectedToInternetException), typeof(Flash), ErrorFilterFlags.IgnoreNoInternetError),
            new ErrorDescriber(typeof(HttpResponse400Exception), typeof(Dialog), ErrorFilterFlags.Ignore400Family),
            new ErrorDescriber(typeof(HttpResponse500Exception), typeof(Dialog), ErrorFilterFlags.Ignore500Family),
            new ErrorDescriber(typeof(HttpResponse204Exception), typeof(Dialog), ErrorFilterFlags.Ignore204)
        };

        public static void RegisterExpectedError(object sender, ErrorDescriber errorDescription)
        {
            _knownErrors = _knownErrors.Where(item => item.ExceptionType != errorDescription.ExceptionType).ToList();
            _knownErrors.Add(errorDescription);
        }

        private static Exception GetExceptionOverridden(Exception exception, uint httpStatusCode)
        {
            Exception newException = exception;
            if (httpStatusCode >= 400 && httpStatusCode < 500)
            {
                newException = new HttpResponse400Exception();
            }
            else if(httpStatusCode >= 500 && httpStatusCode < 600)
            {
                newException = new HttpResponse500Exception();
            }

            return newException;
        }

        internal static void ExceptionOccured<TResult>(object sender, Exception exception,uint httpStatusCode, ErrorFilterFlags filterFlags, Func<Action<object>, Task<TResult>> retryAction = null,Action<object> successCallback = null,  string errorCode = "")
            where TResult : class
        {
            Logger.Initialize(sender.GetType().FullName);
            Logger.Error(exception);

            if ((filterFlags & ErrorFilterFlags.DisableErrorHandling) == ErrorFilterFlags.DisableErrorHandling)
            {
                return;
            }

            if (_errorOccuredCallback == null)
            {
                return;
            }

            exception = GetExceptionOverridden(exception, httpStatusCode);

            ErrorDescriber desc = _knownErrors.FirstOrDefault(item => item.ExceptionType == exception.GetType());

            ErrorDescriber errorDescription = desc ?? new ErrorDescriber(exception.GetType(), typeof(Dialog), ErrorFilterFlags.EnableErrorHandling);

            if ((filterFlags & errorDescription.FilterFlag) == errorDescription.FilterFlag)
            {
                return;
            }

            errorDescription.ErrorCode = errorCode;
            errorDescription.HttpStatusCode = httpStatusCode;
            if (retryAction != null)
            {
                if (successCallback != null)
                {
                    errorDescription.SuccessCallback = successCallback;
                    errorDescription.RetryAction = async x =>
                    {
                        var result = await retryAction(successCallback);
                        successCallback(result);
                    };
                }
            }

            _errorOccuredCallback(errorDescription);
        }
    }
}