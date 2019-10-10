using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

using ComputeShader.Loaders;
using ComputeShader.Models;
using ComputeShader.Shaders;

namespace ComputeShader.Engine
{
    class Game
    {
        public GameWindow Window { get; private set; }
        public Loader Loader { get; private set; }
        public ComputeShader.Shaders.ComputeShader ComputeShader { get; private set; }
        public StaticShader StaticShader { get; private set; }
        public int Tex { get; private set; }
        private int SkyboxTex;


        private RawModel model;
        private float[] vertices = new float[]
        {
            -0.9f, 0.9f, 0,
            -0.9f, -0.9f, 0,
            0.9f, -0.9f, 0,
            0.9f, 0.9f, 0
        };

        private int[] indices = new int[]
        {
            0, 1, 3,
            3, 1, 2
        };

        private float[] textureCoords = new float[]
        {
            0, 0,
            0, 1,
            1, 1,
            1, 0
        };

        private float RotY = 0.0f;
        public Game(int width, int height)
        {
            this.Window = new GameWindow(width, height, new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(), 2, 4, 4));
            this.Loader = new Loader();
            this.model = this.Loader.LoadRawModel(this.vertices, this.textureCoords, null, this.indices);

            this.ComputeShader = new Shaders.ComputeShader(@"..\..\..\Shaders\ComputeShader.glsl");
            this.StaticShader = new StaticShader();

            //string[] cubeMap = new string[]
            //{
            //    "../../../res/right.jpg",
            //    "../../../res/left.jpg",
            //    "../../../res/top.jpg",
            //    "../../../res/bottom.jpg",
            //    "../../../res/back.jpg",
            //    "../../../res/front.jpg",
            //};

            string[] cubeMap = new string[]
            {
                "../../../res/right.png",
                "../../../res/left.png",
                "../../../res/top.png",
                "../../../res/bottom.png",
                "../../../res/back.png",
                "../../../res/front.png",
            };

            this.SkyboxTex = this.Loader.LoadCubeTexture(cubeMap);

            this.Init();

            //this.cameraViewMatrix *= Matrix4.CreateTranslation(-0.5f, 0.0f, 0.0f);
            this.cameraViewMatrix = Matrix4.Identity;
            //this.cameraViewMatrix *= Matrix4.CreateRotationX((float)Math.PI / 10);
            //this.cameraViewMatrix *= Matrix4.CreateTranslation(0.0f, 100.0f, 100.0f);
            //this.cameraViewMatrix *= Matrix4.CreateTranslation(3.0f, 3.0f, 5.0f);
            //this.cameraViewMatrix *= Matrix4.CreateTranslation(0.0f, 100.0f, 10.0f);

            this.cameraViewMatrix *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(10));
            
            //this.cameraViewMatrix *= Matrix4.CreateRotationX(-MathHelper.DegreesToRadians(50));
            this.cameraViewMatrix *= Matrix4.CreateTranslation(0.0f, -10.0f, 60.0f);

            this.cameraViewMatrix *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(RotY));
        }

        private void Init()
        {
            GL.Enable(EnableCap.Texture2D);

            this.Tex = GL.GenTexture();
            //GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, this.Tex);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, this.Window.Width, this.Window.Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            //GL.TexStorage2D(TextureTarget2d.Texture2D, 0, SizedInternalFormat.Rgba32f, this.Window.Width, this.Window.Height);
            GL.BindImageTexture(0, this.Tex, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

            this.Window.RenderFrame += RenderFrame;
        }

        private Vector4 sphere = new Vector4(0.0f, 4.0f, 0.0f, 5.0f);
        private Vector4 sphere2 = new Vector4(-12.0f, 3.0f, -10.0f, 4.0f);

        private Vector4[] spheres = new Vector4[] 
        { 
            new Vector4(0.0f, 4.0f, 0.0f, 5.0f), 
            new Vector4(-12.0f, 3.0f, -10.0f, 4.0f),

            new Vector4(-2.0f, 3.0f, -24.0f, 3.5f),
            new Vector4(-6.0f, -3.0f, -16.0f, 2.5f),
            new Vector4(-4.0f, -2.0f, -6.0f, 6.0f),
            new Vector4(-32.0f, -2.0f, -6.0f, 2.0f),
        };

        private Matrix4 cameraViewMatrix;

        private void RenderFrame(object sender, FrameEventArgs e)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + 1);
            GL.BindTexture(TextureTarget.TextureCubeMap, this.SkyboxTex);

            this.ComputeShader.Start();
            //this.ComputeShader.LoadTextures();
            //this.ComputeShader.LoadSphere(this.sphere);
            //this.ComputeShader.LoadSphere2(sphere2);

            this.ComputeShader.LoadSpheres(this.spheres);

            this.ComputeShader.LoadCameraViewMatrix(this.cameraViewMatrix);

            GL.DispatchCompute(this.Window.Width, this.Window.Height, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            this.StaticShader.Start();

            GL.BindVertexArray(this.model.ID);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            //GL.EnableVertexAttribArray(2);

            GL.ActiveTexture(TextureUnit.Texture0 + 0);
            GL.BindTexture(TextureTarget.Texture2D, this.Tex);

            GL.DrawElements(PrimitiveType.Triangles, this.model.VertexCount, DrawElementsType.UnsignedInt, 0);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            //GL.DisableVertexAttribArray(2);
            GL.BindVertexArray(0);

            this.StaticShader.Stop();
            this.ComputeShader.Stop();

            this.Window.SwapBuffers();

            //if (sphere.Y < 6.0) sphere.Y += 0.1f;
            //if (sphere.Y > -18.0) sphere.Y -= 0.05f;

            this.RotY += 0.5f;

            this.cameraViewMatrix = Matrix4.Identity;
            this.cameraViewMatrix *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(10));

            //this.cameraViewMatrix *= Matrix4.CreateRotationX(-MathHelper.DegreesToRadians(50));
            this.cameraViewMatrix *= Matrix4.CreateTranslation(0.0f, -10.0f, 60.0f);

            this.cameraViewMatrix *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(RotY));
        }

        public void Run()
        {
            this.Window.Run();
        }
    }
}
