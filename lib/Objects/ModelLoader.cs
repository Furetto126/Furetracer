using Assimp;
using Assimp.Configs;

using GlmNet;

namespace Lib
{
    class ModelLoader
    {
        private static List<Triangle> trianglesList = new List<Triangle>(); 

        public static void LoadModel(string path) {

            if (!File.Exists(path)) { Console.WriteLine("Model \"" + path + "\" not found!");  return;  }

            AssimpContext importer = new AssimpContext();
            importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));

            Scene modelScene = importer.ImportFile(path, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessSteps.Triangulate);

            foreach (Mesh mesh in modelScene.Meshes)
            {
                foreach (Face face in mesh.Faces)
                {
                    Vector3D[] vertices = new Vector3D[3];
                    for (int i = 0; i < 3; i++)
                    {
                        Vector3D vertex = mesh.Vertices[face.Indices[i]];
                        vertices[i] = new Vector3D(vertex.X, vertex.Y, vertex.Z);
                    }

                    int materialIndex = mesh.MaterialIndex;
                    Material material = modelScene.Materials[materialIndex];

                    Color4D diffuseColor = material.ColorDiffuse;

                    Color4D emissionColor = material.ColorEmissive;
                    float emissionStrength = emissionColor.A;

                    float smoothness = material.Shininess;
                    float glossiness = material.ShininessStrength;

                    Triangle triangle = new Triangle(
                        new vec3(vertices[0].X, vertices[0].Y, vertices[0].Z),
                        new vec3(vertices[1].X, vertices[1].Y, vertices[1].Z),
                        new vec3(vertices[2].X, vertices[2].Y, vertices[2].Z),
                        new vec3(diffuseColor.R, diffuseColor.G, diffuseColor.B),
                        new vec3(emissionColor.R, emissionColor.G, emissionColor.B),
                        emissionStrength,
                        smoothness,
                        glossiness
                    );

                    trianglesList.Add(triangle);
                }
            }

            importer.Dispose();

            
        }

        public static List<Triangle> GetSceneTriangles()
        {
            return trianglesList;
        }
    }
}
