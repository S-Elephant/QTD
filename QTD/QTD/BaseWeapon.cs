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
    public class BaseWeapon
    {
        SimpleTimer RoF;
        float Damage;
        int Range;
        int CritRate;
        int CritMultiplier;
        int Splash;
        int SplashDmgPerc; // 1-100. Note that setting 0 for this value means 0 damage.
        eProjectile ProjectileType;
        public Vector2 CenterLocation;
        public eTowerTargetSetting TargetSetting = eTowerTargetSetting.First;
        IHaveTarget Owner;
        Vector2 ProjectileSpawnOffset;
        eDamageType DamageType;
        int ImpactFX;
        int ShootFX;

        private bool m_IsRanged;
        public bool IsRanged
        {
            get { return m_IsRanged; }
            private set { m_IsRanged = value; }
        }
       

        //public BaseWeapon(Vector2 centerLoc, int atkDelayInMS, float damage, int range, int critRate, int critMultiplier, int splash, int splashDmgPerc, eProjectile projectileType, IHaveTarget owner, Vector2 projectileSpawnOffset)
        public BaseWeapon(Vector2 centerLoc, WeaponStruct ws, IHaveTarget owner)
        {
            CenterLocation = centerLoc;
            RoF = new SimpleTimer(ws.AttackDelay);
            Damage = ws.Damage;
            DamageType = ws.DamageType;
            Range = ws.Range;
            CritRate = ws.CritChance;
            CritMultiplier = ws.CritMultiplier;
            Splash = ws.Splash;
            SplashDmgPerc = ws.SplashDmgPerc;
            ProjectileType = ws.ProjectileType;
            Owner = owner;
            ProjectileSpawnOffset = ws.ProjectileSpawnOffset;
            ShootFX = ws.ShootFX;
            ImpactFX = ws.ImpactFX;
            if (ws.Range >= 96)
                IsRanged = true;
        }

        public void UpdateForTower(GameTime gameTime, BaseTower tower)
        {
            RoF.Update(gameTime);
            if (RoF.IsDone)
            {
                RoF.Reset();
                ITargetable target = tower.GetTarget(Range);
                if (target != null)
                {
                    if(ShootFX != -1)
                        AudioMgrPooled.Instance.PlaySound(ShootFX);
                    Level.Instance.Projectiles.Add(new BaseProjectile(CenterLocation + ProjectileSpawnOffset, ProjectileType, true, target, Damage, DamageType, Splash, SplashDmgPerc, ImpactFX));
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            RoF.Update(gameTime);
            if (RoF.IsDone)
            {
                RoF.Reset();
                
#warning debug below, need to use target setting and such when tower or just the first one when its a runners weapon.
                if (ProjectileType != eProjectile.None)
                    Level.Instance.Projectiles.Add(new BaseProjectile(CenterLocation + ProjectileSpawnOffset, ProjectileType, true, Owner.Target, Damage, DamageType, Splash, SplashDmgPerc, ImpactFX));
                else
                {
                    if (Owner.Target != null)
                    {
                        if (ShootFX != -1)
                            AudioMgrPooled.Instance.PlaySound(ShootFX);
                        Owner.Target.TakeDamage(Damage, DamageType);
                    }
                    else
                        throw new NullReferenceException("BaseRunner.BaseWeapon: Cannot attack a null-target.");
                }
            }
        }
    }
}