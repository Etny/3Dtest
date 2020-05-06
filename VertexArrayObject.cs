using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace _3Dtest
{
    unsafe class VertexArrayObject<VertexType, IndexType> : IDisposable
        where VertexType : unmanaged
        where IndexType : unmanaged
    {

        private GL gl;

        private uint id;

       //private GLBuffer<VertexType> vbo;
       //private GLBuffer<IndexType> ebo;

        public VertexArrayObject(GL gl, GLBuffer<VertexType> vbo, GLBuffer<IndexType> ebo)
        {
            this.gl = gl;
            //this.vbo = vbo;
            //this.ebo = ebo;

            id = gl.CreateVertexArray();

            Bind();
            vbo.Bind();
            ebo.Bind();
        }

        public void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offset)
        {
            gl.VertexAttribPointer(index, count, type, false, (uint)(vertexSize * sizeof(float)), (void*)(offset));
            gl.EnableVertexAttribArray(index);
        }

        public void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, string fieldName)
        {
            gl.VertexAttribPointer(index, count, type, false, (uint)(sizeof(VertexType)), (void*)Marshal.OffsetOf<VertexType>(fieldName));
            gl.EnableVertexAttribArray(index);
        }


        public void Bind()
        {
            gl.BindVertexArray(id);
        }

        public void Dispose()
        {
            gl.DeleteVertexArray(id);
        }
    }
}
