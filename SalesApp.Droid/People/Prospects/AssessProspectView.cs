using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using Mkopa.Core;
using Mkopa.Core.Extensions;
using Mkopa.Core.Logging;
using Mkopa.Core.Services.DependancyInjection;

namespace MK.Solar.People.Prospects
{
    public class AssessProspectView : BaseView
    {
        public bool Need { get; set; }
        public bool Authority { get; set; }
        public bool Money { get; set; }

        IRegistrationCoordinator coordinator;

        

        private ToggleButton toggleNeed;
        private ToggleButton toggleAuthority;
        private ToggleButton toggleMoney;
        private TextView textScore;

        private ILog _log = Resolver.Instance.Get<ILog>();

        public AssessProspectView(ViewGroup root, Context context)
            : base(context)
        {
           _log.Initialize(this.GetType().FullName);
            if (context != null)
            {
                coordinator = (IRegistrationCoordinator) context;
            }


            if (root != null)
            {
                toggleNeed = root.FindViewById<ToggleButton>(Resource.Id.toggleNeed);
                toggleAuthority = root.FindViewById<ToggleButton>(Resource.Id.toggleAuthority);
                toggleMoney = root.FindViewById<ToggleButton>(Resource.Id.toggleMoney);
                textScore = root.FindViewById<TextView>(Resource.Id.textScore);
            }


             if (toggleNeed != null)
            {
                toggleNeed.Click += OnAssessmentChanged;
                toggleAuthority.Click += OnAssessmentChanged;
                toggleMoney.Click += OnAssessmentChanged;
            }

             if (coordinator != null && coordinator.btnNext != null)
             {
                 coordinator.btnNext.Enabled = false;
                 coordinator.btnNext.Click += OnNextTouched;
                 coordinator.btnCancel.Click += OnCancel;
             }
        }

        public EventHandler AssesmentChanged;

        protected virtual void OnAssessmentChanged(object sender, EventArgs e)
        {
            // Update score here: Hot, Warm, Cold
            var score = 0;
            coordinator.btnNext.Enabled = toggleAuthority.Checked || toggleMoney.Checked || toggleNeed.Checked;

            score = (toggleNeed.Checked ? 1 : 0) + (toggleAuthority.Checked ? 1 : 0) + (toggleMoney.Checked ? 1 : 0);
            string scoreText = "";
            switch (score)
            {
			case 0:
					scoreText = "";
                    coordinator.btnNext.Enabled = false;
                    break;
                case 1:
                    scoreText = "Cold";
                    break;
                case 2:
                    scoreText = "Warm";
                    break;
                case 3:
                    scoreText = "Hot";
                    break;
            }
            textScore.Text = scoreText;

            Need = toggleNeed.Checked;
            Authority = toggleAuthority.Checked;
            Money = toggleMoney.Checked;
        }

        public EventHandler ValidProspect;

        protected virtual void OnValidProspect(EventArgs e)
        {
            EventHandler handler = ValidProspect;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public EventHandler Cancel;

        protected virtual void OnCancel(object sender, EventArgs e)
        {
            EventHandler handler = Cancel;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        void OnNextTouched(object sender, EventArgs e)
        {
             OnValidProspect(EventArgs.Empty);
        }

    }
}