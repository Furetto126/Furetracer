using ImGuiNET;
using GlmNet;
using System.Numerics;

namespace Lib
{
    class ImGuiHandler
    {
        private static int inspectoredElement = int.MinValue;

        public static void ConstructImGui(Shader shader)
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

            if (inspectoredElement != int.MinValue) // A sphere is selected
            {
                Sphere currentSphere = shader.GetSpheresList().ToArray()[inspectoredElement];

                Vector3 position = new Vector3(currentSphere.position.x, currentSphere.position.y, currentSphere.position.z);
                Vector3 color = new Vector3(currentSphere.color.x, currentSphere.color.y, currentSphere.color.z);
                Vector3 emissionColor = new Vector3(currentSphere.emissionColor.x, currentSphere.emissionColor.y, currentSphere.emissionColor.z);

                // Set ImGui elements for the inspector window
                // -------------------------------------------
                ImFontPtr previousFont = ImGui.GetFont();
                ImGui.SetWindowFontScale(20.0f / previousFont.FontSize);

                ImGui.Text("Sphere" + inspectoredElement);

                ImGui.Dummy(new Vector2(1.0f, 10.0f));
                ImGui.SetWindowFontScale(1.0f);

                ImGui.InputFloat3("Position", ref position);
                ImGui.InputFloat("Radius", ref currentSphere.radius);
                ImGui.ColorEdit3("Color", ref color);
                ImGui.ColorEdit3("Emission Color", ref emissionColor);
                ImGui.InputFloat("Emission strength", ref currentSphere.emissionStrength);
                ImGui.InputFloat("Smoothness", ref currentSphere.smoothness);
                ImGui.InputFloat("Glossiness", ref currentSphere.glossiness);

                // Set vector attributes
                currentSphere.position = new vec3(position.X, position.Y, position.Z);
                currentSphere.color = new vec3(color.X, color.Y, color.Z);
                currentSphere.emissionColor = new vec3(emissionColor.X, emissionColor.Y, emissionColor.Z);
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

            ImGui.InputInt("Max ray bounces", ref main.maxBounces);
            ImGui.InputInt("Number of ray per pixel", ref main.numRaysPerPixel);
            ImGui.InputFloat("Ambient weight", ref main.ambientWeight);
            ImGui.InputFloat("FOV", ref main.fov);

            ImGui.Checkbox("Activate raytracer", ref main.isRaytracingActivated);
            ImGui.Checkbox("Progressive rendering", ref main.progressiveRenderingActivated);


            ImGui.End();


            // Bottom panel
            // ------------
            Vector2 bottomPanelSize = new Vector2(windowSize.X - rightPanelSize.X, 300.0f);
            Vector2 bottomPanelPosition = new Vector2(0.0f, windowSize.Y - bottomPanelSize.Y);

            ImGui.SetNextWindowSize(bottomPanelSize);
            ImGui.SetNextWindowPos(bottomPanelPosition);
            ImGuiWindowFlags bottomPanelFlags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize;

            ImGui.Begin("Bottom Panel", bottomPanelFlags);

            //string e = "";
            ImGui.Text(main.consoleOutput.ToString());
            //ImGui.InputText("sus", ref e, 100);

            ImGui.End();


            // Left Panel
            // ----------
            Vector2 leftPanelSize = new Vector2(300.0f, windowSize.Y - bottomPanelSize.Y);
            Vector2 leftPanelPosition = new Vector2(0.0f);

            ImGui.SetNextWindowSize(leftPanelSize);
            ImGui.SetNextWindowPos(leftPanelPosition);
            ImGuiWindowFlags leftPanelFlags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize;

            ImGui.Begin("Left Panel", leftPanelFlags);            

            ImGui.Text("left Panel Text");

            for (int i = 0; i < shader.GetSpheresList().Count; i++)
            {
                if (ImGui.Button("Sphere" + i))
                {
                    inspectoredElement = i;       
                }
            }

            ImGui.End();

            ImGui.PopStyleVar();
        }
    }
}
