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
    public class Modifier
    {
        SimpleTimer Duration;

        public bool IsActive
        {
            get { return !Duration.IsDone; }
        }
       


        public Modifier(emodifierType modifierType)
        {
            

        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw()
        {

        }
    }
}