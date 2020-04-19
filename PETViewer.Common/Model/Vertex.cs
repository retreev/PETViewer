using System.Runtime.InteropServices;
using OpenToolkit.Mathematics;

namespace PETViewer.Common.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex
    {
        // index 0, size 3 * float
        public Vector3 Position;

        // index 1, size 3 * float
        public Vector3 Normal;

        // index 2, size 3 * float (XY = UV, Z = Layer)
        public Vector3 TexCoords;
    }
}
