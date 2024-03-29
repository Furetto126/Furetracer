﻿using ImGuiNET;
using GlmNet;
using System.Numerics;
using Assimp.Unmanaged;

namespace Lib
{
    class ImGuiHandler
    {
        private static string inspectoredElement = "";
        private static string commandSent = "";
        private static string renamed = "";
        private static string saveTarget = "";
        private static bool isRendering = false;
        private static int renderTime = 5;

        public static void ConstructImGui(RaytracingScene scene)
        {
            Vector2 windowSize = ImGui.GetIO().DisplaySize;

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(7.0f, 7.0f));

            // Right panel
            // -----------
            Vector2 rightPanelSize = new Vector2(400.0f, windowSize.Y / 2.0f);
            Vector2 rightPanelPosition = new Vector2(windowSize.X - rightPanelSize.X, 0.0f);

            ImGui.SetNextWindowPos(rightPanelPosition);
            ImGui.SetNextWindowSize(rightPanelSize);
            ImGuiWindowFlags rightPanelFlags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize;

            ImGui.Begin("Right Panel", rightPanelFlags);
            main.isInspectorWindowHovered = ImGui.IsWindowHovered();

            if (!inspectoredElement.Equals("")) // An object is selected
            {
                Object selectedObject = scene.GetObjectByName(inspectoredElement);

                if (selectedObject is Sphere)
                {
                    Sphere currentSphere = (Sphere)selectedObject;

                    Vector3 position = new Vector3(currentSphere.position.x, currentSphere.position.y, currentSphere.position.z);
                    Vector3 color = new Vector3(currentSphere.material.color.x, currentSphere.material.color.y, currentSphere.material.color.z);
                    Vector3 emissionColor = new Vector3(currentSphere.material.emissionColor.x, currentSphere.material.emissionColor.y, currentSphere.material.emissionColor.z);

                    // Set ImGui elements for the inspector window
                    // -------------------------------------------
                    ImFontPtr previousFont = ImGui.GetFont();
                    ImGui.SetWindowFontScale(20.0f / previousFont.FontSize);

                    ImGui.Text(inspectoredElement);
                    ImGui.SameLine();
                    ImGui.SetWindowFontScale(1.0f);
                    ImGui.Dummy(new Vector2(1.0f, 10.0f));

                    ImGui.InputFloat3("Position", ref position);
                    ImGui.InputFloat("Radius", ref currentSphere.radius);
                    ImGui.ColorEdit3("Color", ref color);
                    ImGui.ColorEdit3("Emission Color", ref emissionColor);
                    ImGui.InputFloat("Emission strength", ref currentSphere.material.emissionStrength);
                    ImGui.InputFloat("Smoothness", ref currentSphere.material.smoothness);
                    ImGui.InputFloat("Glossiness", ref currentSphere.material.glossiness);

                    // Set vector attributes
                    currentSphere.position = new vec3(position.X, position.Y, position.Z);
                    currentSphere.material.color = new vec3(color.X, color.Y, color.Z);
                    currentSphere.material.emissionColor = new vec3(emissionColor.X, emissionColor.Y, emissionColor.Z);
                }
                else if (selectedObject is Model)
                {
                    Model currentModel = (Model)selectedObject;

                    Vector3 position = new Vector3(currentModel.position.x, currentModel.position.y, currentModel.position.z);
                    float size = currentModel.size;

                    // Set ImGui elements for the inspector window
                    // -------------------------------------------
                    ImFontPtr previousFont = ImGui.GetFont();
                    ImGui.SetWindowFontScale(20.0f / previousFont.FontSize);

                    ImGui.Text(inspectoredElement);
                    ImGui.SameLine();
                    ImGui.SetWindowFontScale(1.0f);
                    ImGui.Dummy(new Vector2(1.0f, 10.0f));

                    ImGui.InputFloat3("Position", ref position);
                    ImGui.InputFloat("Size", ref size);

                    currentModel.SetPosition(new vec3(position.X, position.Y, position.Z));
                    currentModel.SetSize(size);

                    if (ImGui.Button("debug vertices"))
                    {
                        foreach (Triangle triangle in currentModel.triangles)
                        {
                            Console.WriteLine(triangle.v0 + ", " + triangle.v1 + ", " + triangle.v2);
                        }
                    }
                }
            }

