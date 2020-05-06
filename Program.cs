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
        private static Glfw glfw;
        private static WindowHandle* window;
        private static GL Gl;

        private static uint Vbo;
        private static uint LightVao;

        private static Model backpack;

        private static Texture boxTexture, boxSpecular;

        private static Shader shader;
        private static Shader lightShader;

        private static Vector3[] lightPositions =
        {
            new Vector3( 0.7f,  0.2f,  2.0f),
            new Vector3( 2.3f, -3.3f, -4.0f),
            new Vector3(-4.0f,  2.0f, -12.0f),
            new Vector3( 0.0f,  0.0f, -3.0f)
        };

        private static DirectionalLight Sun;
        private static SpotLight FlashLight;
        private static PointLight[] PointLights;

        private static Camera camera;

        private static float[] verts = {
    -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f,  0.0f,
         0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f,  0.0f,
         0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f,  1.0f,
         0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f,  1.0f,
        -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f,  1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f,  0.0f,

        -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f,  0.0f,
         0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f,  0.0f,
         0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f,  1.0f,
         0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f,  1.0f,
        -0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f,  1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f,  0.0f,

        -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  1.0f,  0.0f,
        -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  1.0f,  1.0f,
        -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  0.0f,  1.0f,
        -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  0.0f,  1.0f,
        -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  0.0f,  0.0f,
        -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  1.0f,  0.0f,

         0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  1.0f,  0.0f,
         0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  1.0f,  1.0f,
         0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  0.0f,  1.0f,
         0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  0.0f,  1.0f,
         0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  0.0f,  0.0f,
         0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  1.0f,  0.0f,

        -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  0.0f,  1.0f,
         0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  1.0f,  1.0f,
         0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  1.0f,  0.0f,
         0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  1.0f,  0.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  0.0f,  0.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  0.0f,  1.0f,

        -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  0.0f,  1.0f,
         0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  1.0f,  1.0f,
         0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  1.0f,  0.0f,
         0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  1.0f,  0.0f,
        -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  0.0f,  0.0f,
        -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  0.0f,  1.0f
};

        private static float Fov = 90;

        private static float lastFrame = 0.0f;
        private static float deltaTime = 0.0f;

        private static float cursorLastX = 400f, cursorLastY = 300;
        private static bool firstMouse = true;

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
            glfw.SetCursorPosCallback(window, MouseInput);
            glfw.SetScrollCallback(window, ScrollInput);
            glfw.SetInputMode(window, CursorStateAttribute.Cursor, CursorModeValue.CursorDisabled);

             OnLoad();

            float currentFrame;

            while (!glfw.WindowShouldClose(window))
            {
                currentFrame = (float)glfw.GetTime();
                deltaTime = currentFrame - lastFrame;
                lastFrame = currentFrame;

                ProcessInput(window);

                OnRender();
            }

            glfw.Terminate();
        }

       

        private unsafe static void OnResize(WindowHandle* window, int width, int height)
        {
            Gl.Viewport(0, 0, (uint)width, (uint)height);
        }

        private static unsafe void OnLoad()
        {
            Gl = GL.GetApi();

            camera = new Camera();

            backpack = new Model(Gl, "res/backpack/backpack.obj");
            //backpack = new Model(Gl, verts);

            Vbo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, Vbo);

            fixed (void* i = &verts[0])
                Gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(sizeof(float) * verts.Length), i, BufferUsageARB.StaticDraw);

            shader = new Shader(Gl, "shader.vert", "shader.frag");
          
            shader.Use();

            shader.SetFloat("material.shininess", 32.0f);

            LightVao = Gl.GenVertexArray();
            Gl.BindVertexArray(LightVao);
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, Vbo);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)0);
            Gl.EnableVertexAttribArray(0);

            Gl.Enable(EnableCap.DepthTest);

            lightShader = new Shader(Gl, "shader.vert", "lightshader.frag");

            Sun = new DirectionalLight(new Vector3(-0.2f, -1.0f, -0.3f), new Vector3(0.976f, 0.717f, 0.423f), new Vector3(0.976f, 0.717f, 0.423f), new Vector3(0.976f, 0.717f, 0.423f));
            FlashLight = new SpotLight(camera.Position, (float)Math.Cos(Util.ToRadians(1.5f)), (float)Math.Cos(Util.ToRadians(20f)));

            PointLights = new PointLight[lightPositions.Length];
            for (int i = 0; i < lightPositions.Length; i++)
                PointLights[i] = new PointLight(lightPositions[i]);
        }

        private unsafe static void OnRender()
        {
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            shader.Use();

            shader.SetVec3("viewPos", camera.Position);
            shader.SetMatrix4x4("view", camera.LookAt());
            shader.SetMatrix4x4("projection", Matrix4x4.CreatePerspectiveFieldOfView(Util.ToRadians(Fov), 800f / 600f, .1f, 100f));
            shader.SetMatrix4x4("model", Matrix4x4.Identity);

            FlashLight.Position = camera.Position;
            FlashLight.Direction = camera.GetDirection();

            Sun.SetValues(shader, "dirLight");
            FlashLight.SetValues(shader, "spotLight");
            for(int i = 0; i < PointLights.Length; i++)
                PointLights[i].SetValues(shader, $"pointLights[{i}]");

            backpack.Draw(shader);

            /*
            lightShader.Use();

            lightShader.SetMatrix4x4("view", camera.LookAt());
            lightShader.SetMatrix4x4("projection", Matrix4x4.CreatePerspectiveFieldOfView(Util.ToRadians(Fov), 800f / 600f, .1f, 100f));

            Matrix4x4 mdl = Matrix4x4.Identity;
            mdl *= Matrix4x4.CreateScale(.3f);

            for(int i = 0; i < lightPositions.Length; i++)
            {
                lightShader.SetMatrix4x4("model", mdl * Matrix4x4.CreateTranslation(lightPositions[i]));

                Gl.BindVertexArray(LightVao);
                Gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
            }*/

            glfw.SwapBuffers(window);
            glfw.PollEvents();
        }
        private unsafe static void ProcessInput(WindowHandle* window)
        {
            var camSpeed = 2.5f * deltaTime;

            if (glfw.GetKey(window, Keys.Escape) == (int)InputAction.Press)
                glfw.SetWindowShouldClose(window, true);
            if (glfw.GetKey(window, Keys.W) == (int)InputAction.Press)
                camera.Position += camera.GetDirection() * camSpeed;
            if (glfw.GetKey(window, Keys.S) == (int)InputAction.Press)
                camera.Position -= camera.GetDirection() * camSpeed;
            if (glfw.GetKey(window, Keys.A) == (int)InputAction.Press)
                camera.Position -= Vector3.Normalize(Vector3.Cross(camera.GetDirection(), camera.Up)) * camSpeed;
            if (glfw.GetKey(window, Keys.D) == (int)InputAction.Press)
                camera.Position += Vector3.Normalize(Vector3.Cross(camera.GetDirection(), camera.Up)) * camSpeed;

        }

        private unsafe static void ScrollInput(WindowHandle* window, double xScroll, double yScroll)
        {
            Fov += (float)yScroll;
            if (Fov < 1)
                Fov = 1;
            if (Fov > 120)
                Fov = 120;
        }

        private unsafe static void MouseInput(WindowHandle* window, double xpos, double ypos)
        {
            if (firstMouse!)
            {
                firstMouse = false;
                cursorLastX = (float)xpos;
                cursorLastY = (float)ypos;
                return;
            }

            float xOffset = (float)(xpos - cursorLastX);
            float yOffset = (float)(cursorLastY - ypos);
            cursorLastX = (float)xpos;
            cursorLastY = (float)ypos;

            float sensitivity = 0.2f;
            xOffset *= sensitivity;
            yOffset *= sensitivity;

            camera.Yaw += xOffset;
            camera.Pitch += yOffset;
            
            if (camera.Pitch > 89f)
                camera.Pitch = 89f;
            if (camera.Pitch < -89f)
                camera.Pitch = -89f;
        }

        private static void GlfwError(Silk.NET.GLFW.ErrorCode error, string msg)
        {
            Console.WriteLine($"Glfw encountered an error (code {error}): {msg}");
        }


    }
}
