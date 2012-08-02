namespace OpenGlobe
{
    using System;

    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.ES10;

    public class WhirlyGlobeRenderEngine
    {
        public const float ClippingNear = 0.001f;
        public const float ClippingDistance = 100F;
        public const float PlanetDistance = 15.0f;
        public const float DefaultFieldOfView = 30.0F;
        private const All FillLight1 = All.Light1;
        private const All FillLight2 = All.Light2;

        private Vector3 mEyeposition;
        private Matrix4 rotation;

        public WhirlyGlobeRenderEngine()
        {
            this.mEyeposition = new Vector3(0.0f, 0.0f, 0.0f);
            this.rotation = Matrix4.Identity;

            this.Planet = new Planet(180, 180);
            this.Planet.Position = new Vector3(0.0f, 0.0f, -PlanetDistance);
        }

        public float FieldOfView { get; set; }

        public Planet Planet { get; set; }

        public Vector2 ViewPortSize { get; set; }

        private void SetClipping()
        {
            GL.Viewport(0, 0, (int)this.ViewPortSize.X, (int)this.ViewPortSize.Y);

            GL.MatrixMode(All.Projection);
            GL.LoadIdentity();

            GL.Enable(All.Normalize);

            // Set the OpenGL projection matrix
            GL.MultMatrix(this.GetFrustumMatrix().ToArray());
            GL.MatrixMode(All.Modelview);
        }

        public Matrix4 GetFrustumMatrix()
        {
            float size = this.GetFieldOfViewSize();
            float aspectRatio = this.ViewPortSize.X / this.ViewPortSize.Y;

            var frustum = Matrix4.CreatePerspectiveOffCenter(
                -size, size, -size / aspectRatio, size / aspectRatio, ClippingNear, ClippingDistance);

            return frustum;
        }

        public float GetFieldOfViewSize()
        {
            var T = (float)Math.Tan(this.FieldOfView / 180F);
            return ClippingNear * T;
        }

        public Matrix4 GetModelMatrix()
        {
            var translation = Matrix4.CreateTranslation(this.Planet.Position.X, this.Planet.Position.Y, this.Planet.Position.Z);
            return Matrix4.Mult(this.rotation, translation);
        }

        /// <summary>
        /// Set up any lighting. 
        /// This is performed every time the surface changes.
        /// </summary>
        private void InitLighting()
        {
            // lights go here
            GL.Light(FillLight1, All.Position, Color4.White.ToArray());
            GL.Light(FillLight1, All.Diffuse, Color4.White.ToArray(0.8F));

            GL.Light(FillLight2, All.Position, Color4.White.ToArray());
            GL.Light(FillLight2, All.Specular, Color4.LightSkyBlue.ToArray());
            GL.Light(FillLight2, All.Diffuse, Color4.White.ToArray(0.8F));

            // materials go here
            GL.Material(All.FrontAndBack, All.Diffuse, Color4.SkyBlue.ToArray());
            GL.Material(All.FrontAndBack, All.Specular, Color4.White.ToArray());
            GL.Material(All.FrontAndBack, All.Shininess, 25);

            GL.LightModel(All.LightModelTwoSide, 0.0f);

            GL.Enable(All.Lighting);
            GL.Enable(FillLight1);
            GL.Enable(FillLight2);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        }

        /// <summary>
        /// This performs the actual drawing of the globe.
        /// </summary>
        private void RenderPlanet()
        {
            GL.PushMatrix();
            GL.MultMatrix(this.GetModelMatrix().ToArray());
            this.Planet.Render(this.ViewPortSize, this.FieldOfView);
            GL.PopMatrix();
        }

        /// <summary>
        /// Rotate the globe.
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="density"></param>
        public void Rotate(Vector2 distance, float density)
        {
            // the amount moved by the user
            float deltaX = distance.X / density / 4F;
            float deltaY = distance.Y / density / 4F;

            // get rotations
            var rotationY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(deltaX));
            var rotationX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(deltaY));

            // create temp matrix
            var afterY = Matrix4.Mult(rotationY, Matrix4.Identity);
            var afterX = Matrix4.Mult(rotationX, afterY);

            // apply temps to global rotation
            this.rotation = Matrix4.Mult(this.rotation, afterX);
        }

        public void Rotate(float angle, float density)
        {
            // the amount moved by the user
            float delta = angle / density;

            // get rotation
            var rot = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(delta));

            // apply temps to global rotation
            this.rotation = Matrix4.Mult(this.rotation, rot);
        }

        /// <summary>
        /// Draw the current frame.
        /// </summary>
        public void RenderFrame()
        {
            // clear the view
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // set up "camera"
            this.SetClipping();

            // set up drawing and lighting
            GL.MatrixMode(All.Modelview);
            GL.Enable(All.Lighting);
            GL.Enable(All.Blend);
            GL.BlendFunc(All.SrcAlpha, All.OneMinusSrcAlpha);

            // remove all material settings
            GL.Material(All.FrontAndBack, All.Diffuse, Color4.White.ToArray());
            GL.Material(All.FrontAndBack, All.Specular, Color4.White.ToArray());

            GL.PushMatrix();
            // move view point
            GL.Translate(-this.mEyeposition.X, -this.mEyeposition.Y, -this.mEyeposition.Z);
            // draw
            this.RenderPlanet();
            GL.PopMatrix();
        }

        /// <summary>
        /// Notify OpenGL that the drawing surface has changed slightly.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void UpdateSurface(int width, int height)
        {
            this.ViewPortSize = new Vector2(width, height);

            GL.Viewport(0, 0, width, height);

            this.SetClipping();
        }

        /// <summary>
        /// Create the drawing surface for the first time.
        /// </summary>
        public void CreateSurface()
        {
            GL.Disable(All.Dither);

            GL.Hint(All.PerspectiveCorrectionHint, All.Fastest);

            GL.Enable(All.CullFace);
            GL.CullFace(All.Back);
            GL.ShadeModel(All.Smooth);
            GL.Enable(All.DepthTest);

            this.InitLighting();
            this.FieldOfView = DefaultFieldOfView;
        }

        public Vector2 GetEarthSizeOnScreen()
        {
            var modelMatrix = Matrix4.Identity.ToArray();
            var projMatrix = this.GetFrustumMatrix().ToArray();
            var viewport = new[] { 0, 0, (int)this.ViewPortSize.X, (int)this.ViewPortSize.Y };

            var points = new[] { // mid points : 
                                   this.Planet.Position.Offset(y: +this.Planet.Radius * this.Planet.Squash), // top
                                   this.Planet.Position.Offset(y: -this.Planet.Radius * this.Planet.Squash), // bottom
                                   this.Planet.Position.Offset(x: -this.Planet.Radius), // left
                                   this.Planet.Position.Offset(x: +this.Planet.Radius) // right
                               };

            var newPoints = new Vector2[4];

            for (int i = 0; i < 4; i++)
            {
                var screenLoc = MiniGlu.Project(points[i], modelMatrix, projMatrix, viewport);
                screenLoc.Y = viewport[3] - screenLoc.Y;

                newPoints[i] = screenLoc.Xy;
            }

            var width = Math.Abs(newPoints[2].X-newPoints[3].X);
            var height = Math.Abs(newPoints[0].Y-newPoints[1].Y);

            return new Vector2(width, height);
        }
    }
}