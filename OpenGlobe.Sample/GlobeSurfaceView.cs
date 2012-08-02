namespace OpenGlobe.Sample
{
    using System;

    using Android.Content;
    using Android.Graphics;
    using Android.Opengl;
    using Android.Util;
    using Android.Views;

    using OpenTK;

    public sealed class GlobeSurfaceView : GLSurfaceView
    {
        private const float MinFOV = 5.0f;

        private const float MaxFOV = 100.0f;

        private float density;
        private float startFOV;
        private OpenGlobeRenderer renderer;
        private GestureListener gestureListener;

        private bool canScroll;

        private float lastAngle;

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

        public void UpdateTexture(string fn)
        {
            this.renderer.UpdateTexture(fn);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            return this.gestureListener.OnTouchEvent(e) || base.OnTouchEvent(e);
        }

        private void Init(Context context)
        {
            this.density = context.Resources.DisplayMetrics.Density;
            this.SetEGLConfigChooser(8, 8, 8, 8, 16, 0);
            this.Holder.SetFormat(Format.Rgba8888);
            
            this.gestureListener = new GestureListener(context);
            this.renderer = new OpenGlobeRenderer(context);

            this.SetRenderer(this.renderer);

            this.gestureListener.Scroll += OnScroll;
            this.gestureListener.Scale += OnScale;
            this.gestureListener.Down += OnDown;
            this.gestureListener.Move += OnMove;

            this.gestureListener.SingleTapUp += OnSingleTapUp;
            this.gestureListener.PointerCountChanged += OnPointerCountChanged;
        }

        private void OnSingleTapUp(object sender, TouchEventArgs e)
        {
            var overlay = this.renderer.GetOverlayAt(new Vector2(e.Event.GetX(), e.Event.GetY()));
        }

        private void OnMove(object sender, TouchEventArgs e)
        {
            if (e.Event.PointerCount > 1)
            {
                var angle = GetRotation(e.Event);
                this.renderer.Rotate(this.lastAngle - angle, this.density);
                this.lastAngle = angle;
            }
        }

        private static float GetRotation(MotionEvent e)
        {
            var deltaX = e.GetX(0) - e.GetX(1);
            var deltaY = e.GetY(0) - e.GetY(1);
            var radians = (float)Math.Atan2(deltaY, deltaX);

            return MathHelper.RadiansToDegrees(radians);
        }

        private void OnPointerCountChanged(object sender, PointerCountChangedEventArgs e)
        {
            this.canScroll = this.canScroll && e.NewCount != 1;
            if (e.OldCount < e.NewCount && e.NewCount > 1)
            {
                this.lastAngle = GetRotation(e.Event);
                this.startFOV = this.renderer.FieldOfView;
            }
        }

        private void OnDown(object sender, TouchEventArgs e)
        {
            this.canScroll = true;
        }

        private void OnScale(object sender, ScaleGestureDetector.ScaleEventArgs ev)
        {
            var zoom = ev.Detector.PreviousSpan / ev.Detector.CurrentSpan;
            var current = this.startFOV * zoom;
            if (current >= MinFOV && current <= MaxFOV)
            {
                this.renderer.FieldOfView = current;
            }
        }

        private void OnScroll(object sender, GestureDetector.ScrollEventArgs e)
        {
            if (this.canScroll && !this.gestureListener.IsScaleInProgress)
            {
                var distance = new Vector2(-e.DistanceX, -e.DistanceY);

                this.renderer.Rotate(distance, this.density);
                e.Handled = true;
            }
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            this.renderer.ViewPortSize = new Vector2(w, h);
        }
    }
}