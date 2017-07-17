using System;
using Android.OS;
using Android.Views;

namespace SalesApp.Droid.UI.Wizardry.Fragments
{
    public class FragmentInfo : WizardOverlayFragment
    {
        /// <summary>
        /// The key for the resource id in the bundle
        /// </summary>
        public const string ResourceIdBundleKey = "ResourceIdBundleKey";

        public event EventHandler ViewCreated;

        private int _resourceId;

        public View InflatedView { get; set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            this._resourceId = this.GetArgument<int>(ResourceIdBundleKey);

            this.InflatedView = inflater.Inflate(this._resourceId, container, false);
            if (this.ViewCreated != null)
            {
                this.ViewCreated(InflatedView,EventArgs.Empty);
            }

            return this.InflatedView;
        }

        public override bool Validate()
        {
            throw new NotImplementedException();
        }

        protected override void InitializeUI(bool isOnUiThread = false)
        {
            throw new NotImplementedException();
        }

        protected override void SetEventHandlers()
        {
            throw new NotImplementedException();
        }

        public override void UpdateUI(bool calledFromUiThread = false)
        {
            throw new NotImplementedException();
        }

        public override void SetViewPermissions()
        {
            throw new NotImplementedException();
        }

       
    }
}