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
    public class InfoPanel
    {
        static readonly Rectangle DrawRect = new Rectangle(800, 100, 400, 512);
        static readonly Texture2D BG = Common.White1px50Trans;
        List<StringBuilder> Texts = new List<StringBuilder>(50);
        static readonly SpriteFont Font = Common.str2Font("InfoPanel");
        float TextSpacingY;

        public InfoPanel()
        {
            TextSpacingY = Font.MeasureString(Common.MeasureString).Y;
        }

        public void SetText(List<StringBuilder> texts)
        {
            Texts.Clear();
            Texts.AddRange(texts);
        }

        public void ClearText()
        {
            Texts.Clear();
        }

        public void Draw()
        {
            if (Texts.Count > 0)
            {
                // BG
                Engine.Instance.SpriteBatch.Draw(BG, DrawRect, Color.Gray);

                // Texts
                float locY = DrawRect.Y + 20;
                foreach (StringBuilder text in Texts)
                {
                    Engine.Instance.SpriteBatch.DrawString(Font, text, new Vector2(DrawRect.X + 10, locY), Color.White);
                    locY += TextSpacingY;
                }
            }
        }
    }
}