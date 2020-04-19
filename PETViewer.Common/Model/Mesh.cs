using System;
using System.Collections.Generic;
using OpenToolkit.Graphics.OpenGL4;

namespace PETViewer.Common.Model
{
    public class Mesh
    {
        private const int StrideOfVector3 = 3 * sizeof(float);

        /*  Mesh Data  */
        private readonly List<Vertex> _vertices;
        private const int StrideOfVertex = 3 * StrideOfVector3;
        private const int OffsetOfVertexPosition = 0;
        private const int OffsetOfVertexNormal = 1 * StrideOfVector3;
        private const int OffsetOfVertexTexCoords = 2 * StrideOfVector3;

        private readonly List<uint> _indices;
        private const int StrideOfUint = sizeof(uint);

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
            for (var i = 0; i < _textures.Count; i++)
            {
                TextureType textureType = _textures[i].Type;

                // activate proper texture unit before binding
                GL.ActiveTexture((TextureUnit) ((int) TextureUnit.Texture0 + i));

                if (TextureType.TextureArray == textureType)
                {
                    shader.SetInt(textureType.ToString(), 0);
                    GL.BindTexture(TextureTarget.Texture2DArray, _textures[i].Id);
                }
                else
                {
                    Console.Error.WriteLine($"Ignoring unknown texture type: {textureType}");
                }
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
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * StrideOfVertex, _vertices.ToArray(),
                BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count * StrideOfUint, _indices.ToArray(),
                BufferUsageHint.StaticDraw);

            // set the vertex attribute pointers
            // vertex Positions
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, StrideOfVertex, OffsetOfVertexPosition);
            // vertex normals
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, StrideOfVertex, OffsetOfVertexNormal);
            // vertex texture coords (uv + layer nr)
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, StrideOfVertex, OffsetOfVertexTexCoords);

            GL.BindVertexArray(0);
        }
    }
}
