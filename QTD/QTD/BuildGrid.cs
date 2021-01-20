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
    public class BuildGrid
    {
        static readonly Texture2D[] Textures = new Texture2D[3]
            {
                Common.str2Tex("BuildGrid/buildable"),
                Common.str2Tex("BuildGrid/neverBuildable"),
                Common.str2Tex("BuildGrid/occupied")
            };
        public const int GRID_SIZE = 32;
        static readonly Vector2 GRID_SIZE_V2 = new Vector2(GRID_SIZE,GRID_SIZE);
        public const int Buildable = 0;
        public const int NeverBuildable = 1;
        public const int Occupied = 2;
        int[,] Blocks;
        Point BlocksCnt;

        private Rectangle m_BuildRect;
        public Rectangle BuildRect
        {
            get { return m_BuildRect; }
            private set { m_BuildRect = value; }
        }
       
        private bool m_CanBuild;
        public bool CanBuild
        {
            get { return m_CanBuild; }
            private set { m_CanBuild = value; }
        }

        public void SetBlock(Point idx, int value)
        {
            Blocks[idx.X, idx.Y] = value;
        }

        public BuildGrid()
        {
            BlocksCnt = new Point(Level.Instance.LevelSize.X / GRID_SIZE, Level.Instance.LevelSize.Y / GRID_SIZE);
            Blocks = new int[BlocksCnt.X, BlocksCnt.Y];
        }

        public void SetUnBuildables(string str)
        {
            string[] strPoints = str.Split('|');
            for (int i = 0; i < strPoints.Length; i++)
            {
                Point p = Common.Str2Point( strPoints[i]);
                Blocks[p.X, p.Y] = NeverBuildable;
            }
        }

        public void Occupy()
        {
            throw new NotImplementedException();
        }

        public void Update(Vector2 mouseWorldLoc, Point buildSize)
        {
            Vector2 snappedLoc = Maths.SnapToGrid(mouseWorldLoc, GRID_SIZE_V2);
            BuildRect = new Rectangle(snappedLoc.Xi(), snappedLoc.Yi(), buildSize.X * GRID_SIZE, buildSize.Y * GRID_SIZE);

            CanBuild = true;
            for (int y = BuildRect.Top / GRID_SIZE; y < BuildRect.Bottom / GRID_SIZE; y++)
            {
                for (int x = BuildRect.Left / GRID_SIZE; x < BuildRect.Right / GRID_SIZE; x++)
                {
                    if ((x < 0 || y < 0 || x >= BlocksCnt.X || y >= BlocksCnt.Y) ||
                        Blocks[x, y] != 0
                        )
                    {
                        CanBuild = false;
                        return;
                    }
                }
            }
        }

        public void Draw()
        {
            for (int y = 0; y < BlocksCnt.Y; y++)
            {
                for (int x = 0; x < BlocksCnt.X; x++)
                {
                    Engine.Instance.SpriteBatch.Draw(Textures[Blocks[x, y]], new Vector2(x * GRID_SIZE, y * GRID_SIZE), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
                }
            }

            if(CanBuild)
                Engine.Instance.SpriteBatch.Draw(Common.White1px50Trans, BuildRect, Color.Gold);
            else
                Engine.Instance.SpriteBatch.Draw(Common.White1px50Trans, BuildRect, Color.Black);
        }
    }
}