using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenToolkit.Mathematics;
using PangLib.PET;
using PangLib.PET.Models;
using PetMesh = PangLib.PET.Models.Mesh;

namespace Common.WIP
{
    public static class MeshHelper
    {
        public static void CreateVerticesAndIndices(PETFile pet, out List<Vertex> vertices, out List<uint> indices)
        {
            Polygon[] petPolygons = pet.Mesh.Polygons;

            // create unique vertices with the index used in the polygons
            Vector3[] uniqueVertices = pet.Mesh.Vertices
                .Select(vertex => new Vector3(vertex.X, vertex.Y, vertex.Z))
                .ToArray();

            uint[] textureLayerNrByPolyId = pet.Mesh.TextureMap;

            Debug.Assert(petPolygons.Length == textureLayerNrByPolyId.Length);

            // every unique vertex can appear multiple times in a model (at points where polygons touch each other)
            vertices = new List<Vertex>();
            indices = new List<uint>();
            for (var polyId = 0; polyId < petPolygons.Length; polyId++)
            {
                Polygon petPoly = petPolygons[polyId];
                Debug.Assert(petPoly.PolygonIndices.Length == 3);
                for (var j = 0; j < petPoly.PolygonIndices.Length; j++)
                {
                    PolygonIndex ppi = petPoly.PolygonIndices[j];
                    int vertexId = polyId * 3 + j; // index for accessing texture layer nr
                    Vertex vertex = new Vertex
                    {
                        Position = uniqueVertices[ppi.Index],
                        Normal = new Vector3(ppi.X, ppi.Y, ppi.Z),
                        TexCoords = new Vector2(
                            ppi.UVMappings[0].U, ppi.UVMappings[0].V)
                    };
                    vertices.Add(vertex);
                    indices.Add((uint) vertexId);
                }
            }

            Debug.Assert(vertices.Count == 3 * petPolygons.Length);

            Debug.Assert(indices.Count == vertices.Count);

            // Mesh mesh = new Mesh(vertices, indices, textures)
            // {
            //     vertices = CreateVertices(pet.Mesh),
            //     boneWeights = CreateBoneWeights(pet),
            //     bindposes = CreateBindPoses(pet.Bones)
            // };
            //
            // Dictionary<int, Vector2[]> uvsById = CreateUvsById(pet.Mesh.Polygons);
            // for (int i = 0; i < uvsById.Count; i++)
            // {
            //     switch (i)
            //     {
            //         case 0:
            //             mesh.uv = uvsById[i];
            //             break;
            //         case 1:
            //             mesh.uv2 = uvsById[i];
            //             break;
            //         default:
            //             throw new NotImplementedException("Currently only two UV mappings are supported");
            //     }
            // }
            //
            // Dictionary<int, List<int>> trianglesByTextureIndex = CreateTrianglesByTextureIndex(pet);
            // mesh.subMeshCount = trianglesByTextureIndex.Count;
            //
            // for (int i = 0; i < mesh.subMeshCount; i++)
            // {
            //     mesh.SetTriangles(trianglesByTextureIndex[i].ToArray(), i);
            // }
            //
            // return mesh;
        }

        // array size is vertex count        
        private static Vector3[] CreateVertices(PetMesh petMesh)
        {
            // create unique vertices with the index used in the polygons
            Vector3[] uniqueVertices = petMesh.Vertices
                .Select(vertex => new Vector3(vertex.X, vertex.Y, vertex.Z))
                .ToArray();

            // every unique vertex can appear multiple times in a model (at points where polygons touch each other)
            Vector3[] vertices = petMesh.Polygons
                .SelectMany(polygon => polygon.PolygonIndices)
                .Select(polygonIndex => uniqueVertices[polygonIndex.Index])
                .ToArray();

            return vertices;
        }

        private static Dictionary<int, Vector2[]> CreateUvsById(IReadOnlyList<Polygon> petPolygons)
        {
            Dictionary<int, Vector2[]> uvs = Enumerable.Range(0, petPolygons[0].PolygonIndices[0].UVMappings.Length)
                .ToDictionary(uvIndex => uvIndex, uvIndex => petPolygons
                    .SelectMany(polygon => polygon.PolygonIndices)
                    .Select(polygonIndex => polygonIndex.UVMappings[uvIndex])
                    .Select(uvMapping => new Vector2(uvMapping.U, 1 - uvMapping.V)) // V in PETFiles is flipped
                    .ToArray());

            return uvs;
        }

        // same size as Materials array
        private static Dictionary<int, List<int>> CreateTrianglesByTextureIndex(PETFile pet)
        {
            // create lists because we don't know beforehand how many textures will be in a submesh
            Dictionary<int, List<int>> trianglesByTextureIndex = Enumerable.Range(0, pet.Textures.Count)
                .ToDictionary(i => i, _ => new List<int>());

            for (int i = 0; i < pet.Mesh.Polygons.Length; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int targetIndex = i * 3 + j;
                    int textureIndex = (int) pet.Mesh.TextureMap[i];

                    trianglesByTextureIndex[textureIndex].Add(targetIndex);
                }
            }

            return trianglesByTextureIndex;
        }
    }
}
