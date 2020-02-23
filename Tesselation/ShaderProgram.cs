using OpenTK.Graphics.OpenGL;
using System.IO;

namespace Tesselation
{
    public class ShaderProgram
    {
        int shader_program;

        public ShaderProgram()
        {
            shader_program = GL.CreateProgram();
        }

        public int Compile()
        {
            GL.LinkProgram(shader_program);

            string log = GL.GetProgramInfoLog(shader_program);
            if (log != "")
                throw new System.Exception(log);

            return shader_program;
        }

        public ShaderProgram addFragmentShader(StreamReader source)
        {
            addShader(source, ShaderType.FragmentShader);
            return this;
        }

        public ShaderProgram addVertexShader(StreamReader source)
        {
            addShader(source, ShaderType.VertexShader);
            return this;
        }

        public ShaderProgram addGeometryShader(StreamReader source)
        {
            addShader(source, ShaderType.GeometryShader);
            return this;
        }

        public ShaderProgram addComputeShader(StreamReader source)
        {
            addShader(source, ShaderType.ComputeShader);
            return this;
        }

        public ShaderProgram addTessControlShader(StreamReader source)
        {
            addShader(source, ShaderType.TessControlShader);
            return this;
        }

        public ShaderProgram addTessEvaluationShader(StreamReader source)
        {
            addShader(source, ShaderType.TessEvaluationShader);
            return this;
        }

        void addShader(StreamReader source, ShaderType type)
        {
            string code = source.ReadToEnd();
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            string log = GL.GetShaderInfoLog(shader);
            if (log != "")
                throw new System.Exception(log);

            GL.AttachShader(shader_program, shader);
        }
    }
}
