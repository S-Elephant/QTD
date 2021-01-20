using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNALib;
using System.Xml.Linq;
using XNALib.Controls;

namespace QTD
{
    public class Level : IActiveState
    {
        public static Level Instance;

        #region Members
        const int MAX_RUNNERS = 200;
        const int MAX_ACTIVE_DEFENDERS = 200;
        const int MAX_TOWERS = 256;
        const int MAX_HUTS = 256;

        #region Runner Progress
        Bar RunnerProgressBar = new Bar(Common.White1px, new Rectangle(Engine.Instance.Width / 2 - 64, 10, 128, 16), Bar.eDirection.Horizontal);
        readonly Rectangle RunnerProgressBarBG = new Rectangle(Engine.Instance.Width / 2 - 64 - 2, 10 - 2, 132, 20); // Don't make static because the resolution must be set before this one is set.
        StringBuilder RunnerProgress = new StringBuilder("0%", 4);
        const string PERC_STR = "%";
        readonly Vector2 RunnerProgressLoc = new Vector2((Engine.Instance.Width / 2) - 12, 9); // Don't make static because the resolution must be set before this one is set.
        static readonly SpriteFont RunnerProgressFont = Common.str2Font("RunnerProgress");
        #endregion
        #region Wave Progress
        Bar WaveProgressBar = new Bar("GUI/bar01_Empty", "GUI/bar01_WaveProgress", new Rectangle(0, Engine.Instance.Height - MiniMap.Size.Y - 16, 256, 16), Bar.eDirection.Horizontal);
        StringBuilder WaveProgress = new StringBuilder("0%", 4);
        readonly Vector2 WaveProgressLoc = new Vector2(128 - 12, Engine.Instance.Height - MiniMap.Size.Y - 16 + 1); // Don't make static because the resolution must be set before this one is set.
        #endregion

        static readonly SpriteFont DebugFont = Common.str2Font("Debug");

        MiniMap MiniMap;
        InfoPanel InfoPanel = new InfoPanel();

        StringBuilder LevelDisplayName;
        public List<BaseRunner> ActiveRunners = new List<BaseRunner>(MAX_RUNNERS);
        /// <summary>
        /// Includes dead defenders.
        /// </summary>
        public List<BaseDefender> SpawnedDeadDefenders;
        public List<BaseDefender> SpawnedAliveDefenders;
        BaseTower testTower;
        public BaseTower testSpawnTower;
        public List<BaseProjectile> Projectiles = new List<BaseProjectile>();
        public List<WayPoint> WayPoints = new List<WayPoint>();
        public Player Player;
        public List<BaseTower> Towers;

        const string PLUS_STR = "+";

        private Point m_LevelSize;
        public Point LevelSize
        {
            get { return m_LevelSize; }
            private set { m_LevelSize = value; }
        }

        public enum eState { Playing, Paused, PreSpawn }
        public eState State = eState.Playing;

        int TotalWaves;
       
        public BaseTower TowerBeneathMouse;
        public BaseTower SelectedTower;
        Pool<HeadUpText> HeadUpTextPool = new Pool<HeadUpText>(25, true, hut => !hut.IsDisposed, () => HeadUpText.PoolInitialize());
        public List<HeadUpText> HeadUpTexts = new List<HeadUpText>(MAX_HUTS);

        Button NextWaveButton, MenuButton, MMButton, SellBtn;
        List<Button> CategoryButtons = new List<Button>();
        const int MAX_TOWERS_PER_CAT = 24;
        List<Button> TowerBuildButtons = new List<Button>(MAX_TOWERS_PER_CAT);
        readonly Vector2 TowerBuildBtnsLoc = new Vector2(MiniMap.Size.X + 32, Engine.Instance.Height - 50); // Do not make static

        public BuildGrid BuildGrid;
        private bool PlaceTowerWaitOneCycle;

        public BGMgr BGMgr;

        public List<Environment> Environments;

#warning pool Visuals
        public List<Visual> Visuals = new List<Visual>(128);

        List<int> BuildableTowers;

