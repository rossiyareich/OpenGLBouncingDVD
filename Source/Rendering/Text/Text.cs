using System.Drawing;
using System.Drawing.Imaging;
using OpenGLBouncingDVD.Source.Rendering.Display;
using OpenGLBouncingDVD.Source.Rendering.Shaders;
using StbiSharp;
using static OpenGLBouncingDVD.Library.GL;

namespace OpenGLBouncingDVD.Source.Rendering.Text
{
    internal class Text
    {
        public Shader shader { private get; set; }

        StbiImage image;
        public uint TexturePtr;

        unsafe uint* texture;

        public unsafe Text(Shader shader, uint* texture)
        {
            this.shader = shader;
            this.texture = texture;
        }

        public unsafe void Load()
        {
            TexturePtr = texture[1];

            glActiveTexture(GL_TEXTURE1);
            Use();
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);

            using (image)
            {
                fixed (byte* bytes = image.Data)
                {
                    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, image.Width, image.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, bytes);
                    glGenerateMipmap(GL_TEXTURE_2D);
                }
            }

            shader.Use();
            int tex1UniformPtr = glGetUniformLocation(shader.ProgramID, "tex1");
            glUniform1i(tex1UniformPtr, 1);

            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            Unuse();
        }

        public void Use()
        {
            int UniformPtr = glGetUniformLocation(shader.ProgramID, "renderText");
            glUniform1i(UniformPtr, 1);
            glActiveTexture(GL_TEXTURE1);
            glBindTexture(GL_TEXTURE_2D, TexturePtr);
        }

        public void Unuse()
        {
            glActiveTexture(GL_TEXTURE1);
            glBindTexture(GL_TEXTURE_2D, 0);
            glDeleteTexture(TexturePtr);
        }


#pragma warning disable CA1416 // Validate platform compatibility
        public void UpdateText(string text)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var bmp = ConvertTextToImage(text, "Arial", 80, FontStyle.Regular, Color.White, (int)DisplayManager.WindowSize.X, (int)DisplayManager.WindowSize.Y, 0, 0))
                {
                    bmp.Save(memoryStream, ImageFormat.Png);
                }
                image = Stbi.LoadFromMemory(memoryStream, 4);
            }
        }

        private Bitmap ConvertTextToImage(string txt, string fontname, int fontsize, FontStyle fontstyle, Color tcolor, int width, int Height, int x, int y)
        {
            Bitmap bmp = new Bitmap(width, Height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                Font font = new Font(fontname, fontsize, fontstyle);
                graphics.DrawString(txt, font, new SolidBrush(tcolor), x, y);
                graphics.Flush();
                font.Dispose();
                graphics.Dispose();
            }
            return bmp;
        }
#pragma warning restore CA1416 // Validate platform compatibility
    }
}
