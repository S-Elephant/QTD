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
    internal class Warning
    {
        private int m_Index;
        public int Index
        {
            get { return m_Index; }
            private set { m_Index = value; }
        }
       
        private StringBuilder m_SB;
        public StringBuilder SB
        {
            get { return m_SB; }
            set
            {
                m_SB = value;
                if(value != null)
                    DrawLoc = new Vector2(Engine.Instance.Width / 2 - WarningMessages.Font.MeasureString(SB).X / 2, DrawLoc.Y);
            }
        }
       
        public SimpleTimer Timer;
        public Vector2 DrawLoc;

        public Warning(StringBuilder sb, int idx)
        {
            Move(idx); // Set before setting the sb
            SB = sb;
            Timer = new SimpleTimer(5000);
        }

        public void Add(StringBuilder sb)
        {
            SB = sb;
            Timer.Reset();
        }

        public void Move(int idx)
        {
            Index = idx;
            DrawLoc = new Vector2(DrawLoc.X, 80 + idx * WarningMessages.Font.MeasureString(Common.MeasureString).Y);
        }
    }

    public class WarningMessages
    {
        public static WarningMessages Instance;
        const int MAX_WARNINGS = 3;
        Warning[] Warnings = new Warning[MAX_WARNINGS];        
        static readonly List<StringBuilder> CommonWarnings = new List<StringBuilder>()
        {
            new StringBuilder("Insufficient Gold."),
            new StringBuilder("Insufficient Supply.")
        };
        internal static readonly SpriteFont Font = Common.str2Font("Warning");

        public WarningMessages()
        {
            for (int i = 0; i < MAX_WARNINGS; i++)
            {
                Warnings[i] = new Warning(null, i);
            }
        }

        public void AddWarning(int commonWarningIdx)
        {
            AddWarning(CommonWarnings[commonWarningIdx]);
        }

        public void AddWarning(StringBuilder sb)
        {
            // Try to find a free spot
            for (int i = 0; i < MAX_WARNINGS; i++)
            {
                if (Warnings[i].SB == null)
                {
                    Warnings[i].Add(sb);
                    return;
                }
            }

            // No free spot was found so move all warnings by -1 and overwrite the first one.
            for (int i = 1; i < MAX_WARNINGS; i++)
            {
                Warnings[i].Move(i - 1); // move the class
                Warning temp = Warnings[i - 1];
                temp.Move(i);
                Warnings[i - 1] = Warnings[i]; // move in the array
                Warnings[i] = temp;
            }
            // Now add a warning on the end.
            Warnings[MAX_WARNINGS - 1].Add(sb);
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < MAX_WARNINGS; i++)
            {
                if (Warnings[i].SB != null)
                {
                    Warnings[i].Timer.Update(gameTime);
                    if (Warnings[i].Timer.IsDone)
                    {
                        Warnings[i].SB = null;
                        for (int j = i+1; j < MAX_WARNINGS; j++)
                        {
                            if (Warnings[j].SB == null)
                                return;
                            Warnings[j].Move(j - 1); // move the class
                            Warning temp = Warnings[j - 1];
                            temp.Move(j);
                            Warnings[j - 1] = Warnings[j]; // move in the array
                            Warnings[j] = temp;
                        }
                        i--;
                    }
                }
            }
        }

        public void Draw()
        {
            for (int i = 0; i < MAX_WARNINGS; i++)
            {
                if(Warnings[i].SB != null)
                    Engine.Instance.SpriteBatch.DrawString(Font, Warnings[i].SB, Warnings[i].DrawLoc, Color.White);
            }
        }
    }
}