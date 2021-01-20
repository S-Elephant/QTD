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

    // redo this shit. make it an interface. then have a type for slow, for stunned, etc? poison needs to know how much damage per thick. make thicks per second. poison also needs to know how long it is applied... fuck change datatstructs and such... and the defender weapon

    internal class ModifierInstance
    {
        public SimpleTimer Timer;
        emodifierType ModType;

        private bool m_IsActive = false;
        public bool IsActive
        {
            get { return m_IsActive; }
            private set { m_IsActive = value; }
        }

        public ModifierInstance(emodifierType modType)
        {
            ModType = modType;

            switch (modType)
            {
                case emodifierType.Slow:
                    Timer = new SimpleTimer(2500);
                    break;
                case emodifierType.Stunned:
                    Timer = new SimpleTimer(Maths.RandomNr(500,1500));
                    break;
                case emodifierType.Rooted:
                    Timer = new SimpleTimer(3000);
                    break;
                case emodifierType.ArmorHalved:
                    Timer = new SimpleTimer(2500);
                    break;
                case emodifierType.Poisoned:
                    Timer = new SimpleTimer(5000);
                    break;
                default:
                    break;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                Timer.Update(gameTime);
                if (Timer.IsDone)
                    IsActive = false;
            }
        }

        public void Activate()
        {
            IsActive = true;
            Timer.Reset();
        }
    }

    public class ActiveModifiers
    {
        

        public ActiveModifiers()
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