using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            List<Texture> textures = new List<Texture>();

            // only create an array with all textures for now
            // handling specular textures etc. can be done some other time, hopefully
            Texture texture = new Texture
            {
                // Path = petTexturePath,
                Id = LoadTextures(pet),
                Type = "texture_array"
            };

            textures.Add(texture);

            _meshes = new List<Mesh>
            {
                new Mesh(vertices, indices, textures)
            };
        }

        private uint LoadTextures(PETFile pet)
        {
            int maxHeight = 64;
            int maxWidth = 64;

            var petTextures = pet.Textures;
            var layerCount = petTextures.Count;

            List<byte> texels = new List<byte>();
            // List<RawImage> images = new List<RawImage>();
            for (var i = 0; i < layerCount; i++)
            {
                string petTexturePath = Path.Combine(_modelDirectory, petTextures[i].FileName);
                byte[] data = LoadImageAsBytes(petTexturePath, maxWidth, maxHeight, out var width, out var height,
                    out var isMasked);

                texels.AddRange(data.ToList());

                // images.Add(new RawImage
                // {
                //     Pixels = data,
                //     Width = width,
                //     Height = height,
                //     IsMasked = isMasked
                // });

                // GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, i, width, height, layerCount, PixelFormat.Rgba,
                //     PixelType.UnsignedByte, data);
            }

            GL.GenTextures(1, out uint textureId);

            GL.BindTexture(TextureTarget.Texture2DArray, textureId);
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8,
                maxWidth, maxHeight, layerCount);

            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, 0,
                maxWidth, maxHeight, layerCount, PixelFormat.Rgba, PixelType.UnsignedByte, texels.ToArray());

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS,
                (int) TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT,
                (int) TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Linear);

            return textureId;
        }

        // TODO avoid loading the same texture multiple times
        private byte[] LoadImageAsBytes(string path, int maxWidth, int maxHeight, out int width, out int height,
            out bool isMasked)
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

            // TODO remove
            image.Mutate(x => x.Resize(maxWidth, maxHeight));

            Debug.Assert(width <= maxWidth && height <= maxHeight);
            Console.Out.WriteLine($"Width: {width}, Height: {height}");

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

                // TODO remove
                image.Mutate(x => x.Resize(maxWidth, maxHeight));

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

    struct RawImage
    {
        internal byte[] Pixels;
        internal int Width;
        internal int Height;
        internal bool IsMasked;
    }
}
