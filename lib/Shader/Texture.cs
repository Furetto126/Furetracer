using StbImageSharp;
using OpenTK.Graphics.OpenGL4;

using System.Runtime.InteropServices;

namespace Lib
{
    class Texture
    {
        IntPtr ptr;
        byte[] data;
        ImageResult imageResult;

        public Texture(string imagePath)
        {
            FileStream? fileStream = null;
            try
            {
                fileStream = File.OpenRead(imagePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                imageResult = ImageResult.FromStream(fileStream);

                data = imageResult.Data;
                ptr = Marshal.AllocHGlobal(data.Length);
                Marshal.Copy(data, 0, ptr, data.Length);
            }
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(ptr);
        }

        public IntPtr texturePtr
        {
            get { return ptr; }
        }

        public ImageResult image
        {
            get { return imageResult; }
        }

        public void SetAsShaderTexture(int textureIndex, string name, Shader shader)
        {
            int textureID;

            GL.GenTextures(textureIndex, out textureID);

            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, new int[] { (int)TextureWrapMode.Repeat });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, new int[] { (int)TextureWrapMode.Repeat });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.Linear });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Linear });

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, texturePtr);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            shader.Use();
            shader.SetInt(name, 0);

            GL.BindTexture(TextureTarget.Texture2D, textureID);
        }
    }
}
