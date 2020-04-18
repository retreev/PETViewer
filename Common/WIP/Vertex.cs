using System.Runtime.InteropServices;
using OpenToolkit.Mathematics;

namespace Common.WIP
{
    [StructLayout(LayoutKind.Sequential, Size = 8 * sizeof(float), Pack = 1)]
    public struct Vertex
    {
        // 3 * float
        public Vector3 Position;

        // 3 * float
        public Vector3 Normal;

        // 2 * float
        public Vector2 TexCoords;

    }
}
