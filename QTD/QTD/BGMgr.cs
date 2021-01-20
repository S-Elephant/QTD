using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALib;

namespace QTD
{
    /// <summary>
    /// A class that contains the background splitted into pieces (because the entire BG may be bigger than the max. 2048x2048 pixels).
    /// </summary>
    internal class BGTile
    {
        /// <summary>
        /// The rendertarget that contains the background tile (2048x2048)
        /// </summary>
        public RenderTarget2D RenderTarget;

        private Rectangle m_AABB;
        public Rectangle AABB
        {
            get { return m_AABB; }
            private set { m_AABB = value; }
        }

        private Vector2 m_DrawLoc;
        /// <summary>
        /// AABB location.
        /// </summary>
        public Vector2 DrawLoc
        {
            get { return m_DrawLoc; }
            private set { m_DrawLoc = value; }
        }

        private Point m_Index;
        /// <summary>
        /// The index of the BGTile on the level.
        /// </summary>
        public Point Index
        {
            get { return m_Index; }
            private set { m_Index = value; }
        }

        public BGTile(Rectangle aabb)
        {
            RenderTarget = new RenderTarget2D(Engine.Instance.Graphics.GraphicsDevice, BGMgr.MAX_TEX_SIZE, BGMgr.MAX_TEX_SIZE, false, SurfaceFormat.Color,DepthFormat.None,4,RenderTargetUsage.PreserveContents);
            AABB = aabb;
            DrawLoc = new Vector2(AABB.X, AABB.Y);

            Index = new Point(aabb.X / BGMgr.MAX_TEX_SIZE, AABB.Y / BGMgr.MAX_TEX_SIZE);
        }

        /// <summary>
        /// Only draw the portion that's within the players camera rectangle.
        /// </summary>
        /// <param name="camRect"></param>
        public void Draw(Rectangle camRect)
        {
            Rectangle destRect;
            Rectangle.Intersect(ref camRect, ref m_AABB, out destRect);
            if (destRect != Rectangle.Empty)
            {
                Rectangle sourceRect = new Rectangle(destRect.X - AABB.X, destRect.Y - AABB.Y, destRect.Width, destRect.Height);
                Engine.Instance.SpriteBatch.Draw(RenderTarget, destRect, sourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            }
        }
    }

    /// <summary>
    /// Handles the background drawing
    /// Merges all BG-tiles into bigger tiles of 2048x2048 (BGTile class) which draws only the visible section (defined by the players camera).
    /// </summary>
    public class BGMgr
    {
        List<BGTile> BGTiles;
        internal const int MAX_TEX_SIZE = 2048;
        /// <summary>
        /// Contains the amount of BGTiles for the X-axis and Y-axis.
        /// </summary>
        Point BGTilesCnt;

        public BGMgr()
        {
        }

        /// <summary>
        /// Returns a texture2D for the minimap. It uses the RenderTarget2D from the MiniMap so it does not create a new one.
        /// </summary>
        /// <param name="renderTarget">The render target to draw the minimap BG onto.</param>
        /// <param name="minimapSize">The size of the minimap.</param>
        /// <param name="ratio">The ratio between the actual level size and the minimap size.</param>
        /// <returns></returns>
        public void RenderMiniMapBG(ref RenderTarget2D renderTarget, Point minimapSize, Vector2 ratio)
        {
            Engine.Instance.Graphics.GraphicsDevice.SetRenderTarget(renderTarget);
            Engine.Instance.SpriteBatch.Begin(); 
            foreach (BGTile bgTile in BGTiles)
                Engine.Instance.SpriteBatch.Draw(bgTile.RenderTarget, new Rectangle((int)(bgTile.AABB.X * ratio.X), (int)(bgTile.AABB.Y * ratio.Y), (int)(bgTile.AABB.Width * ratio.X), (int)(bgTile.AABB.Height * ratio.Y)), Color.White);
            Engine.Instance.SpriteBatch.End();
            Engine.Instance.Graphics.GraphicsDevice.SetRenderTarget(null);
        }

