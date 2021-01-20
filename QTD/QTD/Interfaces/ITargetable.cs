using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace QTD
{
    public interface ITargetable : IHaveTarget
    {
        Vector2 CenterLoc { get; }
        Vector2 FeetLoc { get; }
        List<BaseProjectile> TargetedBy { get; set; }
        void TakeDamage(float amount, eDamageType dmgType, WeaponModifiers wpnMods);
        void ClearTarget();
    }
}