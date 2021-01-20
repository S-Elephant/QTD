using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALib;
using Microsoft.Xna.Framework.Storage;

namespace QTD
{
    public class Engine : IEngine
    {
        /// <summary>
        /// Singleton Instance
        /// </summary>
        public static Engine Instance;

        public StorageDevice StorageDevice;

        /// <summary>
        /// The stats that receives the Update() and Draw() calls
        /// </summary>
        private IActiveState m_ActiveState;
        public IActiveState ActiveState
        {
            get { return m_ActiveState; }
            set { m_ActiveState = value; }
        }

        private Game m_Game;
        public Game Game
        {
            get { return m_Game; }
            set { m_Game = value; }
        }

        private GraphicsDeviceManager m_Graphics;
        public GraphicsDeviceManager Graphics
        {
            get { return m_Graphics; }
            set { m_Graphics = value; }
        }

        /// <summary>
        /// Screen height in pixels
        /// </summary>
        public int Height
        {
            get { return Graphics.PreferredBackBufferHeight; }
        }

        public Rectangle SafeArea
        {
            get { return Graphics.GraphicsDevice.Viewport.TitleSafeArea; }
        }

        /// <summary>
        /// Screensize in pixels
        /// </summary>
        public Size ScreenSize { get { return new Size(Width, Height); } }

        private SpriteBatch m_SpriteBatch;
        public SpriteBatch SpriteBatch
        {
            get { return m_SpriteBatch; }
            set { m_SpriteBatch = value; }
        }

        /// <summary>
        /// Screen width in pixels
        /// </summary>
        public int Width
        {
            get { return Graphics.PreferredBackBufferWidth; }
        }

        private static int EntitityID = -1;
        public static int RetrieveNextEntityID()
        {
            EntitityID++;
            return EntitityID;
        }

        public bool MenuIsActive = true;
        public bool DrawDebug = false;

        /// <summary>
        /// To reduce garbage
        /// </summary>
        public static List<string> StaticStrNumbers;

        public Rectangle ScreenArea { get { return new Rectangle(0, 0, Width, Height); } }

        /// <summary>
        /// Audio manager
        /// </summary>
        //public AudioMgrPooled Audio = new AudioMgrPooled();
    }
}
