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
    public interface IEntity
    {
        int EntityID { get; }
        eEntityType EntityType { get; }
        Rectangle AABB { get; }
        Vector2 FeetLoc { get; }
    }
}
