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
    public class Player
    {
        public Camera Camera;
        const int ScrollSpeed = 3;

        static readonly SpriteFont GUIFont = Common.str2Font("GUI");

        public enum eState { Normal, Building}
        public eState State;

        public TowerStruct BuildThisTower;

        const int GUIIconOffsetX = 180;

        #region Lives
        private int m_Lives;
        public int Lives
        {
            get { return m_Lives; }
            set
            {
                m_Lives = value;
                IsAlive = value > 0;
                LivesSB.Remove(0, LivesSB.Length);
                LivesSB.Append(value);
            }
        }

        private bool m_IsAlive;
        public bool IsAlive
        {
            get { return m_IsAlive; }
            private set { m_IsAlive = value; }
        }

        static readonly Vector2 LivesTextureLocation = new Vector2(GUIIconOffsetX+250, 2);
        static readonly Vector2 LivesLocation = new Vector2(GUIIconOffsetX+250 + 32 + 4, 3);
        static readonly Texture2D LivesTexture = Common.str2Tex("Gui/heart");
        StringBuilder LivesSB = new StringBuilder(4);
        #endregion

        #region Gold
        static Texture2D GoldIcon = Common.str2Tex("GUI/goldCoins");
        static readonly Vector2 GoldIconLocation = new Vector2(GUIIconOffsetX, 0);
        static readonly Vector2 GoldSBLocation = new Vector2(GUIIconOffsetX + 36, 0 + 3);
        private int m_Gold = 0;
        public int Gold
        {
            get { return m_Gold; }
            set
            {
                m_Gold = value;
                GoldSB.Remove(0, GoldSB.Length);
                GoldSB.Append(Gold);
            }
        }
        private StringBuilder m_GoldSB = new StringBuilder("0", 7);
        public StringBuilder GoldSB
        {
            get { return m_GoldSB; }
            private set { m_GoldSB = value; }
        }
        #endregion

        #region Food (Supply)
        static Texture2D SupplyIcon = Common.str2Tex("GUI/food");
        static readonly Vector2 SupplyIconLocation = new Vector2(GUIIconOffsetX+130, 0);
        static readonly Vector2 SupplySBLocation = new Vector2(GUIIconOffsetX+130 + 36, 0 + 3);
        private int m_Supply = 0;
        const string SUPPLY_SEP = " / ";
        public int Supply
        {
            get { return m_Supply; }
            set
            {
                m_Supply = value;
                UpdateSupplySB();
            }
        }
        private StringBuilder m_SupplySB = new StringBuilder("0/0", 7);
        public StringBuilder SupplySB
        {
            get { return m_SupplySB; }
            private set { m_SupplySB = value; }
        }
        Color SupplyDrawColor;
        #endregion
       
        public int Kills;

        private int m_MaxSupply;
        public int MaxSupply
        {
            get { return m_MaxSupply; }
            set
            {
                m_MaxSupply = value;
                UpdateSupplySB();
            }
        }
       

        public Player()
        {
            Camera = new Camera();
        }

        public void ResetForNewLevel(int lives)
        {
            State = eState.Normal;
            Supply = MaxSupply = 0;
            Lives = lives;
        }

        private void UpdateSupplySB()
        {
            SupplySB.Remove(0, SupplySB.Length);
            SupplySB.Append(Supply);
            SupplySB.Append(SUPPLY_SEP);
            SupplySB.Append(MaxSupply);

            if (Supply < MaxSupply)
                SupplyDrawColor = Color.BurlyWood;
            else
                SupplyDrawColor = Color.Red;
        }

        public bool IsSupplyPossible(int supplyCost, int supplyGiveLost)
        {
            return (Supply + supplyCost) <= (MaxSupply-supplyGiveLost);
        }

        public void Update(GameTime gameTime)
        {
            #region Camera
            Vector2 camAdjust = Vector2.Zero;
            if (InputMgr.Instance.Keyboard.State.IsKeyDown(Keys.Up))
                camAdjust += new Vector2(0, -ScrollSpeed);
            if (InputMgr.Instance.Keyboard.State.IsKeyDown(Keys.Right))
                camAdjust += new Vector2(ScrollSpeed, 0);
            if (InputMgr.Instance.Keyboard.State.IsKeyDown(Keys.Down))
                camAdjust += new Vector2(0, ScrollSpeed);
            if (InputMgr.Instance.Keyboard.State.IsKeyDown(Keys.Left))
                camAdjust += new Vector2(-ScrollSpeed, 0);

            if (camAdjust != Vector2.Zero)
            {
                Camera.Location += camAdjust;
                AdjustCameraToLvlBoundary();
            }
            #endregion
        }

        public void AdjustCameraToLvlBoundary()
        {
            if (Camera.Location.X < 0)
                Camera.Location = new Vector2(0, Camera.Location.Y);
            if (Camera.Location.Y < 0)
                Camera.Location = new Vector2(Camera.Location.X, 0);
            if (Camera.Location.X + Engine.Instance.Width > Level.Instance.LevelSize.X)
                Camera.Location = new Vector2(Level.Instance.LevelSize.X - Engine.Instance.Width, Camera.Location.Y);
            if (Camera.Location.Y + Engine.Instance.Height > Level.Instance.LevelSize.Y)
                Camera.Location = new Vector2(Camera.Location.X, Level.Instance.LevelSize.Y - Engine.Instance.Height);
        }

        public void DrawGUI()
        {
            Engine.Instance.SpriteBatch.Draw(GoldIcon, GoldIconLocation, Color.White);
            Engine.Instance.SpriteBatch.DrawString(GUIFont, GoldSB, GoldSBLocation, Color.Goldenrod);

            Engine.Instance.SpriteBatch.Draw(SupplyIcon, SupplyIconLocation, Color.White);
            Engine.Instance.SpriteBatch.DrawString(GUIFont, SupplySB, SupplySBLocation, SupplyDrawColor);

            Engine.Instance.SpriteBatch.Draw(LivesTexture, LivesTextureLocation, Color.White);
            Engine.Instance.SpriteBatch.DrawString(GUIFont, LivesSB, LivesLocation, Color.White);
        }
    }
}
