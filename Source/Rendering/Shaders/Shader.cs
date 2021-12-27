using System.Numerics;
using OpenGLBouncingDVD.Source.Rendering.Cameras;
using static OpenGLBouncingDVD.Library.GL;

namespace OpenGLBouncingDVD.Source.Rendering.Shaders
{
    internal class Shader
    {
        string vertexCode;
        string fragmentCode;

        public uint ProgramID { get; set; }

        public Shader(string vertexCode, string fragmentCode)
        {
            this.vertexCode = vertexCode;
            this.fragmentCode = fragmentCode;
        }

        public void Load()
        {
            //Compile shaders seperately
            uint vertexShaderPtr = glCreateShader(GL_VERTEX_SHADER);
            glShaderSource(vertexShaderPtr, vertexCode);
            glCompileShader(vertexShaderPtr);
            uint fragmentShaderPtr = glCreateShader(GL_FRAGMENT_SHADER);
            glShaderSource(fragmentShaderPtr, fragmentCode);
            glCompileShader(fragmentShaderPtr);

            if (glGetShaderiv(vertexShaderPtr, GL_COMPILE_STATUS, 1)[0] == 0)
            {
                //Failed to compile
                throw new($"Error compiling vertex shader: {glGetShaderInfoLog(vertexShaderPtr)}");
            }

            if (glGetShaderiv(fragmentShaderPtr, GL_COMPILE_STATUS, 1)[0] == 0)
            {
                //Failed to compile
                throw new($"Error compiling vertex shader: {glGetShaderInfoLog(fragmentShaderPtr)}");
            }

            //Link shaders
            ProgramID = glCreateProgram();         //Create a shader program object
            glAttachShader(ProgramID, vertexShaderPtr);
            glAttachShader(ProgramID, fragmentShaderPtr);
            glLinkProgram(ProgramID);              //Link the attached shaders

            //Delete unused unlinked shaders
            glDetachShader(ProgramID, vertexShaderPtr);
            glDetachShader(ProgramID, fragmentShaderPtr);
            glDeleteShader(vertexShaderPtr);
            glDeleteShader(fragmentShaderPtr);
        }

        public void Use() => glUseProgram(ProgramID);               //Apply shaders before drawing

        public void SetMatrix4x4(string uniformName, Matrix4x4 mat)
        {
            int uniformPtr = glGetUniformLocation(ProgramID, uniformName);
            glUniformMatrix4fv(uniformPtr, 1, false, mat.ToFloatArray());
        }

        public void SetVector4(string uniformName, float v0, float v1, float v2, float v3)
        {
            int uniformPtr = glGetUniformLocation(ProgramID, uniformName);
            glUniform4f(uniformPtr, v0, v1, v2, v3);
        }

        public void SetBoolean(string uniformName, bool v)
        {
            int uniformPtr = glGetUniformLocation(ProgramID, uniformName);
            glUniform1i(uniformPtr, v ? 1 : 0);
        }

        public void SetInteger(string uniformName, int v)
        {
            int uniformPtr = glGetUniformLocation(ProgramID, uniformName);
            glUniform1i(uniformPtr, v);
        }
    }
}
