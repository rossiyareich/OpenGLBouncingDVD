using OpenGLBouncingDVD.Source.Rendering.Shaders;
using StbiSharp;
using static OpenGLBouncingDVD.Library.GL;

namespace OpenGLBouncingDVD.Source.Rendering.Textures
{
    internal class Texture
    {
        public int ColorChannelsImage { get; set; }
        public string TexturePath { get; set; }
        public Shader shader { private get; set; }

        StbiImage image;
        public uint TexturePtr;
        unsafe uint* texture;

        public unsafe Texture(int colorChannelsImage, string texturePath, Shader shader, uint* texture)
        {
            ColorChannelsImage = colorChannelsImage;
            TexturePath = texturePath;
            this.shader = shader;
            this.texture = texture;
        }

        public unsafe void Load()
        {
            using (var stream = File.OpenRead(TexturePath))
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                //Stbi.SetFlipVerticallyOnLoad(true);
                image = Stbi.LoadFromMemory(memoryStream, ColorChannelsImage);
            }

            TexturePtr = texture[0];

            glActiveTexture(GL_TEXTURE0);
            Use();
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);

            fixed (byte* bytes = image.Data)
            {
                glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, image.Width, image.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, bytes);
                glGenerateMipmap(GL_TEXTURE_2D);
            }

            shader.Use();
            int tex0UniformPtr = glGetUniformLocation(shader.ProgramID, "tex0");
            glUniform1i(tex0UniformPtr, 0);

            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            Unuse();

        }

        public void Use()
        {
            int UniformPtr = glGetUniformLocation(shader.ProgramID, "renderText");
            glUniform1i(UniformPtr, 0);

            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, TexturePtr);
        }

        public void Unuse()
        {
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, 0);
        }
    }
}
