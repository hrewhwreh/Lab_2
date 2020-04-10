using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Tomogram_viz
{
    class View
    {
        public void SetupView(int width, int height)
        {
            GL.ShadeModel(ShadingModel.Smooth);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Bin.X, 0, Bin.Y, -1, 1);
            GL.Viewport(0, 0, width, height);
        }

        public int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }
        Color TransferFunction(short value, int min, int max_min)
        {
            int newVal = Clamp((value - min) * 255 / max_min, 0, 255);
            return Color.FromArgb(255, newVal, newVal, newVal);
        }
        public void DrawQuadstrip(int layerNumber, int min, int max_min)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            for (int x_coord = 0; x_coord < Bin.X - 1; x_coord++)
            {
                GL.Begin(BeginMode.QuadStrip);
                short value;
                value = Bin.array[x_coord + 0 * Bin.X + layerNumber * Bin.X * Bin.Y];
                GL.Color3(TransferFunction(value, min, max_min));
                GL.Vertex2(x_coord, 0);
                value = Bin.array[x_coord + 1 + 0 * Bin.X + layerNumber * Bin.X * Bin.Y];
                GL.Color3(TransferFunction(value, min, max_min));
                GL.Vertex2(x_coord + 1, 0);
                for (int y_coord = 1; y_coord < Bin.Y - 1; y_coord++)
                {
                    value = Bin.array[x_coord + 1 + (y_coord + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value, min, max_min));
                    GL.Vertex2(x_coord + 1, y_coord + 1);
                    value = Bin.array[x_coord + (y_coord + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value, min, max_min));
                    GL.Vertex2(x_coord, y_coord + 1);
                }
                GL.End();
            }
        }

        public void DrawQuads(int layerNumber, int min, int max_min)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Begin(BeginMode.Quads);
            for (int x_coord = 0; x_coord < Bin.X - 1; x_coord++)
            {
                for (int y_coord = 0; y_coord < Bin.Y - 1; y_coord++)
                {
                    short value;
                    value = Bin.array[x_coord + y_coord * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value, min, max_min));
                    GL.Vertex2(x_coord, y_coord);
                    value = Bin.array[x_coord + (y_coord + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value, min, max_min));
                    GL.Vertex2(x_coord, y_coord + 1);
                    value = Bin.array[x_coord + 1 + (y_coord + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value, min, max_min));
                    GL.Vertex2(x_coord + 1, y_coord + 1);
                    value = Bin.array[x_coord + 1 + y_coord * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value, min, max_min));
                    GL.Vertex2(x_coord + 1, y_coord);
                }
            }
            GL.End();
        }

        Bitmap textureImage;
        int VBOTexture;
        public void Load2DTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, VBOTexture);
            BitmapData data = textureImage.LockBits(
                new System.Drawing.Rectangle(0, 0, textureImage.Width, textureImage.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, data.Scan0);
            textureImage.UnlockBits(data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            ErrorCode er = GL.GetError();
            string str = er.ToString();
        }

        public void genereateTextureImage(int layerNumber, int min, int max_min)
        {
            textureImage = new Bitmap(Bin.X, Bin.Y);
            for (int i = 0; i < Bin.X; i++)
            {
                for (int j = 0; j < Bin.Y; j++)
                {
                    int pixelNumber = i + j * Bin.X + layerNumber * Bin.X * Bin.Y;
                    textureImage.SetPixel(i, j, TransferFunction(Bin.array[pixelNumber], min, max_min));
                }
            }
        }
        public void DrawTexture()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, VBOTexture);

            GL.Begin(BeginMode.Quads);
            GL.Color3(Color.White);
            GL.TexCoord2(0f, 0f);
            GL.Vertex2(0, 0);
            GL.TexCoord2(0f, 1f);
            GL.Vertex2(0, Bin.Y);
            GL.TexCoord2(1f, 1f);
            GL.Vertex2(Bin.X, Bin.Y);
            GL.TexCoord2(1f, 0f);
            GL.Vertex2(Bin.X, 0);
            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }
    }
}
