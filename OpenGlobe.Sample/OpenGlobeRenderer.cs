namespace OpenGlobe.Sample
{
    using System;
    using System.IO;

    using Android.Content;
    using Android.Content.Res;
    using Android.Opengl;

    using Javax.Microedition.Khronos.Egl;
    using Javax.Microedition.Khronos.Opengles;

    using OpenGlobe;

    using OpenTK;

    public class OpenGlobeRenderer : Java.Lang.Object, GLSurfaceView.IRenderer
    {
        private readonly Context context;
        private readonly WhirlyGlobeRenderEngine engine;
        private string nextTexture;

        public OpenGlobeRenderer(Context context)
        {
            this.context = context;
            this.engine = new WhirlyGlobeRenderEngine();
        }

        public void UpdateTexture(string fn)
        {
            this.nextTexture = fn;
        }

        public Vector2 ViewPortSize
        {
            get
            {
                return this.engine.ViewPortSize;
            }

            set
            {
                this.engine.ViewPortSize = value;
            }
        }

        public float FieldOfView
        {
            get
            {
                return this.engine.FieldOfView;
            }

            set
            {
                this.engine.FieldOfView = value;
            }
        }

        #region IRenderer Members

        public void OnDrawFrame(IGL10 gl)
        {
            if (this.nextTexture != null)
            {
                var tmp = this.nextTexture;
                this.nextTexture = null;
                Utils.BindTextureAsset(this.engine.Planet.TextureId, this.context, tmp);
            }

            this.engine.RenderFrame();
        }

        public void OnSurfaceChanged(IGL10 gl, int width, int height)
        {
            this.engine.UpdateSurface(width, height);
        }

        public void OnSurfaceCreated(IGL10 gl, EGLConfig config)
        {
            this.engine.CreateSurface();

            this.SetupPlanet();
        }

        #endregion

        public void Rotate(Vector2 distance, float density)
        {
            this.engine.Rotate(distance, density);
        }

        public void Rotate(float angle, float density)
        {
            this.engine.Rotate(angle, density);
        }

        private void SetupPlanet()
        {
            var texOffset = new Vector2 { X = 0.25F, Y = 0F };
            this.engine.Planet.TextureOffset = texOffset;

            // overlay bitmap
            int flareSource = Utils.CreateResourceTexture(this.context, Resource.Drawable.globeOverlay);

            // overlay font
            using (Stream stream = this.context.Assets.Open("TimesNewRoman.bff", Access.Buffer))
            {
                this.CreateOverlays(flareSource, new TexFont(stream));
            }

            // earth texture
            this.engine.Planet.TextureId = Utils.CreateAssetTexture(this.context, this.nextTexture);
        }

        private void CreateOverlays(int mFlareSource, TexFont texFont)
        {
            // zero
            var overlay = new GlobeOverlay
                {
                    Position = this.engine.Planet.GetGeoCoord(0, 0),
                    Label = "Zero",
                    DotTexture = mFlareSource,
                    Font = texFont
                }; 
            this.engine.Planet.Overlays.Add(overlay);
            
            // melborne : 37.7833� S, 144.9667� E
            overlay = new GlobeOverlay
                {
                    Position = this.engine.Planet.GetGeoCoord(-144.9667F, -37.7833F),
                    Label = "Melborne",
                    DotTexture = mFlareSource,
                    Font = texFont
                };
            this.engine.Planet.Overlays.Add(overlay);

            // capeTown : 33.9767� S, 18.4244� E
            overlay = new GlobeOverlay
                {
                    Position = this.engine.Planet.GetGeoCoord(-18.4244F, -33.9767F),
                    Label = "Cape Town",
                    DotTexture = mFlareSource,
                    Font = texFont
                };
            this.engine.Planet.Overlays.Add(overlay);

            // egypt 30.0100� N, 31.2100� E
            overlay = new GlobeOverlay
                {
                    Position = this.engine.Planet.GetGeoCoord(-31.2100F, 30.0100F),
                    Label = "Egypt",
                    DotTexture = mFlareSource,
                    Font = texFont
                };
            this.engine.Planet.Overlays.Add(overlay);

            // newYork : 40.7142� N, 74.0064� W
            overlay = new GlobeOverlay
                {
                    Position = this.engine.Planet.GetGeoCoord(74.0064F, 40.7142F),
                    Label = "New York",
                    DotTexture = mFlareSource,
                    Font = texFont
                };
            this.engine.Planet.Overlays.Add(overlay);
        }

        public GlobeOverlay GetOverlayAt(Vector2 position)
        {
            // get the size of the globe
            var earthSize = this.engine.GetEarthSizeOnScreen();

            // get projection.model matrix
            var modelMatrix = this.engine.GetModelMatrix().ToArray();
            var projMatrix = this.engine.GetFrustumMatrix().ToArray();
            var viewport = new[] { 0, 0, (int)this.ViewPortSize.X, (int)this.ViewPortSize.Y };

            foreach (var overlay in this.engine.Planet.Overlays)
            {
                var screenLoc = MiniGlu.Project(overlay.Position, modelMatrix, projMatrix, viewport);
                screenLoc.Y = viewport[3] - screenLoc.Y;

                if (screenLoc.Z <= WhirlyGlobeRenderEngine.PlanetDistance)
                {
                    // get dot size/pos on screen
                    float z = Math.Abs(screenLoc.Z - WhirlyGlobeRenderEngine.PlanetDistance);
                    var dotScale = overlay.DotScale * z;
                    var dotSizeX = earthSize.X * dotScale.X;
                    var dotSizeY = earthSize.Y * dotScale.Y;
                    var dotPos = new Vector2(screenLoc.X, screenLoc.Y);

                    // check for hit
                    var relX = Math.Abs(position.X - dotPos.X) + dotSizeX / 2F;
                    var relY = Math.Abs(position.Y - dotPos.Y) + dotSizeY / 2F;
                    if (relX < dotSizeX && relY < dotSizeY)
                    {
                        return overlay;
                    }
                }
            }

            return null;
        }
    }
}