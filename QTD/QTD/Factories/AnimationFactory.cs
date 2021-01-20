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
    public static class AnimationFactory
    {
        const string SpritePath = "Sprites/";

        public static SimpleASprite Create(eAnimation animationType, out Rectangle relativeAABB)
        {
            SimpleASprite sprite;
            Texture2D icon;
            return Create(animationType, out sprite, out sprite, out sprite, out relativeAABB, out icon);
        }

        public static SimpleASprite Create(eAnimation animationType, out SimpleASprite run, out SimpleASprite attack, out SimpleASprite die, out Rectangle relativeAABB, out Texture2D icon)
        {
            run = attack = die = null;
            SimpleASprite result = null;
            icon = null;
            switch (animationType)
            {
                case eAnimation.Smoke1:
                    result = new SimpleASprite(Common.InvalidVector2, SpritePath + "smoke1_96x48_12", 96, 48, 12, 1, 32);
                    relativeAABB = new Rectangle(0, 0, 96, 48);
                    break;
                case eAnimation.Tree1:
                    result = new SimpleASprite(Common.InvalidVector2, SpritePath + "tree1_128px_8", 128, 128, 8, 1, int.MaxValue);
                    relativeAABB = new Rectangle(0, 0, 128, 128);
                    break;
                case eAnimation.Tree2:
                    result = new SimpleASprite(Common.InvalidVector2, SpritePath + "tree2_128px_8", 128, 128, 8, 1, int.MaxValue);
                    relativeAABB = new Rectangle(0, 0, 128, 128);
                    break;
                case eAnimation.Tree3:
                    result = new SimpleASprite(Common.InvalidVector2, SpritePath + "tree3_128px_8", 128, 128, 8, 1, int.MaxValue);
                    relativeAABB = new Rectangle(0, 0, 128, 128);
                    break;
                case eAnimation.Castle1:
                    string tex = "castle1a_192x256";
                    if (Maths.Chance(50))
                        tex = "castle1b_192x256";
                    result = new SimpleASprite(Common.InvalidVector2, SpritePath + tex, 192, 256, 1, 1, int.MaxValue);
                    result.ExtraDrawOffset = new Vector2(0,-96);
                    relativeAABB = new Rectangle(0, 96, 192, 192);
                    break;
                case eAnimation.WindMill1:
                    result = new SimpleASprite(Common.InvalidVector2, SpritePath + "windmill128px", 128, 128, 1, 1, int.MaxValue);
                    relativeAABB = new Rectangle(0, 0, 128, 128);
                    break;
                case eAnimation.WindMill2:
                    result = new SimpleASprite(Common.InvalidVector2, SpritePath + "windmill192px", 192, 192, 1, 1, int.MaxValue);
                    relativeAABB = new Rectangle(0, 0, 192, 192);
                    break;
                case eAnimation.PileOfGold:
                    result = new SimpleASprite(Common.InvalidVector2, SpritePath + "pileOfGold", 64, 64, 1, 1, int.MaxValue);
                    relativeAABB = new Rectangle(0, 0, 64, 64);
                    break;
                case eAnimation.Crocy:
                    run = new SimpleASprite(Common.InvalidVector2, SpritePath + "CrocyRun128px_8", 128, 128, 8, 1, 128);
                    attack = new SimpleASprite(Common.InvalidVector2, SpritePath + "CrocyAttack128px_11", 128, 128, 11, 1, 128);
                    die = new SimpleASprite(Common.InvalidVector2, SpritePath + "CrocyDie128px_11", 128, 128, 11, 1, 128) { LoopOnce = true };
                    relativeAABB = new Rectangle(32, 32, 64, 64);
                    break;
                case eAnimation.Soldier01:
                    run = new SimpleASprite(Common.InvalidVector2, SpritePath + "soldier01Run128px_8", 128, 128, 8, 1, 128);
                    attack = new SimpleASprite(Common.InvalidVector2, SpritePath + "soldier01Attack128px_13", 128, 128, 13, 1, 128);
                    die = new SimpleASprite(Common.InvalidVector2, SpritePath + "soldier01Die128px_8", 128, 128, 8, 1, 128) { LoopOnce = true };
                    relativeAABB = new Rectangle(32, 32, 64, 64);
                    icon = Common.str2Tex("Icons/soldier1");
                    break;
                case eAnimation.Test1:
                    result = new SimpleASprite(Common.InvalidVector2, SpritePath + "luziaWalk64xpx_8", 64, 64, 8, 1, 96);
                    relativeAABB = new Rectangle(0, 8, 64, 48);
                    break;
                case eAnimation.Tower1:
                    result = new SimpleASprite(Common.InvalidVector2, SpritePath + "tower", 63, 128, 1, 1, int.MaxValue);
                    result.ExtraDrawOffset = new Vector2(0, -64);
                    relativeAABB = new Rectangle(0, 0, 64, 128);
                    break;
                default:
                    throw new CaseStatementMissingException();
            }

            return result;
        }
    }
}