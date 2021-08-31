using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class SacrificeProjectile : ModProjectile
    {
        public Player Owner => Main.player[projectile.owner];
        public bool StickingToAnything => projectile.ai[0] == 1f;
        public bool ReturningToOwner => projectile.ai[0] == 2f;
        public bool AbleToHealOwner = true;
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/Sacrifice";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sacrifice");
            ProjectileID.Sets.TrailingMode[projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 8;
        }

        public override void SetDefaults()
        {
            projectile.width = 62;
            projectile.height = 18;
            projectile.friendly = true;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 600;
            projectile.Calamity().rogue = true;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 6;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(AbleToHealOwner);

        public override void ReceiveExtraAI(BinaryReader reader) => AbleToHealOwner = reader.ReadBoolean();


        public override void AI()
        {
            if (ReturningToOwner)
            {
                projectile.timeLeft = 2;
                projectile.velocity = projectile.SafeDirectionTo(Owner.Center) * 24f;
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.Pi + MathHelper.PiOver4;

                // Heal the player and disappear when touching them.
                if (projectile.Hitbox.Intersects(Owner.Hitbox))
                {
                    if (!Owner.moonLeech && AbleToHealOwner)
                    {
                        int healAmount = projectile.Calamity().stealthStrike ? 50 : 4;

                        Owner.HealEffect(healAmount);
                        Owner.statLife += healAmount;
                        if (Owner.statLife > Owner.statLifeMax2)
                            Owner.statLife = Owner.statLifeMax2;
                    }

                    projectile.Kill();
                }
            }
            else if (!StickingToAnything)
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4;
            else if (!Main.dedServ && projectile.timeLeft % 40f == 39f)
            {
                for (int i = 0; i < 60; i++)
                {
                    Dust blood = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(50f, 50f), 267);
                    blood.velocity = Main.rand.NextVector2Circular(3f, 3f);
                    blood.noGravity = true;
                    blood.color = Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat(0.25f, 1f));
                    blood.scale = Main.rand.NextFloat(1f, 1.4f);
                }
            }

            if (StickingToAnything && projectile.timeLeft > 90)
                projectile.timeLeft = 90;
            projectile.StickyProjAI(50);
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            projectile.ModifyHitNPCSticky(15, true);
            projectile.velocity *= 0.5f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor);
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);
            Main.PlaySound(SoundID.Dig, projectile.position);
            projectile.Kill();
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => projectile.RotatingHitboxCollision(targetHitbox.TopLeft(), targetHitbox.Size());

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust blood = Dust.NewDustDirect(projectile.Center, 1, 1, 5, 0, 0, 0, default, 1.5f);
                blood.position += projectile.velocity.SafeNormalize(Vector2.Zero) * projectile.scale * 42f;
                blood.noGravity = true;
            }
        }
    }
}
