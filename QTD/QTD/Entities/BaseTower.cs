using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNALib;
using XNALib.Controls;

namespace QTD
{
    public class BaseTower : IEntity, IHaveTarget
    {
        #region Interface
        private int m_EntityID = int.MinValue;
        public int EntityID
        {
            get { return m_EntityID; }
            private set { m_EntityID = value; }
        }

        public eEntityType EntityType
        {
            get { return eEntityType.Tower; }
        }

        private ITargetable m_Target = null;
        public ITargetable Target
        {
            get { return m_Target; }
            set { m_Target = value; }
        }

        private Vector2 m_FeetLoc;
        public Vector2 FeetLoc
        {
            get { return m_FeetLoc; }
            private set
            {
                m_FeetLoc = value;
                LayerDepth = 1f - value.Y / Utils.DEPTH_DIVIDER;
            }
        }
        private float LayerDepth;

        public Vector2 CenterLoc
        {
            get { return m_CenterLoc; }
        }

        #endregion

        const float SellRefundPerc = 0.5f;

        const int MAX_DEFENDERS = 25;

        #region Members

        private Rectangle m_AABB = Rectangle.Empty;
        public Rectangle AABB
        {
            get { return m_AABB; }
            private set { m_AABB = value; }
        }

        /// <summary>
        /// The total value of the tower plus all upgrades. This can be used to determine the sell/score value.
        /// </summary>
        int TotalValue;
        public int Cost;
        SimpleASprite Animation;

        private int m_ID;
        public int ID
        {
            get { return m_ID; }
            private set { m_ID = value; }
        }

        int SupplyCost;
        int SupplyGive;

        private StringBuilder m_Name;
        public StringBuilder Name
        {
            get { return m_Name; }
            private set { m_Name = value; }
        }
        private StringBuilder m_Desc;
        public StringBuilder Desc
        {
            get { return m_Desc; }
            private set { m_Desc = value; }
        }

        public Color DrawColor
        {
            get { return Animation.DrawColor; }
            set { Animation.DrawColor = value; }
        }
       
        Point BuildSize; // in grid squares
        int BuildTimeInMS;
        List<BaseWeapon> Weapons = null;
        List<BaseDefender> Defenders;
        Vector2 m_CenterLoc;

        private Vector2 m_RallyPoint;
        public Vector2 RallyPoint
        {
            get { return m_RallyPoint; }
            set
            {
                m_RallyPoint = value;
                foreach (BaseDefender bd in Defenders)
                    bd.SetRallyPoint(value, MaxRallyPointRange, RallyPointPersuitRange);
            }
        }

        #region Income
        int IncomePerTick;
        SimpleTimer IncomeTickDelayTimer;

        private int m_IncomePerWave;
        public int IncomePerWave
        {
            get { return m_IncomePerWave; }
            private set { m_IncomePerWave = value; }
        }
       
        #endregion

        List<Button> TowerUpgradeButtons;
        List<Button> SpawnUpgradeButtons;
        static readonly Vector2 SpawnUpgradeBtnsLoc = new Vector2(Engine.Instance.Width - 100, 100);
        static readonly Vector2 TowerUpgradeBtnsLoc = new Vector2(Engine.Instance.Width - 50, 100);

        float MaxRallyPointRange;
        float RallyPointPersuitRange;
        Vector2 ProjectileSpawnOffset;
        Rectangle RelativeAABB;
        List<Point> OccupiedGridIndices;
        Rectangle OccupyRect;

        public readonly static SimpleASprite RallyPointAni = new SimpleASprite(Vector2.Zero, "Sprites/rallyPoint32x48_8", 32, 48, 8, 1, 100);
        private static readonly Vector2 RallyPointFeetOffset = new Vector2(9,45); // Pole in the animation is on the left side so its not 32/2 but 9 for X.


        private bool m_HasFocus;
        public bool HasFocus
        {
            get { return m_HasFocus; }
            set
            {
                m_HasFocus = value;
                if (value)
                {
                    foreach (Button btn in SpawnUpgradeButtons)
                        ControlMgr.Instance.Controls.Add(btn);
                    foreach (Button btn in TowerUpgradeButtons)
                        ControlMgr.Instance.Controls.Add(btn);
                }
                else
                {
                    foreach (Button btn in SpawnUpgradeButtons)
                        ControlMgr.Instance.Controls.Remove(btn);
                    foreach (Button btn in TowerUpgradeButtons)
                        ControlMgr.Instance.Controls.Remove(btn);
                }
            }
        }
       

