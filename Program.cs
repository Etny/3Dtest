using Silk.NET.Input;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using System;
using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using System.Drawing;
using System.IO;
using System;
using System.Numerics;

namespace _3Dtest
{
    class Program
    {
        private static IWindow window;
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

        private static void Main(string[] args)
        {
            //Create a window.
            var options = WindowOptions.Default;
            options.Size = new Size(800, 600);
            options.Title = "3Dtest";

            window = Window.Create(options);


            //Assign events.
            window.Load += OnLoad;
            window.Update += OnUpdate;
            window.Render += OnRender;
            window.Resize += OnResize;

            //Run the window.
            window.Run();
        }

        private static void OnResize(Size obj)
        {
            Gl.Viewport(0, 0, (uint)obj.Width, (uint)obj.Height);
        }

        private static unsafe void OnLoad()
        {
            //Set-up input context.
            IInputContext input = window.CreateInput();
            for (int i = 0; i < input.Keyboards.Count; i++)
                input.Keyboards[i].KeyDown += KeyDown;
            
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

        }

        private unsafe static void OnRender(double obj)
        {
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit));
            Gl.Clear((uint)(ClearBufferMask.DepthBufferBit));

            Gl.Enable(GLEnum.DepthTest);
            Gl.DepthFunc(DepthFunction.Less);

            shader.Use();

            Rot += (float)(obj * 10f);
            if (Rot > 360) Rot %= 360;

            shader.SetMatrix4x4("model", Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, ToRadians(Rot)));
            shader.SetMatrix4x4("view", Matrix4x4.CreateTranslation(new Vector3(0, 0, -3)));
            shader.SetMatrix4x4("projection", Matrix4x4.CreatePerspectiveFieldOfView(ToRadians(45f), 800f / 600f, .1f, 100f));

            Gl.BindVertexArray(Vao);
            Gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }

        private static void OnUpdate(double obj)
        {
            
        }
        

        private static void KeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            //Check to close the window on escape.
            if (arg2 == Key.Escape)
            {
                window.Close();
            }
        }


        private static float ToRadians(float deg)
        {
            return (float)((deg / 180) * Math.PI);
        }
    }
}
