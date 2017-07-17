using System;
using Android.OS;
using Android.Views;
using Java.Interop;
using Object = Java.Lang.Object;

namespace SalesApp.Droid.UI
{
    internal sealed class OnGlobalLayoutListenerImpl : Object, ViewTreeObserver.IOnGlobalLayoutListener
    {
        private readonly ViewTreeObserver _sender;

        public OnGlobalLayoutListenerImpl(ViewTreeObserver sender)
        {
            _sender = sender;
        }

        internal EventHandler Handler;

        public void OnGlobalLayout()
        {
            var eventHandler = Handler;

            if (eventHandler != null)
            {
                eventHandler(_sender, new EventArgs());
            }
        }
    }

    internal sealed class ViewTreeObserverCompat
    {
        private readonly ViewTreeObserver _observer;
        private WeakReference _implementor;

        private ViewTreeObserverCompat(ViewTreeObserver observer)
        {
            _observer = observer;
        }

        public event EventHandler GlobalLayoutCompat
        {
            add
            {
                EventHelper.AddEventHandler<ViewTreeObserver.IOnGlobalLayoutListener, OnGlobalLayoutListenerImpl>(
                    ref _implementor,
                    () => new OnGlobalLayoutListenerImpl(_observer),
                    listener => _observer.AddOnGlobalLayoutListener(listener),
                    implementor => { implementor.Handler += value; });
            }
            remove
            {
                EventHelper.RemoveEventHandler<ViewTreeObserver.IOnGlobalLayoutListener, OnGlobalLayoutListenerImpl>(
                    ref _implementor,
                    implementor => implementor.Handler == null,
                    listener =>
                    {
                        if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBean)
                        {
                            _observer.RemoveGlobalOnLayoutListener(listener);
                        }
                        else
                        {
                            _observer.RemoveOnGlobalLayoutListener(listener);
                        }
                    },
                    implementor => implementor.Handler -= value);
            }
        }

        public static ViewTreeObserverCompat From(ViewTreeObserver observer)
        {
            return new ViewTreeObserverCompat(observer);
        }
    }
}