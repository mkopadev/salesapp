using System;

namespace SalesApp.Droid.Components.UIComponents.Swipe
{
    public class SwipeEventArgs : EventArgs
    {
        public SwipeEventArgs(float deltaX, float deltaY)
        {
            this.DeltaX = deltaX;
            this.DeltaY = deltaY;
        }

        public float DeltaX { get; private set; } 

        public float DeltaY { get; private set; }
    }
}