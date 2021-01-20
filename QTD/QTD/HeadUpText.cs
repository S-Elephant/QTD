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
    public class HeadUpText
    {
        #region Members
        static SpriteFont CriticalFont = Common.str2Font("Critical");
        static SpriteFont IncomeFont = Common.str2Font("Income");
        SpriteFont Font;
        internal StringBuilder Text;
        internal SimpleTimer TTL;
        const float Velocity = 0.25f;
        internal Vector2 Location;
        public bool IsDisposed;
        #endregion

        public HeadUpText()
        {
            TTL = new SimpleTimer(2100);
        }

        public void Initialize(StringBuilder text, Vector2 centerLocation, bool isCritical)
        {
            Text = text;
            IsDisposed = false;
            TTL.Reset();

            if (isCritical)
                Font = CriticalFont;
            else
                Font = IncomeFont;

            Location = new Vector2(centerLocation.X - Font.MeasureString(text).X / 2, centerLocation.Y - Font.MeasureString(text).Y / 2);
        }

        public static HeadUpText PoolInitialize()
        {
            return new HeadUpText();
        }

        public void Update(GameTime gameTime)
        {
            TTL.Update(gameTime);
            if (!TTL.IsDone)
                Location.Y -= Velocity;
            else
                IsDisposed = true;
        }

        public void Draw()
        {
            Engine.Instance.SpriteBatch.DrawString(Font, Text, Location, Color.White);
        }
    }
}