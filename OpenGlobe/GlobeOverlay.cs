namespace OpenGlobe
{
    using OpenTK;
    using OpenTK.Graphics;

    public class GlobeOverlay
    {
        public GlobeOverlay()
            : this(new Vector3())
        {
        }

        public GlobeOverlay(Vector3 position)
        {
            this.Position = position;
            this.DotColor = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            this.LabelColor = new Color4(1.0F, 0.5F, 0.5F, 1F);

            this.Label = null;
            this.DotScale = new Vector2(0.1F, 0.1F);
            this.LabelScale = new Vector2(1F, 1F);
        }

        public string Label { get; set; }

        public Color4 LabelColor { get; set; }

        public int DotTexture { get; set; }

        public Color4 DotColor { get; set; }

        public TexFont Font { get; set; }

        public Vector3 Position { get; set; }

        public Vector2 DotScale { get; set; }

        public Vector2 LabelScale { get; set; }
    }
}