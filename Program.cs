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
using GlmSharp;

namespace _3Dtest
{
    unsafe class Program
    {
        private static Glfw glfw;
        private static WindowHandle* window;
        private static GL Gl;

        private static Model backpack;

        private static Shader shader;
        private static Shader screenShader;
        private static Shader depthShader;

        private static uint fbo;
        private static uint screenTexture;
        private static uint rbo;

        private static uint fboShadow;
        private static uint shadowMap;

        private static uint shadowCube;
        private static mat4[] shadowCubeLookMats = new mat4[6];

        public static uint matrices;

        private static uint quadVbo;
        private static uint quadVao;

        private static Vector3[] lightPositions =
        {
            new Vector3( 0.7f,  0.2f,  2.0f)/*,
            new Vector3( 2.3f, -3.3f, -4.0f),
            new Vector3(-4.0f,  2.0f, -12.0f),
            new Vector3( 0.0f,  0.0f, -3.0f)*/
        };

        private static float[] quadVertices = { // vertex attributes for a quad that fills the entire screen in Normalized Device Coordinates.
            // positions   // texCoords
            -1.0f,  1.0f,  0.0f, 1.0f,
            -1.0f, -1.0f,  0.0f, 0.0f,
             1.0f, -1.0f,  1.0f, 0.0f,

            -1.0f,  1.0f,  0.0f, 1.0f,
             1.0f, -1.0f,  1.0f, 0.0f,
             1.0f,  1.0f,  1.0f, 1.0f
        };

        private static uint shadowMapSize = 1024;

        private static DirectionalLight Sun;
        private static SpotLight FlashLight;
        private static PointLight[] PointLights;

        private static Camera camera;

        private static float Fov = 90;

        private static double lastFrame = 0.0f;
        private static double deltaTime = 0.0f;

        private static float cursorLastX = 400f, cursorLastY = 300;
        private static bool firstMouse = true;

        private unsafe static void Main(string[] args)
        {

            glfw = Glfw.GetApi();

            Console.WriteLine(glfw.GetVersionString());

            glfw.SetErrorCallback(GlfwError);

            //SilkManager.Register<IWindowPlatform>(glfwPlatform);
            SilkManager.Register<GLSymbolLoader>(new Silk.NET.GLFW.GlfwLoader());

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
            //glfw.SwapInterval(1);

            OnLoad();

            double currentFrame;

            while (!glfw.WindowShouldClose(window))
            {
                currentFrame = glfw.GetTimerValue();
                deltaTime = (currentFrame - lastFrame) / glfw.GetTimerFrequency();
                lastFrame = currentFrame;

                ProcessInput(window);

                //Console.WriteLine($"Fps: {Math.Round(1f/deltaTime)}");

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
           
            shader = new Shader(Gl, "shader.vert", "shader.frag");
            shader.Use();
            shader.SetFloat("material.shininess", 32.0f);
            Gl.UniformBlockBinding(shader.ID, Gl.GetUniformBlockIndex(shader.ID, "Matrices"), 0);

            matrices = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.UniformBuffer, matrices);
            Gl.BufferData(BufferTargetARB.UniformBuffer, (uint)(2 * sizeof(Matrix4x4)), null, BufferUsageARB.StaticDraw);
            Gl.BindBufferRange(BufferTargetARB.UniformBuffer, 0, matrices, 0, (uint)(2 * sizeof(Matrix4x4)));

            Matrix4x4 projectionMat = Matrix4x4.CreatePerspectiveFieldOfView(Util.ToRadians(Fov), 800f / 600f, .1f, 100f);
            Gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, (uint)sizeof(Matrix4x4), &projectionMat);

            screenShader = new Shader(Gl, "screenshader.vert", "screenshader.frag");
            depthShader = new Shader(Gl, "depthshader.vert", "depthshader.frag");

            quadVao = Gl.GenVertexArray();
            Gl.BindVertexArray(quadVao);

