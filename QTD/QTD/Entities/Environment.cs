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
    public class Environment : IEntity
    {
        #region Interface
        private int m_EntityID;
        public int EntityID
        {
            get { return m_EntityID; }
            private set { m_EntityID = value; }
        }

        public eEntityType EntityType { get { return eEntityType.Environment; } }

        private Rectangle m_AABB;
        public Rectangle AABB
        {
            get { return m_AABB; }
            private set { m_AABB = value; }
        }

        private Vector2 m_FeetLoc;
        public Vector2 FeetLoc
        {
            get { return m_FeetLoc; }
            private set { m_FeetLoc = value; }
        }
        #endregion

        SimpleASprite Animation;

        public Environment(eAnimation animation, Vector2 location)
        {
            EntityID = Engine.RetrieveNextEntityID();
            Animation = AnimationFactory.Create(animation, out m_AABB);
            Animation.Location = location;
            Animation.RandomizeStartFrame();
            FeetLoc = location + new Vector2(AABB.X + AABB.Width / 2, AABB.Bottom);
        }

        public void Update(GameTime gameTime)
        {
            Animation.Update(gameTime);
        }

        public void Draw()
        {
            Animation.Draw(Engine.Instance.SpriteBatch, Vector2.Zero, 1f - FeetLoc.Y / Utils.DEPTH_DIVIDER);
        }
    }
}