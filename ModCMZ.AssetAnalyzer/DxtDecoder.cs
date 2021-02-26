using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.AssetAnalyzer
{
    public class DxtDecoder
    {
        private static readonly Type _type = typeof(Texture2D).Assembly.GetType("Microsoft.Xna.Framework.Graphics.DxtDecoder");
        private static readonly ConstructorInfo _ctor = _type.GetConstructor(new[] { typeof(int), typeof(int), typeof(SurfaceFormat) });

        private object _instance;

        public DxtDecoder(int width, int height, SurfaceFormat format)
        {
            _instance = _ctor.Invoke(new object[] { width, height, format });
        }

        private static readonly MethodInfo _decode = _type.GetMethod("Decode");
        public Color[] Decode(byte[] source) => (Color[])_decode.Invoke(_instance, new object[] { source });
    }
}
