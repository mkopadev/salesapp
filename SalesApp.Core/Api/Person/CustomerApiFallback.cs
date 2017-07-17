using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.Chama;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Services.Connectivity;

namespace SalesApp.Core.Api.Person
{
    public interface ICustomerApiFallback
    {
        // TODO make this generic
        event SmsServiceCore.SmsSentEventHandler SmsSentEvent;

        event SmsServiceCore.SmsReceivedEventHandler SmsReceivedEvent;

        event SmsServiceCore.SmsFailedEventHandler SmsFailedEvent;

        Task<bool> RegisterCustomer(Customer customer);
    }

    public class CustomerApiFallback : ApiFallbackBase, ICustomerApiFallback
    {
        private SmsServiceCore.SmsReceivedEventHandler _smsReceivedEvent;

        // TODO make this generic
        private SmsServiceCore.SmsSentEventHandler _smsSentEvent;

        private SmsServiceCore.SmsFailedEventHandler _smsFailedEvent;

        public event SmsServiceCore.SmsSentEventHandler SmsSentEvent
        {
            add
            {
                if (this._smsSentEvent == null || !this._smsSentEvent.GetInvocationList().Contains(value))
                {
                    this._smsSentEvent += value;
                }
            }

            remove
            {
                _smsSentEvent -= value;
            }
        }

        public event SmsServiceCore.SmsReceivedEventHandler SmsReceivedEvent
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

        public event SmsServiceCore.SmsFailedEventHandler SmsFailedEvent
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

        public CustomerApiFallback()
        {
        }

        public CustomerApiFallback(ISmsService smsService) : base(smsService)
        {
        }

        public Task<bool> RegisterCustomer(Customer customer)
        {
            // register needed handlers
            SMSService.SmsSent += _smsSentEvent;
            SMSService.SmsReceived += _smsReceivedEvent;
            SMSService.SmsFailed += _smsFailedEvent;

            int group = this.GetSelectedGroup(customer);

            // convert customer to CSV, this can be done nicer I assume
            string content = string.Format(
                "M1;{0};{1};{2};{3};{4};{5};{6};{7};{8}",
                customer.FirstName,
                customer.LastName,
                customer.Phone,
                customer.NationalId,
                customer.Product.ProductTypeId,
                customer.Product.SerialNumber,
                (int)DataChannel.Fallback,
                customer.DsrPhone,
                customer.IsAdditionalProduct ? "1" : "0");

            if (group > 0)
            {
                content += string.Format(";{0}", group);
            }

            var result = this.PostString(content);
            return Task.FromResult(result.Success);
        }

        private int GetSelectedGroup(Customer customer)
        {
            string groupJson = customer.Groups;

            if (string.IsNullOrEmpty(groupJson))
            {
                return 0;
            }

            List<GroupKeyValue> groupSelection = JsonConvert.DeserializeObject<List<GroupKeyValue>>(groupJson);

            if (groupSelection == null || groupSelection.Count == 0)
            {
                return 0;
            }

            int result;
            int.TryParse(groupSelection[groupSelection.Count - 1].Value, out result);
            return result;
        }
    }
}