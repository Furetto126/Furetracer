using GlmNet;

using Lib;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

using ImGuiNET;


namespace 
{
    class main
    {
        #region Global variables and constants

        static int colorTest = 0;

        // settings
        // --------
        const int WIDTH = 1200;
        const int HEIGHT = 1000;

        const float mouseSensitivity = 0.001f;
        static float fov = 45.0f;

        // shader settings
        // ---------------
        const int maxBounces = 3;
        const float ambientWeight = 1.0f;
        const int numRaysPerPixel = 25;

        static bool isRaytracingActivated = false;

        // camera
        // ------
        static vec3 cameraPosition = new vec3(0.0f, 0.0f, 0.0f);
        static vec3 cameraFront = new vec3(0.0f, 0.0f, 1.0f);
        static vec3 cameraUp = new vec3(0.0f, 1.0f, 0.0f);
        static vec3 cameraRight = new vec3(glm.normalize(glm.cross(cameraFront, cameraUp)));

        static readonly float near = 0.1f;
        static readonly float far = 100.0f;

        static float aspectRatio = WIDTH / HEIGHT;

        private static mat4 inverseViewMatrix;

        // mouse and movement
        // ------------------
        private static vec2 lastCursorPosition;
        private static bool isRightMouseButtonPressed = false;
        private static bool isMiddleMouseButtonPressed = false;

        // paths
        // -----
        static string rootDirectory = Common.GetRootDirectory();
        static Shader rayTracerShader = new Shader(Path.Combine(rootDirectory, "src\\shaders\\vertex.vert"), Path.Combine(rootDirectory, "src\\shaders\\fragment.frag"));

        // other
        // -----
        static float deltaTime = 0.0f;
        static float lastFrame = 0.0f;
        static float spamCooldown = 0.0f;

        static int currentWidth = WIDTH, currentHeight = HEIGHT;

        #endregion

        static void Main()
        {
            #region Window initialization
            Glfw.Init();
            Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGL);
            Glfw.WindowHint(Hint.ContextVersionMajor, 4);
            Glfw.WindowHint(Hint.ContextVersionMinor, 5);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
            Glfw.WindowHint(Hint.Doublebuffer, true);
            Glfw.WindowHint(Hint.Decorated, true);

            Window window = Glfw.CreateWindow(WIDTH, HEIGHT, "Ray Tracer Demo", Monitor.None, Window.None);
            if (window == Window.None)
            {
                Console.WriteLine("Failed to create window");
                Glfw.Terminate();
                return;
            }
            Glfw.MakeContextCurrent(window);
            GL.LoadBindings(new GLFWBindingsContext());

            // Enable ImGui
            // ------------
            ImGui.CreateContext();
            //ImGui.StyleColorsDark();

            ImGuiIOPtr io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            // Set GLFW Callbacks
            // ------------------
            Glfw.SetFramebufferSizeCallback(window, FramebufferSizeCallback);
            Glfw.SetCursorPositionCallback(window, CursorPositionCallback);
            Glfw.SetKeyCallback(window, KeyboardCallback);
            Glfw.SetCharCallback(window, CharCallback);
            Glfw.SetMouseButtonCallback(window, MouseButtonCallback);
            Glfw.SetScrollCallback(window, ScrollCallback);

            GL.Enable(EnableCap.DepthTest);
            Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Normal);

            GL.GenVertexArrays(1, out int VAO);
            GL.BindVertexArray(VAO);

            #endregion

            #region Render loop
            while (!Glfw.WindowShouldClose(window))
            {
                Glfw.PollEvents();

                float currentTime = (float)Glfw.GetTime();
                deltaTime = currentTime - lastFrame;
                lastFrame = currentTime;

                // Put debug stuff here
                // --------------------


                // ImGui
                // -----
                //ImGui.NewFrame();
                //ImGui.Begin("Hello chat");
                //ImGui.Text("Goodbye chat");
                //ImGui.End();

                //ImGui.Render();

                // Camera Movement
                // ---------------
                cameraRight = glm.normalize(glm.cross(cameraFront, cameraUp));

                if (isRightMouseButtonPressed)
                {
                    RotateCamera(window);
                }
                else if (isMiddleMouseButtonPressed)
                {
                    MoveCamera(window);
                }

                // Updates
                // -------
                UpdateUniforms(rayTracerShader);
                UpdateScene(rayTracerShader);
                rayTracerShader.Use();

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.BindVertexArray(VAO);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                Glfw.SwapBuffers(window);
            }
            