            ImGui.End();


            // Engine settings panel
            // ---------------------
            Vector2 settingsPanelSize = new Vector2(rightPanelSize.X, windowSize.Y / 2.0f);
            Vector2 settingsPanelPosition = new Vector2(rightPanelPosition.X, windowSize.Y / 2.0f);

            ImGui.SetNextWindowPos(settingsPanelPosition);
            ImGui.SetNextWindowSize(settingsPanelSize);
            ImGuiWindowFlags settingsPanelFlag = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize;

            ImGui.Begin("Settings Panel", settingsPanelFlag);
            main.isSettingsWindowHovered = ImGui.IsWindowHovered();

            ImGui.InputInt("Max ray bounces", ref main.maxBounces);
            ImGui.InputInt("Number of ray per pixel", ref main.numRaysPerPixel);
            ImGui.InputFloat("Ambient weight", ref main.ambientWeight);
            ImGui.InputFloat("FOV", ref main.fov);

            ImGui.Checkbox("Activate raytracer", ref main.isRaytracingActivated);
            ImGui.Checkbox("Progressive rendering", ref main.progressiveRenderingActivated);

            if (ImGui.Button("sus"))
            {
                scene.AddObjectInScene(ModelLoader.LoadModel(Path.Combine(Common.GetRootDirectory(), "models\\cube.obj")));
            }

            //ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX(), settingsPanelSize.Y - 80.0f));
            //ImGui.InputInt("Render time (s)", ref renderTime);

            /*if (ImGui.Button("Render!") && !isRendering)
            {
                main.isRaytracingActivated = true;
                main.progressiveRenderingActivated = true;
                isRendering = true;

                Console.WriteLine("started render for " + renderTime + " seconds");
                Console.WriteLine("start timer");
                Console.WriteLine(isRendering);

                Thread renderThread = new Thread(RenderImage);
                renderThread.Start();
                renderThread.Join();
            }
            else if (ImGui.Button("Stop rendering") && isRendering)
            {
                isRendering = false;
                main.isRaytracingActivated = false;
                main.progressiveRenderingActivated = false;

                Console.WriteLine(isRendering);
            }*/

            ImGui.End();


            // Bottom panel
            // ------------
            Vector2 bottomPanelSize = new Vector2(windowSize.X - rightPanelSize.X, 300.0f);
            Vector2 bottomPanelPosition = new Vector2(0.0f, windowSize.Y - bottomPanelSize.Y);

            ImGui.SetNextWindowSize(bottomPanelSize);
            ImGui.SetNextWindowPos(bottomPanelPosition);
            ImGuiWindowFlags bottomPanelFlags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize;

            ImGui.Begin("Bottom Panel", bottomPanelFlags);
            main.isConsoleWindowHovered = ImGui.IsWindowHovered();

            ImGui.BeginChild("Console output", new Vector2(bottomPanelSize.X, bottomPanelSize.Y - 42.0f), true, ImGuiWindowFlags.AlwaysVerticalScrollbar);

            ImGui.Text(main.consoleOutput.ToString());
            ImGui.SetScrollHereY(1.0f);

            ImGui.EndChild();

            if (ImGui.InputText("Command line", ref commandSent, 100, ImGuiInputTextFlags.EnterReturnsTrue) && !commandSent.Replace(" ", "").Equals(""))
            {
                CommandParser parser = new CommandParser();
                parser.ParseAndExecute(commandSent, scene);

                commandSent = "";
                ImGui.SetKeyboardFocusHere(-1);
            }

            ImGui.End();


            // Left Panel
            // ----------
            Vector2 leftPanelSize = new Vector2(300.0f, windowSize.Y - bottomPanelSize.Y);
            Vector2 leftPanelPosition = new Vector2(0.0f);

