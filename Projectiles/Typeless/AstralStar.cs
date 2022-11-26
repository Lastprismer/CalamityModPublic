﻿using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace CalamityMod.Projectiles.Typeless
{
    public class AstralStar : ModProjectile
    {
        private int noTileHitCounter = 120;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Star");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.alpha = 50;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 3f)
                CalamityUtils.HomeInOnNPC(Projectile, true, 300f, 12f, 20);

            if (Projectile.ai[0] == 2f)
            {
                Projectile.DamageType = RogueDamageClass.Instance;
            }
            int randomToSubtract = Main.rand.Next(1, 3);
            noTileHitCounter -= randomToSubtract;
            if (noTileHitCounter == 0)
            {
                Projectile.tileCollide = true;
            }
            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 20 + Main.rand.Next(40);
                if (Main.rand.NextBool(5))
                {
                    SoundEngine.PlaySound(SoundID.Item9, Projectile.position);
                }
            }
            Projectile.alpha -= 15;
            int num58 = 150;
            if (Projectile.Center.Y >= Projectile.ai[1])
            {
                num58 = 0;
            }
            if (Projectile.alpha < num58)
            {
                Projectile.alpha = num58;
            }
            Projectile.localAI[0] += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * (float)Projectile.direction;
            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * (float)Projectile.direction;
            if (Main.rand.NextBool(16))
            {
                Vector2 value3 = Vector2.UnitX.RotatedByRandom(1.5707963705062866).RotatedBy((double)Projectile.velocity.ToRotation(), default);
                int num59 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AstralOrange>(), Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default, 1.2f);
                Main.dust[num59].velocity = value3 * 0.66f;
                Main.dust[num59].position = Projectile.Center + value3 * 12f;
            }
            if (Main.rand.NextBool(48) && Main.netMode != NetmodeID.Server)
            {
                int num60 = Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f), 16, 1f);
                Main.gore[num60].velocity *= 0.66f;
                Main.gore[num60].velocity += Projectile.velocity * 0.3f;
            }
            if (Projectile.ai[1] == 1f)
            {
                Projectile.light = 0.9f;
                if (Main.rand.NextBool(10))
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AstralOrange>(), Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default, 1.2f);
                }
                if (Main.rand.NextBool(20) && Main.netMode != NetmodeID.Server)
                {
                    Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.position, new Vector2(Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f), Main.rand.Next(16, 18), 1f);
                }
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 120);
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 120);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(200, 100, 250, Projectile.alpha);
        }

        public override void Kill(int timeLeft)
        {
            Projectile.position.X = Projectile.position.X + (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y + (float)(Projectile.height / 2);
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
            for (int num621 = 0; num621 < 5; num621++)
            {
                int num622 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 100, default, 1.2f);
                Main.dust[num622].velocity *= 3f;
                if (Main.rand.NextBool(2))
                {
                    Main.dust[num622].scale = 0.5f;
                    Main.dust[num622].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                }
            }
            for (int num623 = 0; num623 < 5; num623++)
            {
                int num624 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 100, default, 1.7f);
                Main.dust[num624].noGravity = true;
                Main.dust[num624].velocity *= 5f;
                num624 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 100, default, 1f);
                Main.dust[num624].velocity *= 2f;
            }
            if (Main.netMode != NetmodeID.Server)
            {
                for (int num480 = 0; num480 < 3; num480++)
                {
                    Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, new Vector2(Projectile.velocity.X * 0.05f, Projectile.velocity.Y * 0.05f), Main.rand.Next(16, 18), 1f);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 offsets = new Vector2(0f, Projectile.gfxOffY) - Main.screenPosition;
            Color alpha = Projectile.GetAlpha(lightColor);
            Rectangle spriteRec = new Microsoft.Xna.Framework.Rectangle(0, 0, tex.Width, tex.Height);
            Vector2 spriteOrigin = spriteRec.Size() / 2f;
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Texture2D aura = ModContent.Request<Texture2D>("CalamityMod/Projectiles/StarTrail").Value;
            Vector2 drawStart = Projectile.Center + Projectile.velocity;
            Vector2 drawStart2 = Projectile.Center - Projectile.velocity * 0.5f;
            Vector2 spinPoint = new Vector2(0f, -10f);
            float time = Main.player[Projectile.owner].miscCounter % 216000f / 60f;
            Rectangle auraRec = aura.Frame();
            Color pink = Color.Coral * 0.2f;
            Color white = Color.White * 0.5f;
            white.A = 0;
            pink.A = 0;
            Vector2 auraOrigin = new Vector2(auraRec.Width / 2f, 10f);

            //Draw the aura
            Main.EntitySpriteDraw(aura, drawStart + offsets + spinPoint.RotatedBy(MathHelper.TwoPi * time), auraRec, pink, Projectile.velocity.ToRotation() + MathHelper.PiOver2, auraOrigin, 1.5f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(aura, drawStart + offsets + spinPoint.RotatedBy(MathHelper.TwoPi * time + MathHelper.TwoPi / 3f), auraRec, pink, Projectile.velocity.ToRotation() + MathHelper.PiOver2, auraOrigin, 1.1f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(aura, drawStart + offsets + spinPoint.RotatedBy(MathHelper.TwoPi * time + MathHelper.Pi * 4f / 3f), auraRec, pink, Projectile.velocity.ToRotation() + MathHelper.PiOver2, auraOrigin, 1.3f, SpriteEffects.None, 0);
            for (float d = 0f; d < 1f; d += 0.5f)
            {
                float scaleMult = time % 0.5f / 0.5f;
                scaleMult = (scaleMult + d) % 1f;
                float colorMult = scaleMult * 2f;
                if (colorMult > 1f)
                {
                    colorMult = 2f - colorMult;
                }
                Main.EntitySpriteDraw(aura, drawStart2 + offsets, auraRec, white * colorMult, Projectile.velocity.ToRotation() + MathHelper.PiOver2, auraOrigin, 0.3f + scaleMult * 0.5f, SpriteEffects.None, 0);
            }

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 2);
            return false;
        }
    }
}
