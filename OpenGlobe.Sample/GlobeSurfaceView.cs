namespace OpenGlobe.Sample
{
    using System;

    using Android.Content;
    using Android.Graphics;
    using Android.Opengl;
    using Android.Runtime;
    using Android.Util;
    using Android.Views;

    using OpenTK;

    public sealed class GlobeSurfaceView : GLSurfaceView
    {
        private const float MinFOV = 5.0f;

        private const float MaxFOV = 100.0f;

        private static float mZoom;

        private static float mLastZoom;

        private static float mOldDist;

        private static float mStartFOV;

        private static float mCurrentFOV;

        private static Vector2 mCurrentTouchPoint;

        private static Vector2 mMidPoint;

        private static Vector2 mLastTouchPoint;

        private float density;

        private OpenGlobeRenderer renderer;

        private TransformMode mGesture = TransformMode.None;

        public GlobeSurfaceView(Context context)
            : base(context)
        {
            this.Init(context);
        }

        public GlobeSurfaceView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            this.Init(context);
        }

        private void Init(Context context)
        {
            this.density = context.Resources.DisplayMetrics.Density;
            this.SetEGLConfigChooser(8, 8, 8, 8, 16, 0);
            this.Holder.SetFormat(Format.Rgba8888);

            this.renderer = new OpenGlobeRenderer(context);

            this.SetRenderer(this.renderer);
        }

        public void UpdateTexture(string fn)
        {
            this.renderer.UpdateTexture(fn);
        }

        public override bool OnTouchEvent(MotionEvent ev)
        {
            bool retval = true;

            switch (ev.Action & MotionEventActions.Mask)
            {
                case MotionEventActions.Down:
                    this.mGesture = TransformMode.Drag;
                    mLastTouchPoint.X = mCurrentTouchPoint.X = ev.GetX();
                    mLastTouchPoint.Y = mCurrentTouchPoint.Y = ev.GetY();
                    break;

                case MotionEventActions.Up:
                case MotionEventActions.PointerUp:
                    this.mGesture = TransformMode.None;
                    mLastTouchPoint.X = ev.GetX();
                    mLastTouchPoint.Y = ev.GetY();
                    break;

                case MotionEventActions.PointerDown:
                    mOldDist = Spacing(ev);
                    mMidPoint = MidPoint(ev);

                    this.mGesture = TransformMode.Zoom;
                    mStartFOV = this.renderer.FieldOfView;

                    mLastTouchPoint.X = mMidPoint.X;
                    mLastTouchPoint.Y = mMidPoint.Y;
                    mCurrentTouchPoint.X = mMidPoint.X;
                    mCurrentTouchPoint.Y = mMidPoint.Y;

                    break;

                case MotionEventActions.Move:
                    if (this.mGesture == TransformMode.Drag)
                    {
                        retval = this.HandleDragGesture(ev);
                    }
                    else if (this.mGesture == TransformMode.Zoom)
                    {
                        retval = this.HandlePinchGesture(ev);
                    }
                    break;
            }

            return retval;
        }

        public bool HandleDragGesture(MotionEvent ev)
        {
            mLastTouchPoint.X = mCurrentTouchPoint.X;
            mLastTouchPoint.Y = mCurrentTouchPoint.Y;

            mCurrentTouchPoint.X = ev.GetX();
            mCurrentTouchPoint.Y = ev.GetY();

            this.renderer.Rotate(mCurrentTouchPoint, mLastTouchPoint, this.density);

            return true;
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            this.renderer.ViewPortSize = new Vector2(w, h);
        }

        public bool HandlePinchGesture(MotionEvent ev)
        {
            float newDist = Spacing(ev);

            mZoom = mOldDist / newDist;

            if (mZoom > mLastZoom)
            {
                mLastZoom = mZoom;
            }
            else if (mZoom <= mLastZoom)
            {
                mLastZoom = mZoom;
            }

            mCurrentFOV = mStartFOV * mZoom;
            mLastTouchPoint = mMidPoint;

            if (mCurrentFOV >= MinFOV && mCurrentFOV <= MaxFOV)
            {
                this.renderer.FieldOfView = mCurrentFOV;

                return true;
            }

            return false;
        }

        private static float Spacing(MotionEvent e)
        {
            float x = e.GetX(0) - e.GetX(1);
            float y = e.GetY(0) - e.GetY(1);
            return (float)Math.Sqrt(x * x + y * y);
        }

        private static Vector2 MidPoint(MotionEvent e)
        {
            float x = e.GetX(0) + e.GetX(1);
            float y = e.GetY(0) + e.GetY(1);
            return new Vector2(x / 2, y / 2);
        }
    }
}