            ImGui.SetNextWindowSize(leftPanelSize);
            ImGui.SetNextWindowPos(leftPanelPosition);
            ImGuiWindowFlags leftPanelFlags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize;

            ImGui.Begin("Left Panel", leftPanelFlags);
            main.isSceneWindowHovered = ImGui.IsWindowHovered();

            foreach (Object obj in scene.GetObjectsInSceneList())
            {
                string objName = obj.DisplayName;

                if (ImGui.Selectable(objName))
                {
                    inspectoredElement = objName;
                }

                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup("Edit object " + objName);
                }

                if (ImGui.BeginPopup("Edit object " + objName))
                {
                    if (ImGui.MenuItem("Delete"))
                    {
                        inspectoredElement = "";
                        scene.RemoveObjectInScene(obj);
                        ImGui.CloseCurrentPopup();
                    }

                    /*if (ImGui.MenuItem("Rename"))
                    {
                        if (ImGui.InputText("Rename", ref renamed, 100, ImGuiInputTextFlags.EnterReturnsTrue) && !commandSent.Replace(" ", "").Equals(""))
                        {
                            obj.DisplayName = renamed;
                            renamed = "";
                            ImGui.CloseCurrentPopup();
                        }
                    }*/

                    if (ImGui.MenuItem("Duplicate"))
                    {
                        if (obj is Sphere)
                        {
                            Sphere duplicatedObject = new Sphere((Sphere)obj);
                            duplicatedObject.DisplayName = Common.GiveDefaultFreeName(scene, objName);
                            scene.AddObjectInScene(duplicatedObject);
                        }
                        else if (obj is Model)
                        {
                            Model duplicatedObject = new Model((Model)obj);
                            duplicatedObject.DisplayName = Common.GiveDefaultFreeName(scene, objName);
                            scene.AddObjectInScene(duplicatedObject);
                        }
                        
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.EndPopup();
                }
            }

            if (main.scenePopupOpen)
            {
                ImGui.OpenPopup("Scene popup");
            }
            else
            {
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.BeginPopup("Scene popup"))
            {
                if (ImGui.MenuItem("New Sphere"))
                {
                    int suffix = 1;

                    while (scene.ExistsInScene("Sphere" + suffix))
                    {
                        suffix++;
                    }

                    scene.AddObjectInScene(new Sphere("Sphere" + suffix));

                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }

            ImGui.End();
            ImGui.PopStyleVar();
        }

        /*private static void RenderImage()
        {
            // wait and render
            Console.WriteLine("start timer");
            
            while (isRendering)
            {
                
            }

            Console.WriteLine("end timer");

            GL.GetInteger(GetPName.TextureBinding2D, out int backupTexture);
            GL.GetInteger(GetPName.FramebufferBinding, out int backupFramebuffer);

            GL.GenFramebuffers(1, out int outputFramebuffer);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, outputFramebuffer);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, main.currentFrameTexture, 0);

            GL.ActiveTexture(TextureUnit.Texture0);

            byte[] pixelData = new byte[main.currentWidth * main.currentHeight * 3];
            GL.GetTextureImage(main.currentFrameTexture, 0, PixelFormat.Rgb, PixelType.UnsignedByte, pixelData.Length, pixelData);

            using (Image<Rgb24> image = Image.LoadPixelData<Rgb24>(pixelData, main.currentWidth, main.currentHeight))
            {
                if (!Directory.Exists(Path.Combine(Common.GetRootDirectory(), "renders")))
                {
                    Console.WriteLine("directory did not exist, creating it!");
                    Directory.CreateDirectory(Path.Combine(Common.GetRootDirectory(), "renders"));
                }

                Console.WriteLine("rendered image!");
                image.Save(Path.Combine(Common.GetRootDirectory(), "renders\\render1.png"));
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, backupFramebuffer);
            GL.BindTexture(TextureTarget.Texture2D, backupTexture);
        }*/
    }
}
