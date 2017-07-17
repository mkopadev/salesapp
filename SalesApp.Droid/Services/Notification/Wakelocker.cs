using Android.Content;
using Android.OS;

namespace SalesApp.Droid.Services.Notification
{
    public static class Wakelocker
    {
        private static PowerManager.WakeLock _wakeLock;

        public static void Acquire(Context context,string tag)
        {
            Release();
            PowerManager powerManager = context.GetSystemService(Context.PowerService) as PowerManager;
            if (powerManager == null)
            {
                return;
            }
            _wakeLock =
                powerManager.NewWakeLock(
                    WakeLockFlags.Full | WakeLockFlags.AcquireCausesWakeup | WakeLockFlags.OnAfterRelease, tag);
        }

        public static void Release()
        {
            if (_wakeLock == null)
            {
                return;
            }
            _wakeLock.Release();
            _wakeLock = null;
        }
    }
}