        #endregion

        public BaseTower()
        {
            EntityID = Engine.RetrieveNextEntityID();
        }

        /// <summary>
        /// Sets rallypoint.
        /// </summary>
        /// <param name="newWorldLocation">The new desired location. If it's further away than allowed then this procedure will correct the newWorldLocation.</param>
        public void SetRallyPoint(Vector2 newWorldLocation)
        {
#warning bug(minor): it is as if the point is limited arround the centerpoint instead the feetpoint... maybe its the drawoffset or something? it's also noticable that the dfenders return to near the tower center instead of the real rallypoint.
            #region limit new location to MaxRallyPointRange
            float distance = Vector2.Distance(FeetLoc, newWorldLocation);
            if (distance > MaxRallyPointRange)
                newWorldLocation = FeetLoc + Vector2.Normalize(newWorldLocation - FeetLoc) * MaxRallyPointRange; // This line limits the rallypoint to the MaxRallyPointRange
            #endregion

            if (Defenders.Count > 0)
                RallyPoint = newWorldLocation - RallyPointFeetOffset;
        }

        public ITargetable GetTarget(int range)
        {
            if (Target != null)
            {
                if (Vector2.Distance(CenterLoc, Target.FeetLoc) > range)
                    Target = null;
            }
            
            if(Target == null)
            {
                foreach (BaseRunner r in Level.Instance.ActiveRunners)
                {
                    if (Vector2.Distance(CenterLoc, r.FeetLoc) <= range)
                        return r;
                }
            }
            return null;
        }

        public void SetLocation(Vector2 location)
        {
            Animation.Location = location;
            m_CenterLoc = Animation.CenterLocation;
            FeetLoc = Animation.Location + new Vector2(Animation.FrameSize.X / 2, Animation.FrameSize.Y) + Animation.ExtraDrawOffset;

            foreach (BaseWeapon w in Weapons)
                w.CenterLocation = m_CenterLoc;
            foreach (BaseDefender def in Defenders)
                def.SetLocation(m_CenterLoc);
            RallyPoint = m_CenterLoc - RallyPointFeetOffset;

            AABB = RelativeAABB;
            m_AABB.X = (int)(Animation.Location.X + Animation.ExtraDrawOffset.X);
            m_AABB.Y = (int)(Animation.Location.Y + Animation.ExtraDrawOffset.Y);

            // Free grid blocks if there were any occupied
            if (OccupiedGridIndices.Count > 0)
            {
                foreach (Point p in OccupiedGridIndices)
                    Level.Instance.BuildGrid.SetBlock(p, BuildGrid.Buildable);
            }
            OccupyRect = new Rectangle(location.Xi(), location.Yi(), BuildSize.X * BuildGrid.GRID_SIZE, BuildSize.Y * BuildGrid.GRID_SIZE);

            // Occupy grid blocks
            for (int y = OccupyRect.Y / BuildGrid.GRID_SIZE; y < OccupyRect.Bottom / BuildGrid.GRID_SIZE; y++)
            {
                for (int x = OccupyRect.X / BuildGrid.GRID_SIZE; x < OccupyRect.Right / BuildGrid.GRID_SIZE; x++)
                {
                    Level.Instance.BuildGrid.SetBlock(new Point(x, y), BuildGrid.Occupied);
                }
            }
        }

