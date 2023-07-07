using GlmNet;
using CsvHelper;
using System.Globalization;
using RayTracer.lib.Generic;

namespace Lib
{
    class SaveHelper
    {
        static string directoryPath = Path.Combine(Common.GetRootDirectory(), "saves");
        static string savePath = Path.Combine(directoryPath, "save1.emn");
        
        public static void SaveScene(Shader shader)
        {
            List<Sphere> sphereList = shader.GetSpheresList();

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using StreamWriter writer = new(savePath);
            using CsvWriter csv = new(writer, CultureInfo.InvariantCulture);

            // Write header
            csv.WriteField("Type");
            csv.WriteField("Name");
            csv.WriteField("PositionX");
            csv.WriteField("PositionY");
            csv.WriteField("PositionZ");
            csv.WriteField("Radius");
            csv.WriteField("ColorR");
            csv.WriteField("ColorG");
            csv.WriteField("ColorB");
            csv.WriteField("EmissionColorR");
            csv.WriteField("EmissionColorG");
            csv.WriteField("EmissionColorB");
            csv.WriteField("EmissionStrength");
            csv.WriteField("Smoothness");
            csv.WriteField("Glossiness");
            csv.NextRecord();

            // Write SpheresData
            for (int i = 0; i < sphereList.Count; i++)
            {
                Sphere sphere = sphereList[i];

                csv.WriteField("Sphere");
                csv.WriteField("Sphere" + i);

                csv.WriteField(sphere.position.x);
                csv.WriteField(sphere.position.y);
                csv.WriteField(sphere.position.z);
                csv.WriteField(sphere.radius);

                csv.WriteField(sphere.color.x);
                csv.WriteField(sphere.color.y);
                csv.WriteField(sphere.color.z);

                csv.WriteField(sphere.emissionColor.x);
                csv.WriteField(sphere.emissionColor.y);
                csv.WriteField(sphere.emissionColor.z);

                csv.WriteField(sphere.emissionStrength);
                csv.WriteField(sphere.smoothness);
                csv.WriteField(sphere.glossiness);

                csv.NextRecord();
            }
            Console.WriteLine("SAVED with length " + sphereList.Count);
        }

        public static void LoadScene(Shader shader)
        {
            if (!File.Exists(savePath))
            {
                Console.WriteLine("Could not find the save file to load from!");
                return;
            }

            using StreamReader reader = new(savePath);
            using CsvReader csv = new(reader, CultureInfo.InvariantCulture);

            csv.Read();
            csv.ReadHeader();

            shader.GetSpheresList().Clear();

            while (csv.Read())
            {
                switch (csv.GetField<string>("Type"))
                {
                    case "Sphere":
                        {
                            Sphere sphere = new(
                                    new vec3(csv.GetField<float>("PositionX"), csv.GetField<float>("PositionY"), csv.GetField<float>("PositionZ")),
                                    csv.GetField<float>("Radius"),
                                    new vec3(csv.GetField<float>("ColorR"), csv.GetField<float>("ColorG"), csv.GetField<float>("ColorB")),
                                    new vec3(csv.GetField<float>("EmissionColorR"), csv.GetField<float>("EmissionColorG"), csv.GetField<float>("EmissionColorB")),
                                    csv.GetField<float>("EmissionStrength"),
                                    csv.GetField<float>("Smoothness"),
                                    csv.GetField<float>("Glossiness")
                                );
                            shader.AddToSphereList(sphere);
                            Console.WriteLine("Loaded " + csv.GetField<string>("Name"));
                            
                            break;
                        }    
                    // aggiungi qualcos altro bho emoji35
                }
            }
        }

        public static void ClearScene(Shader shader)
        {
            Console.WriteLine("Cleared Scene");

            shader.SetSpheresList(new List<Sphere>());
        }
    }
}
