using System;
using System.Collections.Generic;
using System.Text;

namespace ComputeShader.Models
{
    class RawModel
    {
        public int ID { get; private set; }
        public int VertexCount { get; private set; }
        public RawModel(int id, int vertexCount)
        {
            this.ID = id;
            this.VertexCount = vertexCount;
        }
    }
}
