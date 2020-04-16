using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using PETViewer.Common;

namespace PETViewer
{
    public class Mesh
    {
        public Vertex[] Vertices;
        public Texture[] Textures;

        private uint _vao, _vbo;

        public Mesh(Vertex[] vertices, Texture[] textures)
        {
            Vertices = vertices;
            Textures = textures;

            SetupMesh();
        }

        private void SetupMesh()
        {
            GL.GenVertexArrays(1, out _vao);
            GL.GenBuffers(1, out _vbo);

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * Marshal.SizeOf<Vertex>(), Vertices,
                BufferUsageHint.StaticDraw);

            // vertex positions
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), 0);
            // vertex normals
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(),
                Marshal.OffsetOf<Vertex>(nameof(Vertex.Normal)));
            // vertex texture coords
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(),
                Marshal.OffsetOf<Vertex>(nameof(Vertex.TexCoords)));

            GL.BindVertexArray(0);
        }

        public void Draw(Shader shader)
        {
            uint diffuseNr = 1;
            uint specularNr = 1;

            for (uint i = 0; i < Textures.Length; i++)
            {
                if (Enum.TryParse((uint.Parse(TextureUnit.Texture0.ToString()) + i).ToString(), out TextureUnit tex))
                    GL.ActiveTexture(tex);
                else
                    throw new Exception("couldn't get TextureUnit");

                // TODO maybe add mask
                string number;
                string name = Textures[i].Type;
                if (name.Equals("texture_diffuse"))
                    number = (diffuseNr++).ToString();
                else if (name.Equals("texture_specular"))
                    number = (specularNr++).ToString();
                else
                    throw new Exception("unknown texture type: " + name);

                shader.SetFloat("material." + name + number, i);
                GL.BindTexture(TextureTarget.Texture2D, Textures[i].Id);
            }

            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, Vertices.Length);
            GL.BindVertexArray(0);
        }
    }
}
