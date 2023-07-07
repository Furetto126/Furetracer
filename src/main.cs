using GlmNet;
using Lib;
using ImGuiNET;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using RayTracer.lib;
using RayTracer.lib.Generic;

class main
{
    #region Global variables (not including important ones like window etc) and constants
    private static int colorTest = 0;

    // settings
    // --------
    private const int WIDTH = 1200;
    private const int HEIGHT = 1000;

    private const float mouseSensitivity = 0.001f;
    public static float fov = 45.0f;

    // shader settings
    // ---------------
    public static int maxBounces = 10;
    public static float ambientWeight = 1.0f;
    public static int numRaysPerPixel = 10;
    public static bool isRaytracingActivated = false;

        // Progressive rendering
        // -------------------------------
        public static bool progressiveRenderingActivated = false;
        public static int currentFBO, previousFBO, currentFrameTexture, previousFrameTexture;
        private static int framesFromRendering = 0;

    // camera
    // ------
    public static vec3 cameraPosition = new vec3(0.0f, 0.0f, 0.0f);
    public static vec3 cameraFront = new vec3(0.0f, 0.0f, 1.0f);
    private static vec3 cameraUp = new vec3(0.0f, 1.0f, 0.0f);
    private static vec3 cameraRight = new vec3(glm.normalize(glm.cross(cameraFront, cameraUp)));

    private static float aspectRatio = WIDTH / HEIGHT;

    private static mat4 inverseViewMatrix;

    // mouse and movement
    // ------------------
    private static vec2 lastCursorPosition;
    private static bool isRightMouseButtonPressed = false;
    private static bool isMiddleMouseButtonPressed = false;

    // ImGui stuff
    // -----------
    public static bool isAnyWindowHovered = false;
    public static bool isAnyItemHovered = false;

    public static bool isInspectorWindowHovered = false;
    public static bool isConsoleWindowHovered = false;
    public static bool isSceneWindowHovered = false;
    public static bool isSettingsWindowHovered = false;

    public static bool scenePopupOpen = false;

    // paths
    // -----
    private static readonly string rootDirectory = Common.GetRootDirectory();

    // generic
    // -------
    private static int VAO;
    private static float spamCooldown = 0.0f;
    public static int currentWidth = WIDTH, currentHeight = HEIGHT;

    public static StringWriter consoleOutput = new();
    private static FilteringStringWriter filteringStringWriter = new FilteringStringWriter(consoleOutput);
    #endregion

    private static GameWindow window;
    private static ImGuiController controller;
    private static Shader rayTracerShader = new Shader(Path.Combine(rootDirectory, "src\\shaders\\vertex.vert"), Path.Combine(rootDirectory, "src\\shaders\\fragment.frag"));

    static void Main()
    {
        NativeWindowSettings glfwOptions = new NativeWindowSettings
        {
            Size = new Vector2i(WIDTH, HEIGHT),
            Title = "Raytracing Demo",
            API = ContextAPI.OpenGL,
            APIVersion = new Version(4, 5),
            Flags = ContextFlags.ForwardCompatible
        };

        window = new GameWindow(GameWindowSettings.Default, glfwOptions);
        window.WindowState = WindowState.Maximized;

        // Attach event handlers
        // ---------------------
        window.Load += OnLoad;
        window.UpdateFrame += OnUpdateFrame;
        window.RenderFrame += OnRenderFrame;
        window.Closing += OnClosing;

        // Set Callbacks
        window.KeyDown += OnKeyDown;
        window.MouseDown += OnMouseButtonDown;
        window.MouseUp += OnMouseButtonUp;
        window.MouseWheel += OnMouseWheel;
        window.Resize += OnWindowResize;

        window.Run();
    }

