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
    public class BaseRunner : IEntity, ITargetable
    {
        #region Interface
        private int m_EntityID = int.MinValue;
        public int EntityID
        {
            get { return m_EntityID; }
            private set { m_EntityID = value; }
        }

        public eEntityType EntityType { get { return eEntityType.Runner; } }
        public Vector2 CenterLoc { get { return Animation.Location + CenterLocOffset; } }

        public Vector2 FeetLoc { get { return Animation.Location + FeetLocOffset; } }

        private List<BaseProjectile> m_TargetedBy = new List<BaseProjectile>();
        public List<BaseProjectile> TargetedBy
        {
            get { return m_TargetedBy; }
            set { m_TargetedBy = value; }
        }

        private ITargetable m_Target = null;
        public ITargetable Target
        {
            get { return m_Target; }
            set
            {
                m_Target = value;
            }
        }
        private Rectangle m_AABB = Rectangle.Empty;
        public Rectangle AABB
        {
            get { return m_AABB; }
            private set { m_AABB = value; }
        }
        #endregion
        #region Members
        int Bounty;
        HitPoints HP;
        const int HPBarSizeY = 5;
        Bar HPBar = new Bar(Common.White1px, new Rectangle(int.MinValue, int.MinValue, 1, HPBarSizeY), Bar.eDirection.Horizontal) { DrawColor = Color.Red };
        float HPRegen;
        bool IsMelee; // melee or ranged

        private bool m_IsGround; // ground or flyer
        public bool IsGround
        {
            get { return m_IsGround; }
            private set { m_IsGround = value; }
        }

        public bool IsDisposed = false;
        bool HasWeapons;

        /// <summary>
        /// The the shortest range of all weapons.
        /// </summary>
        int ShortestWeaponRange;
        List<BaseWeapon> Weapons;

        #region Immunities
        bool ImmuneStun;
        bool ImmuneSlow;
        bool ImmuneArmorReduction;
        bool ImmuneRooted;
        #endregion

        // For the wave
        public int StartWPID;

        SimpleASprite Animation;
        private Vector2 CenterLocOffset;
        private Vector2 FeetLocOffset;

        #region Waypoints / Moving
        float Velocity;
        WayPoint StartWP;
        WayPoint CurrentWP;
        WayPoint NextWP;

        private Vector2 m_MoveDir;
        public Vector2 MoveDir
        {
            get { return m_MoveDir; }
            set
            {
                m_MoveDir = value;
                Animation.SetDirectionByDir(value);
            }
        }

        Vector2 MoveIncrement;
        bool Recycles;
        Vector2 WPSpread;
        #endregion

        private float m_DistanceToFinish;
        public float DistanceToFinish
        {
            get { return m_DistanceToFinish; }
            private set { m_DistanceToFinish = value; }
        }

        private float m_PercentageToFinish;
        public float PercentageToFinish
        {
            get { return m_PercentageToFinish; }
            private set { m_PercentageToFinish = value; }
        }

        private float m_DistanceTraveled;
        public float DistanceTraveled
        {
            get { return m_DistanceTraveled; }
            private set { m_DistanceTraveled = value; }
        }

        Rectangle RelativeAABB;
        public eArmorType Armor;

        public static BaseRunner RunnerNearestToFinish;

        #region Fightning
        public bool IsFighting { get { return IsFightingWithDefenders.Count > 0 || IsFightingRanged; } }
        public bool IsFightingRanged;
        public List<ITargetable> TargettedByDefenders;
        public List<ITargetable> IsFightingWithDefenders;
        bool HasRangedWeapon;
        int RangedWpnRange;
        #endregion
        int FinishDmg;
        #endregion

        public BaseRunner()
        {
            EntityID = Engine.RetrieveNextEntityID();
        }

        public void SetLocation(Vector2 location)
        {
            Animation.Location = location;
        }

        public void SetLocation(WayPoint startWP)
        {
            StartWP = CurrentWP = startWP;
            NextWP = CurrentWP.GetNextWP();
            Animation.Location = startWP.Location - FeetLocOffset;
            InitMoveToNextWP();
        }

        public void Die()
        {
            IsDisposed = true;
            ClearIncommings();
            if (Target != null)
                Target.ClearTarget();
            ClearTarget();

            for (int i = 0; i < IsFightingWithDefenders.Count; i++)
            {
                IsFightingWithDefenders[i].ClearTarget();
                i--;
            }
            for (int i = 0; i < TargettedByDefenders.Count; i++)
            {
                TargettedByDefenders[i].ClearTarget();
                i--;
            }

            Level.Instance.ActiveRunners.Remove(this);
            Level.Instance.Player.Gold += Bounty;
            Level.Instance.Player.Kills++;


            // Bounty hut if desired.
            if (SettingsMgr.Instance.ShowBountyValues)
                Level.Instance.AddHut(CenterLoc, Bounty, false);
        }

        /// <summary>
        /// Clears the incommings of any projectiles making them impact on the location of this entity the moment this procedure is called.
        /// </summary>
        public void ClearIncommings()
        {
            foreach (BaseProjectile p in TargetedBy)
                p.TargetLost();
            TargetedBy.Clear();
        }

        public void Initialize(int id)
        {
            RunnerStruct rs = DataStructs.Runners.Find(r => r.ID == id);

            HP = new HitPoints(rs.HP, rs.HP, 0);
            HPBar.Percentage = 100;
            HPRegen = rs.HPRegen;
            Velocity = rs.Velocity;
            IsMelee = rs.IsMelee;
            IsGround = rs.IsGround;
            Recycles = rs.Recycles;
            Bounty = rs.Bounty;
            FinishDmg = rs.FinishDamage;

            ImmuneStun = rs.ImmuneStun;
            ImmuneSlow = rs.ImmuneSlow;
            ImmuneArmorReduction = rs.ImmuneArmorReduction;
            ImmuneRooted = rs.ImmuneRooted;

            Animation = AnimationFactory.Create(rs.AnimationType, out RelativeAABB);
            HPBar.BarDrawRect = new Rectangle(HPBar.BarDrawRect.X, HPBar.BarDrawRect.Y, Animation.FrameSize.X, HPBar.BarDrawRect.Height);
            Animation.DrawColor = rs.DrawColor;
            CenterLocOffset = new Vector2(Animation.FrameSize.X / 2, Animation.FrameSize.Y / 2);
            FeetLocOffset = new Vector2(Animation.FrameSize.X / 2, Animation.FrameSize.Y);
            Armor = rs.Armor;
            m_AABB = RelativeAABB;

            IsFightingWithDefenders = new List<ITargetable>();
            TargettedByDefenders = new List<ITargetable>();

            #region Weapons
            Weapons = new List<BaseWeapon>();
            ShortestWeaponRange = int.MaxValue;
            HasRangedWeapon = false;
            RangedWpnRange = int.MaxValue;
            foreach (WeaponStruct ws in rs.Weapons)
            {
                BaseWeapon w = new BaseWeapon(Common.InvalidVector2, ws, this);
                Weapons.Add(w);
                if (w.IsRanged)
                {
                    HasRangedWeapon = true;
                    if (RangedWpnRange > ws.Range)
                        RangedWpnRange = ws.Range;
                }
                if (ShortestWeaponRange > ws.Range)
                    ShortestWeaponRange = ws.Range;
            }
            #endregion
            HasWeapons = Weapons.Count > 0;

            IsFightingRanged = false;
        }

        /// <summary>
        /// Call this when the other entity died. This prevents this entity from 'focusing' on a dead/non-existent/etc enemy.
        /// </summary>
        public void ClearTarget()
        {
            IsFightingRanged = false;

            IsFightingWithDefenders.Remove(Target);
            TargettedByDefenders.Remove(Target);
            if (IsFightingWithDefenders.Count > 0)
                Target = IsFightingWithDefenders[0];
            else
                Target = null;

            if (HP.CurrentHP > 0)
                InitMoveToNextWP();
        }

        void InitMoveToNextWP()
        {
            WPSpread = Maths.RandomVector2(-WayPoint.Spread, WayPoint.Spread);
            MoveDir = Vector2.Normalize(Maths.GetMoveDir(FeetLoc, NextWP.Location + WPSpread));
            MoveIncrement = Velocity * MoveDir;
        }

        public void TakeDamage(float amount, eDamageType damageType, WeaponModifiers wpnMods)
        {
            amount *= Utils.GetDmgModifier(damageType, Armor);
            HP.CurrentHP -= amount;
            HPBar.Percentage = HP.LifeLeftPercentage;
            if (HP.CurrentHP <= 0)
                Die();
            else
            {
                // Apply mods
                if(wpnMods.Slows)

            }
        }

        public static BaseRunner GetClosestRunner(Vector2 location, out float distance)
        {
            distance = float.MaxValue;
            BaseRunner result = null;
            foreach (BaseRunner r in Level.Instance.ActiveRunners)
            {
                if (result == null || distance > Vector2.Distance(result.FeetLoc, r.FeetLoc))
                {
                    result = r;
                    distance = Vector2.Distance(location, result.FeetLoc);
                }
            }
            return result;
        }

        public void Update(GameTime gameTime)
        {
            Animation.Update(gameTime);

            #region Move
            if (!IsFighting)
            {
                if (Target == null)
                {
                    #region MoveToWP
                    SetLocation(Animation.Location + MoveIncrement);
                    if (Vector2.Distance(FeetLoc, NextWP.Location + WPSpread) <= Velocity)
                    {
                        CurrentWP = NextWP;
                        if (CurrentWP.IsFinish)
                        {
                            ClearIncommings(); // Call before setting a new location to this runner.
                            Level.Instance.Player.Lives -= FinishDmg;
                            if (Recycles)
                                SetLocation(StartWP);
                        }
                        else
                        {
                            NextWP = CurrentWP.GetNextWP();
                            InitMoveToNextWP();
                        }
                    }
                    #endregion
                }
                else
                {
                    #region MoveToTargetIfNeeded
                    {
                        if (Vector2.Distance(FeetLoc, Target.FeetLoc) > 50)
                        {
                            Vector2 moveDir = Maths.GetMoveDir(FeetLoc, Target.FeetLoc);
                            Animation.Location += Velocity * moveDir;
                            Animation.SetDirectionByDir(moveDir);
                        }
                    }
                    #endregion
                }

                m_AABB.X = RelativeAABB.X + Animation.Location.Xi();
                m_AABB.Y = RelativeAABB.Y + Animation.Location.Yi();
            #endregion

                #region Attack
                #region if no target then attempt to set ranged target
                if (Target == null && HasRangedWeapon)
                {
                    Target = (ITargetable)BroadPhase.Instance.GetFirstEntityInRange(this, RangedWpnRange, e => e.EntityType == eEntityType.Defender);
                    if (Target != null)
                        IsFightingRanged = true;
                }
                #endregion

                if (Target != null)
                    Animation.SetDirection(FeetLoc, Target.FeetLoc);
            }
            else
            {
                if (Target == null)
                    Target = IsFightingWithDefenders[0];
                foreach (BaseWeapon wpn in Weapons)
                    wpn.Update(gameTime);
            }
                #endregion

            // Get the percentage from start-->finish based on the first startpoint from the global list.
            DistanceToFinish = Vector2.Distance(FeetLoc, NextWP.Location) + NextWP.ShortestRouteToFinishLength;
            DistanceTraveled = WayPoint.StartPoints[0].ShortestRouteToFinishLength - DistanceToFinish;
            PercentageToFinish = (DistanceTraveled / WayPoint.StartPoints[0].ShortestRouteToFinishLength) * 100;

            // Set this as the runner nearest to the finish if applicable
            if (RunnerNearestToFinish == null || RunnerNearestToFinish.PercentageToFinish < PercentageToFinish)
                RunnerNearestToFinish = this;
        }

        static readonly SpriteFont DebugFont = Common.str2Font("Debug");
        public void DebugDraw()
        {
            //Engine.Instance.SpriteBatch.Draw(Common.White1px50Trans, AABB, null,Color.Pink,0f,Vector2.Zero,SpriteEffects.None,0.5f);
            Engine.Instance.SpriteBatch.DrawString(DebugFont, string.Format("e-id:{0}, t-cnt:{1}, f-cnt:{2}", EntityID, TargettedByDefenders.Count, IsFightingWithDefenders.Count), FeetLoc, Color.White);
        }

        public void Draw()
        {
            Animation.Draw(Engine.Instance.SpriteBatch, Vector2.Zero, 1f - FeetLoc.Y / Utils.DEPTH_DIVIDER);

            //HP bar
            if ((SettingsMgr.Instance.AlwaysShowHPBars && HP.CurrentHP < HP.MaxHP) || InputMgr.Instance.Keyboard.State.IsKeyDown(Keys.LeftAlt))
            {
                HPBar.SetLocation(Animation.Location.Xi(), Animation.Location.Yi() - HPBarSizeY);
                HPBar.Draw(Engine.Instance.SpriteBatch);
            }
        }
    }
}