        #region Waves
        /// <summary>
        /// Contains all waves for this level including those that have not yet started spawning.
        /// </summary>
        public List<Wave> Waves;
        int CurrentWaveNr;
        SimpleTimer WaveStartDelayTimer = new SimpleTimer(1000);
        Bar WaveSpawnBar = new Bar(Common.White1px, new Rectangle(Engine.Instance.Width - 160, 10, 128, 16), Bar.eDirection.Horizontal) { DrawColor = Color.Brown };
        readonly Rectangle WaveSpawnBarBG = new Rectangle(Engine.Instance.Width - 162, 8, 132, 20);
        #endregion
        #endregion

        public Level()
        {
            ControlMgr.Instance.Controls.Clear();
            WarningMessages.Instance = new WarningMessages();

            #region Buttons
            // Next Wave Button
            NextWaveButton = new Button(new Vector2(Engine.Instance.Width - 50, 50), "GUI/nextWave", "GUI/nextWaveHover", null);
            ControlMgr.Instance.Controls.Add(NextWaveButton);
            NextWaveButton.Click += new Button.OnClick(NextWaveButton_Click);

            // Menu Button
            MenuButton = new Button(Vector2.Zero, "GUI/btnMenu", "GUI/btnMenuDown", null);
            ControlMgr.Instance.Controls.Add(MenuButton);
            MenuButton.Click += new Button.OnClick(MenuButton_Click);

            //MM Button
            MMButton = new Button(new Vector2(MenuButton.AABB.Width, 0), "GUI/btnMM", "GUI/btnMMDown", null);
            ControlMgr.Instance.Controls.Add(MMButton);
            MMButton.Click += new Button.OnClick(MMButton_Click);

            // SellBtn
            SellBtn = new Button(new Vector2(Engine.Instance.Width - 150, 0), "GUI/btnRedEmpty", "GUI/btnRedEmptyHover", null) { OverlayTexture = Common.str2Tex("GUI/sell"), IsEntirelyDisabled = true };
            ControlMgr.Instance.Controls.Add(SellBtn);
            SellBtn.Click += new Button.OnClick(SellBtn_Click);
            #endregion

            Player = new Player();

            #region Categories
            // Tower categories
            DataStructs.LoadTowerCategories();

            #region Tower build category buttons
            Vector2 TowerCatBtnsLoc = TowerBuildBtnsLoc;
            foreach (TowerCategoryStruct tcs in DataStructs.TowerCategories)
            {
                Button newTCBtn = new Button(TowerCatBtnsLoc, "GUI/btnEmpty", "GUI/btnEmptyHover", null) { OverlayTexture = tcs.Icon, Tag = tcs };
                if (tcs.TowersInThisCat.Count > 0)
                {
                    TowerCatBtnsLoc.X += 50;
                    CategoryButtons.Add(newTCBtn);
                    newTCBtn.Click += new Button.OnClick(newTCBtn_Click);
                    ControlMgr.Instance.Controls.Add(newTCBtn);
                }
            }
            #endregion
            #region Tower build tower buttons
            Vector2 towerBuildBtnsLoc = TowerBuildBtnsLoc;
            foreach (TowerStruct ts in DataStructs.Towers)
            {
                Button newTBBtn = new Button(towerBuildBtnsLoc, "GUI/btnEmpty", "GUI/btnEmptyHover", null) { OverlayTexture = ts.Icon, Tag = ts, IsEntirelyDisabled = true };
                TowerBuildButtons.Add(newTBBtn);
                towerBuildBtnsLoc.X += 50;
                newTBBtn.Click += new Button.OnClick(newTBBtn_Click);
                ControlMgr.Instance.Controls.Add(newTBBtn);
            }
            #endregion
            #endregion
        }

        #region Build Buttons
        void ReturnToCats()
        {
            foreach (Button btn in TowerBuildButtons)
                btn.IsEntirelyDisabled = true;
            foreach (Button btn in CategoryButtons)
            {
                if(!((bool)btn.Tag2))
                    btn.IsEntirelyDisabled = false;
            }
        }

