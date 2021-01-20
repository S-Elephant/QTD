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
    public class MiniMap
    {
        #region Members
        static readonly Texture2D ViewAreaTexture = Common.str2Tex("GUI/MMViewArea");
        public bool IsHidden = false;
        RenderTarget2D RenderTarget = new RenderTarget2D(Engine.Instance.Graphics.GraphicsDevice, 256, 160); // Is same ratio as 1280x800.
        public static readonly Point Size = new Point(256, 160); // Is same ratio as 1280x800.
        private Rectangle m_DrawRect;
        public Rectangle DrawRect
        {
            get { return m_DrawRect; }
            private set
            {
                m_DrawRect = value;
            }
        }
        Vector2 Location;
        Vector2 Ratio;
        List<LinePrimitive> LPs = new List<LinePrimitive>();

        int ViewAreaWidth, ViewAreaHeight;
        #endregion

        public MiniMap(Vector2 location, Point levelSize)
        {
            SetLocation(location);
            Ratio = new Vector2(Size.X / (float)levelSize.X, Size.Y / (float)levelSize.Y);

            ViewAreaWidth = (int)(Engine.Instance.Width * Ratio.X);
            ViewAreaHeight = (int)(Engine.Instance.Height * Ratio.Y);
        }

        public void SetLocation(Vector2 location)
        {
            Location = location;
            DrawRect = new Rectangle(location.Xi(), location.Yi(), Size.X, Size.Y);
        }

        public Vector2 LocationToWorld(Vector2 mouseScreenLoc)
        {
            return (mouseScreenLoc-Location) / Ratio;
        }

        public void PreRender()
        {
            Level.Instance.BGMgr.RenderMiniMapBG(ref RenderTarget, Size, Ratio);
            #region Waypoints
            foreach (WayPoint wp in Level.Instance.WayPoints)
                LPs.Add(wp.CreateLinesForMinimap(DrawRect.Location.ToVector2(), Ratio, Color.Orange));
            #endregion
        }

        public void Draw()
        {
            if (!IsHidden)
            {
                Engine.Instance.SpriteBatch.Draw(RenderTarget, DrawRect, Color.White);
                LPs.ForEach(l => l.Render(Engine.Instance.SpriteBatch));
                Engine.Instance.SpriteBatch.Draw(ViewAreaTexture, new Rectangle(DrawRect.X + (int)(Level.Instance.Player.Camera.Location.X * Ratio.X),
                                                                                DrawRect.Y + (int)(Level.Instance.Player.Camera.Location.Y * Ratio.Y),
                                                                                ViewAreaWidth,ViewAreaHeight), Color.Red);
            }
        }
    }
}