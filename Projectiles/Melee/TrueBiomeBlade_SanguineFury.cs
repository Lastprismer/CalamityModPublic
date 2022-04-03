﻿using Terraria.Graphics.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using Terraria.Audio;

namespace CalamityMod.Projectiles.Melee
{
    public class SanguineFury : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/Melee/TrueBiomeBlade_SanguineFury";
        private bool initialized = false;
        Vector2 direction = Vector2.Zero;
        public ref float Shred => ref Projectile.ai[0]; //How much the attack is, attacking
        public float ShredRatio => MathHelper.Clamp(Shred / (maxShred * 0.5f), 0f, 1f);
        public ref float PogoCooldown => ref Projectile.ai[1]; //Cooldown for the pogo
        public ref float BounceTime => ref Projectile.localAI[0];
        public ref float ChargeSoundCooldown => ref Projectile.localAI[1];
        public Player Owner => Main.player[Projectile.owner];
        public bool CanPogo => Owner.velocity.Y != 0 && PogoCooldown <= 0; //Only pogo when in the air and if the cooldown is zero
        private bool OwnerCanShoot => Owner.channel && !Owner.noItems && !Owner.CCed;

        public const float pogoStrenght = 16f; //How much the player gets pogoed up
        public const float maxShred = 500; //How much shred you get

