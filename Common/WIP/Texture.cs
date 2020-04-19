namespace Common.WIP
{
    public struct Texture
    {
        public uint Id;
        public TextureType Type;
        public string Path; // we store the path of the texture to compare with other textures
    }

    public sealed class TextureType
    {
        // name of the sampler in the shader
        private readonly string _name;

        public static readonly TextureType TextureArray = new TextureType("texture_array");

        private TextureType(string name)
        {
            _name = name;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