        public void Initialize(int id)
        {
            #warning the struct below is copied and thus is allocates memory. should be done differently like find the index with linq instead and access it by index.
            TowerStruct ts = DataStructs.Towers.Find(t => t.ID == id);

            ID = id;
            Name = new StringBuilder(ts.Name, ts.Name.Length);
            Desc = new StringBuilder(ts.Desc, ts.Desc.Length);
            TotalValue = Cost = ts.Cost;
            BuildSize = ts.BuildSize;
            BuildTimeInMS = ts.BuildTimeInMS;
            MaxRallyPointRange = ts.MaxRallyPointRange;
            RallyPointPersuitRange = ts.RallyPointPersuitRange;
            
            OccupiedGridIndices = new List<Point>(ts.BuildSize.X * ts.BuildSize.Y);
            OccupyRect = Rectangle.Empty;
            
            if (ts.SupplyCost > 0)
            {
                SupplyCost = ts.SupplyCost;
                SupplyGive = 0;
                Level.Instance.Player.Supply += ts.SupplyCost;
            }
            else
            {
                SupplyCost = 0;
                SupplyGive = ts.SupplyCost * -1;
                Level.Instance.Player.MaxSupply += SupplyGive;
            }
          
            m_HasFocus = false;

            #region Income
            if (ts.IncomeTickDelayInMS != 0)
                IncomeTickDelayTimer = new SimpleTimer(ts.IncomeTickDelayInMS);
            else
                IncomeTickDelayTimer = null;
            IncomePerTick = ts.IncomePerTick;
            IncomePerWave = ts.IncomePerWave;
            #endregion

            Animation = AnimationFactory.Create(ts.AnimationType, out RelativeAABB);
            Animation.DrawColor = ts.DrawColor;
            DrawColor = Color.White;

            #region Weapons
            Weapons = new List<BaseWeapon>();
            if (ts.Weapons.Count > 0)
            {
                foreach (WeaponStruct ws in ts.Weapons)
                    Weapons.Add(new BaseWeapon(m_CenterLoc, ws, this));
            }
            #endregion

            Vector2 btnOffset = Vector2.Zero;
            #region Spawns
            if (SpawnUpgradeButtons != null)
            {
                foreach (Button sbtn in SpawnUpgradeButtons)
                    sbtn.Click -= new Button.OnClick(spawnUpgBtn_Click);
            }
            SpawnUpgradeButtons = new List<Button>();

            if (ts.Defenders.Count == 0)
                Defenders = new List<BaseDefender>(0);
            else
            {
                Defenders = new List<BaseDefender>(MAX_DEFENDERS);
                foreach (DefenderSpawnStruct dss in ts.Defenders)
                {
                    int maxDefenders = dss.Max;
                    int currentInitialCnt = 0;

                    for (int i = 0; i < dss.Max; i++)
                    {
                        BaseDefender bd = new BaseDefender();
                        bd.Initialize(dss.ID);
                        bd.SetRallyPoint(RallyPoint, MaxRallyPointRange, RallyPointPersuitRange);
                        Defenders.Add(bd);
                        if (currentInitialCnt < dss.InitialAmount)
                        {
                            bd.Spawn();
                            currentInitialCnt++;
                        }
                    }

                    if (dss.InitialAmount < dss.Max)
                    {
                        Button spawnUpgBtn = new Button(SpawnUpgradeBtnsLoc + btnOffset, "GUI/btnEmpty", "GUI/btnEmptyHover", null) { OverlayTexture = Common.str2Tex("Icons/soldier1"), Tag = dss };
                        spawnUpgBtn.Click += new Button.OnClick(spawnUpgBtn_Click);
                        btnOffset.Y += 50;
                        SpawnUpgradeButtons.Add(spawnUpgBtn);
                    }
                }
            }
            #endregion

            #region Tower Upgrades
            if (TowerUpgradeButtons != null)
            {
                foreach (Button tbtn in TowerUpgradeButtons)
                    tbtn.Click -= new Button.OnClick(TowerUpgBtn_Click);
            }
            TowerUpgradeButtons = new List<Button>();

            foreach (TowerUpgrade twrUpg in ts.Upgrades)
            {
                Button newTowerUpgBtn = new Button(TowerUpgradeBtnsLoc + btnOffset, "GUI/btnTowerUpgEmpty", "GUI/btnTowerUpgEmptyHover", null) { OverlayTexture = twrUpg.Icon, Tag = twrUpg };
                btnOffset.Y += 50;
                newTowerUpgBtn.Click += new Button.OnClick(TowerUpgBtn_Click);
                TowerUpgradeButtons.Add(newTowerUpgBtn);
            }
            #endregion
        }

        void TowerUpgBtn_Click(Button button)
        {
            TowerUpgrade twrUpg = (TowerUpgrade)button.Tag;

            TowerStruct ts = DataStructs.Towers.Find(t => t.ID == twrUpg.NewTowerID);
            int supplyCost = ts.SupplyCost - SupplyCost;

            if (Level.Instance.Player.Gold >= twrUpg.Cost && Level.Instance.Player.IsSupplyPossible(supplyCost, SupplyGive))
            {
                Level.Instance.Player.Gold -= twrUpg.Cost;
                HasFocus = false; // Removes the buttons from the controlmgr
                Vector2 location = Animation.Location;
                Initialize(twrUpg.NewTowerID);
                Animation.Location = location;
            }
            else
            {
#warning todo: display error message here or play error sound.
            }
        }

