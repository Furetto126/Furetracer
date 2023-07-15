using GlmNet;

using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace Lib {

    class Shader
    {
        List<Sphere> previousSpheresList = new();
        List<Model> previousModelsList = new();

        int ID;

        public struct MaterialData
        {
            public vec3 color;              //needs 16 bytes of alignment
            public float smoothness;

            public vec3 emissionColor;
            public float emissionStrength;

            private readonly vec3 pad0;    //padding because otherwise OpenGL will cry
            public float glossiness;
        }

        public struct SphereData
        {
            public vec3 position;           //needs 16 bytes of alignment
            public float radius;

            public MaterialData material;
        }

        public struct TriangleData
        {
            public vec3 v0;
            private readonly float pad0;

            public vec3 v1;
            private readonly float pad1;   // Lots of padding because i don't even want to risk getting other errors and bugs

            public vec3 v2;
            private readonly float pad2;

            public MaterialData material;
        }

        public Shader(string vertexPath, string fragmentPath)
        {
            string vertexCode;
            string fragmentCode;

            try
            {
                vertexCode = File.ReadAllText(vertexPath);
                fragmentCode = File.ReadAllText(fragmentPath);

                int vertex, fragment;

                unsafe
                {
                    vertex = GL.CreateShader(ShaderType.VertexShader);
                    GL.ShaderSource(vertex, vertexCode);
                    GL.CompileShader(vertex);
                    CheckCompileErrors(vertex, "vertex");

                    fragment = GL.CreateShader(ShaderType.FragmentShader);
                    GL.ShaderSource(fragment, fragmentCode);
                    GL.CompileShader(fragment);
                    CheckCompileErrors(fragment, "fragment");

                    ID = GL.CreateProgram();
                    GL.AttachShader(ID, vertex);
                    GL.AttachShader(ID, fragment);
                    GL.LinkProgram(ID);
                    CheckCompileErrors(ID, "program");

                    GL.DeleteShader(vertex);
                    GL.DeleteShader(fragment);

                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void CheckCompileErrors(int shader, string type)
        {
            int success;
            if (type != "program")
            {
                GL.GetShader(shader, ShaderParameter.CompileStatus, out success);
                if (success == 0)
                {
                    string log = GL.GetShaderInfoLog(shader);
                    Console.WriteLine(type + " compilation failed:\n " + log);
                }
            }
            else
            {
                GL.GetProgram(shader, GetProgramParameterName.LinkStatus, out success);
                if (success == 0)
                {
                    string log = GL.GetProgramInfoLog(shader);
                    Console.WriteLine(type + " linking failed:\n " + log);
                }
            }
        }

        #region set uniforms
        public void Use()
        {
            GL.UseProgram(ID);
        }

        public void SetBool(string name, bool value)
        {
            GL.Uniform1(GL.GetUniformLocation(ID, name), Convert.ToInt32(value));
        }

        public void SetInt(string name, int value)
        {
            GL.Uniform1(GL.GetUniformLocation(ID, name), value);
        }

        public void SetFloat(string name, float value)
        {
            GL.Uniform1(GL.GetUniformLocation(ID, name), value);
        }

        public void SetVec2(string name, vec2 value)
        {
            GL.Uniform2(GL.GetUniformLocation(ID, name), value.x, value.y);
        }

        public void SetVec3(string name, vec3 value)
        {
            GL.Uniform3(GL.GetUniformLocation(ID, name), value.x, value.y, value.z);
        }

        public void SetMat4(string name, mat4 value)
        {
            IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(value.to_array(), 0);

            unsafe
            {
                GL.UniformMatrix4(GL.GetUniformLocation(ID, name), 1, false, (float*)ptr.ToPointer());
            }
        }
        #endregion

        public void SendSpheresToShader(RaytracingScene scene)
        {
            List<Sphere> spheresList = scene.GetSpheresInScene();

            if (spheresList != previousSpheresList)
            {
                previousSpheresList = spheresList;
                SetInt("numSpheres", spheresList.Count());

                SphereData[] sphereDataArray = spheresList.Select(s => new SphereData
                {
                    position = s.position,
                    radius = s.radius,
                    material = new MaterialData
                    {
                        color = s.material.color,
                        smoothness = s.material.smoothness,

                        emissionColor = s.material.emissionColor,
                        emissionStrength = s.material.emissionStrength,

                        glossiness = s.material.glossiness
                    }
                }).ToArray();

                int bindingPoint = 0;
                int bufferSize = Marshal.SizeOf(typeof(SphereData)) * sphereDataArray.Length;
                GL.GenBuffers(1, out int sphereBuffer);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingPoint, sphereBuffer);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, bufferSize, sphereDataArray, BufferUsageHint.DynamicDraw);
            }  
        }

        public void SendTrianglesToShader(RaytracingScene scene)
        {
            List<Model> modelsList = scene.GetModelsInScene();

            if (modelsList != previousModelsList)
            {
                previousModelsList = modelsList;
                List<TriangleData> triangleDataList = new List<TriangleData>();

                foreach (Model model in modelsList)
                {
                    foreach (Triangle triangle in model.triangles)
                    {
                        triangleDataList.Add(new TriangleData
                        {
                            v0 = triangle.v0,
                            v1 = triangle.v1,
                            v2 = triangle.v2,

                            material = new MaterialData
                            {
                                color = triangle.material.color,
                                emissionColor = triangle.material.emissionColor,
                                emissionStrength = triangle.material.emissionStrength,
                                smoothness = triangle.material.smoothness,
                                glossiness = triangle.material.glossiness
                            }
                        });
                    }
                }

                SetInt("numTriangles", triangleDataList.Count());

                TriangleData[] triangleDataArray = triangleDataList.ToArray();

                int bindingPoint = 1;
                int bufferSize = Marshal.SizeOf(typeof(TriangleData)) * triangleDataArray.Length;
                GL.GenBuffers(1, out int triangleBuffer);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingPoint, triangleBuffer);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, bufferSize, triangleDataArray, BufferUsageHint.DynamicDraw);
            }
        }

        public int GetProgramShader()
        {
            return ID;
        }
    }
}
