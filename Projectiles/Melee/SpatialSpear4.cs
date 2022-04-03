using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Melee
{
    public class SpatialSpear4 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spear");
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 25;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.5f, 0.5f, 0.05f);
            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver4;
            if (Projectile.localAI[1] == 0f)
            {
                Projectile.scale -= 0.01f;
                Projectile.alpha += 15;
                if (Projectile.alpha >= 125)
                {
                    Projectile.alpha = 130;
                    Projectile.localAI[1] = 1f;
                }
            }
            else if (Projectile.localAI[1] == 1f)
            {
                Projectile.scale += 0.01f;
                Projectile.alpha -= 15;
                if (Projectile.alpha <= 0)
                {
                    Projectile.alpha = 0;
                    Projectile.localAI[1] = 0f;
                }
            }
            if (Main.rand.NextBool(8))
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, 244, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft > 20)
                return false;

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 90);
            target.AddBuff(BuffID.Frostburn, 90);
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 90);
        }

        public override void Kill(int timeLeft)
        {
            for (int num795 = 4; num795 < 12; num795++)
            {
                float num796 = Projectile.oldVelocity.X * (30f / num795);
                float num797 = Projectile.oldVelocity.Y * (30f / num795);
                int num798 = Dust.NewDust(new Vector2(Projectile.oldPosition.X - num796, Projectile.oldPosition.Y - num797), 8, 8, 244, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 100, default, 1.8f);
                Main.dust[num798].noGravity = true;
                Dust dust = Main.dust[num798];
                dust.velocity *= 0.5f;
                num798 = Dust.NewDust(new Vector2(Projectile.oldPosition.X - num796, Projectile.oldPosition.Y - num797), 8, 8, 244, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 100, default, 1.4f);
                dust = Main.dust[num798];
                dust.velocity *= 0.05f;
            }
        }
    }
}