        public Projectile Wheel;
        public bool Dashing;
        public Vector2 DashStart;


        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sanguine Fury");
        }
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = Projectile.height = 70;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = OmegaBiomeBlade.SuperPogoAttunement_LocalIFrames;
            Projectile.timeLeft = OmegaBiomeBlade.SuperPogoAttunement_LocalIFrames;
        }

        public override bool CanDamage()
        {
            return Projectile.timeLeft <= 2; //Prevent spam click abuse
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            float bladeLenght = 130 * Projectile.scale;
            float bladeWidth = 86 * Projectile.scale;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Owner.Center, Owner.Center + (direction * bladeLenght), bladeWidth, ref collisionPoint);
        }

        public void Pogo()
        {
            if (CanPogo && Main.myPlayer == Owner.whoAmI)
            {
                Owner.velocity = -direction.SafeNormalize(Vector2.Zero) * pogoStrenght; //Bounce
                Owner.fallStart = (int)(Owner.position.Y / 16f);
                PogoCooldown = 30; //Cooldown
                SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, Projectile.position);

                Vector2 hitPosition = Owner.Center + (direction * 100 * Projectile.scale);
                BounceTime = 20f; //Used only for animation

                for (int i = 0; i < 8; i++)
                {
                    Vector2 hitPositionDisplace = direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-10f, 10f);
                    Vector2 flyDirection = -direction.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4));
                    Particle smoke = new SmallSmokeParticle(hitPosition + hitPositionDisplace, flyDirection * 9f, Color.Crimson, new Color(130, 130, 130), Main.rand.NextFloat(1.8f, 2.6f), 155 - Main.rand.Next(30));
                    GeneralParticleHandler.SpawnParticle(smoke);

                    Particle Glow = new StrongBloom(hitPosition - hitPositionDisplace * 3, -direction * 6 * Main.rand.NextFloat(0.5f, 1f), Color.Crimson * 0.5f, 0.01f + Main.rand.NextFloat(0f, 0.2f), 20 + Main.rand.Next(40));
                    GeneralParticleHandler.SpawnParticle(Glow);
                }
                for (int i = 0; i < 3; i++)
                {
                    Vector2 hitPositionDisplace = direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-10f, 10f);
                    Vector2 flyDirection = -direction.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4));

                    Particle Rock = new StoneDebrisParticle(hitPosition - hitPositionDisplace * 3, flyDirection * Main.rand.NextFloat(3f, 6f), Color.Beige, 1f + Main.rand.NextFloat(0f, 0.4f), 30 + Main.rand.Next(50), 0.1f);
                    GeneralParticleHandler.SpawnParticle(Rock);
                }
            }
        }

        public override void AI()
        {
            if (!initialized) //Initialization. Here its litterally just playing a sound tho lmfao
            {
                SoundEngine.PlaySound(SoundID.Item90, Projectile.Center);
                initialized = true;

                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.type == ProjectileType<SanguineFuryWheel>() && proj.owner == Owner.whoAmI)
                    {
                        if (CalamityUtils.AngleBetween(Owner.Center - Owner.Calamity().mouseWorld, Owner.Center - proj.Center) > MathHelper.PiOver4)
                        {
                            proj.Kill();
                            break;
                        }

                        Wheel = proj;
                        Dashing = true;
                        DashStart = Owner.Center;
                        Wheel.timeLeft = 60;
                        Owner.GiveIFrames(OmegaBiomeBlade.SuperPogoAttunement_SlashIFrames);
                        break;
                    }
                }
            }

            if (!OwnerCanShoot)
            {
                Projectile.Kill();
                return;
            }

            if (Shred >= maxShred)
                Shred = maxShred;
            if (Shred < 0)
                Shred = 0;

            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.56f, 0.56f) * ShredRatio);

            //Manage position and rotation
            direction = Owner.SafeDirectionTo(Owner.Calamity().mouseWorld, Vector2.Zero);
            direction.Normalize();
            Projectile.rotation = direction.ToRotation();
            Projectile.Center = Owner.Center + (direction * 60);

            //Scaling based on shred
            Projectile.localNPCHitCooldown = OmegaBiomeBlade.SuperPogoAttunement_LocalIFrames - (int)(MathHelper.Lerp(0, OmegaBiomeBlade.SuperPogoAttunement_LocalIFrames - OmegaBiomeBlade.SuperPogoAttunement_LocalIFramesCharged, ShredRatio)); //Increase the hit frequency
            Projectile.scale = 1f + (ShredRatio * 1f); //SWAGGER


            if ((Wheel == null || !Wheel.active) && Dashing)
            {
                Dashing = false;
                Owner.velocity *= 0.1f; //Abrupt stop

                SoundEngine.PlaySound(Mod.GetLegacySoundSlot(Terraria.ModLoader.SoundType.Custom, "Sounds/Custom/MeatySlash"), Projectile.Center);
                if (Owner.whoAmI == Main.myPlayer)
                {
                    Projectile proj = Projectile.NewProjectileDirect(Owner.Center - DashStart / 2f, Vector2.Zero, ProjectileType<SanguineFuryDash>(), (int)(Projectile.damage * OmegaBiomeBlade.SuperPogoAttunement_SlashDamageBoost), 0, Owner.whoAmI);
                    if (proj.modProjectile is SanguineFuryDash dash)
                    {
                        dash.DashStart = DashStart;
                        dash.DashEnd = Owner.Center;
                    }
                }
            }

            Owner.Calamity().LungingDown = false;

            if (Dashing)
            {
                Owner.Calamity().LungingDown = true;
                Owner.fallStart = (int)(Owner.position.Y / 16f);
                Owner.velocity = Owner.SafeDirectionTo(Wheel.Center, Vector2.Zero) * 60f;

                if (Owner.Distance(Wheel.Center) < 60f)
                    Wheel.active = false;
            }


            if (Collision.SolidCollision(Owner.Center + (direction * 100 * Projectile.scale) - Vector2.One * 5f, 10, 10) && !Dashing)
            {
                Pogo();
                Projectile.netUpdate = true;
                Projectile.netSpam = 0;
            }

            //Make the owner look like theyre holding the sword bla bla
            Owner.heldProj = Projectile.whoAmI;
            Owner.direction = Math.Sign(direction.X);
            Owner.itemRotation = direction.ToRotation();
            if (Owner.direction != 1)
            {
                Owner.itemRotation -= 3.14f;
            }
            Owner.itemRotation = MathHelper.WrapAngle(Owner.itemRotation);
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            //Play a sound when the blade throw is available
            if (ShredRatio > 0.25 && Owner.whoAmI == Main.myPlayer)
            {
                if (ChargeSoundCooldown <= 0)
                {
                    var chargeSound = SoundEngine.PlaySound(SoundID.DD2_SonicBoomBladeSlash);
                    CalamityUtils.SafeVolumeChange(ref chargeSound, 2.5f);
                    ChargeSoundCooldown = 20;
                }
            }
            else
            {
                ChargeSoundCooldown--;
            }

            Shred -= OmegaBiomeBlade.SuperPogoAttunement_ShredDecayRate;
            PogoCooldown--;
            BounceTime--;
            if (Projectile.timeLeft <= 2)
                Projectile.timeLeft = 2;
        }

        //Since the iframes vary, adjust the damage to be consistent no matter the iframes. The true scaling happens between the BaseDamage and the FulLChargeDamage
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (Owner.HeldItem.modItem is OmegaBiomeBlade sword && Main.rand.NextFloat() <= OmegaBiomeBlade.SuperPogoAttunement_ShredderProc)
                sword.OnHitProc = true;

            float deviationFromBaseDamage = damage / (float)OmegaBiomeBlade.SuperPogoAttunement_BaseDamage;
            float currentDamage = (int)(MathHelper.Lerp(OmegaBiomeBlade.SuperPogoAttunement_BaseDamage * deviationFromBaseDamage, OmegaBiomeBlade.SuperPogoAttunement_FullChargeDamage * deviationFromBaseDamage, ShredRatio));

            //Adjust the damage to make it constant based on the local iframes
            float damageReduction = Projectile.localNPCHitCooldown / (float)OmegaBiomeBlade.SuperPogoAttunement_LocalIFrames;

            damage = (int)(currentDamage * damageReduction);
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            float deviationFromBaseDamage = damage / (float)OmegaBiomeBlade.SuperPogoAttunement_BaseDamage;
            float currentDamage = (int)(MathHelper.Lerp(OmegaBiomeBlade.SuperPogoAttunement_BaseDamage * deviationFromBaseDamage, OmegaBiomeBlade.SuperPogoAttunement_FullChargeDamage * deviationFromBaseDamage, ShredRatio));

            //Adjust the damage to make it constant based on the local iframes
            float damageReduction = Projectile.localNPCHitCooldown / (float)OmegaBiomeBlade.SuperPogoAttunement_LocalIFrames;

            damage = (int)(currentDamage * damageReduction);
        }


        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) => ShredTarget();
        public override void OnHitPvp(Player target, int damage, bool crit) => ShredTarget();

        private void ShredTarget()
        {
            if (Main.myPlayer != Owner.whoAmI)
                return;

            Owner.fallStart = (int)(Owner.position.Y / 16f);
            // get lifted up
            if (PogoCooldown <= 0)
            {
                SoundEngine.PlaySound(SoundID.NPCHit30, Projectile.Center); //Sizzle
                Shred += 62; //Augment the shredspeed
                if (Owner.velocity.Y > 0)
                    Owner.velocity.Y = -2f; //Get "stuck" into the enemy partly
                Owner.GiveIFrames(OmegaBiomeBlade.SuperPogoAttunement_ShredIFrames); // i framez.
                PogoCooldown = 20;
            }
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCHit43, Projectile.Center);
            if (ShredRatio > 0.8 && Owner.whoAmI == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.Center, direction * 16f, ProjectileType<SanguineFuryWheel>(), (int)(Projectile.damage * OmegaBiomeBlade.SuperPogoAttunement_ShotDamageBoost), Projectile.knockBack, Owner.whoAmI, Shred);
            }
            if (Dashing)
            {
                Owner.velocity *= 0.1f; //Abrupt stop
            }
            Owner.Calamity().LungingDown = false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D handle = GetTexture("CalamityMod/Items/Weapons/Melee/OmegaBiomeBlade");
            Texture2D blade = GetTexture("CalamityMod/Projectiles/Melee/TrueBiomeBlade_SanguineFury");

            int bladeAmount = 4;

            float drawAngle = direction.ToRotation();
            float drawRotation = drawAngle + MathHelper.PiOver4;

            Vector2 drawOrigin = new Vector2(0f, handle.Height);
            Vector2 drawOffset = Owner.Center + direction * 10f - Main.screenPosition;

            spriteBatch.Draw(handle, drawOffset, null, lightColor, drawRotation, drawOrigin, Projectile.scale, 0f, 0f);

            //Turn on additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            GameShaders.Misc["CalamityMod:BasicTint"].UseOpacity(MathHelper.Clamp(BounceTime, 0f, 20f) / 20f);
            GameShaders.Misc["CalamityMod:BasicTint"].UseColor(new Color(207, 248, 255));
            GameShaders.Misc["CalamityMod:BasicTint"].Apply();

            //Update the parameters
            drawOrigin = new Vector2(0f, blade.Height);

            spriteBatch.Draw(blade, drawOffset, null, Color.Lerp(Color.White, lightColor, 0.5f) * 0.9f, drawRotation, drawOrigin, Projectile.scale, 0f, 0f);


            for (int i = 0; i < bladeAmount; i++) //Draw extra copies
            {
                blade = GetTexture("CalamityMod/Projectiles/Melee/TrueBiomeBlade_SanguineFuryExtra");

                drawAngle = direction.ToRotation();

                float circleCompletion = (float)Math.Sin(Main.GlobalTime * 5 + i * MathHelper.PiOver2);
                drawRotation = drawAngle + MathHelper.PiOver4 + (circleCompletion * MathHelper.Pi / 10f) - (circleCompletion * (MathHelper.Pi / 9f) * ShredRatio);

                drawOrigin = new Vector2(0f, blade.Height);

                Vector2 drawOffsetStraight = Owner.Center + direction * (float)Math.Sin(Main.GlobalTime * 7) * 10 - Main.screenPosition; //How far from the player
                Vector2 drawDisplacementAngle = direction.RotatedBy(MathHelper.PiOver2) * circleCompletion.ToRotationVector2().Y * (20 + 40 * ShredRatio); //How far perpendicularly
                Vector2 drawOffsetFromBounce = direction * MathHelper.Clamp(BounceTime, 0f, 20f) / 20f * 20f;

                spriteBatch.Draw(blade, drawOffsetStraight + drawDisplacementAngle + drawOffsetFromBounce, null, Color.Lerp(Color.White, lightColor, 0.5f) * 0.8f, drawRotation, drawOrigin, Projectile.scale, 0f, 0f);
            }

            //Back to normal
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(initialized);
            writer.WriteVector2(direction);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            initialized = reader.ReadBoolean();
            direction = reader.ReadVector2();
        }
    }
}
