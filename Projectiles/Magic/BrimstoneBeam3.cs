using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class BrimstoneBeam3 : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Beam");
        }

        public override void SetDefaults()
        {
            projectile.width = 2;
            projectile.height = 2;
            projectile.friendly = true;
            projectile.magic = true;
            projectile.penetrate = -1;
            projectile.extraUpdates = 100;
            projectile.timeLeft = 30;
        }

        public override void Kill(int timeLeft)
        {
            int numProj = 2;
            float rotation = MathHelper.ToRadians(10);
            if (projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < numProj; i++)
                {
                    Vector2 perturbedSpeed = new Vector2(projectile.velocity.X, projectile.velocity.Y).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numProj - 1)));
                    Projectile.NewProjectile(projectile.Center.X, projectile.Center.Y, perturbedSpeed.X, perturbedSpeed.Y, ModContent.ProjectileType<BrimstoneBeam4>(), projectile.damage, projectile.knockBack, projectile.owner, 0f, 0f);
                }
            }
        }

		public override void AI()
		{
			Vector2 vector33 = projectile.position;
			vector33 -= projectile.velocity;
			projectile.alpha = 255;
			int num249 = 235;
			int num448 = Dust.NewDust(vector33, 1, 1, num249, 0f, 0f, 0, default, 1.5f);
			Main.dust[num448].position = vector33;
			Main.dust[num448].velocity *= 0.1f;
			Main.dust[num448].noGravity = true;
		}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120);
        }
    }
}
