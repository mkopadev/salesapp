using Java.Lang;

namespace SalesApp.Droid.Components
{
    public class JavaObjectHandler : Object
    {
        private System.Object _instance;
        public JavaObjectHandler(System.Object instance) { _instance = instance; }
        public System.Object Instance { get { return _instance; } }
    }
}