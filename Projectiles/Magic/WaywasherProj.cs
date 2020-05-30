using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class WaywasherProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Waywasher Blast");
        }

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.friendly = true;
            projectile.ignoreWater = true;
            projectile.alpha = 0;
            projectile.penetrate = 2;
            projectile.timeLeft = 300;
            projectile.magic = true;
        }

        public override void AI()
        {
            projectile.rotation += (Math.Abs(projectile.velocity.X) + Math.Abs(projectile.velocity.Y)) * 0.02f * (float)projectile.direction;
            Lighting.AddLight(projectile.Center, 0f, 0.1f, 0.7f);
            for (int num105 = 0; num105 < 2; num105++)
            {
                float num99 = projectile.velocity.X / 3f * (float)num105;
                float num100 = projectile.velocity.Y / 3f * (float)num105;
                int num101 = 4;
                int num102 = Dust.NewDust(new Vector2(projectile.position.X + (float)num101, projectile.position.Y + (float)num101), projectile.width - num101 * 2, projectile.height - num101 * 2, 33, 0f, 0f, 0, new Color(64, 224, 208), 1.2f);
                Dust dust = Main.dust[num102];
                dust.noGravity = true;
                dust.velocity *= 0.1f;
                dust.velocity += projectile.velocity * 0.1f;
                dust.position.X -= num99;
                dust.position.Y -= num100;
            }
            if (Main.rand.NextBool(5))
            {
                int num103 = 4;
                int num104 = Dust.NewDust(new Vector2(projectile.position.X + (float)num103, projectile.position.Y + (float)num103), projectile.width - num103 * 2, projectile.height - num103 * 2, 33, 0f, 0f, 0, new Color(64, 224, 208), 0.6f);
                Main.dust[num104].velocity *= 0.25f;
                Main.dust[num104].velocity += projectile.velocity * 0.5f;
            }
            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item10, projectile.position);
            for (int k = 0; k < 10; k++)
            {
                Dust.NewDust(projectile.position + projectile.velocity, projectile.width, projectile.height, 33, projectile.oldVelocity.X * 0.5f, projectile.oldVelocity.Y * 0.5f, 0, new Color(0, 142, 255), 1f);
            }
        }
    }
}
