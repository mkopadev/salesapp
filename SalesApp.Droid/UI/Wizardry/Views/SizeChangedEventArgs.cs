using System;

namespace SalesApp.Droid.UI.Wizardry.Views
{
    public class SizeChangedEventArgs : EventArgs
    {
        public SizeChangedEventArgs(int w, int h, int oldw, int oldh)
        {
            W = w;
            H = h;
            Oldw = oldw;
            Oldh = oldh;
        }

        public int W { get; set; }

        public int H { get; set; }

        public int Oldw { get; set; }

        public int Oldh { get; set; }
    }
}