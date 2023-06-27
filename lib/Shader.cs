using GlmNet;

using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace Lib
{
    class Shader
    {
        int ID;
        List<Sphere> spheresList = new();

        public struct Material
        {
            public vec3 color;              //needs 16 bytes of alignment
            public float smoothness;

            public vec3 emissionColor;
            public float emissionStrength;

            private readonly vec3 _pad0;    //padding because otherwise OpenGL will cry
            public float glossiness;
        }

        public struct SphereData
        {
            public vec3 position;           //needs 16 bytes of alignment
            public float radius;

            public Material material;
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

        #region Spheres
        public void AddToSphereList(Sphere sphere)

        {
            spheresList.Add(sphere);
            SendSpheresToShader();
        }

        public void RemoveFromSphereList(int index)
        {
            List<Sphere> removedElementList = GetSpheresList();
            removedElementList.RemoveAt(index);

            SetSpheresList(removedElementList);
        }
      
        public List<Sphere> GetSpheresList()
        {
            return spheresList;
        }

        public void SetSpheresList(List<Sphere> spheres)
        {
            spheresList = spheres;
        }

        public void SendSpheresToShader()
        {
            SetInt("numSpheres", spheresList.ToArray().Length);

            SphereData[] sphereDataArray = spheresList.Select(s => new SphereData
            {
                position = s.position,
                radius = s.radius,
                material = new Material
                {
                    color = s.color,
                    smoothness = s.smoothness,

                    emissionColor = s.emissionColor,
                    emissionStrength = s.emissionStrength,
                    
                    glossiness = s.glossiness
                }
            }).ToArray();

            int bindingPoint = 0;
            int bufferSize = Marshal.SizeOf(typeof(SphereData)) * sphereDataArray.Length;
            GL.GenBuffers(1, out int sphereBuffer);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingPoint, sphereBuffer);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, bufferSize, sphereDataArray, BufferUsageHint.DynamicDraw);
        }
        #endregion

        public int GetProgramShader()
        {
            return ID;
        }
    }
}
