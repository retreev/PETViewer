using System.Collections.Generic;
using System.Linq;
using OpenTK;
using PangLib.PET;
using PangLib.PET.DataModels;
using PETViewer.Common;

namespace PETViewer
{
    public class Model
    {
        private Mesh[] _meshes;
        private Texture[] _texturesLoaded; // TODO make truly global, so loading multiple models doesnt load existing texutres again?

        public Model(string directory)
        {
            LoadModel(directory);
        }

        private void LoadModel(string directory)
        {
            PETFile pet = new PETFile("../models_dont_commit/item0_01.pet");

            Vector3[] uniqueVertices = pet.Vertices.Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray();

            List<PangLib.PET.DataModels.Texture> petTextures = pet.Textures;

            Vertex[] vertices = pet.Polygons.SelectMany(p => p.PolygonIndices).Select(pi => new Vertex
            {
                Position = uniqueVertices[pi.Index],
                TexCoords = new Vector2(pi.UVMappings[0].U, pi.UVMappings[0].V),
                Normal = new Vector3(pi.X, pi.Y, pi.Z)
            }).ToArray();

            _meshes = new[] {new Mesh(vertices, new Texture[] {new Texture(Util.GetTexturePath())})};
        }

        // same size as Materials array
        private static Dictionary<int, List<int>> CreateTrianglesByTextureIndex(PETFile pet)
        {
            // create lists because we don't know beforehand how many textures will be in a submesh

            Dictionary<int, List<int>> trianglesByTextureIndex = Enumerable.Range(0, pet.Textures.Count)
                .ToDictionary(i => i, _ => new List<int>());

            for (int i = 0; i < pet.Polygons.Count; i++)
            {
                Polygon polygon = pet.Polygons[i];
                for (int j = 0; j < 3; j++)
                {
                    int targetIndex = i * 3 + j;
                    int textureIndex = (int) polygon.TextureIndex;
                    trianglesByTextureIndex[textureIndex].Add(targetIndex);
                }
            }

            return trianglesByTextureIndex;
        }

        public void Draw(Shader shader)
        {
            foreach (Mesh mesh in _meshes)
            {
                mesh.Draw(shader);
            }
        }
    }
}
