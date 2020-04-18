using System.Collections.Generic;
using OpenToolkit.Graphics.OpenGL4;

namespace Common.WIP
{
    public class Mesh
    {
        /*  Mesh Data  */
        private readonly List<Vertex> _vertices;
        private const int SizeOfVertex = 8 * sizeof(float);
        private const int OffsetOfVertexPosition = 0;
        private const int OffsetOfVertexNormal = 3 * sizeof(float);
        private const int OffsetOfVertexTexCoords = 6 * sizeof(float);

        private readonly List<uint> _indices;
        private const int SizeOfUint = sizeof(uint);

        private readonly List<Texture> _textures;

        /*  Render data  */
        private uint _vao, _vbo, _ebo;

        public Mesh(List<Vertex> vertices, List<uint> indices, List<Texture> textures)
        {
            _vertices = vertices;
            _indices = indices;
            _textures = textures;

            SetupMesh();
        }

        // render the mesh
        public void Draw(Shader shader)
        {
            // bind appropriate textures
            uint diffuseNr = 1;
            uint specularNr = 1;
            uint normalNr = 1;
            uint heightNr = 1;

            for (var i = 0; i < _textures.Count; i++)
            {
                // activate proper texture unit before binding
                GL.ActiveTexture((TextureUnit) ((int) TextureUnit.Texture0 + i));
                // retrieve texture number (the N in diffuse_textureN)
                string name = _textures[i].Type;
                string number = name switch
                {
                    "texture_diffuse" => (diffuseNr++).ToString(),
                    "texture_specular" => (specularNr++).ToString(),
                    "texture_normal" => (normalNr++).ToString(),
                    "texture_height" => (heightNr++).ToString(),
                    _ => ""
                };

                // now set the sampler to the correct texture unit
                shader.SetInt($"{name}{number}", i);
                // and finally bind the texture
                GL.BindTexture(TextureTarget.Texture2D, _textures[i].Id);
            }

            // draw mesh
            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);

            // always good practice to set everything back to defaults once configured.
            GL.ActiveTexture(TextureUnit.Texture0);
        }

        private void SetupMesh()
        {
            // create buffers/arrays
            GL.GenVertexArrays(1, out _vao);
            GL.GenBuffers(1, out _vbo);
            GL.GenBuffers(1, out _ebo);

            GL.BindVertexArray(_vao);
            // load data into vertex buffers
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            // A great thing about structs is that their memory layout is sequential for all its items.
            // The effect is that we can simply pass a pointer to the struct and it translates perfectly to a glm::vec3/2 array which
            // again translates to 3/2 floats which translates to a byte array.
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * SizeOfVertex, _vertices.ToArray(),
                BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count * SizeOfUint, _indices.ToArray(),
                BufferUsageHint.StaticDraw);

            // set the vertex attribute pointers
            // vertex Positions
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, SizeOfVertex, OffsetOfVertexPosition);
            // vertex normals
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, SizeOfVertex, OffsetOfVertexNormal);
            // vertex texture coords
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, SizeOfVertex, OffsetOfVertexTexCoords);

            GL.BindVertexArray(0);
        }
    }
}
