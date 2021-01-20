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
    public struct Block
    {
        public Rectangle ColRect;
        public Point GridIdx;
        public List<IEntity> Entities;
        private Color DebugColor;
        const int MAX_ENTITIES_PER_BLOCK = 35;

        public Block(Point gridIdx)
        {
            Entities = new List<IEntity>(MAX_ENTITIES_PER_BLOCK);
            GridIdx = gridIdx;
            ColRect = new Rectangle(gridIdx.X * BroadPhase.Instance.GridSize+BroadPhase.Instance.Location.X, gridIdx.Y * BroadPhase.Instance.GridSize+BroadPhase.Instance.Location.Y, BroadPhase.Instance.GridSize, BroadPhase.Instance.GridSize);
            DebugColor = Misc.RandomColor();
        }

        public void ClearExceptTowers()
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                if (!(Entities[i] is BaseTower))
                {
                    Entities.RemoveAt(i);
                    i--;
                }
            }
        }

        public void Update(GameTime gameTime)
        {

        }

        public void DebugDraw()
        {
            if(Entities.Count == 0)
                Engine.Instance.SpriteBatch.Draw(Common.White1px50Trans, ColRect, DebugColor);
            else
                Engine.Instance.SpriteBatch.Draw(Common.White1px50Trans, ColRect, Color.Black);
        }
    }
}