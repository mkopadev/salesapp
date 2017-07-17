using System;
using Java.Lang;
using Object = Java.Lang.Object;

namespace SalesApp.Droid.UI
{
    public class LambdaRunnable : Object, IRunnable
    {
        private Action runnable;

        public LambdaRunnable(Action runnable)
        {
            this.runnable = runnable;
        }

        public void Run()
        {
            runnable();
        }
    }
}