        void newTBBtn_Click(Button button)
        {
            TowerStruct ts = (TowerStruct)button.Tag;

            ReturnToCats();

            if (ts.Cost <= Player.Gold)
            {
                if (Player.IsSupplyPossible(ts.SupplyCost, 0))
                {
                    Player.Gold -= ts.Cost;
                    Player.BuildThisTower = ts;
                    Player.State = QTD.Player.eState.Building;
                    PlaceTowerWaitOneCycle = true;
                }
                else
                {
                    WarningMessages.Instance.AddWarning(1);
                    AudioMgrPooled.Instance.PlaySound(AudioConstants.Error);
                }
            }
            else
            {
                WarningMessages.Instance.AddWarning(0);
                AudioMgrPooled.Instance.PlaySound(AudioConstants.Error);
            }
        }

        /// <summary>
        /// Click event for tower category buttons
        /// </summary>
        /// <param name="button"></param>
        void newTCBtn_Click(Button button)
        {
            TowerCategoryStruct tcs = (TowerCategoryStruct)button.Tag;
            foreach (Button btn in CategoryButtons)
                btn.IsEntirelyDisabled = true;
            foreach (Button btn in TowerBuildButtons)
            {
                TowerStruct ts = (TowerStruct)btn.Tag;
                if (tcs.TowersInThisCat.Contains(ts) && BuildableTowers.Contains(ts.ID))
                {
                    btn.IsEntirelyDisabled = false;

                    if (ts.Requirements.Count > 0)
                    {
                        // Check if the player meets the tower requirements (excluding gold and supply)
                        bool requirementsAreMet = false;
                        for (int i = 0; i < ts.Requirements.Count; i++) // OR-loop
                        {
                            bool andReqMet = true;
                            for (int j = 0; j < ts.Requirements[i].Count; j++) // AND-loop
                            {
                                if (Towers.Find(t => t.ID == ts.Requirements[i][j]) == null)
                                    andReqMet = false;
                            }

                            if (andReqMet)
                            {
                                requirementsAreMet = true;
                                break;
                            }
                        }
                        btn.IsEnabled = requirementsAreMet;
                    }
                }
            }
        }

        #endregion

        void SellBtn_Click(Button button)
        {
            SelectedTower.Sell();
            SellBtn.IsEntirelyDisabled = true;
            SelectedTower = null;
        }

        void MMButton_Click(Button button)
        {
            MiniMap.IsHidden = !MiniMap.IsHidden;
        }

        void MenuButton_Click(Button button)
        {
            throw new NotImplementedException();
        }

        ~Level()
        {
            NextWaveButton.Click -= new Button.OnClick(NextWaveButton_Click);
            MenuButton.Click -= new Button.OnClick(MenuButton_Click);
            MMButton.Click -= new Button.OnClick(MMButton_Click);
            SellBtn.Click -= new Button.OnClick(SellBtn_Click);

            foreach (Button btn in CategoryButtons)
                btn.Click -= new Button.OnClick(newTCBtn_Click);

            foreach (Button btn in TowerBuildButtons)
                btn.Click -= new Button.OnClick(newTBBtn_Click);
        }

        void NextWaveButton_Click(Button button)
        {
            SpawnNextWave();
        }