        void spawnUpgBtn_Click(Button button)
        {
            DefenderSpawnStruct dss = (DefenderSpawnStruct)button.Tag;
            if (CanUpgradeSpawnCnt(dss.ID, true))
            {
                UpgradeDefenderCnt(dss.ID);
                button.IsEnabled = CanUpgradeSpawnCnt(dss.ID, false);
            }
            else
            {
#warning todo: display error message here or play error sound.
            }
        }

        /// <summary>
        /// Spawns an exta defender if possible
        /// </summary>
        public void UpgradeDefenderCnt(int ddsID)
        {
            foreach (BaseDefender bd in Defenders)
            {
                if (bd.ID == ddsID && !bd.IsSpawned)
                {
                    bd.Spawn();

                    AudioMgrPooled.Instance.PlaySound(AudioConstants.BuildingUpgraded);
                    TowerStruct ts = DataStructs.Towers.Find(t => t.ID == ID);
                    DefenderSpawnStruct dds = ts.Defenders.Find(d => d.ID == ddsID);
                    TotalValue += dds.UpgCntCost;
                    Level.Instance.Player.Gold -= dds.UpgCntCost;
                    break;
                }
            }
        }

        /// <summary>
        /// Checks if the player can spawn a new defender of that ID.
        /// Checks if the player has enough gold.
        /// Checks if there are any defenders with that matching ID that have not yet been spawned.
        /// </summary>
        /// <param name="ddsID"></param>
        /// <returns></returns>
        public bool CanUpgradeSpawnCnt(int ddsID, bool includeGoldCheck)
        {
            if (Defenders.Where(d => d.ID == ddsID && !d.IsSpawned).Count() > 0)
            {
                TowerStruct ts = DataStructs.Towers.Find(t => t.ID == ID);
                DefenderSpawnStruct dds = ts.Defenders.Find(d => d.ID == ddsID);

                if (includeGoldCheck && Level.Instance.Player.Gold < dds.UpgCntCost)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        public void Sell()
        {
            AudioMgrPooled.Instance.PlaySound(AudioConstants.SellBuy);
            Level.Instance.Player.Gold += (int)(TotalValue * SellRefundPerc);
            Level.Instance.Towers.Remove(this);
            Level.Instance.Player.Supply -= SupplyCost;
            Level.Instance.Player.MaxSupply -= SupplyGive;
        }

        public void Update(GameTime gameTime)
        {
            Animation.Update(gameTime);
            foreach (BaseWeapon wpn in Weapons)
                wpn.UpdateForTower(gameTime, this);

            #region Spawn
            foreach (BaseDefender defender in Defenders)
            {
                defender.Update(gameTime);
            }
            #endregion

            if (IncomeTickDelayTimer != null && Level.Instance.State == Level.eState.Playing) // The last check prevents the incoem towers from generating income during the PreSpawn state.
            {
                IncomeTickDelayTimer.Update(gameTime);
                if (IncomeTickDelayTimer.IsDone)
                {
                    IncomeTickDelayTimer.Reset();
                    Level.Instance.Player.Gold += IncomePerTick;
                    Level.Instance.AddHut(CenterLoc, IncomePerTick, false);
                }
            }
        }

        public void DebugDraw()
        {
            //Engine.Instance.SpriteBatch.Draw(Common.White1px50Trans, AABB, Color.Pink);
            Engine.Instance.SpriteBatch.Draw(Common.White1px50Trans, new Rectangle(FeetLoc.Xi() - 1, FeetLoc.Yi() - 1, 3, 3), Color.Red);
        }

        public void Draw()
        {
            Animation.Draw(Engine.Instance.SpriteBatch, Vector2.Zero, LayerDepth);
            DebugDraw();

            if (Defenders.Count > 0)
                RallyPointAni.Draw(Engine.Instance.SpriteBatch, RallyPoint, 0f);

            foreach (BaseDefender defender in Defenders)
            {
                defender.Draw();
                defender.DebugDraw();
            }
        }
    }
}