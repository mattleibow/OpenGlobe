namespace OpenGlobe
{
    using System;

    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.ES10;

    public class WhirlyGlobeRenderEngine
    {
        private const float ClippingNear = 0.1f;
        private const float ClippingDistance = 20F;
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
            this.Planet.Position = new Vector3(0.0f, 0.0f, -10.0f);
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
            float aspectRatio = this.ViewPortSize.X / this.ViewPortSize.Y;
            float size = ClippingNear * (float)(Math.Tan(this.FieldOfView / 180));
            GL.Frustum(-size, size, -size / aspectRatio, size / aspectRatio, ClippingNear, ClippingDistance);
            GL.MatrixMode(All.Modelview);
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

            GL.Translate(this.Planet.Position.X, this.Planet.Position.Y, this.Planet.Position.Z);
            GL.MultMatrix(this.rotation.ToArray());

            float size = ClippingNear / (float)(Math.Tan(this.FieldOfView / 180));
            this.Planet.Render(this.ViewPortSize, size);

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
    }
}