        public void Load(string xmlName)
        {
            // Resets
            SpawnedDeadDefenders = new List<BaseDefender>(MAX_ACTIVE_DEFENDERS);
            SpawnedAliveDefenders = new List<BaseDefender>(MAX_ACTIVE_DEFENDERS);
            Towers = new List<BaseTower>(MAX_TOWERS);
            TowerBeneathMouse = SelectedTower = null;
            NextWaveButton.IsEnabled = true;
            Player.ResetForNewLevel(10);
            WaveProgressBar.Percentage = 0;
            State = eState.PreSpawn;
            Environments = new List<Environment>();

            XDocument doc = XDocument.Load("Data/Levels/" + xmlName + ".xml");

            #region Level Info
            XElement levelInfoNode = doc.Root.Element("LevelInfo");
            if (levelInfoNode == null)
                throw new NullReferenceException("LevelInfo node not found. " + xmlName);

            // Level display name
            LevelDisplayName = new StringBuilder(levelInfoNode.Element("Name").Value);
            // Start Gold
            Player.Gold = int.Parse(levelInfoNode.Element("StartGold").Value);
            // Level Size
            LevelSize = Common.Str2Point(levelInfoNode.Element("LevelSize").Value);
            if (LevelSize.X < 1280 || LevelSize.Y < 800)
                throw new Exception("Level size must be at least 1280x800. Otherwise there would be a black border around the level plus the camera can not stay in such a small boundary.");

            // Broadphase
            BroadPhase.Instance = new BroadPhase(int.Parse(levelInfoNode.Element("CollisionGridSize").Value), LevelSize);
            BroadPhase.Instance.Init();
            #endregion

            #region Load Waypoints
            XElement wpMainNode = doc.Root.Element("Waypoints");
            if (wpMainNode == null)
                throw new NullReferenceException("Waypoints node missing. " + xmlName);

            WayPoint.Spread = int.Parse(wpMainNode.Attribute("spread").Value);

            WayPoint.StartPoints = new List<WayPoint>();
            foreach (XElement wp in wpMainNode.Elements())
            {
                List<int> nextWpIds = new List<int>();
                XElement nextWPsNode = wp.Element("NextWaypoints");
                if (nextWPsNode != null)
                {
                    foreach (XElement nextwpID in nextWPsNode.Elements())
                        nextWpIds.Add(int.Parse(nextwpID.Value));
                }

                // Start & Finish
                bool isStart = false;
                if (wp.Attribute("isStart") != null)
                    isStart = bool.Parse(wp.Attribute("isStart").Value);
                bool isFinish = false;
                if (wp.Attribute("isFinish") != null)
                    isFinish = bool.Parse(wp.Attribute("isFinish").Value);

                WayPoints.Add(new WayPoint(int.Parse(wp.Attribute("id").Value), Common.Str2Vector(wp.Attribute("location").Value), isStart, isFinish, nextWpIds.ToArray()));
            }

            WayPoints.ForEach(w => w.Initialize());
            WayPoint.CalculateTotalRoutelength();
            #endregion

            // Waves after waypoints.
            CurrentWaveNr = 0;
            #region Waves
            Waves = new List<Wave>();

            XElement wavesmainNode = doc.Root.Element("Waves");
            if (wavesmainNode == null)
                throw new NullReferenceException("Node Waves missing." + xmlName);

            foreach (XElement waveNode in wavesmainNode.Elements())
            {
                Wave newWave = new Wave(int.Parse(waveNode.Attribute("nr").Value), int.Parse(waveNode.Attribute("spawnDelay").Value));
                newWave.TimeUntilNextWaveInMS = int.Parse(waveNode.Attribute("timeUntilNextWave").Value);

                foreach (XElement startWPNode in waveNode.Elements())
                {
                    List<BaseRunner> runners = new List<BaseRunner>();
                    int startWPID = int.Parse(startWPNode.Attribute("id").Value);
                    foreach (XElement runnerNode in startWPNode.Elements())
                    {
                        int runnerID = int.Parse(runnerNode.Attribute("id").Value);
                        int amount = int.Parse(runnerNode.Attribute("amount").Value);

                        for (int i = 0; i < amount; i++)
                        {
                            BaseRunner newRunner = new BaseRunner();
                            newRunner.Initialize(runnerID);
                            newRunner.SetLocation(WayPoints.Find(w => w.ID == startWPID));
                            runners.Add(newRunner);
                        }
                    }
                    newWave.WaveRunners.Add(new WaveSpawnHelper(startWPID, runners.ToArray()));
                }
                Waves.Add(newWave);
            }
            TotalWaves = Waves.Count;
            #endregion

            #region BuildGrid
            BuildGrid = new BuildGrid();
            XElement unBuildablesNode = doc.Root.Element("UnBuildables");
            if (unBuildablesNode != null)
                BuildGrid.SetUnBuildables(unBuildablesNode.Value);
            #endregion

            #region BackGround (before minimap region)
            BGMgr = new BGMgr();
            XElement bgMainNode = doc.Root.Element("BackGround");
            if (bgMainNode != null)
            {
                BGMgr.Load(bgMainNode);
            }
            #endregion

            #region MiniMap
            MiniMap = new MiniMap(new Vector2(0, Engine.Instance.Height - MiniMap.Size.Y),LevelSize);
            MiniMap.PreRender();
            #endregion

            #region Initial towers
            XElement initialTowersMainNode = doc.Root.Element("InitialTowers");
            if (initialTowersMainNode != null)
            {
                foreach (XElement initialTowerNode in initialTowersMainNode.Elements())
                {
                    BaseTower bt = new BaseTower();
                    bt.Initialize(int.Parse(initialTowerNode.Element("ID").Value));
                    bt.SetLocation(Common.Str2Vector(initialTowerNode.Element("GridIdx").Value) * BuildGrid.GRID_SIZE);
                    AddTower(bt);
                }
            }
            #endregion

            #region Buildable Towers
            BuildableTowers = new List<int>();
            XElement buildableTowersMainNode = doc.Root.Element("BuildableTowers");
            if (BuildableTowers != null)
            {
                if (buildableTowersMainNode.Attribute("type").Value == "restricted")
                {
                    // Add all towers
                    for (int i = 0; i < DataStructs.Towers.Count; i++)
                        BuildableTowers.Add(DataStructs.Towers[i].ID);

                    // Remove restricted towers
                    foreach (XElement restrictedTowerNode in buildableTowersMainNode.Elements())
                        BuildableTowers.Remove(int.Parse(restrictedTowerNode.Value));
                }
                else
                {
                    // Add allowed towers
                    foreach (XElement restrictedTowerNode in buildableTowersMainNode.Elements())
                        BuildableTowers.Add(int.Parse(restrictedTowerNode.Value));
                }
            }
            else
            {
                // Add all towers
                for (int i = 0; i < DataStructs.Towers.Count; i++)
                    BuildableTowers.Add(DataStructs.Towers[i].ID);
            }
            #endregion

            #region Perma Disable CategoryButtons when they have no towers at all (place this code after region: Buildable Towers)
            foreach (Button btn in CategoryButtons)
            {
                TowerCategoryStruct tcs = (TowerCategoryStruct)btn.Tag;
                bool hasNoTowers = true;
                for (int i = 0; i < tcs.TowersInThisCat.Count; i++)
                {
                    if (BuildableTowers.Contains(tcs.TowersInThisCat[i].ID))
                    {
                        hasNoTowers = false;
                        break;
                    }
                }
                btn.Tag2 = hasNoTowers;
                btn.IsEntirelyDisabled = hasNoTowers;
            }
            #endregion

            #region Environments
            XElement environmentMainNode = doc.Root.Element("Environments");
            if (environmentMainNode != null)
            {
                foreach (XElement environmentNode in environmentMainNode.Elements())
                    Environments.Add(new Environment((eAnimation)Enum.Parse(typeof(eAnimation), environmentNode.Element("Type").Value), Common.Str2Vector(environmentNode.Element("Location").Value)));
            }
            #endregion

            // GC
            GC.Collect();
        }

