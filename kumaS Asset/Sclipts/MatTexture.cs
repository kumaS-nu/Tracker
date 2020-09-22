using UnityEngine;
using OpenCvSharp;
using System.Runtime.InteropServices;

namespace kumaS
{
    public static class MatTexture
    {
        /// <summary>
        /// Mat から Texture の変換      Converts OpenCV Mat to Unity texture
        /// </summary>
        /// <returns>texture</returns>
        /// <param name="mat">Mat</param>
        /// <param name="outTexture">texture</param>
        public static Texture2D Mat2Texture(Mat mat, Texture2D outTexture = null)
        {
            Size size = mat.Size();

            if (null == outTexture || outTexture.width != size.Width || outTexture.height != size.Height)
            {
                outTexture = new Texture2D(size.Width, size.Height);
            }

            int count = size.Width * size.Height;
            Color32[] data = new Color32[count];
            byte[] temp = new byte[count * 3];
            Marshal.Copy(mat.Data, temp, 0, temp.Length);
            for(int i = 0; i < count; i++)
            {
                data[count - i - 1] = new Color32(temp[3 * i + 2], temp[3 * i + 1], temp[3 * i], 255);
            }
            outTexture.SetPixels32(data);
            outTexture.Apply();

            return outTexture;

        }
    }
}