using System.Collections.Generic;
using System.Threading;

namespace SalesApp.Droid.Services
{
    public class TaskCancelationService
    {
        private readonly List<CancellationTokenSource> _tokenSources = new List<CancellationTokenSource>();

        public CancellationToken Token
        {
            get
            {
                if (_tokenSources.Count == 0 || _tokenSources[_tokenSources.Count - 1].IsCancellationRequested)
                {
                    _tokenSources.Add(new CancellationTokenSource());
                    return Token;
                }
                return _tokenSources[_tokenSources.Count - 1].Token;
            }
        }

        public void Cancel()
        {
            if (_tokenSources.Count == 0)
            {
                return;
            }
            _tokenSources[_tokenSources.Count - 1].Cancel(false);
        }

    }
}