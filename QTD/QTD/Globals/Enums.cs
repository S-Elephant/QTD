using System;

namespace QTD
{
    public enum eEntityType { Tower, Runner, TowerProjectile, RunnerProjectile, Weapon, Defender, Environment }
    public enum eArmorType { Light, Medium, Heavy, Holy, Boss }
    public enum eDamageType { Normal, Piercing, Magic }
    public enum emodifierType { Slow, Stunned, Rooted, ArmorHalved, Poisoned }
    public enum eAnimation { None = 0, Test1, Tower1, Soldier01, Crocy, PileOfGold, WindMill1, WindMill2, Castle1, Tree1, Tree2, Tree3, Smoke1 }
    public enum eProjectile { None = 0, TestBullet1, CannonBall16, CannonBall32 }
    public enum eTowerTargetSetting { First, RandomEveryShot, LowestHP, HighestHP, ClosestToFinish }
    [Flags]
    public enum eAttackableTargets { None = 0, All=1, Ground, Air, Ghost }
}
