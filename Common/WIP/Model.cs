using System;
using System.Collections.Generic;
using System.IO;
using OpenToolkit.Graphics.OpenGL4;
using PangLib.PET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Common.WIP
{
    public class Model
    {
        private List<Mesh> _meshes;
        private string _modelDirectory;

        public Model(string path)
        {
            LoadModel(path);
        }

        public void Draw(Shader shader)
        {
            foreach (Mesh mesh in _meshes)
            {
                mesh.Draw(shader);
            }
        }

        private void LoadModel(string path)
        {
            FileInfo modelFile = new FileInfo(path);

            _modelDirectory = modelFile.Directory?.ToString();

            FileStream fileStream =
                File.OpenRead(modelFile.ToString());
            PETFile pet = new PETFile(fileStream);

            MeshHelper.CreateVerticesAndIndices(pet, out var vertices, out var indices);
            var petTextures = pet.Textures;
            var petTextureMap = pet.Mesh.TextureMap;

            List<Texture> textures = new List<Texture>();

            // TODO how do we know which type a texture has?
            for (var i = 0; i < petTextures.Count; i++)
            {
                var petTexture = petTextures[i];
                string petTexturePath = Path.Combine(_modelDirectory, petTexture.FileName);
                Texture texture = new Texture
                {
                    Path = petTexturePath,
                    Id = LoadTextureFromFile(petTexturePath)
                };

                // TODO maybe specular textures are identified by the starting "!"
                // pure diffuse by starting "]"
                // has mask by starting with "~[2#"
                if (petTexture.FileName.Contains("specular"))
                {
                    texture.Type = "texture_specular";
                }
                else
                {
                    texture.Type = "texture_diffuse";
                }

                textures.Add(texture);
            }

            _meshes = new List<Mesh>
            {
                new Mesh(vertices, indices, textures)
            };
        }

        private uint LoadTextureFromFile(string path)
        {
            GL.GenTextures(1, out uint textureId);

            byte[] data = LoadImageAsBytes(path, out var width, out var height, out var isMasked);

            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            // // Now, set the wrapping mode. S is for the X axis, and T is for the Y axis.
            // // We set this to Repeat so that textures will repeat when wrapped. Not demonstrated here since the texture coordinates exactly match
            // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
            //     (int) TextureWrapMode.Repeat);
            // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
            //     (int) TextureWrapMode.Repeat);
            // // First, we set the min and mag filter. These are used for when the texture is scaled down and up, respectively.
            // // Here, we use Linear for both. This means that OpenGL will try to blend pixels, meaning that textures scaled too far will look blurred.
            // // You could also use (amongst other options) Nearest, which just grabs the nearest pixel, which makes the texture look pixelated if scaled too far.
            // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            //     (int) TextureMinFilter.LinearMipmapLinear);
            // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            //     (int) TextureMagFilter.Linear);

            return textureId;
        }

        // TODO avoid loading the same texture multiple times
        private byte[] LoadImageAsBytes(string path, out int width, out int height, out bool isMasked)
        {
            // TODO don't load all as RGBA and instead find out the type, then switch over components like:
            ///GLenum format;
            /// if (nrComponents == 1)
            ///     format = GL_RED;
            /// else if (nrComponents == 3)
            //    format = GL_RGB;
            /// else if (nrComponents == 4)
            ///     format = GL_RGBA;
            // Load the image
            Image<Rgba32> image = Image.Load<Rgba32>(path);
            width = image.Width;
            height = image.Height;

            // ImageSharp loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
            // But since the V coord in PET files is stored upside down it fixes itself
            // image.Mutate(x => x.Flip(FlipMode.Vertical));

            // Get an array of the pixels, in ImageSharp's internal format.
            Rgba32[] tempPixels = image.GetPixelSpan().ToArray();

            // TODO make better
            // Check if a mask for this file exist and if yes, use it's Red channel for the textures Alpha
            isMasked = MaskFileExists(path, out var maskPath);
            Rgba32[] maskTempPixels = null;
            if (isMasked)
            {
                Console.Out.WriteLine($"Loading mask for '{path}': {maskPath}");
                Image<Rgba32> maskImage = Image.Load<Rgba32>(maskPath);
                // maskImage.Mutate(x => x.Flip(FlipMode.Vertical));
                maskTempPixels = maskImage.GetPixelSpan().ToArray();
            }

            // Convert ImageSharp's format into a byte array, so we can use it with OpenGL.
            List<byte> pixels = new List<byte>();
            for (var i = 0; i < tempPixels.Length; i++)
            {
                var p = tempPixels[i];
                pixels.Add(p.R);
                pixels.Add(p.G);
                pixels.Add(p.B);
                if (isMasked)
                {
                    pixels.Add(maskTempPixels[i].R);
                }
                else
                {
                    pixels.Add(p.A);
                }
            }

            return pixels.ToArray();
        }

        private static bool MaskFileExists(string path, out string maskPath)
        {
            maskPath = path.Replace(".jpg", "_mask.jpg");
            return new FileInfo(maskPath).Exists;
        }
    }
}
