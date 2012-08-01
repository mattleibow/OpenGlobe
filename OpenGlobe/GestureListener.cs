namespace OpenGlobe
{
    using System;

    using Android.Content;
    using Android.Views;

    public class GestureListener : Java.Lang.Object,
                                   GestureDetector.IOnDoubleTapListener,
                                   GestureDetector.IOnGestureListener,
                                   ScaleGestureDetector.IOnScaleGestureListener
    {
        private readonly GestureDetector gestureScanner;
        private readonly ScaleGestureDetector scaleGestureScanner;

        public GestureListener(Context context)
        {
            this.numberOfPointers = 0;
            this.gestureScanner = new GestureDetector(this);
            this.scaleGestureScanner = new ScaleGestureDetector(context, this);
        }

        public bool IsScaleInProgress
        {
            get
            {
                return this.scaleGestureScanner.IsInProgress;
            }
        }

        #region Implementation of IOnScaleGestureListener

        public bool OnScale(ScaleGestureDetector detector)
        {
            var args = new ScaleGestureDetector.ScaleEventArgs(false, detector);

            var handler = this.Scale;
            if (handler != null)
            {
                handler(this, args);
            }

            return args.Handled;
        }

        public bool OnScaleBegin(ScaleGestureDetector detector)
        {
            var args = new ScaleGestureDetector.ScaleBeginEventArgs(true, detector);

            var handler = this.ScaleBegin;
            if (handler != null)
            {
                handler(this, args);
            }

            return args.Handled;
        }

        public void OnScaleEnd(ScaleGestureDetector detector)
        {
            var args = new ScaleGestureDetector.ScaleEndEventArgs(detector);

            var handler = this.ScaleEnd;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        #endregion

        #region Implementation of IOnDoubleTapListener

        public virtual bool OnDoubleTap(MotionEvent e)
        {
            var args = new View.TouchEventArgs(false, e);

            var handler = this.DoubleTap;
            if (handler != null)
            {
                handler(this, args);
            }

            return args.Handled;
        }

        public virtual bool OnDoubleTapEvent(MotionEvent e)
        {
            var args = new View.TouchEventArgs(false, e);

            var handler = this.DoubleTapEvent;
            if (handler != null)
            {
                handler(this, args);
            }

            return args.Handled;
        }

        public virtual bool OnSingleTapConfirmed(MotionEvent e)
        {
            var args = new View.TouchEventArgs(false, e);

            var handler = this.SingleTapConfirmed;
            if (handler != null)
            {
                handler(this, args);
            }

            return args.Handled;
        }

        #endregion

        #region Implementation of IOnGestureListener

        public virtual bool OnDown(MotionEvent e)
        {
            var args = new View.TouchEventArgs(false, e);

            var handler = this.Down;
            if (handler != null)
            {
                handler(this, args);
            }

            return args.Handled;
        }

        public virtual bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            return this.OnFling(new GestureDetector.FlingEventArgs(false, e1, e2, velocityX, velocityY));
        }

        public virtual void OnLongPress(MotionEvent e)
        {
            var args = new View.TouchEventArgs(false, e);

            var handler = this.LongPress;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public virtual bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return this.OnScroll(new GestureDetector.ScrollEventArgs(false, e1, e2, distanceX, distanceY));
        }

        public virtual void OnShowPress(MotionEvent e)
        {
            var args = new View.TouchEventArgs(false, e);

            var handler = this.ShowPress;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public virtual bool OnSingleTapUp(MotionEvent e)
        {
            var args = new View.TouchEventArgs(false, e);

            var handler = this.SingleTapUp;
            if (handler != null)
            {
                handler(this, args);
            }

            return args.Handled;
        }

        #endregion

        #region Implementation of IScrollFlingListener

        public bool OnFling(GestureDetector.FlingEventArgs e)
        {
            var handler = this.FlingGesture;
            if (handler != null)
            {
                handler(this, e);
            }

            return e.Handled;
        }

        public bool OnScroll(GestureDetector.ScrollEventArgs e)
        {
            var handler = this.Scroll;
            if (handler != null)
            {
                handler(this, e);
            }

            return e.Handled;
        }

        #endregion

        public bool OnMove(View.TouchEventArgs e)
        {
            var handler = this.Move;
            if (handler != null)
            {
                handler(this, e);
            }

            return e.Handled;
        }

        public void OnPointerCountChanged(PointerCountChangedEventArgs e)
        {
            var handler = this.PointerCountChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private int numberOfPointers;

        public virtual bool OnTouchEvent(MotionEvent e)
        {
            int pointerCount = e.PointerCount;
            switch (e.Action)
            {
                case MotionEventActions.Up:
                    pointerCount = 0;
                    break;
                case MotionEventActions.Down:
                    pointerCount = 1;
                    break;
                case MotionEventActions.Move:
                    this.OnMove(new View.TouchEventArgs(false, e));
                    break;
            }

            if (pointerCount != this.numberOfPointers)
            {
                this.OnPointerCountChanged(new PointerCountChangedEventArgs(this.numberOfPointers, pointerCount, e));
                this.numberOfPointers = pointerCount;
            }

            var scale = this.scaleGestureScanner.OnTouchEvent(e);
            var gestures = this.gestureScanner.OnTouchEvent(e);

            return gestures || scale;
        }

        public event EventHandler<View.TouchEventArgs> DoubleTap;

        public event EventHandler<View.TouchEventArgs> DoubleTapEvent;

        public event EventHandler<View.TouchEventArgs> SingleTapConfirmed;

        public event EventHandler<View.TouchEventArgs> Down;

        public event EventHandler<View.TouchEventArgs> Move;

        public event EventHandler<PointerCountChangedEventArgs> PointerCountChanged;

        public event EventHandler<View.TouchEventArgs> LongPress;

        public event EventHandler<View.TouchEventArgs> ShowPress;

        public event EventHandler<View.TouchEventArgs> SingleTapUp;

        public event EventHandler<GestureDetector.ScrollEventArgs> Scroll;

        public event EventHandler<GestureDetector.FlingEventArgs> FlingGesture;

        public event EventHandler<ScaleGestureDetector.ScaleEventArgs> Scale;

        public event EventHandler<ScaleGestureDetector.ScaleBeginEventArgs> ScaleBegin;

        public event EventHandler<ScaleGestureDetector.ScaleEndEventArgs> ScaleEnd;
    }
}