using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace ComputeShader.Shaders
{
    abstract class Shader
    {
        public int ProgramID { get; protected set; }
        public abstract void GetUniformLocations();
        public abstract void BindAttributesLocations();

        public void Start()
        {
            GL.UseProgram(this.ProgramID);
        }

        public void Stop()
        {
            GL.UseProgram(0);
        }

        protected int CreateShader(string shaderSource, ShaderType type)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, File.ReadAllText(shaderSource));
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if(status != 1)
            {
                GL.GetShaderInfoLog(shader, out string info);
                throw new Exception($"Shader didn't compile properly! {info}");
            }

            return shader;
        }

        protected int GetUniformLocation(string name)
        {
            return GL.GetUniformLocation(this.ProgramID, name);
        }

        protected void BindAttribute(int index, string name)
        {
            GL.BindAttribLocation(this.ProgramID, index, name);
        }

        protected void LoadVector3(Vector3 vector, int location)
        {
            GL.Uniform3(location, vector);
        }

        protected void LoadVector4(Vector4 vector, int location)
        {
            GL.Uniform4(location, vector);
        }
        protected void LoadMat4(Matrix4 matrix, int location)
        {
            GL.ProgramUniformMatrix4(this.ProgramID, location, false, ref matrix);
        }
    }
}
