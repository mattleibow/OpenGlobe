namespace OpenGlobe
{
    using System.Diagnostics;
    using System.IO;

    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.ES10;

    public class TexFont
    {
        private const int HeaderLength = 20;

        private const int CharacterCount = 256;

        private readonly int[] characterWidths;

        private readonly int textureId;

        private int bpp;

        private int colCount;

        private int firstCharOffset;

        private int fntCellHeight;

        private int fntCellWidth;

        private int fntTexHeight;

        private int fntTexWidth;

        public TexFont(Stream stream)
            : this()
        {
            if (!this.LoadFontAlt(stream))
            {
                Debug.WriteLine("There was a problem loading the texture.");
            }
        }

        public TexFont()
        {
            // Initialise parameters
            this.Color = new Color4(1.0f, 1.0f, 1.0f, 1.0f);

            // Set scale to neutral
            this.Scale = new Vector2(1.0f, 1.0f);

            // Array to hold character width data
            this.characterWidths = new int[CharacterCount];

            // Generate GL texture ID
            GL.GenTextures(1, out this.textureId);
            GL.BindTexture(All.Texture2D, this.textureId);

            // Set texture parameters
            GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Nearest);
            GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Nearest);
            GL.TexParameter(All.Texture2D, All.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(All.Texture2D, All.TextureWrapT, (int)All.ClampToEdge);
        }

        /// <summary>
        /// Set a scale for the text
        /// </summary>
        public Vector2 Scale { get; set; }

        /// <summary>
        /// Set colour for text quads.
        /// </summary>
        public Color4 Color { get; set; }

        /// <summary>
        /// Gets the height of the text
        /// </summary>
        public float LineHeight
        {
            get
            {
                return this.fntCellHeight * this.Scale.Y;
            }
        }

        public bool LoadFontAlt(Stream stream)
        {
            try
            {
                this.ReadHeader(stream);
                this.ReadCharacterWidths(stream);
                this.ReadTextureData(stream);
            }
            catch (IOException)
            {
                throw;
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void ReadTextureData(Stream stream)
        {
            // Get bitmap
            int bitLen = (this.fntTexHeight * this.fntTexWidth) * (this.bpp / 8);
            var bits = new byte[bitLen];
            stream.Read(bits, 0, bitLen);

            // Get image format
            var format = All.Alpha;
            switch (this.bpp)
            {
                case 8: // Alpha channel info only
                    format = All.Alpha;
                    break;

                case 24: // RGB Texture
                    format = All.Rgb;
                    break;

                case 32: // RGBA Texture
                    format = All.Rgba;
                    break;
            }

            // Flip image scanlines
            int place = 0;
            int lineLen = this.fntTexWidth * (this.bpp / 8);
            var pixels = new byte[bits.Length];
            for (int lines = this.fntTexHeight - 1; lines > 0; --lines)
            {
                int offset = lines * lineLen;
                for (int x = 0; x < lineLen; x++)
                {
                    pixels[place++] = bits[offset + x];
                }
            }

            // Place bitmap in texture
            GL.BindTexture(All.Texture2D, this.textureId);
            GL.TexImage2D(All.Texture2D, 0, (int)format, this.fntTexWidth, this.fntTexHeight, 0, format, All.UnsignedByte, pixels);
        }

        /// <summary>
        /// Read width information
        /// </summary>
        private void ReadCharacterWidths(Stream stream)
        {
            for (int i = 0; i < CharacterCount; ++i)
            {
                this.characterWidths[i] = stream.ReadByte();
            }
        }

        /// <summary>
        /// Read header and check read size is correct
        /// </summary>
        private void ReadHeader(Stream stream)
        {
            var head = new byte[HeaderLength];
            if (stream.Read(head, 0, HeaderLength) < HeaderLength)
            {
                throw new IOException("Header read failed");
            }

            ByteBuffer headBuf = ByteBuffer.Wrap(head);
            // Check header
            int h0 = headBuf.Byte;
            int h1 = headBuf.Byte;

            // Check header signature
            if (h0 != 0xBF || h1 != 0xF2) // BFF2
            {
                throw new IOException("Bad header signature");
            }

            // Get image width and height
            this.fntTexWidth = headBuf.Int32;
            this.fntTexHeight = headBuf.Int32;

            // Get cell dimensions
            this.fntCellWidth = headBuf.Int32;
            this.fntCellHeight = headBuf.Int32;

            // Sanity check (prevent divide by zero)
            if (this.fntCellWidth <= 0 || this.fntCellHeight <= 0)
            {
                throw new IOException("Invalid header content");
            }

            // Pre-calculate column count
            this.colCount = this.fntTexWidth / this.fntCellWidth;

            // Get colour depth
            this.bpp = headBuf.Byte;

            // Get base offset
            this.firstCharOffset = headBuf.Byte;
        }

        /// <summary>
        /// Print a line of text to screen at specified co-ords
        /// </summary>
        public void PrintAt(string text, float x, float y, float z)
        {
            // Calculate quad size from scaling factors
            float cellHeight = this.fntCellHeight * this.Scale.Y;

            // UV Array for crop rectangle
            var uvArray = new int[4];
            uvArray[3] = this.fntCellHeight;

            // Set up GL for rendering the text
            GL.Color4(this.Color.R, this.Color.G, this.Color.B, this.Color.A);
            GL.Enable(All.Texture2D);
            GL.BindTexture(All.Texture2D, this.textureId);

            // Loop through each character of the string
            for (int i = 0; i < text.Length; i++)
            {
                char letter = text[i];

                // Calculate glyph position within texture
                int glyph = letter - this.firstCharOffset;
                int col = glyph % this.colCount;
                int row = (glyph / this.colCount) + 1;

                int charWidth = this.characterWidths[letter];

                // Update the crop rect
                uvArray[0] = col * this.fntCellWidth;
                uvArray[1] = this.fntTexHeight - (row * this.fntCellHeight);
                uvArray[2] = charWidth;

                // Set crop area (this specifies where to SByte the rectangle to write to the screen.
                GL.TexParameter(All.Texture2D, All.TextureCropRectOes, uvArray);

                GL.Oes.DrawTex(x, y, z, this.Scale.X * charWidth, cellHeight);

                // Add character width to offset for next glyph
                x += (this.Scale.X * this.characterWidths[glyph + this.firstCharOffset]);
            }
        }

        /// <summary>
        /// Return the length (pixels) of a string
        /// </summary>
        public int MeasureString(string text)
        {
            float len = 0.0f;

            for (int index = 0; index != text.Length; ++index)
            {
                len += (this.Scale.X * this.characterWidths[text[index]]);
            }

            return (int)len;
        }
    }
}