            // Cleanup
            // -------
            GL.DeleteVertexArrays(1, new int[] { VAO });
            GL.DeleteProgram(rayTracerShader.GetProgramShader());

            ImGui.DestroyContext();

            Glfw.DestroyWindow(window);
            Glfw.Terminate();

            return;
            #endregion
        }

        #region Methods
        static void UpdateScene(Shader shader)
        {
            shader.SendSpheresToShader();
        }

        static void UpdateUniforms(Shader shader)
        {
            // generic
            // -------
            shader.SetFloat("time", (float)Glfw.GetTime());
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
            shader.SetBool("raytracingActivated", isRaytracingActivated);

            // matrices
            // --------
            inverseViewMatrix = glm.inverse(glm.lookAt(cameraPosition, cameraPosition + cameraFront, cameraUp));
            shader.SetMat4("viewMatrixInverse", inverseViewMatrix);
        }

        static void RotateCamera(Window window)
        {
            Glfw.GetCursorPosition(window, out double x, out double y);

            vec2 currentCursorPosition = new vec2((float)x, (float)y);
            vec2 cursorOffset = currentCursorPosition - lastCursorPosition;
            lastCursorPosition = currentCursorPosition;

            float xOffset = -cursorOffset.x * mouseSensitivity;
            float yOffset = -cursorOffset.y * mouseSensitivity;
            float rotationSpeed = 1.0f;

            cameraFront = glm.normalize(cameraFront);
            cameraFront += (cameraRight * xOffset - cameraUp * yOffset) * rotationSpeed;
            cameraFront = glm.normalize(cameraFront);
        }

        static void MoveCamera(Window window)
        {
            Glfw.GetCursorPosition(window, out double x, out double y);

            vec2 currentCursorPosition = new vec2((float)x, (float)y);
            vec2 cursorOffset = currentCursorPosition - lastCursorPosition;
            lastCursorPosition = currentCursorPosition;

            float xOffset = cursorOffset.x;
            float yOffset =  cursorOffset.y;

            float cameraSpeed = 0.1f;
            vec3 tangent = glm.normalize(new vec3(cameraFront.z, 0.0f, -cameraFront.x));
            vec3 bitangent = glm.cross(cameraFront, tangent);

            vec3 movement = tangent * xOffset + bitangent * yOffset;
            movement *= cameraSpeed;

            cameraPosition += movement;
        }
        #endregion

        #region Callbacks
        static void FramebufferSizeCallback(Window window, int width, int height)
        {
            currentWidth = width;
            currentHeight = height;

            GL.Viewport(0, 0, width, height);

            if (currentWidth != 0 && currentHeight != 0)
            {
                aspectRatio = (float)currentWidth / currentHeight;
            }else
            {
                aspectRatio = 1.1f;
            }
        }

        static void KeyboardCallback(Window window, Keys key, int scancode, InputState state, ModifierKeys modifier)
        {
            bool ctrlPressed = (modifier & ModifierKeys.Control) != 0;
            bool shiftPressed = (modifier & ModifierKeys.Shift) != 0;

            if (state == InputState.Press)
            {
                ImGui.GetIO().KeysDown[(int)key] = true;
            }
            else if (state == InputState.Release)
            {
                ImGui.GetIO().KeysDown[(int)key] = false;
            }

            if (ctrlPressed && (Glfw.GetTime() - spamCooldown) > 0.5)
            {
                switch (key)
                {
                    case Keys.S:
                    {
                        SaveHelper.SaveScene(rayTracerShader);
                        spamCooldown = (float)Glfw.GetTime();
                        break;
                    }
                    case Keys.L:
                    {
                        SaveHelper.LoadScene(rayTracerShader);
                        spamCooldown = (float)Glfw.GetTime();
                        break;
                    }
                    case Keys.D:
                    {
                        SaveHelper.ClearScene(rayTracerShader);
                        spamCooldown = (float)Glfw.GetTime();
                        break;
                    }
                    case Keys.A:
                    {
                        isRaytracingActivated = !isRaytracingActivated;
                        spamCooldown = (float)Glfw.GetTime();
                        break;
                    }
                }
            }
            else if (shiftPressed)
            {

            }
            else
            {
                if ((Glfw.GetTime() - spamCooldown) > 0.5)
                {
                    // Has cooldown
                    switch (key)
                    {
                        case Keys.Escape:
                        {
                            // At this point, all program tasks have been completed and the application is ready to gracefully terminate.
                            // Therefore, the following command ensures that the program is closed in an elegant and orderly manner.
                            // https://cdn.discordapp.com/attachments/870272324111323237/1106268518783144088/image.png
                            Glfw.SetWindowShouldClose(window, true);
                            break;
                        }
                        case Keys.V:
                        {
                            spamCooldown = (float)Glfw.GetTime();

                            if (colorTest == 0)
                            {
                                rayTracerShader.AddToSphereList(new Sphere(cameraPosition, 10.0f, new vec3(0.0f), new vec3(1.0f), 1.0f, 0.0f, 0.0f));
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
                else
                {
                    // Doesn't have cooldown
                    switch (key)
                    {
                        // Add stuff if needed
                    }
                }
            }
        }

        static void CharCallback(Window window, uint codepoint)
        {
            ImGui.GetIO().AddInputCharacter((char)codepoint);
        }

        static void ScrollCallback(Window window, double xoffset, double yoffset)
        {
            /*fov -= (float)yoffset;
            if (fov < 1.0) fov = 1.0f;
            if (fov > 45.0) fov = 45.0f;*/

            float cameraSpeed = 1.5f;
            cameraPosition += cameraFront * -(float)yoffset * cameraSpeed;

        }

        static void MouseButtonCallback(Window window, MouseButton button, InputState state, ModifierKeys modifier)
        {
            if (state == InputState.Press)
            {
                ImGui.GetIO().MouseDown[(int)button] = true;
            }
            else if (state == InputState.Release) {
                ImGui.GetIO().MouseDown[(int)button] = false;
            }

            if (button == MouseButton.Right)
            {
                if (state == InputState.Press)
                {
                    Glfw.GetCursorPosition(window, out double x, out double y);
                    lastCursorPosition = new vec2((float)x, (float)y);
                    isRightMouseButtonPressed = true;

                    Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Disabled);
                }
                else if (state == InputState.Release)
                {
                    isRightMouseButtonPressed = false;
                    Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Normal);
                }
            }
            else if (button == MouseButton.Middle)
            {
                if (state == InputState.Press)
                {
                    Glfw.GetCursorPosition(window, out double x, out double y);
                    lastCursorPosition = new vec2((float)x, (float)y);
                    isMiddleMouseButtonPressed = true;
                    Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Disabled);
                }
                else if (state == InputState.Release) 
                {
                    isMiddleMouseButtonPressed= false;
                    Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Normal);
                }
            }else if (button == MouseButton.Left && state == InputState.Press) {
                Glfw.GetCursorPosition(window, out double x, out double y);
                lastCursorPosition = new vec2((float)x, (float)y);

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
                    List<Sphere> removedElementList = rayTracerShader.GetSpheresList();
                    removedElementList.RemoveAt(sphereHitIndex);

                    rayTracerShader.SetSpheresList(removedElementList);
                }

            }
        }

        static void CursorPositionCallback(Window window, double x, double y)
        {
            if (isRightMouseButtonPressed)
            {
                RotateCamera(window);
            }
            else if (isMiddleMouseButtonPressed)
            {
                MoveCamera(window);
            }
        }
        #endregion
    }
}

