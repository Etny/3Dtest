using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace _3Dtest
{
    class Texture
    {
        private static readonly Dictionary<TextureType, InternalFormat> encodings = new Dictionary<TextureType, InternalFormat>()
        {
            {TextureType.Diffuse, InternalFormat.Srgb},
            {TextureType.Specular, InternalFormat.Rgba},
            {TextureType.Normal, InternalFormat.Rgba}
        };

        public static readonly Dictionary<TextureType, string> ShaderName = new Dictionary<TextureType, string>()
        {
            {TextureType.Diffuse, "texture_diffuse"},
            {TextureType.Specular, "texture_specular"},
            {TextureType.Normal, "texture_normal"}
        };


        public uint ID { get; private set; }
        public string path;
        public TextureType TextureType;

        private GL gl;

        public Texture(GL gl, string imgPath, TextureType type)
        {
            this.gl = gl;
            this.TextureType = type;
            this.path = imgPath;

            ID = gl.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, ID);

            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);

            LoadTexture(imgPath);
        }

        public Texture(GL gl, string imgPath, TextureType type, int unit) : this(gl, imgPath, type)
        {
            BindToUnit(unit);
        }
        
        private unsafe void LoadTexture(string path)
        {
            Image<Rgba32> img = (Image<Rgba32>)Image.Load(path);
            img.Mutate(x => x.Flip(FlipMode.Vertical));

            InternalFormat format;
            encodings.TryGetValue(TextureType, out format);
            //format = InternalFormat.Rgba;

            fixed (void* i = &MemoryMarshal.GetReference(img.GetPixelSpan())) 
                gl.TexImage2D(TextureTarget.Texture2D, 0, (int)format, (uint)img.Width, (uint)img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, i);

            img.Dispose();

            gl.GenerateMipmap(TextureTarget.Texture2D);
        }   

        public void BindToUnit(int unit)
        {
            gl.ActiveTexture(TextureUnit.Texture0 + unit);
            Use();
        }

        public void Use()
        {
            gl.BindTexture(TextureTarget.Texture2D, ID);
        }

    }

    public enum TextureType
    {
        Diffuse, Specular, Normal
    }
}
