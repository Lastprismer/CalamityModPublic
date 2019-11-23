using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Melee.Spears
{
    public class InsidiousImpalerProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Insidious Impaler");
        }

        public override void SetDefaults()
        {
            projectile.width = 40;
            projectile.aiStyle = 19;
            projectile.melee = true;
            projectile.timeLeft = 90;
            projectile.height = 40;
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.ownerHitCheck = true;
            projectile.hide = true;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 5;
        }

        public override void AI()
        {
            Main.player[projectile.owner].direction = projectile.direction;
            Main.player[projectile.owner].heldProj = projectile.whoAmI;
            Main.player[projectile.owner].itemTime = Main.player[projectile.owner].itemAnimation;
            projectile.position.X = Main.player[projectile.owner].position.X + Main.player[projectile.owner].width / 2 - projectile.width / 2;
            projectile.position.Y = Main.player[projectile.owner].position.Y + Main.player[projectile.owner].height / 2 - projectile.height / 2;
            projectile.position += projectile.velocity * projectile.ai[0];
            if (projectile.ai[0] == 0f)
            {
                projectile.ai[0] = 3f;
                projectile.netUpdate = true;
            }
            if (Main.player[projectile.owner].itemAnimation < Main.player[projectile.owner].itemAnimationMax / 3)
			{
                projectile.ai[0] -= 1.1f;
                if (projectile.localAI[0] == 0f && Main.myPlayer == projectile.owner)
                {
                    projectile.localAI[0] = 1f;
                    Projectile.NewProjectile(projectile.Center.X + projectile.velocity.X, projectile.Center.Y + projectile.velocity.Y,
                        projectile.velocity.X * 2.4f, projectile.velocity.Y * 2.4f, ModContent.ProjectileType<InsidiousHarpoon>(), (int)(projectile.damage * 0.25), projectile.knockBack * 0.85f, projectile.owner, 0f, 0f);
				}
			}
            else
            {
                projectile.ai[0] += 0.95f;
            }

            if (Main.player[projectile.owner].itemAnimation == 0)
                projectile.Kill();

            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 2.355f;
            if (projectile.spriteDirection == -1)
                projectile.rotation -= 1.57f;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 360);
            target.AddBuff(BuffID.Venom, 360);
        }
    }
}
