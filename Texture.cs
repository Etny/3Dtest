﻿using Silk.NET.OpenGL;
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
        public uint ID { get; private set; }

        private GL gl;

        public Texture(GL gl, string imgPath)
        {
            this.gl = gl;

            ID = gl.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, ID);

            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);

            LoadTexture(imgPath);
        }

        private unsafe void LoadTexture(string path)
        {
            Image<Rgba32> img = (Image<Rgba32>)Image.Load(path);
            img.Mutate(x => x.Flip(FlipMode.Vertical));

            fixed (void* i = &MemoryMarshal.GetReference(img.GetPixelSpan())) 
                gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, (uint)img.Width, (uint)img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, i);

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
}
