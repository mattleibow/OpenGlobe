namespace OpenGlobe
{
    using System;

    using Android.Content;
    using Android.Graphics;
    using Android.Opengl;

    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.ES10;

    public static class Utils
    {
        public static int CreateResourceTexture(Context context, int resource, bool useMipmapping = false)
        {
            int textureId;
            GL.GenTextures(1, out textureId);
            BindResource(textureId, context, resource, useMipmapping);
            return textureId;
        }

        public static int CreateAssetTexture(Context context, string filename, bool useMipmapping = false)
        {
            int textureId;
            GL.GenTextures(1, out textureId);
            BindAsset(textureId, context, filename, useMipmapping);
            return textureId;
        }

        public static void BindAsset(int textureId, Context context, string filename, bool useMipmapping = false)
        {
            using (var stream = context.Assets.Open(filename))
            using (var image = BitmapFactory.DecodeStream(stream))
            {
                BindTexture(textureId, image, useMipmapping);
                image.Recycle();
            }
        }

        public static void BindResource(int textureId, Context context, int resource, bool useMipmapping = false)
        {
            using (var image = BitmapFactory.DecodeResource(context.Resources, resource))
            {
                BindTexture(textureId, image, useMipmapping);
                image.Recycle();
            }
        }

        public static void BindTexture(int textureId, Bitmap image, bool useMipmapping = false)
        {
            GL.BindTexture(All.Texture2D, textureId);

            GLUtils.TexImage2D((int)All.Texture2D, 0, image, 0);

            if (useMipmapping)
            {
                GL.TexParameter(All.Texture2D, All.TextureMinFilter, new[] { (int)All.LinearMipmapNearest });
                GL.TexParameter(All.Texture2D, All.TextureMagFilter, new[] { (int)All.LinearMipmapNearest });
            }
            else
            {
                GL.TexParameter(All.Texture2D, All.TextureMinFilter, new[] { (int)All.Linear });
                GL.TexParameter(All.Texture2D, All.TextureMagFilter, new[] { (int)All.Linear });
            }
        }

        public static void RenderTextureAt(Vector2 postion, Vector2 windowsSize, int textureId, Vector2 scaledSize, Color4 c)
        {
            float[] squareVertices = { -1.0f, -1.0f, 0, //
                                        1.0f, -1.0f, 0, //
                                       -1.0f,  1.0f, 0, //
                                        1.0f,  1.0f, 0 };

            float[] textureCoords = { 0.0f, 0.0f, //
                                      1.0f, 0.0f, //
                                      0.0f, 1.0f, //
                                      1.0f, 1.0f };

            float aspectRatio = windowsSize.Y / windowsSize.X;

            float scaledX = 2.0f * postion.X / windowsSize.X;
            float scaledY = 2.0f * postion.Y / windowsSize.Y * aspectRatio;

            GL.Disable(All.DepthTest);

            GL.Disable(All.Lighting);

            GL.Disable(All.CullFace);
            GL.DisableClientState(All.ColorArray);

            GL.MatrixMode(All.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Ortho(-1.0f, 1.0f, -1.0f * aspectRatio, 1.0f * aspectRatio, -1.0f, 1000);

            GL.MatrixMode(All.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Translate(scaledX, scaledY, 0);

            GL.Scale(scaledSize.X, scaledSize.Y, 1);

            GL.VertexPointer(3, All.Float, 0, (squareVertices));
            GL.EnableClientState(All.VertexArray);

            GL.Enable(All.Texture2D);
            GL.Enable(All.Blend);
            GL.BlendFunc(All.One, All.OneMinusSrcColor);
            GL.BindTexture(All.Texture2D, textureId);
            GL.TexCoordPointer(2, All.Float, 0, (textureCoords));
            GL.EnableClientState(All.TextureCoordArray);

            GL.Color4(c.R, c.G, c.B, c.A);

            GL.DrawArrays(All.TriangleStrip, 0, 4);

            GL.MatrixMode(All.Projection);
            GL.PopMatrix();

            GL.MatrixMode(All.Modelview);
            GL.PopMatrix();

            GL.Enable(All.DepthTest);
            GL.Enable(All.Lighting);

            GL.Disable(All.Blend);
        }

        public static Vector3 Get3DPointFromLatLongDegrees(float latitude, float longitude, float radius)
        {
            float lonRad = MathHelper.DegreesToRadians(longitude);
            float latRad = MathHelper.DegreesToRadians(latitude);

            float x = (float)(radius * Math.Sin(latRad) * Math.Cos(lonRad));
            float y = (float)(radius * Math.Sin(lonRad));
            float z = (float)(radius * Math.Cos(latRad) * Math.Cos(lonRad));

            return new Vector3(-x, y, z);
        }
    }
}