        /// <summary>
        /// Loads the background into the BGTiles rendertargets.
        /// </summary>
        /// <param name="bgMainNode"></param>
        public void Load(XElement bgMainNode)
        {
            // Get number of BGTiles per x and y
            BGTilesCnt = new Point((int)Math.Ceiling(Level.Instance.LevelSize.X / (float)MAX_TEX_SIZE), (int)Math.Ceiling(Level.Instance.LevelSize.Y / (float)MAX_TEX_SIZE));

            // Create the BGTiles
            BGTiles = new List<BGTile>();
            for (int y = 0; y < BGTilesCnt.Y; y++)
            {
                for (int x = 0; x < BGTilesCnt.X; x++)
                    BGTiles.Add(new BGTile(new Rectangle(x * MAX_TEX_SIZE, y * MAX_TEX_SIZE, MAX_TEX_SIZE, MAX_TEX_SIZE)));
            }

            #region Fill the BGTiles with a single tile if applicable
            string strfillSheet = bgMainNode.Attribute("fillTileSheet").Value;
            if (!string.IsNullOrEmpty(strfillSheet))
            {
                Texture2D fillTextureSheet = GraphicsLib.Str2TexFromStream(Engine.Instance.Graphics.GraphicsDevice, "TileSheets/"+strfillSheet);
                Rectangle fillSourceRect = Common.Str2Rectangle(bgMainNode.Attribute("fillTileSource").Value);
                Point tilesPerSheet = new Point((int)Math.Ceiling(MAX_TEX_SIZE / (float)fillSourceRect.Width), (int)Math.Ceiling(MAX_TEX_SIZE / (float)fillSourceRect.Height));

                RenderTarget2D fillTexRenderTarget = new RenderTarget2D(Engine.Instance.Graphics.GraphicsDevice, fillSourceRect.Width, fillSourceRect.Height);
                Engine.Instance.Graphics.GraphicsDevice.SetRenderTarget(fillTexRenderTarget);
                Engine.Instance.SpriteBatch.Begin();
                Engine.Instance.SpriteBatch.Draw(fillTextureSheet, Vector2.Zero, fillSourceRect, Color.White);
                Engine.Instance.SpriteBatch.End();

                foreach (BGTile bgTile in BGTiles)
                {
                    Engine.Instance.Graphics.GraphicsDevice.SetRenderTarget(bgTile.RenderTarget);
                    Engine.Instance.SpriteBatch.Begin();

                    for (int y = 0; y < tilesPerSheet.Y; y++)
                    {
                        for (int x = 0; x < tilesPerSheet.X; x++)
                            Engine.Instance.SpriteBatch.Draw(fillTexRenderTarget, new Vector2(x * fillSourceRect.Width, y * fillSourceRect.Height), Color.White);
                    }
                    Engine.Instance.SpriteBatch.End();
                }
                fillTexRenderTarget.Dispose();
                Engine.Instance.Graphics.GraphicsDevice.SetRenderTarget(null); // after this point the rendertarget must be recreated before it is ever used again.
                GC.Collect(); // Not sure if this forced collect collects the disposed rendertarget(s). Normally Dispose() releases resources immediately.
            }
            #endregion

            #region Load the actual tiles
            XElement tilesMainNode = bgMainNode.Element("Tiles");
            if (tilesMainNode != null)
            {
                foreach (XElement spriteSheetNode in tilesMainNode.Elements())
                {
                    // Load the texture sheet
                    Texture2D sheetTex = GraphicsLib.Str2TexFromStream(Engine.Instance.Graphics.GraphicsDevice, "TileSheets/" + spriteSheetNode.Attribute("name").Value);

                    // Loop all nodes in the xml (each node represents a tile)
                    foreach (XElement tileNode in spriteSheetNode.Elements())
                    {
                        Vector2 tileDestLoc = Common.Str2Vector(tileNode.Attribute("location").Value);
                        Rectangle tileSourceRect = Common.Str2Rectangle(tileNode.Attribute("sourceRectangle").Value);
                        Rectangle tileDestRectangle = new Rectangle((int)tileDestLoc.X, (int)tileDestLoc.Y, tileSourceRect.Width, tileSourceRect.Height);

                        foreach (BGTile bgTile in BGTiles)
                        {
                            if (tileDestRectangle.Intersects(bgTile.AABB))
                            {
                                Engine.Instance.Graphics.GraphicsDevice.SetRenderTarget(bgTile.RenderTarget);
                                Engine.Instance.SpriteBatch.Begin();
                                Engine.Instance.SpriteBatch.Draw(sheetTex, new Vector2(tileDestLoc.X - bgTile.AABB.X, tileDestLoc.Y - bgTile.AABB.Y), tileSourceRect, Color.White);
                                Engine.Instance.SpriteBatch.End();
                                Engine.Instance.Graphics.GraphicsDevice.SetRenderTarget(null);
                                break; // Unlike a sprite a tile can only be placed on a single BGTile. So break out to safe a little CPU.
                            }
                        }
                    }
                    GC.Collect(); // Not sure if this forced collect collects the disposed rendertarget(s). Normally Dispose() releases resources immediately.
                }
            }
            #endregion

            #region Load the objects (they may have any valid size defined by sourceRect) if applicable.
            XElement objectsMainNode = bgMainNode.Element("Objects");
            if (objectsMainNode != null)
            {
                RenderTarget2D objectRTarget = new RenderTarget2D(Engine.Instance.Graphics.GraphicsDevice, MAX_TEX_SIZE, MAX_TEX_SIZE);
                foreach (XElement objectNode in objectsMainNode.Elements())
                {
                    Texture2D objTextureSheet = GraphicsLib.Str2TexFromStream(Engine.Instance.Graphics.GraphicsDevice, "TileSheets/" + objectNode.Attribute("sheet").Value);
                    Rectangle objSourceRect = Common.Str2Rectangle(objectNode.Attribute("sourceRectangle").Value);
                    Vector2 objDestination = Common.Str2Vector(objectNode.Attribute("location").Value);
                    Rectangle objDestRect = new Rectangle(objDestination.Xi(), objDestination.Yi(), objSourceRect.Width, objSourceRect.Height);

                    foreach (BGTile bgTile in BGTiles)
                    {
                        if (bgTile.AABB.Intersects(objDestRect))
                        {
                            Engine.Instance.Graphics.GraphicsDevice.SetRenderTarget(bgTile.RenderTarget);
                            Engine.Instance.SpriteBatch.Begin();
                            Engine.Instance.SpriteBatch.Draw(objTextureSheet, objDestination - new Vector2(bgTile.DrawLoc.X * bgTile.Index.X, bgTile.DrawLoc.Y * bgTile.Index.Y), objSourceRect, Color.White);
                            Engine.Instance.SpriteBatch.End();
                        }
                    }
                    Engine.Instance.Graphics.GraphicsDevice.SetRenderTarget(null);
                }
                objectRTarget.Dispose();
                GC.Collect(); // Not sure if this forced collect collects the disposed rendertarget(s). Normally Dispose() releases resources immediately.
            }
            #endregion
        }

        /// <summary>
        /// Call this to draw the background.
        /// </summary>
        public void Draw()
        {
            Rectangle camRect = new Rectangle(Level.Instance.Player.Camera.Location.Xi(), Level.Instance.Player.Camera.Location.Yi(), Engine.Instance.Width, Engine.Instance.Height);
            foreach (BGTile bgTile in BGTiles)
                bgTile.Draw(camRect);
        }
    }
}