using Android.App;
using Android.Content;
using Android.Telephony;
using Java.Lang;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Connectivity;

namespace SalesApp.Droid.Services.Connectivity
{
    public class AndroidSmsService : SmsServiceCore, ISmsService, ISmsServiceEventListener
    {
        private static readonly ILog Log = LogManager.Get(typeof (AndroidSmsService));

        private readonly Context _context;
        private readonly PendingIntent _sentPI;
        private readonly PendingIntent _deliveredPI;
        private SMSSentReceiver _smsSentBroadcastReceiver;
        private SMSDeliveredReceiver _smsDeliveredBroadcastReceiver;

        public static string Sent
        {
            get { return "SMS_SENT"; }
        }

        public static string Delivered
        {
            get { return "SMS_DELIVERED"; }
        }

        public AndroidSmsService()
        {
            Log.Verbose("Initialize AndroidSmsService");
            _context = SalesApplication.Instance.ApplicationContext;
            _sentPI = PendingIntent.GetBroadcast(_context, 0, new Intent(Sent), 0);
            _deliveredPI = PendingIntent.GetBroadcast(_context, 0, new Intent(Delivered), 0);
        }

        public bool SendSms(string phoneNumber, string message)
        {
            SmsManager smsManager = SmsManager.Default;
            try
            {
                // TODO fix this better
                try
                {
                    //UnregisterReceiver();
                }
                catch (Exception e)
                {
                    Log.Warning(e);
                }
                RegisterReceiver();
                Log.Verbose(string.Format("Sending [{0}] to: [{1}]", message, phoneNumber));
                smsManager.SendTextMessage(phoneNumber, null, message, _sentPI, _deliveredPI);
                return true;
            }
            catch (IllegalArgumentException iae)
            {
                Log.Error(iae);
                return false;
            }
       }

        /// <summary>
        /// When using this class, make sure the Receiver
        /// </summary>
        public void UnregisterReceiver()
        {
            Log.Verbose("UnregisterReceiver");
            _context.UnregisterReceiver(_smsSentBroadcastReceiver);
            _context.UnregisterReceiver(_smsDeliveredBroadcastReceiver);
        }

        public void RegisterReceiver()
        {
            Log.Verbose("RegisterReceiver");
            _smsSentBroadcastReceiver = new SMSSentReceiver();
            _smsSentBroadcastReceiver.ServiceCore = this;
            _smsDeliveredBroadcastReceiver = new SMSDeliveredReceiver();
            _smsDeliveredBroadcastReceiver.ServiceCore = this;

            // set the correct listeners
            _context.RegisterReceiver(_smsSentBroadcastReceiver, new IntentFilter(Sent));
            _context.RegisterReceiver(_smsDeliveredBroadcastReceiver, new IntentFilter(Delivered));

        }


        [BroadcastReceiver(Exported = true)] //, Permission = "//receiver/@android:android.permission.SEND_SMS")]
        public class SMSSentReceiver : BroadcastReceiver
        {
            internal SmsServiceCore ServiceCore { get; set; }

            public override void OnReceive(Context context, Intent intent)
            {
                Log.Verbose("SMSSentReceiver.OnReceive : " + ResultCode);
                switch ((int) ResultCode)
                {
                    case (int) Result.Ok:
                        ServiceCore.OnSmsSent();
                        break;
                    case (int) SmsResultError.GenericFailure:
                        ServiceCore.OnSmsFailed();
                        break;
                    case (int) SmsResultError.NoService:
                        ServiceCore.OnSmsFailed();
                        break;
                    case (int) SmsResultError.NullPdu:
                        ServiceCore.OnSmsFailed();
                        break;
                    case (int) SmsResultError.RadioOff:
                        ServiceCore.OnSmsFailed();
                        break;
                    default:
                        ServiceCore.OnSmsFailed();
                        break;
                }

                try
                {
                    context.UnregisterReceiver(this);
                }
                catch (System.Exception e)
                {
                    Log.Warning(e);
                }
            }
        }

        [BroadcastReceiver(Exported = true)] //, Permission = "//receiver/@android:android.permission.SEND_SMS")]
        public class SMSDeliveredReceiver : BroadcastReceiver
        {
            internal SmsServiceCore ServiceCore { get; set; }

            public override void OnReceive(Context context, Intent intent)
            {
                Log.Verbose("SMSDeliveredReceiver.OnReceive : " + ResultCode);
                switch ((int) ResultCode)
                {
                    case (int) Result.Ok:
                        ServiceCore.OnSmsReceived();
                        break;
                    case (int) Result.Canceled:
                        ServiceCore.OnSmsFailed();
                        break;
                }
                try
                {
                    context.UnregisterReceiver(this);
                }
                catch (System.Exception e)
                {
                    Log.Warning(e);
                }
            }
        }
    }
}