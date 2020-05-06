using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace _3Dtest
{
    unsafe class GLBuffer<DataType> : IDisposable
        where DataType : unmanaged
    {

        private GL gl;

        private uint id;
        private BufferTargetARB bufferType;

        public GLBuffer(GL gl, Span<DataType> data, BufferTargetARB bufferType)
        {
            this.gl = gl;
            this.bufferType = bufferType;

            id = gl.CreateBuffer();
            Bind();

            fixed (void* i = &data.ToArray()[0])
                gl.BufferData(bufferType, (uint)data.Length, i, BufferUsageARB.StaticDraw);
        }

        public void Bind()
        {
            gl.BindBuffer(bufferType, id);
        }

        public void Dispose()
        {
            gl.DeleteBuffer(id);
        }
    }
}
