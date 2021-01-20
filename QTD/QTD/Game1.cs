using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using XNALib;
using System.IO;

namespace QTD
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Engine.Instance = new Engine();
            Engine.Instance.Game = this;
            Engine.Instance.Graphics = graphics;
            Global.Content = Content;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            #region Resolution
            Resolution.Init(ref graphics);
#if WINDOWS
            Resolution.SetResolution(1280, 800, false);
            Resolution.SetVirtualResolution(1280, 800); // Don't change this anymore after this. Use SetResolution() instead.
#endif
#if XBOX
            Resolution.SetResolution(1280, 720, true);
            Resolution.SetVirtualResolution(1280, 720); // Don't change this anymore after this. Use SetResolution() instead.
#endif
            #endregion

            Engine.Instance.SpriteBatch = spriteBatch;
            InputMgr.Instance = new InputMgr();
            InputMgr.Instance.Mouse = new Mouse2("Mouse/mouse01");
            XNALib.Controls.ControlMgr.Instance = new XNALib.Controls.ControlMgr(spriteBatch);

            // Audio
            AudioMgrPooled.Instance = new AudioMgrPooled();
            AudioMgrPooled.Instance.AddSound(5, "sellBuy");
            AudioMgrPooled.Instance.AddSound(10, "buildingSelected");
            AudioMgrPooled.Instance.AddSound(10, "buildingUpgraded");
            AudioMgrPooled.Instance.AddSound(10, "mouseClick");
            AudioMgrPooled.Instance.AddSound(5, "error1");
            AudioMgrPooled.Instance.AddSound(5, "explosion01");
            AudioMgrPooled.Instance.AddSound(5, "explosion02");
            AudioMgrPooled.Instance.AddSound(5, "cannonShootFX1");

            // Settings
            SettingsMgr.Instance = new SettingsMgr();
            SettingsMgr.Instance.Load();

            // Data
            DataStructs.LoadDefenders();
            DataStructs.LoadTowers();
            DataStructs.LoadRunners();

            // Cache folder
            if (!Directory.Exists("Cache"))
                Directory.CreateDirectory("Cache");

            // First state
            Level.Instance = new Level();
            Level.Instance.Load("L001");
            Engine.Instance.ActiveState = Level.Instance;          
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            InputMgr.Instance.Update(gameTime);
            AudioMgrPooled.Instance.Update(gameTime);

            Engine.Instance.ActiveState.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Resolution.GetScaleMatrix() * Level.Instance.Player.Camera.CamMatrix);
            Engine.Instance.ActiveState.Draw();
            spriteBatch.End();

            spriteBatch.Begin();
            if (Engine.Instance.ActiveState is Level)
                Level.Instance.DrawGUI();
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
