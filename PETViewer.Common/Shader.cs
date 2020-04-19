using System;
using System.IO;
using System.Text;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;

namespace PETViewer.Common
{
    public class Shader : IDisposable
    {
        public int Id;

        public Shader(string vertPath, string fragPath, string geometryPath = null)
        {
            // vertex shader
            var vShaderCode = LoadSource(vertPath);
            var vertex = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertex, vShaderCode);
            CompileShader(vertex, "VERTEX");

            // fragment shader
            var fShaderCode = LoadSource(fragPath);
            var fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragment, fShaderCode);
            CompileShader(fragment, "FRAGMENT");

            // if geometry shader is given, compile geometry shader
            var geometry = 0;
            if (geometryPath != null)
            {
                var gShaderCode = LoadSource(geometryPath);
                geometry = GL.CreateShader(ShaderType.GeometryShader);
                GL.ShaderSource(geometry, gShaderCode);
                CompileShader(geometry, "GEOMETRY");
            }

            // create shader program
            Id = GL.CreateProgram();
            GL.AttachShader(Id, vertex);
            GL.AttachShader(Id, fragment);
            if (geometryPath != null)
            {
                GL.AttachShader(Id, geometry);
            }

            LinkProgram(Id, "PROGRAM");

            // delete the shaders as they're now linked into the program and no longer necessary
            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);
            if (geometryPath != null)
            {
                GL.DeleteShader(geometry);
            }
        }


        private static void CompileShader(int shader, string type)
        {
            // Try to compile the shader
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int) All.True)
            {
                GL.GetShaderInfoLog(shader, 1024, out _, out var infoLog);
                throw new Exception($"ERROR::SHADER_COMPILATION_ERROR of type: {type}\n{infoLog}");
            }
        }

        private static void LinkProgram(int program, string type)
        {
            // We link the program
            GL.LinkProgram(program);

            // Check for linking errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int) All.True)
            {
                GL.GetProgramInfoLog(program, 1024, out _, out var infoLog);
                throw new Exception($"ERROR::PROGRAM_LINKING_ERROR of type: {type}\n{infoLog}");
            }
        }

        // activate the shader
        public void Use()
        {
            GL.UseProgram(Id);
        }

        /* utility uniform functions */

        public void SetBool(string name, bool value)
        {
            GL.Uniform1(GL.GetUniformLocation(Id, name), value ? 1 : 0);
        }

        public void SetInt(string name, int value)
        {
            GL.Uniform1(GL.GetUniformLocation(Id, name), value);
        }

        public void SetFloat(string name, float value)
        {
            GL.Uniform1(GL.GetUniformLocation(Id, name), value);
        }

        public void SetVec2(string name, Vector2 value)
        {
            GL.Uniform2(GL.GetUniformLocation(Id, name), value.X, value.Y);
        }

        public void SetVec3(string name, Vector3 value)
        {
            GL.Uniform3(GL.GetUniformLocation(Id, name), value.X, value.Y, value.Z);
        }

        public void SetVec4(string name, Vector4 value)
        {
            GL.Uniform4(GL.GetUniformLocation(Id, name), value.X, value.Y, value.Z, value.W);
        }

        public void SetMat2(string name, Matrix2 value)
        {
            GL.UniformMatrix2(GL.GetUniformLocation(Id, name), true, ref value);
        }

        public void SetMat3(string name, Matrix3 value)
        {
            GL.UniformMatrix3(GL.GetUniformLocation(Id, name), true, ref value);
        }

        public void SetMat4(string name, Matrix4 value)
        {
            GL.UniformMatrix4(GL.GetUniformLocation(Id, name), true, ref value);
        }

        // load the entire file into a string
        private static string LoadSource(string path)
        {
            // when dragging a file onto the executable the base path for the stream reader somehow gets changes
            // thus set append the base path containing the shaders manually
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            using (var sr = new StreamReader(Path.Combine(baseDirectory, path), Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        // This section is dedicated to cleaning up the shader after it's finished.
        // Doing this solely in a finalizer results in a crash because of the Object-Oriented Language Problem
        // ( https://www.khronos.org/opengl/wiki/Common_Mistakes#The_Object_Oriented_Language_Problem )
        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                GL.DeleteProgram(Id);

                _disposedValue = true;
            }
        }

        ~Shader()
        {
            Dispose(false);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
