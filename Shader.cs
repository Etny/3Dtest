using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace _3Dtest
{
    class Shader
    {
        public uint ID { get; private set; }

        private GL gl;

        public Shader(GL gl, string vertexPath, string fragmentPath)
        {
            this.gl = gl;

            uint vert = 0, frag = 0;

            LoadShader(ref vert, ShaderType.VertexShader, vertexPath);
            LoadShader(ref frag, ShaderType.FragmentShader, fragmentPath);

            ID = gl.CreateProgram();
            gl.AttachShader(ID, vert);
            gl.AttachShader(ID, frag);
            gl.LinkProgram(ID);

            string log = gl.GetProgramInfoLog(ID);
            if (!string.IsNullOrWhiteSpace(log))
                Console.WriteLine($"Shader program link error: {log}");

            gl.DeleteShader(vert);
            gl.DeleteShader(frag);
        }

        public Shader(GL gl, string vertexPath, string geometryPath, string fragmentPath)
        {
            this.gl = gl;

            uint vert = 0, geom = 0, frag = 0;

            LoadShader(ref vert, ShaderType.VertexShader, vertexPath);
            LoadShader(ref geom, ShaderType.GeometryShader, geometryPath);
            LoadShader(ref frag, ShaderType.FragmentShader, fragmentPath);

            ID = gl.CreateProgram();
            gl.AttachShader(ID, vert);
            gl.AttachShader(ID, geom);
            gl.AttachShader(ID, frag);
            gl.LinkProgram(ID);

            string log = gl.GetProgramInfoLog(ID);
            if (!string.IsNullOrWhiteSpace(log))
                Console.WriteLine($"Shader program link error: {log}");

            gl.DeleteShader(vert);
            gl.DeleteShader(geom);
            gl.DeleteShader(frag);
        }

        private void LoadShader(ref uint shader, ShaderType type, string path)
        {
            shader = gl.CreateShader(type);
            gl.ShaderSource(shader, File.ReadAllText(path));
            gl.CompileShader(shader);

            string log = gl.GetShaderInfoLog(shader);
            if (!string.IsNullOrWhiteSpace(log))
                Console.WriteLine($"Shader compile error when compiling shader at {path} of type {type}: {log}");
        }

        public void Use()
        {
            gl.UseProgram(ID);
        }

        public void SetFloat(string uniform, float f)
        {
            Use();
            gl.Uniform1(gl.GetUniformLocation(ID, uniform), f);
        }

        public void SetBool(string uniform, bool b)
        {
            Use();
            gl.Uniform1(gl.GetUniformLocation(ID, uniform), b ? 1 : 0);
        }

        public void SetInt(string uniform, int i)
        {
            Use();
            gl.Uniform1(gl.GetUniformLocation(ID, uniform), i);
        }

        public void SetVec2(string uniform, Vector2 vec2)
        {
            Use();
            gl.Uniform2(gl.GetUniformLocation(ID, uniform), vec2);
        }

        public void SetVec3(string uniform, Vector3 vec3)
        {
            Use();
            gl.Uniform3(gl.GetUniformLocation(ID, uniform), vec3);
        }

        public void SetVec3(string uniform, float x, float y, float z)
            => SetVec3(uniform, new Vector3(x, y, z));

        public void SetVec4(string uniform, Vector4 vec4)
        {
            Use();
            gl.Uniform4(gl.GetUniformLocation(ID, uniform), vec4);
        }

        public unsafe void SetMatrix4x4(string uniform, Matrix4x4 mat4)
        {
            Use();
            gl.UniformMatrix4(gl.GetUniformLocation(ID, uniform), 1, false, (float*)&mat4);
        }

        public unsafe void SetMatrix4x4(string uniform, GlmSharp.mat4 mat4)
        {
            Use();
            gl.UniformMatrix4(gl.GetUniformLocation(ID, uniform), 1, false, (float*)&mat4);
        }


    }
}