        void SpawnNextWave()
        {
            if (CurrentWaveNr + 1 <= Waves.Count)
            {
                CurrentWaveNr++;
                Waves[CurrentWaveNr - 1].StartSpawn();
                WaveStartDelayTimer.Reset(Waves[CurrentWaveNr - 1].TimeUntilNextWaveInMS);

                // Bar
                WaveProgressBar.Percentage = (CurrentWaveNr / (float)TotalWaves) * 100;
                WaveProgress.Remove(0, WaveProgress.Length);
                WaveProgress.Append(WaveProgressBar.Percentage);
                WaveProgress.Append(PERC_STR);

                // Set state if applicable
                if(State == eState.PreSpawn)
                    State = eState.Playing;

                // Income per wave
                foreach (BaseTower t in Towers)
                {
                    Player.Gold += t.IncomePerWave;
                    Level.Instance.AddHut(t.CenterLoc, t.IncomePerWave, false);
                }
            }
            else // There are no more waves to spawn.
            {
                NextWaveButton.IsEnabled = false;
                if (ActiveRunners.Count == 0)
                {
#warning todo: level victory here
                }
            }
        }

        public void AddTower(BaseTower t)
        {
            Towers.Add(t);
            BroadPhase.Instance.AddEntity(t);
        }

        public void RemoveTower(BaseTower t)
        {
            Towers.Remove(t);
            BroadPhase.Instance.RemoveEntity(t);
        }

