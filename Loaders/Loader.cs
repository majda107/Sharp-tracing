using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

using ComputeShader.Models;

namespace ComputeShader.Loaders
{
    class Loader
    {
        private List<int> vaos;
        private List<int> vbos;
        private List<int> textures;
        public Loader()
        {
            this.vaos = new List<int>();
            this.vbos = new List<int>();
            this.textures = new List<int>();
        }

        public RawModel LoadRawModel(float[] vertices, float[] textureCoords, float[] normals, int[] indices)
        {
            int modelID = this.CreateBindVAO();
            this.BindIndices(indices);
            this.StoreDataInAttributeList(0, 3, vertices);
            this.StoreDataInAttributeList(1, 2, textureCoords);
            //this.StoreDataInAttributeList(2, 3, normals);
            this.UnbindVAO();

            return new RawModel(modelID, indices.Length);
        }

        public int LoadCubeTexture(string[] textureFiles)
        {
            int id = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, id);

            for (int i = 0; i < textureFiles.Length; i++)
            {
                Bitmap bmp = (Bitmap)Bitmap.FromFile(textureFiles[i]);
                BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bmp.UnlockBits(data);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            return id;
        }

        private void BindIndices(int[] indices)
        {
            int vboID = this.CreateVBO();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);
        }

        private void StoreDataInAttributeList(int index, int coordinateSize, float[] data)
        {
            int vboID = this.CreateVBO();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(index, coordinateSize, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private int CreateBindVAO()
        {
            int vaoID = GL.GenVertexArray(); 
            this.vaos.Add(vaoID);

            GL.BindVertexArray(vaoID);
            return vaoID;
        }

        private void UnbindVAO()
        {
            GL.BindVertexArray(0);
        }
        private int CreateVBO()
        {
            int vboID = GL.GenBuffer();
            this.vbos.Add(vboID);
            return vboID;
        }
    }
}
