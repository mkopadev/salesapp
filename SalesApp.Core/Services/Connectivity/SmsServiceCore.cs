using System;
using System.Linq;

namespace SalesApp.Core.Services.Connectivity
{
    public abstract class SmsServiceCore : ISmsServiceEventListener
    {
        public delegate void SmsSentEventHandler(object sender, EventArgs e);

        private SmsSentEventHandler _smsSentEvent;
        public event SmsSentEventHandler SmsSent
        {
            add
            {
                if (_smsSentEvent == null || !_smsSentEvent.GetInvocationList().Contains(value))
                {
                    _smsSentEvent += value;
                }
            }
            remove
            {
                _smsSentEvent -= value;
            }
        }

        public delegate void SmsReceivedEventHandler(object sender, EventArgs e);

        private SmsReceivedEventHandler _smsReceivedEvent;
        public event SmsReceivedEventHandler SmsReceived
        {
            add
            {
                if (_smsReceivedEvent == null || !_smsReceivedEvent.GetInvocationList().Contains(value))
                {
                    _smsReceivedEvent += value;
                }
            }
            remove
            {
                _smsReceivedEvent -= value;
            }
        }

        public delegate void SmsFailedEventHandler(object sender, EventArgs e);

        private SmsFailedEventHandler _smsFailedEvent;
        public event SmsFailedEventHandler SmsFailed
        {
            add
            {
                if (_smsFailedEvent == null || !_smsFailedEvent.GetInvocationList().Contains(value))
                {
                    _smsFailedEvent += value;
                }
            }
            remove
            {
                _smsFailedEvent -= value;
            }
        }

        public virtual void OnSmsSent()
        {
            var handler = _smsSentEvent;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public virtual void OnSmsReceived()
        {
            var handler = _smsReceivedEvent;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public virtual void OnSmsFailed()
        {
            var handler = _smsFailedEvent;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}