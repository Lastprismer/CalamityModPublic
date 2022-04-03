using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
namespace CalamityMod.Projectiles.Rogue
{
    public class AuroradicalSplitter : ModProjectile
    {
        public int[] dustTypes = new int[]
        {
            ModContent.DustType<AstralBlue>(),
            ModContent.DustType<AstralOrange>()
        };

        public override string Texture => "CalamityMod/Projectiles/Rogue/AuroradicalStar";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Auroradical Star");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 100;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 50;
            Projectile.Calamity().rogue = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void AI()
        {
            //Rotation
            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * (float)Projectile.direction;

            //Lighting
            Lighting.AddLight(Projectile.Center, 0.3f, 0.5f, 0.1f);

            //sound effects
            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 20 + Main.rand.Next(40);
                if (Main.rand.NextBool(5))
                {
                    SoundEngine.PlaySound(SoundID.Item, (int)Projectile.position.X, (int)Projectile.position.Y, 9);
                }
            }

            //Change the scale size a little bit to make it pulse in and out
            float scaleAmt = (float)Main.mouseTextColor / 200f - 0.35f;
            scaleAmt *= 0.2f;
            Projectile.scale = scaleAmt + 0.95f;

            //Spawn dust
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] > 15f)
            {
                int astral = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Main.rand.Next(dustTypes), 0f, 0f, 100, default, 0.8f);
                Main.dust[astral].noGravity = true;
                Main.dust[astral].velocity *= 0f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, Projectile.alpha);

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 180);
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 180);
        }

        public override void Kill(int timeLeft)
        {
            if (Main.myPlayer != Projectile.owner)
                return;
            SoundEngine.PlaySound(SoundID.Item, (int)Projectile.position.X, (int)Projectile.position.Y, 9, 1f, 0f);
            for (float i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi / 5f * i;
                int star = Projectile.NewProjectile(Projectile.Center, angle.ToRotationVector2() * 5f, ModContent.ProjectileType<AuroradicalStar>(), (int)(Projectile.damage * 0.87), Projectile.knockBack, Projectile.owner, 0f, 0f);
                Main.projectile[star].Calamity().stealthStrike = Projectile.Calamity().stealthStrike;
            }
            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = 96;
            Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
            for (int d = 0; d < 2; d++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Main.rand.Next(dustTypes), 0f, 0f, 50, default, 1f);
            }
            for (int d = 0; d < 20; d++)
            {
                int astral = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Main.rand.Next(dustTypes), 0f, 0f, 0, default, 1.5f);
                Main.dust[astral].noGravity = true;
                Main.dust[astral].velocity *= 3f;
                astral = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 173, 0f, 0f, 50, default, 1f);
                Main.dust[astral].velocity *= 2f;
                Main.dust[astral].noGravity = true;
            }
            for (int g = 0; g < 3; g++)
            {
                Gore.NewGore(Projectile.position, new Vector2(Projectile.velocity.X * 0.05f, Projectile.velocity.Y * 0.05f), Main.rand.Next(16, 18), 1f);
            }
        }
    }
}
