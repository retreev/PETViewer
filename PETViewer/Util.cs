using System.Linq;
using OpenTK;
using PangLib.PET;

namespace PETViewer
{
    public static class Util
    {
        public static Vertex[] LoadPet()
        {
            PETFile pet = new PETFile("../models_dont_commit/item0_01.pet");

            Vector3[] uniqueVertices = pet.Vertices.Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray();

            Vertex[] vertices = pet.Polygons.SelectMany(p => p.PolygonIndices).Select(pi => new Vertex
            {
                Position = uniqueVertices[pi.Index],
                TexCoords = new Vector2(pi.UVMappings[0].U, pi.UVMappings[0].V),
                Normals = new Vector3(pi.X, pi.Y, pi.Z)
            }).ToArray();

            return vertices;
        }

        public static string GetTexturePath()
        {
            return "../models_dont_commit/]item0_01-01b.jpg";
        }
    }
}
