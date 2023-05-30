using GlmNet;

using Lib;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GLFW
{
    class main
    {
        #region Global variables and constants

        static int colorTest = 0;

        // settings
        // --------
        const int WIDTH = 800;
        const int HEIGHT = 600;

        const float mouseSensitivity = 0.03f;
        static float fov = 45.0f;

        // shader settings
        // ---------------
        const int maxBounces = 10;
        const float ambientWeight = 0.0f;
        const int numRaysPerPixel = 10;

        // camera
        // ------
        static vec3 cameraPosition = new vec3(0.0f, 0.0f, -3.0f);
        static vec3 cameraFront = new vec3(0.0f, 0.0f, -1.0f);
        static vec3 cameraUp = new vec3(0.0f, 1.0f, 0.0f);

        static float near = 0.1f;
        static float far = 100.0f;

        static float yaw = -90.0f;
        static float pitch = 0.0f;
        static float lastX = WIDTH * 0.5f, lastY = HEIGHT * 0.5f;

        // paths
        // -----
        static string rootDirectory = Common.GetRootDirectory();
        static Shader rayTracerShader = new Shader(Path.Combine(rootDirectory, "src\\shaders\\vertex.vert"), Path.Combine(rootDirectory, "src\\shaders\\fragment.frag"));

        // other
        // -----
        static bool firstFrameMouse = true;
        static bool shouldHideMouse = true;
        static float deltaTime = 0.0f;
        static float lastFrame = 0.0f;
        static float spamCooldown = 0.0f;

        static int currentWidth, currentHeight;

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

            Glfw.SetFramebufferSizeCallback(window, FramebufferSizeCallback);
            Glfw.SetCursorPositionCallback(window, MouseCallback);
            Glfw.SetScrollCallback(window, ScrollCallback);

            GL.Enable(EnableCap.DepthTest);
            Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Disabled);
            #endregion

            #region Render loop
            int VAO;

            GL.GenVertexArrays(1, out VAO);
            GL.BindVertexArray(VAO);

            while (!Glfw.WindowShouldClose(window))
            {
                float currentTime = (float)Glfw.GetTime();
                deltaTime = currentTime - lastFrame;
                lastFrame = currentTime;

                Glfw.GetFramebufferSize(window, out currentWidth, out currentHeight);

                if (shouldHideMouse)
                {
                    Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Disabled);
                }
                else
                {
                    Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Normal);
                }

                // input
                // -----
                ProcessInput(window);

                // render
                // ------
                vec3 bgColor = shouldHideMouse ? new vec3(0.2f, 0.3f, 0.3f) : new vec3(0.5f, 0.5f, 0.5f);

                GL.ClearColor(bgColor.x, bgColor.y, bgColor.z, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.ActiveTexture(TextureUnit.Texture0);

                // update uniforms
                // ---------------
                UpdateUniforms(rayTracerShader);
                SetShaderMatrices(rayTracerShader);
                UpdateScene(rayTracerShader);
                rayTracerShader.Use();

                // draw
                // ----
                GL.BindVertexArray(VAO);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                Glfw.SwapBuffers(window);
                Glfw.PollEvents();
            }
           
            GL.DeleteVertexArrays(1, new int[] { VAO });
            
            GL.DeleteProgram(rayTracerShader.GetProgramShader());
            Glfw.Terminate();

            return;
            #endregion
        }

        #region Methods
        static void UpdateScene(Shader shader)
        {
            shader.SendSpheresToShader();
        }
        
        static void ProcessInput(Window window)
        {
            float cameraSpeed = 5.0f * deltaTime;
            
            // At this point, all program tasks have been completed and the application is ready to gracefully terminate.
            // Therefore, the following command ensures that the program is closed in an elegant and orderly manner.
            // https://cdn.discordapp.com/attachments/870272324111323237/1106268518783144088/image.png
            if (Glfw.GetKey(window, Keys.Enter) == InputState.Press) Glfw.SetWindowShouldClose(window, true);

            // Key press cooldown
            if ((Glfw.GetTime() - spamCooldown) > 0.2)
            {
                if (Glfw.GetKey(window, Keys.Escape) == InputState.Press)
                {
                    spamCooldown = (float)Glfw.GetTime();
                    shouldHideMouse = !shouldHideMouse;
                }
            }

            if (shouldHideMouse)
            {
                if (Glfw.GetKey(window, Keys.LeftAlt) == InputState.Press)
                {
                    if ((Glfw.GetTime() - spamCooldown) > 0.2)
                    {
                        if (Glfw.GetKey(window, Keys.S) == InputState.Press)
                        {
                            spamCooldown = (float)Glfw.GetTime();
                            SaveHelper.SaveScene(rayTracerShader);
                        }

                        if (Glfw.GetKey(window, Keys.A) == InputState.Press)
                        {
                            spamCooldown = (float)Glfw.GetTime();
                            SaveHelper.LoadScene(rayTracerShader);
                        }

                        if (Glfw.GetKey(window, Keys.D) == InputState.Press)
                        {
                            spamCooldown = (float)Glfw.GetTime();
                            SaveHelper.ClearScene(rayTracerShader);
                        }
                    }
                }
                else
                {
                    if ((Glfw.GetTime() - spamCooldown) > 0.5)
                    {
                        if (Glfw.GetKey(window, Keys.V) == InputState.Press)
                        {
                            spamCooldown = (float)Glfw.GetTime();

                            switch (colorTest)
                            {
                                case 0:
                                    rayTracerShader.AddToSphereList(new Sphere(cameraPosition, 1.0f, new vec3(1.0f, 0.0f, 0.0f), new vec3(1.0f, 0.0f, 0.0f), 0.0f, 0.0f, 0.0f));
                                    Console.WriteLine("red");
                                    colorTest += 1;
                                    break;
                                case 1:
                                    rayTracerShader.AddToSphereList(new Sphere(cameraPosition, 1.0f, new vec3(0.0f, 1.0f, 0.0f), new vec3(0.0f, 1.0f, 0.0f), 1.0f, 0.0f, 0.0f));
                                    Console.WriteLine("green");
                                    colorTest += 1;
                                    break;
                                case 2:
                                    rayTracerShader.AddToSphereList(new Sphere(cameraPosition, 1.0f, new vec3(0.0f, 0.0f, 1.0f), new vec3(0.0f, 0.0f, 1.0f), 0.0f, 0.0f, 0.0f));
                                    Console.WriteLine("blue");
                                    colorTest = 0;
                                    break;
                            }  
                        }
                    }
                }

                if (Glfw.GetKey(window, Keys.LeftControl) == InputState.Press) cameraSpeed *= 2.0f;
                if (Glfw.GetKey(window, Keys.W) == InputState.Press)
                {
                    cameraPosition.x -= cameraSpeed * cameraFront.x;
                    cameraPosition.z -= cameraSpeed * cameraFront.z;
                }
                    
                if (Glfw.GetKey(window, Keys.S) == InputState.Press) cameraPosition += cameraSpeed * cameraFront;
                if (Glfw.GetKey(window, Keys.A) == InputState.Press) cameraPosition -= glm.normalize(glm.cross(cameraFront, cameraUp)) * cameraSpeed;
                if (Glfw.GetKey(window, Keys.D) == InputState.Press) cameraPosition += glm.normalize(glm.cross(cameraFront, cameraUp)) * cameraSpeed;
                if (Glfw.GetKey(window, Keys.Space) == InputState.Press) cameraPosition += cameraSpeed * cameraUp;
                if (Glfw.GetKey(window, Keys.LeftShift) == InputState.Press) cameraPosition -= cameraSpeed * cameraUp;
                if (Glfw.GetKey(window, Keys.R) == InputState.Press) fov = 45.0f;
            }
        }
        static void SetShaderMatrices(Shader shader)
        {
            mat4 model, view, projection;

            model = new mat4(1.0f);
            view = glm.lookAt(cameraPosition, cameraPosition + cameraFront, cameraUp);
            projection = glm.perspective(glm.radians(fov), currentWidth / currentHeight, near, far);

            shader.SetMat4("modelMatrix", model);
            shader.SetMat4("viewMatrix", view);
            shader.SetMat4("projectionMatrix", projection);

            shader.SetMat4("viewMatrixInverse", glm.inverse(view));
            shader.SetMat4("projectionMatrixInverse", glm.inverse(projection));
        }
        static void UpdateUniforms(Shader shader)
        {
            // generic
            // -------
            shader.SetFloat("time", (float)Glfw.GetTime());
            shader.SetVec2("resolution", new vec2(currentWidth, currentHeight));
            shader.SetVec3("cameraPosition", cameraPosition);

            // shader settings
            // ---------------
            shader.SetInt("maxBounces", maxBounces);
            shader.SetInt("numRaysPerPixel", numRaysPerPixel);
            shader.SetFloat("ambientWeight", ambientWeight);
        }
        #endregion

        #region Callbacks
        static void FramebufferSizeCallback(Window window, int width, int height)
        {
            GL.Viewport(0, 0, width, height);
        }
        static void MouseCallback(Window window, double xpos, double ypos)
        {
            if (firstFrameMouse)
            {
                lastX = (float)xpos;
                lastY = (float)ypos;
                firstFrameMouse = false;
            }

            if (shouldHideMouse)
            {
                float realMouseSensitivity = mouseSensitivity * (fov / 45);

                float xoffset = (float)xpos - lastX;
                float yoffset = lastY - (float)ypos;
                lastX = (float)xpos;
                lastY = (float)ypos;

                xoffset *= realMouseSensitivity;
                yoffset *= realMouseSensitivity;

                yaw += xoffset;
                pitch -= yoffset;

                if (pitch > 89.9) pitch = 89.9f;
                if (pitch < -89.9) pitch = -89.9f;

                vec3 direction;
                direction.x = glm.cos(glm.radians(yaw)) * glm.cos(glm.radians(pitch));
                direction.y = glm.sin(glm.radians(pitch));
                direction.z = glm.sin(glm.radians(-yaw)) * glm.cos(glm.radians(pitch));
                cameraFront = glm.normalize(direction);
            }
        }
        static void ScrollCallback(Window window, double xoffset, double yoffset)
        {
            fov -= (float)yoffset;
            if (fov < 1.0) fov = 1.0f;
            if (fov > 45.0) fov = 45.0f;
        }
        #endregion
    }
}