        Vector2 MouseWorldLocThisCycle;
        public void Update(GameTime gameTime)
        {
            #region First things to do
            MouseWorldLocThisCycle = InputMgr.Instance.Mouse.Location + Player.Camera.Location; // This line comes first in the Update().
            BaseRunner.RunnerNearestToFinish = null;
            ControlMgr.Instance.Update(gameTime);
            #endregion

            if (State != eState.Paused)
            {
                #region Collision
                BroadPhase.Instance.ClearEntities();
                foreach (BaseRunner r in ActiveRunners)
                    BroadPhase.Instance.AddEntity(r);
                foreach (BaseDefender d in SpawnedAliveDefenders)
                    BroadPhase.Instance.AddEntity(d);
                #endregion

                // Environements
                foreach (Environment e in Environments)
                    e.Update(gameTime);

                // Update runners
                foreach (BaseRunner r in ActiveRunners)
                    r.Update(gameTime);

                // Update towers (and defenders)
                foreach (BaseTower tower in Towers)
                    tower.Update(gameTime);

                // Visuals
                for (int i = 0; i < Visuals.Count; i++)
                {
                    Visuals[i].Update(gameTime);
                    if (Visuals[i].IsDisposed)
                    {
                        Visuals.RemoveAt(i);
                        i--;
                    }
                }

                // Update global rallypoint animation.
                BaseTower.RallyPointAni.Update(gameTime);

                // Update projectiles
                foreach (BaseProjectile p in Projectiles)
                    p.Update(gameTime);
                Projectiles.RemoveAll(p => p.IsDisposed);

                // Update player
                Player.Update(gameTime);

                #region Update waves
                if (State != eState.PreSpawn)
                {
                    for (int i = 0; i < Waves.Count; i++)
                        Waves[i].Update(gameTime);
                    WaveStartDelayTimer.Update(gameTime);
                    if (WaveStartDelayTimer.IsDone) // The WaveStartDelayTimer is reset in the SpawnNextWave() procedure.
                        SpawnNextWave();
                }
                #endregion

                #region MiniMap
                bool MouseIsOverMiniMap = false;
                if (!MiniMap.IsHidden && Collision.PointIsInRect(InputMgr.Instance.Mouse.Location, MiniMap.DrawRect))
                {
                    MouseIsOverMiniMap = true;

                    if (InputMgr.Instance.Mouse.LeftButtonIsDown)
                    {
                        Vector2 MiniMapToWorldLoc = MiniMap.LocationToWorld(InputMgr.Instance.Mouse.Location);
                        Player.Camera.Location = MiniMapToWorldLoc - new Vector2(Engine.Instance.Width / 2, Engine.Instance.Height / 2);
                        Player.AdjustCameraToLvlBoundary();
                    }
                }
                #endregion

                if (!MouseIsOverMiniMap && Player.State != QTD.Player.eState.Building)
                {
                    #region Get tower beneath mouse
                    if (TowerBeneathMouse != null && (SelectedTower == null || (SelectedTower != TowerBeneathMouse)))
                        TowerBeneathMouse.DrawColor = Color.White;
                    TowerBeneathMouse = null;
                    foreach (BaseTower tower in Towers)
                    {
                        if (Collision.PointIsInRect(MouseWorldLocThisCycle, tower.AABB))
                        {
                            if ((TowerBeneathMouse == null || TowerBeneathMouse.AABB.Bottom > tower.AABB.Bottom) && (tower != SelectedTower)) // When there was already a tower beneath the mouse then take the one that has the lowest Y-value for the AABB.Bottom.
                            {
                                TowerBeneathMouse = tower;
                                TowerBeneathMouse.DrawColor = Color.Gray;
                            }
                        }
                    }
                    #endregion

                    #region Get Clicked Tower
                    if (InputMgr.Instance.Mouse.LeftButtonIsPressed)
                    {
                        if (SelectedTower != null)
                        {
                            SelectedTower.DrawColor = Color.White;
                            SelectedTower.HasFocus = false;
                        }                            
                        
                        SelectedTower = TowerBeneathMouse;
                        if (SelectedTower != null)
                        {
                            SelectedTower.DrawColor = Color.Wheat;
                            SelectedTower.HasFocus = true;
                            AudioMgrPooled.Instance.PlaySound(AudioConstants.MouseClick);
                        }

                        #region SellBtn
                        SellBtn.IsEntirelyDisabled = SelectedTower == null;
                        #endregion
                    }
                    #endregion

                    #region SetRallyPoint if applicable
                    if (SelectedTower != null && InputMgr.Instance.Mouse.RightButtonIsPressed)
                    {
                        SelectedTower.SetRallyPoint(MouseWorldLocThisCycle);
                    }
                    #endregion
                }

                // Allow to cancel a category / tower placement
                if (InputMgr.Instance.Keyboard.IsPressed(Keys.Escape))
                {
                    if (Player.State == QTD.Player.eState.Building)
                    {
                        Player.Gold += Player.BuildThisTower.Cost;
                        Player.State = QTD.Player.eState.Normal;
                    }
                    ReturnToCats();
                }

                #region BuildGrid & Building a new tower
                if (Player.State == QTD.Player.eState.Building)
                {
                    BuildGrid.Update(MouseWorldLocThisCycle, Player.BuildThisTower.BuildSize);
                    if (!PlaceTowerWaitOneCycle)
                    {
                        if (BuildGrid.CanBuild && InputMgr.Instance.Mouse.LeftButtonIsPressed)
                        {
                            Player.State = QTD.Player.eState.Normal;
                            Player.Gold -= Player.BuildThisTower.Cost;
                            BaseTower t = new BaseTower();
                            t.Initialize(Player.BuildThisTower.ID);
                            t.SetLocation(BuildGrid.BuildRect.Location.ToVector2());
                            AddTower(t);
                        }
                    }
                    else
                        PlaceTowerWaitOneCycle = false;
                }
                #endregion

                #region Percentage Bar
                int runnerPercToFinish = 0;
                if (BaseRunner.RunnerNearestToFinish != null)
                {
                    RunnerProgressBar.Percentage = BaseRunner.RunnerNearestToFinish.PercentageToFinish;
                    runnerPercToFinish = (int)BaseRunner.RunnerNearestToFinish.PercentageToFinish;
                }
                else
                    RunnerProgressBar.Percentage = 0;
                RunnerProgress.Remove(0, RunnerProgress.Length);
                RunnerProgress.Append(runnerPercToFinish);
                RunnerProgress.Append(PERC_STR);
                #endregion

                WaveSpawnBar.Percentage = WaveStartDelayTimer.PercentageComplete;

                #region huts
                HeadUpTextPool.CleanUp();
                for (int i = 0; i < HeadUpTexts.Count; i++)
                {
                    HeadUpTexts[i].Update(gameTime);
                    if (HeadUpTexts[i].IsDisposed)
                    {
                        HeadUpTexts.RemoveAt(i);
                        i--;
                    }
                }
                #endregion

                #region Button info panel
#warning todo: optimize this shit. cannot loop each update through all buttons...
                InfoPanel.ClearText();
                foreach (Button btn in CategoryButtons)
                {
                    if (!btn.IsEntirelyDisabled)
                    {
                        if (btn.HasFocus)
                            InfoPanel.SetText(((TowerCategoryStruct)btn.Tag).Info);
                    }
                }
                foreach (Button btn in TowerBuildButtons)
                {
                    if (!btn.IsEntirelyDisabled)
                    {
                        if (btn.HasFocus)
                            InfoPanel.SetText(((TowerStruct)btn.Tag).Info);
                    }
                }
                #endregion

                WarningMessages.Instance.Update(gameTime);

                // Game over check
                if (!Player.IsAlive)
                {
#warning todo: gameover here
                }
            }
        }

