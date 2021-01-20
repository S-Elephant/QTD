using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNALib;

namespace QTD
{
    public class Visual
    {
        public bool IsDisposed { get { return Animation.IsDisposed; } }

        SimpleASprite Animation;
        public Visual()
        {
#warning todo: build class. this one must be pooled.
        }

        public void Update(GameTime gameTime)
        {
            Animation.Update(gameTime);
        }

        public void Draw()
        {
            // draw using the right depth.
        }
    }
}