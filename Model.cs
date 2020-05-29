using System;
using System.Collections.Generic;
using System.Text;
using ai = Assimp;
using System.Numerics;
using Silk.NET.OpenGL;

namespace _3Dtest
{
    class Model
    {

        private GL gl;

        private List<Mesh> meshes;
        private List<Texture> loadedTextures;

        private string directory;

        public Model(GL gl, string filePath)
        {
            this.gl = gl;

            loadModel(filePath);
        }

        private void loadModel(string path)
        {
            var assimp =  new ai.AssimpContext();

            ai.Scene scene = assimp.ImportFile(path, ai.PostProcessSteps.Triangulate | ai.PostProcessSteps.FlipUVs | ai.PostProcessSteps.CalculateTangentSpace);

            if(scene == null || scene.RootNode == null || scene.SceneFlags.HasFlag(ai.SceneFlags.Incomplete))
            {
                Console.WriteLine($"Assimp error when importing file \'{path}'");
                return;
            }

            directory = path.Substring(0, path.LastIndexOf("/")+1);
            loadedTextures = new List<Texture>();
            meshes = new List<Mesh>();

            proccessNode(scene.RootNode, scene);
        }

        private void proccessNode(ai.Node node, ai.Scene scene)
        {
            foreach(int index in node.MeshIndices)
            {
                ai.Mesh mesh = scene.Meshes[index];
                meshes.Add(proccessMesh(mesh, scene));
            }

            foreach(ai.Node child in node.Children)
                proccessNode(child, scene);
        }

        private Mesh proccessMesh(ai.Mesh mesh, ai.Scene scene)
        {
            //Vertex[] vertices = new Vertex[mesh.VertexCount];
            //uint[] indices = new uint[mesh.FaceCount * 3];
            List<Vertex> vertices = new List<Vertex>();
            List<uint> indices = new List<uint>();
            Texture[] textures;

            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Vertex v = new Vertex();

                ai.Vector3D mvp = mesh.Vertices[i];
                v.Position = new Vector3(mvp.X, mvp.Y, mvp.Z);

                ai.Vector3D mvn = mesh.Normals[i];
                v.Normal = new Vector3(mvn.X, mvn.Y, mvn.Z);

                ai.Vector3D mvt = mesh.Tangents[i];
                v.Tangent = new Vector3(mvt.X, mvt.Y, mvt.Z);

                ai.Vector3D mvb = mesh.BiTangents[i];
                v.Bitangent = new Vector3(mvb.X, mvb.Y, mvb.Z);

                if (mesh.TextureCoordinateChannelCount > 0)
                {
                    ai.Vector3D mvc = mesh.TextureCoordinateChannels[0][i];
                    v.TexCoords = new Vector2(mvc.X, mvc.Y);
                }
                else
                    v.TexCoords = new Vector2(0, 0);

                vertices.Add(v);
            }
            
            for(int i = 0; i < mesh.FaceCount; i++)
                for (int j = 0; j < mesh.Faces[i].IndexCount; j++)
                    indices.Add((uint)mesh.Faces[i].Indices[j]);

            if (mesh.MaterialIndex >= 0)
            {
                ai.Material material = scene.Materials[mesh.MaterialIndex];

                List<Texture> tempTextures = new List<Texture>();

                Texture[] diffuse = loadMaterialTextures(material, ai.TextureType.Diffuse, TextureType.Diffuse);
                tempTextures.AddRange(diffuse);

                Texture[] specular = loadMaterialTextures(material, ai.TextureType.Specular, TextureType.Specular);
                tempTextures.AddRange(specular);

                Texture[] normal = loadMaterialTextures(material, ai.TextureType.Height, TextureType.Normal);
                tempTextures.AddRange(normal);

                textures = tempTextures.ToArray();
            }
            else
                textures = new Texture[0];

            return new Mesh(gl, vertices.ToArray(), indices.ToArray(), textures);
        }

        private Texture[] loadMaterialTextures(ai.Material material, ai.TextureType type, TextureType textureType)
        {
            Texture[] textures = new Texture[material.GetMaterialTextureCount(type)];

            for(int i = 0; i < textures.Length; i++)
            {
                ai.TextureSlot slot;
                material.GetMaterialTexture(type, i, out slot);

                string path = directory + slot.FilePath;
               
                Texture texture;

                Texture loaded = loadedTextures.Find(x => x.path == path);

                if (loaded != null)
                    texture = loaded;
                else
                {
                    texture = new Texture(gl, path, textureType);
                    loadedTextures.Add(texture);
                    Console.WriteLine($"Texture file path: {path}");
                }

                textures[i] = texture;
            }

            return textures;
        }

        public void Draw(Shader shader)
        {
           // meshes[0].Draw(shader);
           // return;

            foreach (Mesh mesh in meshes)
                mesh.Draw(shader);
        }
    }
}