            quadVbo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, quadVbo);
            fixed (void* i = &quadVertices[0])
                Gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(quadVertices.Length * sizeof(float)), i, BufferUsageARB.StaticDraw);

            Gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)0);
            Gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)(2 * sizeof(float)));
            Gl.EnableVertexAttribArray(0);
            Gl.EnableVertexAttribArray(1);

            Gl.BindVertexArray(0);

            Gl.Enable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.StencilTest);
            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(CullFaceMode.Back);

            fbo = Gl.GenFramebuffer();
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            screenTexture = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2D, screenTexture);
            Gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgb, 800, 600, 0, PixelFormat.Rgb, PixelType.UnsignedByte, null);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, 0);

            Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, screenTexture, 0);

            rbo = Gl.GenRenderbuffer();
            Gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
            Gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, 800, 600);
            Gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            Gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.DepthStencilAttachment, GLEnum.Renderbuffer, rbo);

            
            fboShadow = Gl.GenFramebuffer();
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, fboShadow);
            
            shadowMap = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2D, shadowMap);
            Gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.DepthComponent, 
                shadowMapSize, shadowMapSize, 0, PixelFormat.DepthComponent, PixelType.Float, null);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            float[] borderColor = new float[] { 1, 1, 1, 1 };
            fixed(float* i = &borderColor[0])
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, i);

            shadowCube = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.TextureCubeMap, shadowCube);
            for (int i = 0; i < 6; i++)
                Gl.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, (int)InternalFormat.DepthComponent, 
                    shadowMapSize, shadowMapSize, 0, PixelFormat.DepthComponent, PixelType.Float, null);
            Gl.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            Gl.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            //Gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.DepthAttachment, GLEnum.Texture2D, shadowMap, 0);
            Gl.FramebufferTexture(GLEnum.Framebuffer, GLEnum.DepthAttachment, shadowCube, 0);
            Gl.DrawBuffer(DrawBufferMode.None);
            Gl.ReadBuffer(ReadBufferMode.None);
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            Gl.BindTexture(TextureTarget.TextureCubeMap, 0);



            if (Gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
                Console.WriteLine("Oh boy, Framebuffer bad!");
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            Sun = new DirectionalLight(new Vector3(0.65854764f, -0.5150382f, -0.54868096f), new Vector3(0.01f, 0.01f, 0.01f), new Vector3(0.976f, 0.717f, 0.423f), new Vector3(0.976f, 0.717f, 0.423f));
            FlashLight = new SpotLight(camera.Position, (float)Math.Cos(Util.ToRadians(1.5f)), (float)Math.Cos(Util.ToRadians(20f)));

            PointLights = new PointLight[lightPositions.Length];
            for (int i = 0; i < lightPositions.Length; i++)
                PointLights[i] = new PointLight(lightPositions[i]);

            vec3 lightPos = new vec3(PointLights[0].Position.X, PointLights[0].Position.Y, PointLights[0].Position.Z);
            shadowCubeLookMats[0] = mat4.LookAt(lightPos, lightPos + new vec3(1, 0, 0), new vec3(0, -1, 0));
            shadowCubeLookMats[1] = mat4.LookAt(lightPos, lightPos + new vec3(-1, 0, 0), new vec3(0, -1, 0));
            shadowCubeLookMats[2] = mat4.LookAt(lightPos, lightPos + new vec3(0, 1, 0), new vec3(0, 0, 1));
            shadowCubeLookMats[3] = mat4.LookAt(lightPos, lightPos + new vec3(0, -1, 0), new vec3(0, 0, -1));
            shadowCubeLookMats[4] = mat4.LookAt(lightPos, lightPos + new vec3(0, 0, 1), new vec3(0, -1, 0));
            shadowCubeLookMats[5] = mat4.LookAt(lightPos, lightPos + new vec3(0, 0, -1), new vec3(0, -1, 0));

        }

        private unsafe static void OnRender()
        {
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit));
            
            Gl.Viewport(0, 0, shadowMapSize, shadowMapSize);
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, fboShadow);
            Gl.Clear((uint)ClearBufferMask.DepthBufferBit);
            Gl.Enable(EnableCap.DepthTest);
            //Gl.CullFace(CullFaceMode.Front);

            //Ortho LightSpace
            /*mat4 lP = mat4.Ortho(-10, 10, -10, 10, .1f, 15f);
            mat4 lL = mat4.LookAt(new GlmSharp.vec3(0, 0, 10), new GlmSharp.vec3(0), new GlmSharp.vec3(0, 1, 0));
            mat4 lS = lP * lL;*/
            //Persp LightSpace
            mat4 LP = mat4.Perspective(Util.ToRadians(90), shadowMapSize / shadowMapSize, .1f, 25);

            depthShader.Use();
            //depthShader.SetMatrix4x4("lightSpace", lS);
            //for (int i = 0; i < 6; i++)
            //    depthShader.SetMatrix4x4($"shadowMats[{i}]", LP * shadowCubeLookMats[i]);
            depthShader.SetFloat("farPlane", 25);
            depthShader.SetVec3("lightPos", lightPositions[0]);

            for(int i = 0; i < 6; i++)
            {
                Gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.DepthAttachment, GLEnum.TextureCubeMapPositiveX + i, shadowCube, 0);
                depthShader.SetMatrix4x4("lightSpace", LP * shadowCubeLookMats[i]);
                RenderScene(depthShader, true);
            }
            //RenderScene(depthShader, true);

            Gl.Viewport(0, 0, 800, 600);
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit));

            shader.Use();
            shader.SetVec3("viewPos", camera.Position);
            //shader.SetMatrix4x4("lightSpace", lS);
            shader.SetFloat("farPlane", 25);
            shader.SetInt("shadowMap", 12);
            Gl.ActiveTexture(TextureUnit.Texture12);
            Gl.BindTexture(TextureTarget.TextureCubeMap, shadowCube);
            //Gl.CullFace(CullFaceMode.Back);


            Matrix4x4 viewMat = camera.LookAt();
            Gl.BindBuffer(BufferTargetARB.UniformBuffer, matrices);
            Gl.BufferSubData(BufferTargetARB.UniformBuffer, sizeof(Matrix4x4), (uint)sizeof(Matrix4x4), &viewMat);

            RenderScene(shader, true);
            
            screenShader.Use();
            screenShader.SetInt("screen", 0);

            Gl.BindVertexArray(quadVao);
            Gl.Disable(EnableCap.DepthTest);
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit));
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2D, screenTexture);
            Gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
            
            glfw.SwapBuffers(window);
            glfw.PollEvents();
        }

        private static void RenderScene(Shader shader, bool setLighting)
        {
            shader.Use();
    
            shader.SetMatrix4x4("model", Matrix4x4.Identity);
            //shader.SetMatrix4x4("model", Matrix4x4.Identity * Matrix4x4.CreateFromAxisAngle(new Vector3(0.5f, 0.2f, 1), 45f) * Matrix4x4.CreateScale(2f));

            if (setLighting)
            {
                FlashLight.Position = camera.Position;
                FlashLight.Direction = camera.GetDirection();

                Sun.SetValues(shader, 0);
                FlashLight.SetValues(shader, 0);
                for (int i = 0; i < PointLights.Length; i++)
                    PointLights[i].SetValues(shader, i);

                shader.SetInt("dirLightCount", 0);
                shader.SetInt("spotLightCount", 1);
                shader.SetInt("pointLightCount", PointLights.Length);
            }

            Gl.Enable(EnableCap.DepthTest);
            backpack.Draw(shader);

            //shader.SetMatrix4x4("model", Matrix4x4.Identity * Matrix4x4.CreateTranslation(new Vector3(0, 0, 4)) * Matrix4x4.CreateScale(new Vector3((float)Math.Sin(glfw.GetTime()) / 2 + 1f)));
            //backpack.Draw(shader);
        }

        private unsafe static void ProcessInput(WindowHandle* window)
        {
            float camSpeed = (float)(2.5f * deltaTime);

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
            if (glfw.GetKey(window, Keys.G) == (int)InputAction.Press)
                Console.WriteLine($"Dir: {camera.GetDirection()}, Pos: {camera.Position}, Yaw: {camera.Yaw}, Pitch: {camera.Pitch}");
            

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
