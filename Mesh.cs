using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace _3Dtest
{
    class Mesh
    {
        public Vertex[] Vertecis;
        public uint[] Indices;
        public Texture[] Textures;

        private GL gl;

        private GLBuffer<Vertex> VBO;
        private GLBuffer<uint> EBO;
        private VertexArrayObject<Vertex, uint> VAO;

        public Mesh(GL gl, Vertex[] vertecis, uint[] indices, Texture[] textures)
        {
            this.gl = gl;

            this.Vertecis = vertecis;
            this.Indices = indices;
            this.Textures = textures;

            setupMesh();
        }

        private void setupMesh()
        {
            VBO = new GLBuffer<Vertex>(gl, Vertecis.AsSpan(), BufferTargetARB.ArrayBuffer);
            EBO = new GLBuffer<uint>(gl, Indices.AsSpan(), BufferTargetARB.ArrayBuffer);
            VAO = new VertexArrayObject<Vertex, uint>(gl, VBO, EBO);

            VAO.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, "Position");
            VAO.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, "Normal");
            VAO.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, "TexCoords");
        }


        public unsafe void Draw(Shader shader)
        {
            int diffuseNumber = 1;
            int specularNumber = 1;

            for(int i = 0; i < Textures.Length; i++)
            {   
                string textureName = 
                    Textures[i].TextureType == TextureType.Diffuse ? 
                        "texture_diffuse" + diffuseNumber++ 
                    : 
                        "texture_sepcular" + specularNumber++;

                shader.SetInt($"material.{textureName}", i);

                Textures[i].BindToUnit((int)TextureUnit.Texture0 + i);
            }
            gl.ActiveTexture(TextureUnit.Texture0);

            VAO.Bind();
            gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, (void*)0);
        }
    }

    struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoords;
    }
}