    private static void OnLoad()
    {
        // Initialize VAO
        // ----------------------------------
        GL.GenVertexArrays(1, out VAO);
        GL.BindVertexArray(VAO);

        // Set up FBOs and Textures
        // -----------------------
        GL.GenFramebuffers(1, out currentFBO);
        GL.GenFramebuffers(1, out previousFBO);
        GL.GenTextures(1, out currentFrameTexture);
        GL.GenTextures(1, out previousFrameTexture);

        // Set up current texture
        GL.BindTexture(TextureTarget.Texture2D, currentFrameTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, currentWidth, currentHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        // Set up previous texture
        GL.BindTexture(TextureTarget.Texture2D, previousFrameTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, currentWidth, currentHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        // Attach textures
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, currentFBO);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, currentFrameTexture, 0);

        // Completeness check
        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            Console.WriteLine("Framebuffer error: " + status);
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, previousFBO);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, previousFrameTexture, 0);

        // Completeness check
        status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            Console.WriteLine("Framebuffer error: " + status);
        }
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        // Initialize ImGui
        controller = new ImGuiController(WIDTH, HEIGHT);

        rayTracerShader.LoadMesh(Path.Combine(Common.GetRootDirectory(), "models\\flower.obj"));
        rayTracerShader.SendTrianglesToShader();

        Console.SetOut(filteringStringWriter);
    }

    private static void OnUpdateFrame(FrameEventArgs e)
    {
        cameraRight = glm.normalize(glm.cross(cameraFront, cameraUp));

        if (!progressiveRenderingActivated)
        {
            if (isRightMouseButtonPressed)
            {
                RotateCamera();
            }
            else if (isMiddleMouseButtonPressed)
            {
                MoveCamera();
            }
        }

        isAnyItemHovered = ImGui.IsAnyItemHovered();
        isAnyWindowHovered = isInspectorWindowHovered || isConsoleWindowHovered || isSceneWindowHovered || isSettingsWindowHovered;

        UpdateUniforms(rayTracerShader);
        UpdateScene(rayTracerShader);
    }

    private static void OnRenderFrame(FrameEventArgs e)
    {
        // Bind currentFBO
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, currentFBO);

        GL.ClearColor(Color4.Black);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        if (progressiveRenderingActivated)
        {
            framesFromRendering++;
        }else
        {
            framesFromRendering = 0;
        }

        rayTracerShader.SetInt("framesFromRenderStart", framesFromRendering);
        rayTracerShader.Use();

        // Bind previousFrameTexture
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, previousFrameTexture);
        rayTracerShader.SetInt("previousFrameTex", 0);

        // Render the full-screen quad
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        // Unbind current FBO
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        // Swap current and previous FBO and textures
        Common.Swap(ref currentFBO, ref previousFBO);
        Common.Swap(ref currentFrameTexture, ref previousFrameTexture);

        // Bind the previous frame texture
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, previousFrameTexture);

        // Draw ImGui
        controller.Update(window, (float)e.Time);
        ImGuiHandler.ConstructImGui(rayTracerShader);
        controller.Render();
        ImGuiController.CheckGLError("End of frame");

        window.SwapBuffers();
    }

    private static void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        GL.DeleteVertexArrays(1, new int[] { VAO });
        GL.DeleteProgram(rayTracerShader.GetProgramShader());
    }

    #region Callbacks
    private static void OnKeyDown(KeyboardKeyEventArgs e)
    {
        bool ctrlPressed = e.Modifiers == KeyModifiers.Control;
        bool shiftPressed = e.Modifiers == KeyModifiers.Shift;

        if (e.Key == Keys.Backspace && ImGui.GetIO().WantTextInput)
        {
            ImGui.GetIO().AddInputCharacter('\b');
        }else
        {
            controller.PressChar((char)e.Key);
        }
        

        if (ctrlPressed)
        {
            if (GLFW.GetTime() - spamCooldown > 0.5)
            {
                switch (e.Key)
                {
                    case Keys.S:
                    {
                        SaveHelper.SaveScene(rayTracerShader);
                        spamCooldown = (float)GLFW.GetTime();
                        break;
                    }
                    case Keys.L:
                    {
                        SaveHelper.LoadScene(rayTracerShader);
                        spamCooldown = (float)GLFW.GetTime();
                        break;
                    }
                    case Keys.D:
                    {
                        SaveHelper.ClearScene(rayTracerShader);
                        spamCooldown = (float)GLFW.GetTime();
                        break;
                    }
                    case Keys.A:
                    {
                        isRaytracingActivated = !isRaytracingActivated;
                        spamCooldown = (float)GLFW.GetTime();
                        break;
                    }
                }
            }
        }
        else if (shiftPressed)
        {

        }
        else
        {
            if (GLFW.GetTime() - spamCooldown > 0.5)
            {
                switch (e.Key)
                {
                    case Keys.Escape:
                    {
                        // At this point, all program tasks have been completed and the application is ready to gracefully terminate.
                        // Therefore, the following command ensures that the program is closed in an elegant and orderly manner.
                        // https://cdn.discordapp.com/attachments/870272324111323237/1106268518783144088/image.png
                        window.Close();
                        break;
                    }

                    case Keys.V:
                    {
                        spamCooldown = (float)GLFW.GetTime();

                        if (colorTest == 0)
                        {
                            rayTracerShader.AddToSphereList(new Sphere(cameraPosition, 100.0f, new vec3(0.0f), new vec3(1.0f), 1.0f, 0.0f, 0.0f));
                            colorTest = 1;
                        }
                        else
                        {
                            float smoothnessAndGlossiness = (float)new Random().NextDouble();
                            rayTracerShader.AddToSphereList(new Sphere(cameraPosition, 1.0f, new vec3((float)new Random().NextDouble(), (float)new Random().NextDouble(), (float)new Random().NextDouble()), new vec3((float)new Random().NextDouble(), (float)new Random().NextDouble(), (float)new Random().NextDouble()), 0.0f, smoothnessAndGlossiness, smoothnessAndGlossiness));
                        }
                        break;
                    }
                }
            }
            else // No cooldown
            {
                switch (e.Key)
                {

                }
            }
        }
    }

    private static void OnMouseWheel(MouseWheelEventArgs e)
    {
        controller.MouseScroll(e.Offset);

        if (!isAnyWindowHovered && !progressiveRenderingActivated)
        {
            cameraPosition += cameraFront * -(float)e.OffsetY * 1.5f;
        }

    }

    private static void OnMouseButtonDown(MouseButtonEventArgs e)
    { 
        switch (e.Button)
        {
            case MouseButton.Right:
            {
                if (!isAnyWindowHovered && !progressiveRenderingActivated)
                {
                    lastCursorPosition = new vec2(window.MouseState.Position.X, window.MouseState.Position.Y);
                    isRightMouseButtonPressed = true;

                    window.CursorVisible = false;
                    window.CursorGrabbed = true;
                }
                else if (isSceneWindowHovered && !isAnyItemHovered)
                {
                    scenePopupOpen = true;
                }

                break;
            }

            case MouseButton.Left:
            {
                if (!isAnyWindowHovered && !progressiveRenderingActivated)
                {
                    lastCursorPosition = new vec2(window.MouseState.Position.X, window.MouseState.Position.Y);

                    int sphereHitIndex = RayCaster.CheckSphereIntersectionCoord(
                        lastCursorPosition,
                        cameraPosition,
                        rayTracerShader,
                        new vec2(currentWidth, currentHeight),
                        fov,
                        inverseViewMatrix
                    );

                    if (sphereHitIndex != int.MinValue)
                    {
                        //rayTracerShader.RemoveFromSphereList(sphereHitIndex);
                    }
                }

                scenePopupOpen = false;

                break;
            }

            case MouseButton.Middle:
            {
                if (!isAnyWindowHovered && !progressiveRenderingActivated)
                {
                    lastCursorPosition = new vec2(window.MouseState.Position.X, window.MouseState.Position.Y);
                    isMiddleMouseButtonPressed = true;

                    window.CursorVisible = false;
                    window.CursorGrabbed = true;

                }

                scenePopupOpen = false;

                break;
            }

        }



    }

    private static void OnMouseButtonUp(MouseButtonEventArgs e)
    {
        if (!isAnyWindowHovered && !progressiveRenderingActivated)
        {
            if (e.Button == MouseButton.Right)
            {
                isRightMouseButtonPressed = false;

                window.CursorVisible = true;
                window.CursorGrabbed = false;
            }
            else if (e.Button == MouseButton.Middle)
            {
                isMiddleMouseButtonPressed = false;

                window.CursorVisible = true;
                window.CursorGrabbed = false;
            }
        }
    }

    private static void OnWindowResize(ResizeEventArgs e)
    {
        currentWidth = e.Width;
        currentHeight = e.Height;

        GL.Viewport(0, 0, currentWidth, currentHeight);
        controller.WindowResized(currentWidth, currentHeight);

        if (currentWidth != 0 && currentHeight != 0)
        {
            aspectRatio = (float)currentWidth / currentHeight;
        }
        else
        {
            aspectRatio = 1.1f;
        }

        GL.BindTexture(TextureTarget.Texture2D, currentFrameTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, currentWidth, currentHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

        // Update the size of the previous texture
        GL.BindTexture(TextureTarget.Texture2D, previousFrameTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, currentWidth, currentHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

        // Update the size of the current framebuffer
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, currentFBO);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, currentFrameTexture, 0);

        // Update the size of the previous framebuffer
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, previousFBO);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, previousFrameTexture, 0);

        // Check framebuffer completeness
        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            Console.WriteLine("Framebuffer error: " + status);
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }
    #endregion

    #region Methods
    private static void UpdateScene(Shader shader)
    {
        shader.SendSpheresToShader();
    }

    private static void UpdateUniforms(Shader shader)
    {
        // generic
        // -------
        shader.SetFloat("time", (float)GLFW.GetTime());
        shader.SetVec2("resolution", new vec2(currentWidth, currentHeight));
        if (currentHeight != 0 && currentWidth != 0)
        {
            shader.SetFloat("aspectRatio", aspectRatio);
        }

        // camera
        // ------
        shader.SetVec3("cameraPosition", cameraPosition);
        shader.SetVec3("cameraDirection", cameraFront);
        shader.SetVec3("cameraRight", cameraRight);
        shader.SetFloat("fov", fov);

        // shader settings
        // ---------------
        shader.SetInt("maxBounces", maxBounces);
        shader.SetInt("numRaysPerPixel", numRaysPerPixel);
        shader.SetFloat("ambientWeight", ambientWeight);
        shader.SetBool("progressiveRenderingActivated", progressiveRenderingActivated);
        shader.SetBool("raytracingActivated", isRaytracingActivated);

        // matrices
        // --------
        inverseViewMatrix = glm.inverse(glm.lookAt(cameraPosition, cameraPosition + cameraFront, cameraUp));
        shader.SetMat4("viewMatrixInverse", inverseViewMatrix);
    }

    private static void RotateCamera()
    {
        vec2 currentCursorPosition = new vec2(window.MouseState.Position.X, window.MouseState.Position.Y);
        vec2 cursorOffset = currentCursorPosition - lastCursorPosition;
        lastCursorPosition = currentCursorPosition;

        float xOffset = -cursorOffset.x * mouseSensitivity;
        float yOffset = -cursorOffset.y * mouseSensitivity;
        float rotationSpeed = 1.0f;

        cameraFront = glm.normalize(cameraFront);
        cameraFront += (cameraRight * xOffset - cameraUp * yOffset) * rotationSpeed;
        cameraFront = glm.normalize(cameraFront);
    }

    private static void MoveCamera()
    {
        vec2 currentCursorPosition = new vec2(window.MouseState.Position.X, window.MouseState.Position.Y);
        vec2 cursorOffset = currentCursorPosition - lastCursorPosition;
        lastCursorPosition = currentCursorPosition;

        float xOffset = cursorOffset.x;
        float yOffset = cursorOffset.y;

        float cameraSpeed = 0.1f;
        vec3 tangent = glm.normalize(new vec3(cameraFront.z, 0.0f, -cameraFront.x));
        vec3 bitangent = glm.cross(cameraFront, tangent);

        vec3 movement = tangent * xOffset + bitangent * yOffset;
        movement *= cameraSpeed;

        cameraPosition += movement;
    }
    #endregion
}

