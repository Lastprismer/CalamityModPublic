﻿using System;
using CalamityMod.Balancing;
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Ranged
{
    public class CardHeart : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.alpha = 255;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 1;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.Bullet;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0.5f / 255f, (255 - Projectile.alpha) * 0f / 255f, (255 - Projectile.alpha) * 0f / 255f);
            Projectile.rotation -= (MathHelper.ToRadians(90) * Projectile.direction);
            Projectile.spriteDirection = Projectile.direction;
            if (Main.rand.NextBool())
            {
                int dust = Dust.NewDust(Projectile.position, 1, 1, DustID.Web, 0f, 0f, 0, default, 0.5f);
                Main.dust[dust].velocity *= 0f;
                Main.dust[dust].noGravity = true;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < 2; k++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Web, Projectile.oldVelocity.X * 0.15f, Projectile.oldVelocity.Y * 0.15f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int heal = (int)Math.Round(hit.Damage * 0.01);
            if (heal > BalancingConstants.LifeStealCap)
                heal = BalancingConstants.LifeStealCap;

            if (Main.player[Main.myPlayer].lifeSteal <= 0f || heal <= 0 || target.lifeMax <= 5)
                return;

            CalamityGlobalProjectile.SpawnLifeStealProjectile(Projectile, Main.player[Projectile.owner], heal, ProjectileID.VampireHeal, BalancingConstants.LifeStealRange);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }
    }
}
