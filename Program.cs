using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using ComputeShader.Engine;
using ComputeShader.Shaders;

namespace ComputeShader
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game(1024, 1024);
            game.Run();
        }
    }
}
