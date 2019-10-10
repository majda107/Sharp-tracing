using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace ComputeShader.Shaders
{
    class StaticShader: Shader
    {
        public int VertexShaderID { get; private set; }
        public int FragmentShaderID { get; private set; }
        public StaticShader()
        {
            this.ProgramID = GL.CreateProgram();
            this.VertexShaderID = this.CreateShader(@"..\..\..\Shaders\VertexShader.glsl", ShaderType.VertexShader);
            this.FragmentShaderID = this.CreateShader(@"..\..\..\Shaders\FragmentShader.glsl", ShaderType.FragmentShader);
            
            GL.AttachShader(this.ProgramID, this.VertexShaderID);
            GL.AttachShader(this.ProgramID, this.FragmentShaderID);
            GL.LinkProgram(this.ProgramID);
            GL.ValidateProgram(this.ProgramID);

            this.BindAttributesLocations();
            this.GetUniformLocations();
        }

        public override void GetUniformLocations()
        {
            
        }

        public override void BindAttributesLocations()
        {
            this.BindAttribute(0, "position");
            this.BindAttribute(1, "textureCoord");
        }
    }
}
