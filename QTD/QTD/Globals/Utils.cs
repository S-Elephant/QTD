using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNALib;
using System.IO;

namespace QTD
{
    public static class Utils
    {
        public const float DEPTH_DIVIDER = 10000f;

        public static float GetDmgModifier(eDamageType dmgType, eArmorType armorType)
        {
            switch (dmgType)
            {
                case eDamageType.Normal:
                    switch (armorType)
                    {
                        case eArmorType.Light:
                            return 1.5f;
                        case eArmorType.Medium:
                            return 1f;
                        case eArmorType.Heavy:
                            return 0.5f;
                        case eArmorType.Holy:
                            return 0.5f;
                        case eArmorType.Boss:
                            return 0.8f;
                        default:
                            throw new CaseStatementMissingException();
                    }
                case eDamageType.Piercing:
                    switch (armorType)
                    {
                        case eArmorType.Light:
                            return 0.5f;
                        case eArmorType.Medium:
                            return 1f;
                        case eArmorType.Heavy:
                            return 1.5f;
                        case eArmorType.Holy:
                            return 0.5f;
                        case eArmorType.Boss:
                            return 0.8f;
                        default:
                            throw new CaseStatementMissingException();
                    }
                case eDamageType.Magic:
                    switch (armorType)
                    {
                        case eArmorType.Light:
                            return 1.5f;
                        case eArmorType.Medium:
                            return 1f;
                        case eArmorType.Heavy:
                            return 1f;
                        case eArmorType.Holy:
                            return 1.5f;
                        case eArmorType.Boss:
                            return 0.8f;
                        default:
                            throw new CaseStatementMissingException();
                    }
                default:
                    throw new CaseStatementMissingException();
            }
        }

        /*/// <summary>
        /// Copies a region of sourceTex to destTex
        /// </summary>
        /// <param name="sourceTex">The source texture. For example a spritesheet.</param>
        /// <param name="destTex">The destination texture. This one is NOT overwritten. It's 'appended'.</param>
        /// <param name="destLoc">The location on the destination texture to draw the copied region</param>
        /// <param name="sourceRect">The source rectangle on the source texture. This will be the region that will be copied.</param>
        public static void CopyTexture(Texture2D sourceTex, ref Texture2D destTex, Vector2 destLoc, ref Rectangle sourceRect)
        {
            RenderTarget2D rTarget = new RenderTarget2D(Engine.Instance.Graphics.GraphicsDevice, destTex.Width, destTex.Height);
            Engine.Instance.Graphics.GraphicsDevice.SetRenderTarget(rTarget);
            Engine.Instance.SpriteBatch.Begin();
            Engine.Instance.SpriteBatch.Draw(destTex, Vector2.Zero, Color.White);
            Engine.Instance.SpriteBatch.Draw(sourceTex, destLoc, sourceRect, Color.White);
            Engine.Instance.SpriteBatch.End();
            destTex = rTarget;
        }*/
    }
}