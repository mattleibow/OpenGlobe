namespace OpenGlobe
{
    using System;
    using System.Collections.Generic;

    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.ES10;

    public class Planet
    {
        private float[] colorData;

        private float[] normalData;

        private float[] textureData;

        private float[] vertexData;

        private Vector2 textureOffset;

        public Planet(int stacks, int slices, float radius = 1.0F, float squash = 1.0F)
        {
            this.Overlays = new List<GlobeOverlay>();
            this.textureOffset = Vector2.Zero;
            this.Position = Vector3.Zero;

            this.Stacks = stacks;
            this.Slices = slices;
            this.Radius = radius;
            this.Squash = squash;

            this.MakeGeometry();
        }

        public int TextureId { get; set; }

        public float Squash { get; private set; }

        public float Radius { get; private set; }

        public int Stacks { get; private set; }

        public int Slices { get; private set; }

        public Vector2 TextureOffset
        {
            get
            {
                return this.textureOffset;
            }
            set
            {
                this.textureOffset = value;

                this.UpdateTextureOffset();
            }
        }

        public Vector3 Position { get; set; }

        public List<GlobeOverlay> Overlays { get; private set; }

        private void MakeGeometry()
        {
            int vIndex = 0; // vertex index
            int nIndex = 0; // normal index
            int tIndex = 0; // texture index

            // vertices
            this.vertexData =  new float[3 * ((this.Slices * 2 + 2) * this.Stacks)];
            // color data
            this.colorData =   new float[4 * ((this.Slices * 2 + 2) * this.Stacks)];
            // normal pointers for lighting
            this.normalData =  new float[3 * ((this.Slices * 2 + 2) * this.Stacks)];
            // texture coords
            this.textureData = new float[2 * ((this.Slices * 2 + 2) * this.Stacks)]; 

            // latitude
            for (int phiIdx = 0; phiIdx < this.Stacks; phiIdx++)
            {
                // starts at -1.57 goes up to +1.57 radians
                // the first circle
                float phi0 = (float)Math.PI * ((phiIdx + 0) * (1.0f / this.Stacks) - 0.5f);

                // the next, or second one.
                float phi1 = (float)Math.PI * ((phiIdx + 1) * (1.0f / this.Stacks) - 0.5f);

                var cosPhi0 = (float)Math.Cos(phi0);
                var sinPhi0 = (float)Math.Sin(phi0);
                var cosPhi1 = (float)Math.Cos(phi1);
                var sinPhi1 = (float)Math.Sin(phi1);

                // longitude
                for (int thetaIdx = 0; thetaIdx < this.Slices; thetaIdx++)
                {
                    // increment along the longitude circle each "slice"

                    var theta = (float)(2.0f * (float)Math.PI * thetaIdx * (1.0 / (this.Slices - 1)));
                    var cosTheta = (float)Math.Cos(theta);
                    var sinTheta = (float)Math.Sin(theta);

                    // we're generating a vertical pair of points, such
                    // as the first point of stack 0 and the first point of
                    // stack 1
                    // above it. This is how TRIANGLESTRIPS work,
                    // taking a set of 4 vertices and essentially drawing two
                    // triangles
                    // at a time. The first is v0-v1-v2 and the next is
                    // v2-v1-v3. Etc.

                    // get x-y-z for the first vertex of stack

                    this.vertexData[vIndex] = this.Radius * cosPhi0 * cosTheta;
                    this.vertexData[vIndex + 1] = this.Radius * (sinPhi0 * this.Squash);
                    this.vertexData[vIndex + 2] = this.Radius * (cosPhi0 * sinTheta);

                    this.vertexData[vIndex + 3] = this.Radius * cosPhi1 * cosTheta;
                    this.vertexData[vIndex + 4] = this.Radius * (sinPhi1 * this.Squash);
                    this.vertexData[vIndex + 5] = this.Radius * (cosPhi1 * sinTheta);

                    // normal pointers for lighting

                    this.normalData[nIndex + 0] = (cosPhi0 * cosTheta);
                    this.normalData[nIndex + 2] = cosPhi0 * sinTheta;
                    this.normalData[nIndex + 1] = sinPhi0;
                    // get x-y-z for the first vertex of stack N
                    this.normalData[nIndex + 3] = cosPhi1 * cosTheta;
                    this.normalData[nIndex + 5] = cosPhi1 * sinTheta;
                    this.normalData[nIndex + 4] = sinPhi1;

                    // textures
                    this.SetTexturePoints(thetaIdx, phiIdx, tIndex);

                    // move to the next set
                    vIndex += 2 * 3;
                    nIndex += 2 * 3;
                    tIndex += 2 * 2;
                }

                // degenerate triangle to connect stacks and maintain winding order

                this.vertexData[vIndex + 0] = this.vertexData[vIndex + 3] = this.vertexData[vIndex - 3];
                this.vertexData[vIndex + 1] = this.vertexData[vIndex + 4] = this.vertexData[vIndex - 2];
                this.vertexData[vIndex + 2] = this.vertexData[vIndex + 5] = this.vertexData[vIndex - 1];

                this.normalData[nIndex + 0] = this.normalData[nIndex + 3] = this.normalData[nIndex - 3];
                this.normalData[nIndex + 1] = this.normalData[nIndex + 4] = this.normalData[nIndex - 2];
                this.normalData[nIndex + 2] = this.normalData[nIndex + 5] = this.normalData[nIndex - 1];

                this.DegenerateTextureTriangles(tIndex);
            }
        }

        private void DegenerateTextureTriangles(int tIndex)
        {
            this.textureData[tIndex + 0] = this.textureData[tIndex + 2] = this.textureData[tIndex - 2];
            this.textureData[tIndex + 1] = this.textureData[tIndex + 3] = this.textureData[tIndex - 1];
        }

        private void SetTexturePoints(int thetaIdx, int phiIdx, int tIndex)
        {
            float texX = thetaIdx * (1.0f / (this.Slices - 1));

            float texY = (phiIdx + 0) * (1.0f / this.Stacks);
            this.textureData[tIndex + 0] = texX + this.TextureOffset.X;
            this.textureData[tIndex + 1] = texY + this.TextureOffset.Y;

            texY = (phiIdx + 1) * (1.0f / this.Stacks);
            this.textureData[tIndex + 2] = texX + this.TextureOffset.X;
            this.textureData[tIndex + 3] = texY + this.TextureOffset.Y;
        }

        public void UpdateTextureOffset()
        {
            int tIndex = 0; // texture index
            for (int phiIdx = 0; phiIdx < this.Stacks; phiIdx++)
            {
                for (int thetaIdx = 0; thetaIdx < this.Slices; thetaIdx++)
                {
                    this.SetTexturePoints(thetaIdx, phiIdx, tIndex);
                    tIndex += 2 * 2;
                }
                this.DegenerateTextureTriangles(tIndex);
            }
        }

        public void Render(Vector2 windowSize, float size)
        {
            GL.MatrixMode(All.Modelview);
            GL.Enable(All.CullFace);
            GL.CullFace(All.Back);

            GL.EnableClientState(All.VertexArray);
            GL.EnableClientState(All.NormalArray);
            GL.EnableClientState(All.ColorArray);

            GL.Enable(All.Texture2D);
            GL.EnableClientState(All.TextureCoordArray);
            GL.BindTexture(All.Texture2D, this.TextureId);
            GL.TexCoordPointer(2, All.Float, 0, this.textureData);

            GL.MatrixMode(All.Modelview);
            GL.VertexPointer(3, All.Float, 0, this.vertexData);
            GL.NormalPointer(All.Float, 0, this.normalData);
            GL.ColorPointer(4, All.Float, 0, this.colorData);
            GL.DrawArrays(All.TriangleStrip, 0, (this.Slices + 1) * 2 * (this.Stacks - 1) + 2);

            foreach (GlobeOverlay p in this.Overlays)
            {
                this.ExecuteOverlay(p, windowSize, size);
            }

            GL.Disable(All.Blend);
            GL.Disable(All.Texture2D);
            GL.DisableClientState(All.TextureCoordArray);
        }

        private void ExecuteOverlay(GlobeOverlay overlay, Vector2 windowSize, float size)
        {
            Vector3 screenLoc = MiniGlu.GetScreenLocation(overlay.Position);
            if (screenLoc.Z <= 10F)
            {
                float x = screenLoc.X - windowSize.X / 2F;
                float y = screenLoc.Y - windowSize.Y / 2F;

                var dotScale = DrawScale(size, overlay.DotScale, screenLoc.Z);
                var dotPos = new Vector2(x, -y);
                Utils.RenderTextureAt(dotPos, windowSize, overlay.DotTexture, dotScale, overlay.DotColor);

                var labelScale = DrawScale(size, overlay.LabelScale, screenLoc.Z);
                var textPos = new Vector3(screenLoc.X, windowSize.Y - screenLoc.Y, -10);
                this.RenderConstName(overlay, textPos, labelScale, overlay.LabelColor);
            }
        }

        private void RenderConstName(GlobeOverlay overlay, Vector3 xyz, Vector2 labelScale, Color4 c)
        {
            if (overlay.Font != null && !string.IsNullOrWhiteSpace(overlay.Label))
            {
                GL.Disable(All.Lighting);
                GL.BlendFunc(All.SrcAlpha, All.OneMinusSrcAlpha);
                GL.Enable(All.Blend);

                overlay.Font.Color = c;
                overlay.Font.Scale = labelScale;

                int length = overlay.Font.MeasureString(overlay.Label);
                float centeredX = xyz.X - (length / 2F);
                float centeredY = xyz.Y + (overlay.Font.LineHeight / 2F);
                overlay.Font.PrintAt(overlay.Label, centeredX, centeredY, xyz.Z);
            }
        }

        private static Vector2 DrawScale(float size, Vector2 vector, float z)
        {
            Vector2 scaledSize = vector * size;
            Vector2 drawScale = scaledSize * (10F - z);
            return drawScale;
        }

        public Vector3 GetGeoCoord(float latitude, float longitude)
        {
            return Utils.Get3DPointFromLatLongDegrees(latitude, longitude, this.Radius);
        }
    }
}