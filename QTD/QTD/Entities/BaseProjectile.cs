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
    public class BaseProjectile : IEntity
    {
        #region Interface
        private int m_EntityID = int.MinValue;
        public int EntityID
        {
            get { return m_EntityID; }
            private set { m_EntityID = value; }
        }
       
        private eEntityType m_EntityType;
        public eEntityType EntityType
        {
            get { return m_EntityType; }
            set { m_EntityType = value; }
        }

        private Rectangle m_AABB = Rectangle.Empty;
        public Rectangle AABB
        {
            get { throw new Exception("Not used."); }
            private set { throw new Exception("Not used."); }
        }

        public Vector2 FeetLoc
        {
            get { return Location + FeetOffset; }
        }
        Vector2 FeetOffset;
       
        #endregion

        static readonly List<Texture2D> Textures = new List<Texture2D>()
        {
            Common.str2Tex("Projectiles/projectile01"),
            Common.str2Tex("Projectiles/cannonBall16px"),
            Common.str2Tex("Projectiles/cannonBall32px"),
        };

        Texture2D Texture;
        Vector2 Location;
        Color DrawColor = Color.White;

        private ITargetable m_Target;
        public ITargetable Target
        {
            get { return m_Target; }
            set
            {
                m_Target = value;
                if(value != null)
                    value.TargetedBy.Add(this);
            }
        }
       
        float Velocity;
        public bool IsDisposed = false;
        float Damage;
        int Splash;
        int SplashDmgPerc;
        Vector2 Destination; // only used when there is no target
        Vector2 MoveIncrement; // only used when there is no target
        eDamageType DamageType;
        int ImpactFX;

        public BaseProjectile(Vector2 centerLocation, eProjectile projectileType, bool isTowerProjectile, ITargetable target, float damage, eDamageType damageType, int splash, int splashDmgPerc, int impactFX)
        {
            EntityID = Engine.RetrieveNextEntityID();
            switch (projectileType)
            {
                case eProjectile.None:
                    throw new Exception("Trying to create a bullet with eProjectile.None.");
                case eProjectile.TestBullet1:
                    Texture = Textures[0];
                    Velocity = 6f;
                    break;
                case eProjectile.CannonBall16:
                    Texture = Textures[1];
                    Velocity = 6f;
                    break;
                case eProjectile.CannonBall32:
                    Texture = Textures[2];
                    Velocity = 6f;
                    break;
                default:
                    throw new CaseStatementMissingException();
            }
            FeetOffset = new Vector2(Texture.Width / 2, Texture.Height);

            if (isTowerProjectile)
                EntityType = eEntityType.TowerProjectile;
            else
                EntityType = eEntityType.RunnerProjectile;

            Target = target;
            Damage = damage;
            DamageType = damageType;

            Splash = splash;
            SplashDmgPerc = splashDmgPerc;
            Location = centerLocation - new Vector2(Texture.Width / 2, Texture.Height / 2);
            ImpactFX = impactFX;
        }

        public void TargetLost()
        {
            Destination = Target.FeetLoc;
            Target = null;
            MoveIncrement = Velocity * Maths.GetMoveDir(Location, Destination);
        }

        void Dispose()
        {
            IsDisposed = true;
            if (Target != null)
                Target.TargetedBy.Remove(this);
        }

        public void Update(GameTime gameTime)
        {
            if (Target != null)
            {
                Location += Velocity * Maths.GetMoveDir(Location, Target.CenterLoc);
                if (Vector2.Distance(Target.CenterLoc, Location) <= Velocity)
                {
                    if(ImpactFX != -1)
                        AudioMgrPooled.Instance.PlaySound(ImpactFX);
                    Target.TakeDamage(Damage, DamageType);
                    if (Splash > 0)
                    {
                        #warning implement splash here. don't forget to exclude the target because it was already hit.
                    }
                    Dispose();
                }
            }
            else
            {
                Location += MoveIncrement;
                if (Vector2.Distance(Destination, Location) <= Velocity)
                    Dispose();
            }
        }

        public void Draw()
        {
            Engine.Instance.SpriteBatch.Draw(Texture, Location, null, DrawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f - FeetLoc.Y / Utils.DEPTH_DIVIDER);
        }
    }
}