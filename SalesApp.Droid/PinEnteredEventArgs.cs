using System;

namespace SalesApp.Droid
{
	public class PinChangedEventArgs : EventArgs
	{
		public PinChangedEventArgs (string pin)
		{
			Pin = pin;
		}

		public string Pin {
			get;
			private set;
		}
	}
}


