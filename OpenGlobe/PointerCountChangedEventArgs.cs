namespace OpenGlobe
{
    using System;

    using Android.Views;

    public class PointerCountChangedEventArgs : EventArgs
    {
        public PointerCountChangedEventArgs(int oldCount, int newCount, MotionEvent motionEvent)
        {
            this.OldCount = oldCount;
            this.NewCount = newCount;
            Event = motionEvent;
        }

        public int OldCount { get; private set; }
        public int NewCount { get; private set; }

        public MotionEvent Event { get; private set; }
    }
}