namespace OpenGlobe
{
    using OpenTK;
    using OpenTK.Graphics;

    public static class Matrix4Extensions
    {
        public static float[] ToArray(this Matrix4 matrix)
        {
            float[] array = new float[16];

            array[00] = matrix.M11;
            array[01] = matrix.M12;
            array[02] = matrix.M13;
            array[03] = matrix.M14;

            array[04] = matrix.M21;
            array[05] = matrix.M22;
            array[06] = matrix.M23;
            array[07] = matrix.M24;

            array[08] = matrix.M31;
            array[09] = matrix.M32;
            array[10] = matrix.M33;
            array[11] = matrix.M34;

            array[12] = matrix.M41;
            array[13] = matrix.M42;
            array[14] = matrix.M43;
            array[15] = matrix.M44;

            return array;
        }

        public static float[] ToArray(this Color4 color)
        {
            float[] array = new float[16];

            array[00] = color.R;
            array[01] = color.G;
            array[02] = color.B;
            array[03] = color.A;

            return array;
        }

        public static float[] ToArray(this Color4 color, float alpha)
        {
            float[] array = new float[16];

            array[00] = color.R;
            array[01] = color.G;
            array[02] = color.B;
            array[03] = alpha;

            return array;
        }

        public static Matrix4 ToMatrix4(this float[] array)
        {
            Matrix4 matrix = new Matrix4();

            matrix.M11 = array[00];
            matrix.M12 = array[01];
            matrix.M13 = array[02];
            matrix.M14 = array[03];

            matrix.M21 = array[04];
            matrix.M22 = array[05];
            matrix.M23 = array[06];
            matrix.M24 = array[07];

            matrix.M31 = array[08];
            matrix.M32 = array[09];
            matrix.M33 = array[10];
            matrix.M34 = array[11];

            matrix.M41 = array[12];
            matrix.M42 = array[13];
            matrix.M43 = array[14];
            matrix.M44 = array[15];

            return matrix;
        }

        public static Vector3 Offset(this Vector3 v, float x = 0, float y = 0, float z = 0)
        {
            return new Vector3(v.X + x, v.Y + y, v.Z + z);
        }
    }
}