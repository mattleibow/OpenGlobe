namespace OpenGlobe
{
    using System;

    using OpenTK;
    using OpenTK.Graphics.ES10;

    public static class MiniGlu
    {
        public static Vector3 GetScreenLocation(Vector3 from)
        {
            var mvmatrix = new float[16];
            var projmatrix = new float[16];
            var viewport = new int[4];

            GL.GetInteger(All.Viewport, viewport);
            GL.GetFloat(All.ModelviewMatrix, mvmatrix);
            GL.GetFloat(All.ProjectionMatrix, projmatrix);

            var xyz = Project(from, mvmatrix, projmatrix, viewport);
            xyz.Y = viewport[3] - xyz.Y;

            return xyz;
        }

        public static Vector3 Project(Vector3 from, float[] modelMatrix, float[] projMatrix, int[] viewport)
        {
            var aryIn = new [] { from.X, from.Y, from.Z, 1.0f };
            var aryOut = new float[4];

            MultMatrixVector3(modelMatrix, aryIn, aryOut);
            MultMatrixVector3(projMatrix, aryOut, aryIn);

            if (Math.Abs(aryIn[3] - 0.0f) < 0.0001)
            {
                aryIn[3] = 1.0f;
            }

            aryIn[0] /= aryIn[3];
            aryIn[1] /= aryIn[3];
            aryIn[2] /= aryIn[3];

            // Map x, y and z to range 0-1
            aryIn[0] = aryIn[0] * 0.5f + 0.5f;
            aryIn[1] = aryIn[1] * 0.5f + 0.5f;
            aryIn[2] = aryIn[2] * 0.5f + 0.5f;

            // Map x,y to viewport
            float x = aryIn[0] * viewport[2] + viewport[0];
            float y = aryIn[1] * viewport[3] + viewport[1];
            float z = aryIn[3];

            return new Vector3(x, y, z);
        }

        public static void MultMatrixVector3(float[] matrix, float[] aryIn, float[] aryOut)
        {
            int i;

            for (i = 0; i < 4; i++)
            {
                aryOut[i] = aryIn[0] * matrix[0 * 4 + i] + //
                            aryIn[1] * matrix[1 * 4 + i] + //
                            aryIn[2] * matrix[2 * 4 + i] + //
                            aryIn[3] * matrix[3 * 4 + i];
            }
        }
    }
}