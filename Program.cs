using Silk.NET.Input;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using Silk.NET.GLFW;
using System;
using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using System.Drawing;
using System.IO;
using System.Numerics;
using Silk.NET.Core.Loader;
using Silk.NET.Core.Platform;
using Silk.NET.Windowing.Desktop;

namespace _3Dtest
{
    unsafe class Program
    {
        private static IWindow window1;
        private static Glfw glfw;
        private static WindowHandle* window;
        private static GL Gl;

        private static uint Vbo;
        private static uint Vao;
        private static uint Ebo;

        private static Texture boxTexture, faceTexture;

        private static Shader shader;

        private static float[] verts = {
    -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
     0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
     0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
     0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
    -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
    -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

    -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
     0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
     0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
     0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
    -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
    -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

    -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
    -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
    -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
    -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
    -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
    -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

     0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
     0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
     0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
     0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
     0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
     0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

    -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
     0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
     0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
     0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
    -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
    -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

    -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
     0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
     0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
     0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
    -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
    -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
};
    

        private static uint[] indices = {  // note that we start from 0!
            0, 1, 3,
            1, 2, 3
            
        };

        private static float Rot = 225;

        private unsafe static void Main(string[] args)
        {

            glfw = Glfw.GetApi();

            Console.WriteLine(glfw.GetVersionString());

            glfw.SetErrorCallback(GlfwError);

            SilkManager.Register<GLSymbolLoader>(new GlfwLoader());

            glfw.Init();
            glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
            glfw.WindowHint(WindowHintInt.ContextVersionMinor, 3);
            glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

            window = glfw.CreateWindow(800, 600, "3Dtest", null, null);

            if (window == null)
            {
                Console.WriteLine("Window creation failed");
                glfw.Terminate();
                return;
            }


            glfw.MakeContextCurrent(window);
            glfw.SetWindowSizeCallback(window, OnResize);


             OnLoad();

            while (!glfw.WindowShouldClose(window))
            {
                ProcessInput(window);

                OnRender();
            }

            glfw.Terminate();
        }

       

        private unsafe static void OnResize(WindowHandle* window, int width, int height)
        {
           // Gl.Viewport(0, 0, (uint)width, (uint)height);
        }

        private static unsafe void OnLoad()
        {
            Gl = GL.GetApi();
            

            Vao = Gl.GenVertexArray();
            Gl.BindVertexArray(Vao);

            Vbo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, Vbo);

            fixed (void* i = &verts[0])
                Gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(sizeof(float) * verts.Length), i, BufferUsageARB.StaticDraw);

            /*
            Ebo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, Ebo);

            fixed (void* i = &indices[0])
                Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(sizeof(uint) * indices.Length), i, BufferUsageARB.StaticDraw);
                */

            shader = new Shader(Gl, "shader.vert", "shader.frag");

            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);
            Gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
            Gl.EnableVertexAttribArray(0);
            Gl.EnableVertexAttribArray(1);

            boxTexture = new Texture(Gl, "res/container.jpg");
            faceTexture = new Texture(Gl, "res/container.jpg");

            shader.Use();

            shader.SetInt("texture1", 0);
            shader.SetInt("texture2", 1);

            boxTexture.BindToUnit(0);
            faceTexture.BindToUnit(1);


            Gl.Enable(EnableCap.DepthTest);
            
        }

        private unsafe static void OnRender()
        {
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            shader.Use();

            Rot = (float)(glfw.GetTime() * 10f);
            if (Rot > 360) Rot %= 360;

            shader.SetMatrix4x4("model", Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, ToRadians(Rot)));
            shader.SetMatrix4x4("view", Matrix4x4.CreateTranslation(new Vector3(0, 0, -3)));
            shader.SetMatrix4x4("projection", Matrix4x4.CreatePerspectiveFieldOfView(ToRadians(45f), 800f / 600f, 1f, 100f));

            Gl.BindVertexArray(Vao);
            Gl.DrawArrays(PrimitiveType.Triangles, 0, 36);

            glfw.SwapBuffers(window);
            glfw.PollEvents();
        }
        private unsafe static void ProcessInput(WindowHandle* window)
        {
            if (glfw.GetKey(window, Keys.Escape) == (int)InputAction.Press)
                glfw.SetWindowShouldClose(window, true);
        }

        private static void GlfwError(Silk.NET.GLFW.ErrorCode error, string msg)
        {
            Console.WriteLine($"Glfw encountered an error (code {error}): {msg}");
        }



        private static float ToRadians(float deg)
        {
            return (float)((deg / 180) * Math.PI);
        }
    }
}
