﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Healing
{
    public class EmpyreanHealOrb : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Healing";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 3;
        }

        public override void AI()
        {
            Projectile.HealingProjectile((int)Projectile.ai[1], (int)Projectile.ai[0], 6f, 15f);
            int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, 0f, 0f, 100);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0f;
            Main.dust[dust].position.X -= Projectile.velocity.X * 0.2f;
            Main.dust[dust].position.Y += Projectile.velocity.Y * 0.2f;
        }
    }
}