        public void AddHut(Vector2 centerLoc, int value, bool isCritical)
        {
            HeadUpText hut = HeadUpTextPool.New();
            StringBuilder sb = new StringBuilder(7);
            if (!isCritical)
                sb.Append(PLUS_STR);
            sb.Append(value);
            hut.Initialize(sb, centerLoc, isCritical);
            HeadUpTexts.Add(hut);
        }

        public void DrawGUI()
        {
            #region Runner Progress
            Engine.Instance.SpriteBatch.Draw(Common.White1px, RunnerProgressBarBG, Color.DarkGray);

            RunnerProgressBar.DrawColor = Color.Green;
            if (RunnerProgressBar.Percentage > 90)
                RunnerProgressBar.DrawColor = Color.Red;
            else if (RunnerProgressBar.Percentage > 75)
                RunnerProgressBar.DrawColor = Color.Orange;
            else if (RunnerProgressBar.Percentage > 50)
                RunnerProgressBar.DrawColor = Color.Goldenrod;

            RunnerProgressBar.Draw(Engine.Instance.SpriteBatch);
            Engine.Instance.SpriteBatch.DrawString(RunnerProgressFont, RunnerProgress, RunnerProgressLoc, Color.White);
            #endregion

            if (!MiniMap.IsHidden)
            {
                WaveProgressBar.Draw(Engine.Instance.SpriteBatch);
                Engine.Instance.SpriteBatch.DrawString(RunnerProgressFont, WaveProgress, WaveProgressLoc, Color.Black);
            }

            //Engine.Instance.SpriteBatch.DrawString(DebugFont, string.Format("Mouse screenloc:{0}. MouseWorldLoc:{1}", InputMgr.Instance.Mouse.Location, InputMgr.Instance.Mouse.Location + Player.Camera.Location), new Vector2(10, 50), Color.White);
            //Engine.Instance.SpriteBatch.DrawString(DebugFont, string.Format("camera loc:{0}", Player.Camera.Location), new Vector2(10, 70), Color.White);
            if(TowerBeneathMouse != null)
                Engine.Instance.SpriteBatch.DrawString(DebugFont, string.Format("Tower below mouse:{0}", TowerBeneathMouse.Name), new Vector2(10, 90), Color.White);
            if (SelectedTower != null)
                Engine.Instance.SpriteBatch.DrawString(DebugFont, string.Format("SelectedTower:{0}. {1}", SelectedTower.Name, SelectedTower.Desc), new Vector2(10, 110), Color.White);

            Player.DrawGUI();

            InfoPanel.Draw();

            Engine.Instance.SpriteBatch.Draw(Common.White1px, WaveSpawnBarBG, Color.Black);
            WaveSpawnBar.Draw(Engine.Instance.SpriteBatch);

            MiniMap.Draw();
            WarningMessages.Instance.Draw();

            ControlMgr.Instance.Draw();
            InputMgr.Instance.DrawMouse(Engine.Instance.SpriteBatch);
        }

        public void Draw()
        {
            //BroadPhase.Instance.DebugDraw();
            BGMgr.Draw();

            foreach (BaseRunner r in ActiveRunners)
            {
                r.Draw();
                r.DebugDraw();
            }
            foreach (Environment e in Environments)
                e.Draw();
            foreach (Visual v in Visuals)
                v.Draw();
            foreach (BaseTower tower in Towers)
                tower.Draw();
            foreach (BaseProjectile p in Projectiles)
                p.Draw();
            foreach (WayPoint wp in WayPoints)
                wp.DebugDraw();
            foreach (HeadUpText hut in HeadUpTexts)
                hut.Draw();

            if(Player.State == Player.eState.Building)
                BuildGrid.Draw();
        }
    }
}