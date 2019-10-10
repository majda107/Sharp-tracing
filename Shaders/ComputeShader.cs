using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace ComputeShader.Shaders
{
    class ComputeShader: Shader
    {
        public int ComputeShaderID { get; private set; }

        private int sphereLocation;
        private int sphereLocation2;
        private int[] spheresLocation = new int[6];
        private int cameraViewMatrixLocation;

        private int img_outputLocation;
        private int cubeMapLocation;
        public ComputeShader(string file)
        {
            this.ProgramID = GL.CreateProgram();
            this.ComputeShaderID = this.CreateShader(file, ShaderType.ComputeShader);

            GL.AttachShader(this.ProgramID, this.ComputeShaderID);
            GL.LinkProgram(this.ProgramID);
            GL.ValidateProgram(this.ProgramID);

            this.BindAttributesLocations();
            this.GetUniformLocations();
        }

        public override void BindAttributesLocations()
        {
            
        }

        public override void GetUniformLocations()
        {
            this.sphereLocation = this.GetUniformLocation("sphere");
            this.sphereLocation2 = this.GetUniformLocation("sphere2");
            this.cameraViewMatrixLocation = this.GetUniformLocation("cameraViewMatrix");

            this.img_outputLocation = this.GetUniformLocation("img_output");
            this.cubeMapLocation = this.GetUniformLocation("cubeMap");

            for(int i = 0; i < this.spheresLocation.Length; i++)
            {
                this.spheresLocation[i] = this.GetUniformLocation($"spheres[{i}]");
            }
        }

        public void LoadTextures()
        {
            GL.Uniform1(this.img_outputLocation, 0);
            GL.Uniform1(this.cubeMapLocation, 1);
        }

        public void LoadSphere(Vector4 sphere)
        {
            this.LoadVector4(sphere, this.sphereLocation);
        }

        public void LoadSphere2(Vector4 sphere2)
        {
            this.LoadVector4(sphere2, this.sphereLocation2);
        }

        public void LoadSpheres(Vector4[] spheres)
        {
            for(int i = 0; i < this.spheresLocation.Length; i++)
            {
                if (i > this.spheresLocation.Length - 1) break;
                this.LoadVector4(spheres[i], this.spheresLocation[i]);
            }
        }

        public void LoadCameraViewMatrix(Matrix4 cameraViewMatrix)
        {
            this.LoadMat4(cameraViewMatrix, this.cameraViewMatrixLocation);
        }
    }
}
