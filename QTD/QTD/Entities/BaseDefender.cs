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
    public class BaseDefender : IEntity, ITargetable
    {
        #region Interface
        private int m_EntityID = int.MinValue;
        public int EntityID
        {
            get { return m_EntityID; }
            private set { m_EntityID = value; }
        }

        public eEntityType EntityType{get { return eEntityType.Defender; }}
        public Vector2 CenterLoc { get { return Animation.Location + CenterLocOffset; } }
        public Vector2 FeetLoc { get { return Animation.Location + FeetLocOffset; } }

        private List<BaseProjectile> m_TargetedBy = new List<BaseProjectile>();
        public List<BaseProjectile> TargetedBy
        {
            get { return m_TargetedBy; }
            set { m_TargetedBy = value; }
        }

        /*private ITargetable m_TargetedByEnemy = null;
        public ITargetable TargetedByEnemy
        {
            get { return m_TargetedByEnemy; }
            set
            {
                m_TargetedByEnemy = value;
                if (value != null)
                    Target = TargetedByEnemy;
            }
        }*/

        private ITargetable m_Target = null;
        public ITargetable Target
        {
            get { return m_Target; }
            set
            {
                if (m_Target != null)
                {
                    ((BaseRunner)m_Target).TargettedByDefenders.Remove(this);
                    ((BaseRunner)m_Target).IsFightingWithDefenders.Remove(this);
                }

                m_Target = value;

                if (value != null)
                    ((BaseRunner)value).TargettedByDefenders.Add(this);
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
        HitPoints HP;
        const int HPBarSizeY = 5;
        Bar HPBar = new Bar(Common.White1px, new Rectangle(int.MinValue, int.MinValue, 1, HPBarSizeY), Bar.eDirection.Horizontal) { DrawColor = Color.Green };
        float HPRegen;
        bool IsMelee; // melee or ranged
        bool IsGround; // ground or flyer
        public bool IsDisposed = false;
        public int MeleeSightRange;

        #region Immunities
        bool ImmuneStun;
        bool ImmuneSlow;
        bool ImmuneArmorReduction;
        bool ImmuneRooted;
        #endregion


        private int m_ID;
        /// <summary>
        /// dds ID
        /// </summary>
        public int ID
        {
            get { return m_ID; }
            private set { m_ID = value; }
        }
       

        SimpleASprite Animation;
        SimpleASprite RunAni, AtkAni, DieAni;
        private Vector2 CenterLocOffset;
        private Vector2 FeetLocOffset;

        public enum eState { Running, Fighting, Dying, Dead, ReturnToRally }

        public eState State;
       
        /// <summary>
        /// The the shortest range of all weapons.
        /// </summary>
        int ShortestWeaponRange;
        List<BaseWeapon> Weapons;

        float Velocity;

        private bool m_IsAlive;
        public bool IsAlive
        {
            get { return m_IsAlive; }
            private set { m_IsAlive = value; }
        }

        private bool m_IsSpawned = false;
        public bool IsSpawned
        {
            get { return m_IsSpawned; }
            set
            {
                m_IsSpawned = value;
            }
        }
       
        SimpleTimer RespawnTimer;
        Rectangle RelativeAABB;
        eArmorType Armor;

        Texture2D Icon;

        private Vector2 RallyPoint;
        private float MaxRallyPointRange;
        private float RallyPointPersuitRange;

        Vector2 ReturnDest;
        Vector2 MoveIncrement;
        #endregion

        public BaseDefender()
        {
            EntityID = Engine.RetrieveNextEntityID();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="location">This will be the location of the feet. This procedure will take care of the FeetOffset.</param>
        public void SetLocation(Vector2 location)
        {
            Animation.Location = location - FeetLocOffset;
        }

        public void SetRallyPoint(Vector2 rallyPointWorldLoc, float maxRallyPointRange, float rallyPointPersuitRange)
        {
            RallyPoint = rallyPointWorldLoc;
            MaxRallyPointRange = maxRallyPointRange;
            RallyPointPersuitRange = rallyPointPersuitRange;
            if (State == eState.Running || State == eState.ReturnToRally)
                SetLocationToNearRallyPoint();
        }

        public void Die()
        {
            ClearIncommings();
            IsAlive = false;
            Level.Instance.SpawnedDeadDefenders.Add(this);
            Level.Instance.SpawnedAliveDefenders.Remove(this);
            if (Target != null)
                Target.ClearTarget();
            ClearTarget();
            State = eState.Dying; // After cleartarget();
            SwitchAnimation(DieAni);
        }

        public void ClearTarget()
        {
            Target = null;
            if (State == eState.Fighting)
            {
                State = eState.Running;
                SwitchAnimation(RunAni);
            }
        }

        public void ClearIncommings()
        {
            foreach (BaseProjectile p in TargetedBy)
                p.TargetLost();
            TargetedBy.Clear();
        }

        public void Spawn()
        {
            IsSpawned = true;
            Resurrect();
        }

        Vector2 GetRandomRallyLoc()
        {
            Vector2 result;
            do
            {
                result = new Vector2(Maths.RandomNr((int)(RallyPoint.X - MaxRallyPointRange), (int)(RallyPoint.X + MaxRallyPointRange)),
                                             Maths.RandomNr((int)(RallyPoint.Y - MaxRallyPointRange), (int)(RallyPoint.Y + MaxRallyPointRange)));
            } while (Vector2.Distance(result, RallyPoint) > MaxRallyPointRange);
            return result;
        }

        void SetLocationToNearRallyPoint()
        {
            SetLocation(GetRandomRallyLoc());
        }

        public void Resurrect()
        {
            IsAlive = true;
            Level.Instance.SpawnedAliveDefenders.Add(this);
            Level.Instance.SpawnedDeadDefenders.Remove(this);

            SwitchAnimation(RunAni);
            State = eState.Running;
            HP.HealFull();
            SetLocationToNearRallyPoint();
        }

        public void Initialize(int id)
        {
            DefenderStruct ds = DataStructs.Defenders.Find(d => d.ID == id);

            ID = id;
            HP = new HitPoints(ds.HP, ds.HP, 0);
            HPBar.Percentage = 100;
            HPRegen = ds.HPRegen;
            Velocity = ds.Velocity;
            IsMelee = ds.IsMelee;
            IsGround = ds.IsGround;
            MeleeSightRange = ds.MeleeSightRange;
            State = eState.Running;
            Armor = ds.ArmorType;

            IsAlive = true;
            IsSpawned = false;
            RespawnTimer = new SimpleTimer(ds.SpawnDelay);

            #region Weapons
            Weapons = new List<BaseWeapon>();
            ShortestWeaponRange = int.MaxValue;
            foreach (WeaponStruct ws in ds.Weapons)
            {
                BaseWeapon w = new BaseWeapon(Common.InvalidVector2, ws, this);
                Weapons.Add(w);
                if (ShortestWeaponRange > ws.Range)
                    ShortestWeaponRange = ws.Range;
            }
            if (Weapons.Count == 0)
                throw new Exception("A defender must at least have one weapon.");
            #endregion

            AnimationFactory.Create(ds.AnimationType, out RunAni, out AtkAni, out DieAni, out RelativeAABB, out Icon);
            Animation = RunAni;

            HPBar.BarDrawRect = new Rectangle(HPBar.BarDrawRect.X, HPBar.BarDrawRect.Y, Animation.FrameSize.X, HPBar.BarDrawRect.Height);
            Animation.DrawColor = ds.DrawColor;
            CenterLocOffset = new Vector2(Animation.FrameSize.X / 2, Animation.FrameSize.Y / 2);
            m_AABB = RelativeAABB;

            #region Feetoffset
            Vector2 extraFeetOffset = Vector2.Zero;
            switch (ds.AnimationType)
            {
                case eAnimation.Soldier01:
                    extraFeetOffset = new Vector2(0, -32);
                    break;
                case eAnimation.Crocy:
                    extraFeetOffset = new Vector2(0, -32);
                    break;
                default:
                    throw new CaseStatementMissingException("This animation was not added to the BaseDefender.Initialize() or it was not set (None).");
            }
            FeetLocOffset = new Vector2(Animation.FrameSize.X / 2, Animation.FrameSize.Y) + extraFeetOffset;
            #endregion
        }

        void StartReturnToRally()
        {
            State = eState.ReturnToRally;
            SwitchAnimation(RunAni);
            ReturnDest = GetRandomRallyLoc();
            Vector2 moveDir = Maths.GetMoveDir(FeetLoc, ReturnDest);
            MoveIncrement = Velocity * moveDir;
            Animation.SetDirectionByDir(moveDir);
        }

        void SwitchAnimation(SimpleASprite newAnimation)
        {
            newAnimation.Location = Animation.Location;
            newAnimation.CurrentFrame.Y = Animation.CurrentFrame.Y; // Direction
            Animation = newAnimation;
        }

        public void TakeDamage(float amount, eDamageType damageType, WeaponModifiers wpnMods)
        {
            amount *= Utils.GetDmgModifier(damageType, Armor);
            HP.CurrentHP -= amount;
            HPBar.Percentage = HP.LifeLeftPercentage;
            if (HP.CurrentHP <= 0)
                Die();
        }

        BaseRunner GetNewTarget(bool onlyUnoccupiedTargets)
        {
            // Get all runners in range
            List<IEntity> runnersInRange = BroadPhase.Instance.GetAllEntitiesInRange(this, MeleeSightRange, e => e.EntityType == eEntityType.Runner);

            float nearestRunnerNotTargetted = float.MaxValue;
            float nearestRunnerTargetted = float.MaxValue;
            BaseRunner target = null;
            bool skipTargettedRunners = onlyUnoccupiedTargets;

            foreach (BaseRunner r in runnersInRange)
            {
                if (r.IsGround) // Ignore flying enemies.
                {
                    float distance = Vector2.Distance(FeetLoc, r.FeetLoc);
                    if (r.TargettedByDefenders.Count > 0) // Add targetted runner
                    {
                        if (!skipTargettedRunners && nearestRunnerTargetted > distance)
                        {
                            nearestRunnerTargetted = distance;
                            target = r;
                        }
                    }
                    else
                    {
                        if (nearestRunnerNotTargetted > distance) // add untargetted runner
                        {
                            nearestRunnerNotTargetted = distance;
                            target = r;
                            skipTargettedRunners = true;
                        }
                    }
                }
            }

            return target;
        }

        /// <summary>
        /// Walks back to a point near the rally point if applicable.
        /// </summary>
        void WalkBackToNearRallyPoint()
        {
            if (State != eState.ReturnToRally && Vector2.Distance(FeetLoc, RallyPoint) > MaxRallyPointRange)
            {
                ClearTarget();
                StartReturnToRally();
            }

            if (State == eState.ReturnToRally)
            {
                Animation.Location += MoveIncrement;
                if (Vector2.Distance(FeetLoc, ReturnDest) <= Velocity)
                    State = eState.Running;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (IsSpawned)
            {
                if (IsAlive)
                {
                    // Update animation
                    Animation.Update(gameTime);

                    if (State == eState.Fighting)
                    {
                        #region Attempt to attack another unoccupied target when the current target has > 1 defender
                        if (((BaseRunner)Target).TargettedByDefenders.Count > 1)
                        {
                            BaseRunner newTarget = GetNewTarget(true);
                            if (newTarget != null)
                            {
                                Target = newTarget;
                                State = eState.Running; // If the new target would be in range then the next cycle will set this state to fighting but if we don't set it to fighting the defender might not walk to the new enemy.
                            }
                        }
                        #endregion

                        #region Update weapons
                        foreach (BaseWeapon wpn in Weapons)
                            wpn.Update(gameTime);
                        #endregion
                    }
                    else
                    {
                        // Get new target if needed
                        if (Target == null)
                            Target = GetNewTarget(false);

                        if (Target != null)
                        {
                            float distanceToTarget = Vector2.Distance(FeetLoc, Target.FeetLoc);
                            if (distanceToTarget <= MeleeSightRange)
                            {
                                if (distanceToTarget > ShortestWeaponRange)
                                {
                                    #region Run to target
                                    SwitchAnimation(RunAni);
                                    Vector2 moveDir = Maths.GetMoveDir(FeetLoc, Target.FeetLoc);
                                    Animation.Location += moveDir * Velocity;
                                    Animation.SetDirectionByDir(moveDir);
                                    #endregion
                                }
                                else
                                {
                                    #region Start fighting
                                    State = eState.Fighting;
                                    SwitchAnimation(AtkAni);
                                    ((BaseRunner)Target).IsFightingWithDefenders.Add(this);
                                    #endregion
                                }
                            }
                            else // Runner walked out of sightrange. So release the target and walk back to the rallypoint if needed. (looking for a new target in range will occur next cycle).
                            {
                                WalkBackToNearRallyPoint();
                            }
                        }
                        else
                        {
                            WalkBackToNearRallyPoint();
                        }
                    }

                    m_AABB.X = RelativeAABB.X + Animation.Location.Xi();
                    m_AABB.Y = RelativeAABB.Y + Animation.Location.Yi();
                }
                else
                {
                    if (State == eState.Dying)
                    {
                        Animation.Update(gameTime);
                        if (DieAni.IsDisposed)
                        {
                            State = eState.Dead;
                            DieAni.Reset();
                        }
                    }
                    else
                    {
                        #region Not alive but spawned so respawn code goes here
                        RespawnTimer.Update(gameTime);
                        if (RespawnTimer.IsDone)
                        {
                            RespawnTimer.Reset();
                            Resurrect();
                        }
                        #endregion
                    }
                }
            }            
        }

        public void DebugDraw()
        {
            //Engine.Instance.SpriteBatch.Draw(Common.White1px50Trans, AABB, Color.Pink);
            Engine.Instance.SpriteBatch.Draw(Common.White1px50Trans, new Rectangle(FeetLoc.Xi()-1,FeetLoc.Yi()-1,3,3), Color.Red);
        }

        public void Draw()
        {
            if (IsSpawned && (IsAlive || State == eState.Dying))
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