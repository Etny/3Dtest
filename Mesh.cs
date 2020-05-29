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
        public Vertex[] Vertices;
        public uint[] Indices;
        public Texture[] Textures;

        private GL gl;

        private uint VAO, VBO, EBO;

        public Mesh(GL gl, Vertex[] vertices, uint[] indices, Texture[] textures)
        {
            this.gl = gl;

            this.Vertices = vertices;
            this.Indices = indices;
            this.Textures = textures;

            setupMesh();
        }

        private unsafe void setupMesh()
        {
            VBO = gl.GenBuffer();
            EBO = gl.GenBuffer();
            VAO = gl.GenVertexArray();

            gl.BindVertexArray(VAO);

            gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
            fixed(void* i = &Vertices[0])
                gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(Vertices.Length * sizeof(Vertex)), i,  BufferUsageARB.StaticDraw);

            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, EBO);
            fixed (void* i = &Indices[0])
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(Indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);

            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)0);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(3 * sizeof(float)));
            gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(6 * sizeof(float)));
            gl.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(8 * sizeof(float)));
            gl.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(11 * sizeof(float)));
            gl.EnableVertexAttribArray(0);
            gl.EnableVertexAttribArray(1);
            gl.EnableVertexAttribArray(2);
            gl.EnableVertexAttribArray(3);
            gl.EnableVertexAttribArray(4);

        }


        public unsafe void Draw(Shader shader)
        {
            int diffuseNumber = 1;
            int specularNumber = 1;
            int normalNumber = 1;

            for(int i = 0; i < Textures.Length; i++)
            {
                string textureName = "unknown";

                switch (Textures[i].TextureType)
                {
                    case TextureType.Diffuse:
                        textureName = "texture_diffuse" + diffuseNumber++;
                         break;
                    case TextureType.Specular:
                        textureName = "texture_specular" + specularNumber++;
                        break;
                    case TextureType.Normal:
                        textureName = "texture_normal" + normalNumber++;
                        break;
                }

                shader.SetInt($"material.{textureName}", i);

                Textures[i].BindToUnit(i);
            }
            gl.ActiveTexture(TextureUnit.Texture0);

            gl.BindVertexArray(VAO);
            gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, (void*)0);
        }
    }

    struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoords;
        public Vector3 Tangent;
        public Vector3 Bitangent;
    }
}
