﻿using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using CalamityMod;
namespace CalamityMod.NPCs.ProfanedGuardians
{
    [AutoloadBossHead]
    public class ProfanedGuardianBoss3 : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Profaned Guardian");
            Main.npcFrameCount[npc.type] = 6;
			NPCID.Sets.TrailingMode[npc.type] = 1;
		}

        public override void SetDefaults()
        {
            npc.npcSlots = 3f;
            npc.aiStyle = -1;
            npc.damage = 90;
            npc.width = 100;
            npc.height = 80;
            npc.defense = 35;
            npc.Calamity().RevPlusDR(0.05f);
            npc.LifeMaxNERB(25000, 35000, 200000);
            double HPBoost = CalamityMod.CalamityConfig.BossHealthPercentageBoost * 0.01;
            npc.lifeMax += (int)(npc.lifeMax * HPBoost);
            npc.knockBackResist = 0f;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.canGhostHeal = false;
            aiType = -1;
            npc.boss = true;
            Mod calamityModMusic = ModLoader.GetMod("CalamityModMusic");
            if (calamityModMusic != null)
                music = calamityModMusic.GetSoundSlot(SoundType.Music, "Sounds/Music/Guardians");
            else
                music = MusicID.Boss1;
            for (int k = 0; k < npc.buffImmune.Length; k++)
            {
                npc.buffImmune[k] = true;
            }
            npc.buffImmune[BuffID.Ichor] = false;
            npc.buffImmune[BuffID.CursedInferno] = false;
			npc.buffImmune[BuffID.StardustMinionBleed] = false;
			npc.buffImmune[BuffID.Oiled] = false;
            npc.buffImmune[BuffID.BetsysCurse] = false;
            npc.buffImmune[ModContent.BuffType<AstralInfectionDebuff>()] = false;
            npc.buffImmune[ModContent.BuffType<AbyssalFlames>()] = false;
            npc.buffImmune[ModContent.BuffType<ArmorCrunch>()] = false;
            npc.buffImmune[ModContent.BuffType<DemonFlames>()] = false;
            npc.buffImmune[ModContent.BuffType<GodSlayerInferno>()] = false;
            npc.buffImmune[ModContent.BuffType<Nightwither>()] = false;
            npc.buffImmune[ModContent.BuffType<Shred>()] = false;
            npc.buffImmune[ModContent.BuffType<WhisperingDeath>()] = false;
            npc.buffImmune[ModContent.BuffType<SilvaStun>()] = false;
            npc.HitSound = SoundID.NPCHit52;
            npc.DeathSound = SoundID.NPCDeath55;
        }

        public override void FindFrame(int frameHeight)
        {
            npc.frameCounter += 0.15f;
            npc.frameCounter %= Main.npcFrameCount[npc.type];
            int frame = (int)npc.frameCounter;
            npc.frame.Y = frame * frameHeight;
        }

        public override void AI()
        {
			npc.TargetClosest(false);
			Player player = Main.player[npc.target];
			if (npc.timeLeft < 1800)
				npc.timeLeft = 1800;

			bool expertMode = Main.expertMode;
			bool isHoly = player.ZoneHoly;
			bool isHell = player.ZoneUnderworldHeight;
			npc.defense = (isHoly || isHell || CalamityWorld.bossRushActive) ? 40 : 99999;
			Vector2 vectorCenter = npc.Center;

			if (Math.Sign(npc.velocity.X) != 0)
            {
                npc.spriteDirection = -Math.Sign(npc.velocity.X);
            }
            npc.spriteDirection = Math.Sign(npc.velocity.X);
            int num1009 = (npc.ai[0] == 0f) ? 1 : 2;
            int num1010 = (npc.ai[0] == 0f) ? 60 : 80;
            for (int num1011 = 0; num1011 < 2; num1011++)
            {
                if (Main.rand.Next(3) < num1009)
                {
                    int dustType = Main.rand.Next(2);
                    if (dustType == 0)
                    {
                        dustType = 244;
                    }
                    else
                    {
                        dustType = 107;
                    }
                    int num1012 = Dust.NewDust(npc.Center - new Vector2((float)num1010), num1010 * 2, num1010 * 2, dustType, npc.velocity.X * 0.5f, npc.velocity.Y * 0.5f, 90, default, 1.5f);
                    Main.dust[num1012].noGravity = true;
                    Main.dust[num1012].velocity *= 0.2f;
                    Main.dust[num1012].fadeIn = 1f;
                }
            }
            if (CalamityGlobalNPC.doughnutBoss < 0 || !Main.npc[CalamityGlobalNPC.doughnutBoss].active)
            {
                npc.active = false;
                npc.netUpdate = true;
                return;
            }
            if (npc.ai[0] == 0f)
            {
                Vector2 vector96 = new Vector2(npc.Center.X, npc.Center.Y);
                float num784 = Main.npc[CalamityGlobalNPC.doughnutBoss].Center.X - vector96.X;
                float num785 = Main.npc[CalamityGlobalNPC.doughnutBoss].Center.Y - vector96.Y;
                float num786 = (float)Math.Sqrt((double)(num784 * num784 + num785 * num785));
                if (num786 > 90f)
                {
                    num786 = 21f / num786; //8f
                    num784 *= num786;
                    num785 *= num786;
                    npc.velocity.X = (npc.velocity.X * 15f + num784) / 16f;
                    npc.velocity.Y = (npc.velocity.Y * 15f + num785) / 16f;
                    return;
                }
                if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < 21f) //8f
                {
                    npc.velocity *= 1.12f; //1.05f
                }
                if (Main.netMode != NetmodeID.MultiplayerClient && ((expertMode && Main.rand.NextBool(50)) || Main.rand.NextBool(100)))
                {
                    npc.TargetClosest(true);
                    vector96 = new Vector2(npc.Center.X, npc.Center.Y);
                    num784 = player.Center.X - vector96.X;
                    num785 = player.Center.Y - vector96.Y;
                    num786 = (float)Math.Sqrt((double)(num784 * num784 + num785 * num785));
                    num786 = 21f / num786; //8f
                    npc.velocity.X = num784 * num786;
                    npc.velocity.Y = num785 * num786;
                    npc.ai[0] = 1f;
                    npc.netUpdate = true;
                }
            }
            else
            {
                Vector2 value4 = player.Center - npc.Center;
                value4.Normalize();
                value4 *= 22f; //9f
                npc.velocity = (npc.velocity * 99f + value4) / 100f;
                Vector2 vector97 = new Vector2(npc.Center.X, npc.Center.Y);
                float num787 = Main.npc[CalamityGlobalNPC.doughnutBoss].Center.X - vector97.X;
                float num788 = Main.npc[CalamityGlobalNPC.doughnutBoss].Center.Y - vector97.Y;
                float num789 = (float)Math.Sqrt((double)(num787 * num787 + num788 * num788));
                if (num789 > 700f)
                {
                    npc.ai[0] = 0f;
                }
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                npc.localAI[0] += expertMode ? 2f : 1f;
                if (npc.localAI[0] >= 600f)
                {
                    npc.localAI[0] = 0f;
                    npc.TargetClosest(true);
                    if (Collision.CanHit(npc.position, npc.width, npc.height, player.position, player.width, player.height))
                    {
                        Main.PlaySound(SoundID.Item20, npc.position);
                        Vector2 value9 = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
                        float spread = 45f * 0.0174f;
                        double startAngle = Math.Atan2(npc.velocity.X, npc.velocity.Y) - spread / 2;
                        double deltaAngle = spread / 8f;
                        double offsetAngle;
                        int damage = 0;
                        int projectileShot = ModContent.ProjectileType<HealOrbProv>();
                        int i;
                        for (i = 0; i < 3; i++)
                        {
                            offsetAngle = startAngle + deltaAngle * (i + i * i) / 2f + 32f * i;
                            Projectile.NewProjectile(value9.X, value9.Y, (float)(Math.Sin(offsetAngle) * 5f), (float)(Math.Cos(offsetAngle) * 5f), projectileShot, damage, 0f, Main.myPlayer, 0f, 0f);
                            Projectile.NewProjectile(value9.X, value9.Y, (float)(-Math.Sin(offsetAngle) * 5f), (float)(-Math.Cos(offsetAngle) * 5f), projectileShot, damage, 0f, Main.myPlayer, 0f, 0f);
                        }
                    }
                }
            }
        }

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (npc.spriteDirection == 1)
				spriteEffects = SpriteEffects.FlipHorizontally;

			Texture2D texture2D15 = Main.npcTexture[npc.type];
			Texture2D texture2D16 = ModContent.GetTexture("CalamityMod/NPCs/ProfanedGuardians/ProfanedGuardianBoss3Glow2");
			Vector2 vector11 = new Vector2((float)(Main.npcTexture[npc.type].Width / 2), (float)(Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type] / 2));
			Color color36 = Color.White;
			float amount9 = 0.5f;
			int num153 = 5;

			for (int num155 = 1; num155 < num153; num155 += 2)
			{
				Color color38 = lightColor;
				color38 = Color.Lerp(color38, color36, amount9);
				color38 = npc.GetAlpha(color38);
				color38 *= (float)(num153 - num155) / 15f;
				Vector2 vector41 = npc.oldPos[num155] + new Vector2((float)npc.width, (float)npc.height) / 2f - Main.screenPosition;
				vector41 -= new Vector2((float)texture2D15.Width, (float)(texture2D15.Height / Main.npcFrameCount[npc.type])) * npc.scale / 2f;
				vector41 += vector11 * npc.scale + new Vector2(0f, 4f + npc.gfxOffY);
				spriteBatch.Draw(texture2D15, vector41, new Rectangle?(npc.frame), color38, npc.rotation, vector11, npc.scale, spriteEffects, 0f);
			}

			Vector2 vector43 = npc.Center - Main.screenPosition;
			vector43 -= new Vector2((float)texture2D15.Width, (float)(texture2D15.Height / Main.npcFrameCount[npc.type])) * npc.scale / 2f;
			vector43 += vector11 * npc.scale + new Vector2(0f, 4f + npc.gfxOffY);
			spriteBatch.Draw(texture2D15, vector43, new Rectangle?(npc.frame), npc.GetAlpha(lightColor), npc.rotation, vector11, npc.scale, spriteEffects, 0f);

			texture2D15 = ModContent.GetTexture("CalamityMod/NPCs/ProfanedGuardians/ProfanedGuardianBoss3Glow");
			Color color37 = Color.Lerp(Color.White, Color.Yellow, 0.5f);
			Color color42 = Color.Lerp(Color.White, Color.Violet, 0.5f);

			for (int num163 = 1; num163 < num153; num163++)
			{
				Color color41 = color37;
				color41 = Color.Lerp(color41, color36, amount9);
				color41 = npc.GetAlpha(color41);
				color41 *= (float)(num153 - num163) / 15f;
				Vector2 vector44 = npc.oldPos[num163] + new Vector2((float)npc.width, (float)npc.height) / 2f - Main.screenPosition;
				vector44 -= new Vector2((float)texture2D15.Width, (float)(texture2D15.Height / Main.npcFrameCount[npc.type])) * npc.scale / 2f;
				vector44 += vector11 * npc.scale + new Vector2(0f, 4f + npc.gfxOffY);
				spriteBatch.Draw(texture2D15, vector44, new Rectangle?(npc.frame), color41, npc.rotation, vector11, npc.scale, spriteEffects, 0f);

				Color color43 = color42;
				color43 = Color.Lerp(color43, color36, amount9);
				color43 = npc.GetAlpha(color43);
				color43 *= (float)(num153 - num163) / 15f;
				spriteBatch.Draw(texture2D16, vector44, new Rectangle?(npc.frame), color43, npc.rotation, vector11, npc.scale, spriteEffects, 0f);
			}

			spriteBatch.Draw(texture2D15, vector43, new Rectangle?(npc.frame), color37, npc.rotation, vector11, npc.scale, spriteEffects, 0f);

			spriteBatch.Draw(texture2D16, vector43, new Rectangle?(npc.frame), color42, npc.rotation, vector11, npc.scale, spriteEffects, 0f);

			return false;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = 1;
            return true;
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            name = "A Profaned Guardian";
            potionType = ItemID.GreaterHealingPotion;
        }

        public override void OnHitPlayer(Player player, int damage, bool crit)
        {
            player.AddBuff(BuffID.OnFire, 600, true);
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.lifeMax = (int)(npc.lifeMax * 0.7f * bossLifeScale);
            npc.damage = (int)(npc.damage * 0.8f);
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(npc.position, npc.width, npc.height, 244, hitDirection, -1f, 0, default, 1f);
            }
            if (npc.life <= 0)
            {
                Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/ProfanedGuardianBossGores/ProfanedGuardianBossH"), 1f);
                Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/ProfanedGuardianBossGores/ProfanedGuardianBossH2"), 1f);
                Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/ProfanedGuardianBossGores/ProfanedGuardianBossH3"), 1f);
                Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/ProfanedGuardianBossGores/ProfanedGuardianBossH4"), 1f);
                Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/ProfanedGuardianBossGores/ProfanedGuardianBossH5"), 1f);
                for (int k = 0; k < 50; k++)
                {
                    Dust.NewDust(npc.position, npc.width, npc.height, 244, hitDirection, -1f, 0, default, 1f);
                }
            }
        }
    }
}
