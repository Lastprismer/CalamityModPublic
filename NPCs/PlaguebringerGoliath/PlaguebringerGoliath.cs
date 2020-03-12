﻿using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Vanity;
using CalamityMod.Items.LoreItems;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Furniture.Trophies;
using CalamityMod.Items.TreasureBags;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using CalamityMod;
namespace CalamityMod.NPCs.PlaguebringerGoliath
{
    [AutoloadBossHead]
    public class PlaguebringerGoliath : ModNPC
    {
        private const float MissileAngleSpread = 60;
        private const int MissileProjectiles = 8;
        private int MissileCountdown = 0;
        private int despawnTimer = 120;
        private int chargeDistance = 0;
        private bool charging = false;
        private bool halfLife = false;
        private bool canDespawn = false;
        private bool flyingFrame2 = false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("The Plaguebringer Goliath");
            Main.npcFrameCount[npc.type] = 4;
        }

        public override void SetDefaults()
        {
            npc.damage = 100;
            npc.npcSlots = 64f;
            npc.width = 198;
            npc.height = 198;
            npc.defense = 40;
            npc.Calamity().RevPlusDR(0.25f);
			npc.LifeMaxNERB(58500, 77275, 3700000);
            double HPBoost = (double)CalamityMod.CalamityConfig.BossHealthPercentageBoost * 0.01;
            npc.lifeMax += (int)((double)npc.lifeMax * HPBoost);
            npc.knockBackResist = 0f;
            npc.aiStyle = -1;
            aiType = -1;
            npc.boss = true;
            npc.value = Item.buyPrice(0, 25, 0, 0);
            NPCID.Sets.TrailCacheLength[npc.type] = 8;
            NPCID.Sets.TrailingMode[npc.type] = 1;
            for (int k = 0; k < npc.buffImmune.Length; k++)
            {
                npc.buffImmune[k] = true;
            }
            npc.buffImmune[BuffID.Ichor] = false;
            npc.buffImmune[BuffID.CursedInferno] = false;
			npc.buffImmune[BuffID.Frostburn] = false;
			npc.buffImmune[BuffID.Daybreak] = false;
			npc.buffImmune[BuffID.BetsysCurse] = false;
			npc.buffImmune[BuffID.StardustMinionBleed] = false;
			npc.buffImmune[BuffID.Oiled] = false;
            npc.buffImmune[ModContent.BuffType<AbyssalFlames>()] = false;
            npc.buffImmune[ModContent.BuffType<ArmorCrunch>()] = false;
            npc.buffImmune[ModContent.BuffType<DemonFlames>()] = false;
            npc.buffImmune[ModContent.BuffType<GodSlayerInferno>()] = false;
            npc.buffImmune[ModContent.BuffType<HolyFlames>()] = false;
            npc.buffImmune[ModContent.BuffType<Nightwither>()] = false;
            npc.buffImmune[ModContent.BuffType<Shred>()] = false;
            npc.buffImmune[ModContent.BuffType<WhisperingDeath>()] = false;
            npc.buffImmune[ModContent.BuffType<SilvaStun>()] = false;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.HitSound = SoundID.NPCHit4;
            npc.DeathSound = SoundID.NPCDeath14;
            Mod calamityModMusic = ModLoader.GetMod("CalamityModMusic");
            if (calamityModMusic != null)
                music = calamityModMusic.GetSoundSlot(SoundType.Music, "Sounds/Music/PlaguebringerGoliath");
            else
                music = MusicID.Boss3;
            bossBag = ModContent.ItemType<PlaguebringerGoliathBag>();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(halfLife);
            writer.Write(canDespawn);
            writer.Write(flyingFrame2);
            writer.Write(MissileCountdown);
            writer.Write(despawnTimer);
            writer.Write(chargeDistance);
            writer.Write(charging);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            halfLife = reader.ReadBoolean();
            canDespawn = reader.ReadBoolean();
            flyingFrame2 = reader.ReadBoolean();
            MissileCountdown = reader.ReadInt32();
            despawnTimer = reader.ReadInt32();
            chargeDistance = reader.ReadInt32();
            charging = reader.ReadBoolean();
        }

        public override void AI()
        {
			// Mode variables
			bool death = CalamityWorld.death || CalamityWorld.bossRushActive;
			bool revenge = CalamityWorld.revenge || CalamityWorld.bossRushActive;
            bool expertMode = Main.expertMode || CalamityWorld.bossRushActive;

            // Light
            Lighting.AddLight((int)((npc.position.X + (float)(npc.width / 2)) / 16f), (int)((npc.position.Y + (float)(npc.height / 2)) / 16f), 0.15f, 0.35f, 0.05f);

            // Show message
            if (!halfLife && ((double)npc.life <= (double)npc.lifeMax * 0.5 || death))
            {
                string key = "Mods.CalamityMod.PlagueBossText";
                Color messageColor = Color.Lime;
                if (Main.netMode == NetmodeID.SinglePlayer)
                    Main.NewText(Language.GetTextValue(key), messageColor);
                else if (Main.netMode == NetmodeID.Server)
                    NetMessage.BroadcastChatMessage(NetworkText.FromKey(key), messageColor);

                halfLife = true;
            }

            // Missile countdown
            if (halfLife && MissileCountdown == 0)
                MissileCountdown = 600;
            if (MissileCountdown > 1)
                MissileCountdown--;

			Vector2 vectorCenter = npc.Center;

            // Count nearby players
            int num1038 = 0;
            for (int num1039 = 0; num1039 < 255; num1039++)
            {
                if (Main.player[num1039].active && !Main.player[num1039].dead && (vectorCenter - Main.player[num1039].Center).Length() < 1000f)
                    num1038++;
            }

            // Defense gain
            if (expertMode)
            {
                int num1040 = death ? 20 : (int)(20f * (1f - (float)npc.life / (float)npc.lifeMax));
                npc.defense = npc.defDefense + num1040;
            }

            // Target
            if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead || !Main.player[npc.target].active)
                npc.TargetClosest(true);

			Player player = Main.player[npc.target];

            // Distance from target
            Vector2 distFromPlayer = player.Center - vectorCenter;

            // Enrage
            bool aboveGroundEnrage = ((double)player.position.Y < Main.worldSurface * 16.0 || player.position.Y > (float)((Main.maxTilesY - 200) * 16)) && !CalamityWorld.bossRushActive;

            bool jungleEnrage = false;
            if (!player.ZoneJungle && !CalamityWorld.bossRushActive)
                jungleEnrage = true;

			bool diagonalDash = revenge && ((double)npc.life <= (double)npc.lifeMax * 0.8 || death);

			// Despawn
			if (!player.active || player.dead || Vector2.Distance(player.Center, vectorCenter) > 5600f)
            {
				npc.TargetClosest(false);
				player = Main.player[npc.target];
				if (!player.active || player.dead || Vector2.Distance(player.Center, vectorCenter) > 5600f)
				{
					if (despawnTimer > 0)
						despawnTimer--;
				}
            }
            else
                despawnTimer = 120;

            canDespawn = despawnTimer <= 0;
            if (canDespawn)
            {
				if (npc.velocity.Y > 3f)
					npc.velocity.Y = 3f;
				npc.velocity.Y -= 0.2f;
				if (npc.velocity.Y < -16f)
					npc.velocity.Y = -16f;

				if (npc.timeLeft > 60)
                    npc.timeLeft = 60;

				if (npc.ai[0] != -1f)
				{
					npc.ai[0] = -1f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					MissileCountdown = 0;
					chargeDistance = 0;
					npc.netUpdate = true;
				}
				return;
            }

            // Phase switch
            if (npc.ai[0] == -1f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float num595 = npc.ai[1];
                    int num596;
                    do
                    {
                        num596 = Main.rand.Next(3);
                        if (MissileCountdown == 1)
                            num596 = 4;
                        else if (num596 == 1)
                            num596 = 2;
                        else if (num596 == 2)
                            num596 = 3;
                    }

                    while ((float)num596 == num595);
                    if (num596 == 0 && diagonalDash && distFromPlayer.Length() < 1800f)
                    {
                        switch (Main.rand.Next(3))
                        {
                            case 0:
                                chargeDistance = 0;
                                break;
                            case 1:
                                chargeDistance = 400;
                                break;
                            case 2:
                                chargeDistance = -400;
                                break;
                        }
                    }
                    npc.ai[0] = (float)num596;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                }
            }

            // Charge phase
            else if (npc.ai[0] == 0f)
            {
				float num1044 = revenge ? 28f : 26f;
				if (aboveGroundEnrage)
					num1044 += 6f;
				if ((double)npc.life < (double)npc.lifeMax * 0.66 || death)
					num1044 += 2f;
				if ((double)npc.life < (double)npc.lifeMax * 0.33 || death)
					num1044 += 2f;
				if (npc.Calamity().enraged > 0 || (CalamityMod.CalamityConfig.BossRushXerocCurse && CalamityWorld.bossRushActive))
					num1044 += 2f;

				int num1043 = 2;
                if ((npc.ai[1] > (float)(2 * num1043) && npc.ai[1] % 2f == 0f) || distFromPlayer.Length() > 1800f)
                {
                    npc.ai[0] = -1f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.netUpdate = true;
                    return;
                }

                // Charge
                if (npc.ai[1] % 2f == 0f)
                {
                    npc.TargetClosest(true);

                    float playerLocation = vectorCenter.X - player.Center.X;

                    if (Math.Abs(npc.Center.Y - (player.Center.Y - (float)chargeDistance)) < 20f)
                    {
						if (diagonalDash)
						{
							switch (Main.rand.Next(3))
							{
								case 0:
									chargeDistance = 0;
									break;
								case 1:
									chargeDistance = 400;
									break;
								case 2:
									chargeDistance = -400;
									break;
							}
						}

                        charging = true;

                        npc.ai[1] += 1f;
                        npc.ai[2] = 0f;

                        float num1045 = player.position.X + (float)(player.width / 2) - vectorCenter.X;
                        float num1046 = player.position.Y + (float)(player.height / 2) - vectorCenter.Y;
                        float num1047 = (float)Math.Sqrt((double)(num1045 * num1045 + num1046 * num1046));

                        num1047 = num1044 / num1047;
                        npc.velocity.X = num1045 * num1047;
                        npc.velocity.Y = num1046 * num1047;

						npc.Calamity().newAI[1] = npc.velocity.X;
						npc.Calamity().newAI[2] = npc.velocity.Y;

						npc.direction = playerLocation < 0 ? 1 : -1;
                        npc.spriteDirection = npc.direction;

                        Main.PlaySound(15, (int)npc.position.X, (int)npc.position.Y, 0);
                        return;
                    }

                    charging = false;

                    float num1048 = revenge ? 14f : 12f;
                    float num1049 = revenge ? 0.25f : 0.22f;
                    if ((double)npc.life < (double)npc.lifeMax * 0.66 || death)
                    {
                        num1048 += 1f;
                        num1049 += 0.05f;
                    }
                    if ((double)npc.life < (double)npc.lifeMax * 0.33 || death)
                    {
                        num1048 += 1f;
                        num1049 += 0.05f;
                    }
                    if (npc.Calamity().enraged > 0 || (CalamityMod.CalamityConfig.BossRushXerocCurse && CalamityWorld.bossRushActive))
                    {
                        num1048 += 2f;
                        num1049 += 0.1f;
                    }

                    if (vectorCenter.Y < (player.Center.Y - (float)chargeDistance))
                        npc.velocity.Y += num1049;
                    else
                        npc.velocity.Y -= num1049;

                    if (npc.velocity.Y < -num1048)
                        npc.velocity.Y = -num1048;
                    if (npc.velocity.Y > num1048)
                        npc.velocity.Y = num1048;

                    if (Math.Abs(vectorCenter.X - player.Center.X) > 650f)
                        npc.velocity.X += num1049 * (float)npc.direction;
                    else if (Math.Abs(vectorCenter.X - player.Center.X) < 500f)
                        npc.velocity.X -= num1049 * (float)npc.direction;
                    else
                        npc.velocity.X *= 0.8f;

                    if (npc.velocity.X < -num1048)
                        npc.velocity.X = -num1048;
                    if (npc.velocity.X > num1048)
                        npc.velocity.X = num1048;

                    npc.direction = playerLocation < 0 ? 1 : -1;
                    npc.spriteDirection = npc.direction;

					npc.netUpdate = true;

					if (npc.netSpam > 10)
						npc.netSpam = 10;
                }

                // Slow down after charge
                else
                {
                    if (npc.velocity.X < 0f)
                        npc.direction = -1;
                    else
                        npc.direction = 1;

                    npc.spriteDirection = npc.direction;

                    int num1050 = revenge ? 525 : 550;
                    if (npc.Calamity().enraged > 0 || (CalamityMod.CalamityConfig.BossRushXerocCurse && CalamityWorld.bossRushActive))
                        num1050 = 300;
                    else if (aboveGroundEnrage || CalamityWorld.bossRushActive)
                        num1050 = 400;
                    else if ((double)npc.life < (double)npc.lifeMax * 0.33 || death)
                        num1050 = revenge ? 450 : 475;
                    else if ((double)npc.life < (double)npc.lifeMax * 0.66)
                        num1050 = revenge ? 475 : 500;

                    int num1051 = 1;
                    if (vectorCenter.X < player.Center.X)
                        num1051 = -1;

					if (npc.direction == num1051 && (Math.Abs(vectorCenter.X - player.Center.X) > (float)num1050 || Math.Abs(vectorCenter.Y - player.Center.Y) > (float)num1050))
					{
						npc.ai[2] = 1f;
					}

                    if (npc.ai[2] != 1f)
                    {
                        charging = true;

						// Velocity fix if PBG slowed
						if (npc.velocity.Length() < num1044)
							npc.velocity = new Vector2(npc.Calamity().newAI[1], npc.Calamity().newAI[2]);

						npc.Calamity().newAI[0] += 1f;
						if (npc.Calamity().newAI[0] > 90f)
							npc.velocity *= 1.01f;

						return;
                    }

                    npc.TargetClosest(true);

                    npc.spriteDirection = npc.direction;

                    charging = false;

                    npc.velocity *= 0.9f;
                    float num1052 = revenge ? 0.12f : 0.1f;
                    if ((double)npc.life < (double)npc.lifeMax * 0.8 || death)
                    {
                        npc.velocity *= 0.98f;
                        num1052 += 0.05f;
                    }
                    if ((double)npc.life < (double)npc.lifeMax * 0.6 || death)
                    {
                        npc.velocity *= 0.98f;
                        num1052 += 0.05f;
                    }
                    if ((double)npc.life < (double)npc.lifeMax * 0.4 || death)
                    {
                        npc.velocity *= 0.98f;
                        num1052 += 0.05f;
                    }

                    if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < num1052)
                    {
                        npc.ai[2] = 0f;
                        npc.ai[1] += 1f;
						npc.Calamity().newAI[0] = 0f;
                    }

					npc.netUpdate = true;

					if (npc.netSpam > 10)
						npc.netSpam = 10;
				}
            }

            // Move closer if too far away
            else if (npc.ai[0] == 2f)
            {
                npc.TargetClosest(true);

                float playerLocation = vectorCenter.X - player.Center.X;
                npc.direction = playerLocation < 0 ? 1 : -1;
                npc.spriteDirection = npc.direction;

                float num1055 = player.position.X + (float)(player.width / 2) - vectorCenter.X;
                float num1056 = player.position.Y + (float)(player.height / 2) - 200f - vectorCenter.Y;
                float num1057 = (float)Math.Sqrt((double)(num1055 * num1055 + num1056 * num1056));
                if (num1057 < 600f)
                {
                    npc.ai[0] = ((double)npc.life <= (double)npc.lifeMax * 0.66 || death) ? 5f : 1f;
                    npc.ai[1] = 0f;
                    npc.netUpdate = true;
                    return;
                }

                // Move closer
                Movement(100f, 350f, 450f, player);
            }

            // Spawn bees
            else if (npc.ai[0] == 1f)
            {
                charging = false;

                npc.TargetClosest(true);

                Vector2 vector119 = new Vector2(npc.position.X + (float)(npc.width / 2) + (float)(80 * npc.direction), npc.position.Y + (float)npc.height * 1.2f);
                float num1058 = player.position.X + (float)(player.width / 2) - vectorCenter.X;
                float num1059 = player.position.Y + (float)(player.height / 2) - vectorCenter.Y;
                float num1060 = (float)Math.Sqrt((double)(num1058 * num1058 + num1059 * num1059));

                npc.ai[1] += 1f;
                npc.ai[1] += (float)(num1038 / 2);
                if ((double)npc.life < (double)npc.lifeMax * 0.75)
                    npc.ai[1] += 0.25f;
                if ((double)npc.life < (double)npc.lifeMax * 0.5)
                    npc.ai[1] += 0.25f;

                bool flag103 = false;
                if (npc.ai[1] > 40f)
                {
                    npc.ai[1] = 0f;
                    npc.ai[2] += 1f;
                    flag103 = true;
                }

                if (flag103)
                {
                    Main.PlaySound(3, (int)npc.position.X, (int)npc.position.Y, 8);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int randomAmt = expertMode ? 2 : 4;
                        if (Main.rand.Next(randomAmt) == 0)
                            randomAmt = ModContent.NPCType<PlagueBeeLargeG>();
                        else
                            randomAmt = ModContent.NPCType<PlagueBeeG>();

                        if (expertMode && NPC.CountNPCS(ModContent.NPCType<PlagueMine>()) < (aboveGroundEnrage ? 4 : 2))
                            NPC.NewNPC((int)vector119.X, (int)vector119.Y, ModContent.NPCType<PlagueMine>(), 0, 0f, 0f, 0f, 0f, 255);

                        if (revenge && NPC.CountNPCS(ModContent.NPCType<PlaguebringerShade>()) < (aboveGroundEnrage ? 2 : 1))
                            NPC.NewNPC((int)vector119.X, (int)vector119.Y, ModContent.NPCType<PlaguebringerShade>(), 0, 0f, 0f, 0f, 0f, 255);

                        if (NPC.CountNPCS(ModContent.NPCType<PlagueBeeLargeG>()) < 2)
                        {
                            int num1062 = NPC.NewNPC((int)vector119.X, (int)vector119.Y, randomAmt, 0, 0f, 0f, 0f, 0f, 255);
                            Main.npc[num1062].velocity.X = (float)Main.rand.Next(-200, 201) * (CalamityWorld.bossRushActive ? 0.04f : 0.02f);
                            Main.npc[num1062].velocity.Y = (float)Main.rand.Next(-200, 201) * (CalamityWorld.bossRushActive ? 0.04f : 0.02f);
                            Main.npc[num1062].localAI[0] = 60f;
                            Main.npc[num1062].netUpdate = true;
                        }
                    }
                }

                // Move closer if too far away
                if (num1060 > 600f)
                    Movement(100f, 350f, 450f, player);
                else
                    npc.velocity *= 0.9f;

                float playerLocation = vectorCenter.X - player.Center.X;
                npc.direction = playerLocation < 0 ? 1 : -1;
                npc.spriteDirection = npc.direction;

                if (npc.ai[2] > 2f)
                {
                    npc.ai[0] = -1f;
                    npc.ai[1] = 1f;
                    npc.netUpdate = true;
                }
            }

            // Missile spawn
            else if (npc.ai[0] == 5f)
            {
                charging = false;

                npc.TargetClosest(true);

                Vector2 vector119 = new Vector2(npc.position.X + (float)(npc.width / 2) + (float)(80 * npc.direction), npc.position.Y + (float)npc.height * 1.2f);
                float num1058 = player.position.X + (float)(player.width / 2) - vectorCenter.X;
                float num1059 = player.position.Y + (float)(player.height / 2) - vectorCenter.Y;
                float num1060 = (float)Math.Sqrt((double)(num1058 * num1058 + num1059 * num1059));

                npc.ai[1] += 1f;
                npc.ai[1] += (float)(num1038 / 2);
                bool flag103 = false;
                if ((double)npc.life < (double)npc.lifeMax * 0.25 || death)
                    npc.ai[1] += 0.25f;
                if ((double)npc.life < (double)npc.lifeMax * 0.1 || death)
                    npc.ai[1] += 0.25f;

                if (npc.ai[1] > 40f)
                {
                    npc.ai[1] = 0f;
                    npc.ai[2] += 1f;
                    flag103 = true;
                }

                if (flag103)
                {
                    Main.PlaySound(2, (int)npc.position.X, (int)npc.position.Y, 88);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (expertMode && NPC.CountNPCS(ModContent.NPCType<PlagueMine>()) < (aboveGroundEnrage ? 6 : 4))
                            NPC.NewNPC((int)vector119.X, (int)vector119.Y, ModContent.NPCType<PlagueMine>(), 0, 0f, 0f, 0f, 0f, 255);

                        if (revenge && NPC.CountNPCS(ModContent.NPCType<PlaguebringerShade>()) < 1 && aboveGroundEnrage)
                            NPC.NewNPC((int)vector119.X, (int)vector119.Y, ModContent.NPCType<PlaguebringerShade>(), 0, 0f, 0f, 0f, 0f, 255);

                        float projectileSpeed = revenge ? 6f : 5f;
                        float num1071 = player.position.X + (float)player.width * 0.5f - vector119.X;
                        float num1072 = player.position.Y + (float)player.height * 0.5f - vector119.Y;
                        float num1073 = (float)Math.Sqrt((double)(num1071 * num1071 + num1072 * num1072));

                        num1073 = projectileSpeed / num1073;
                        num1071 *= num1073;
                        num1072 *= num1073;

                        if (NPC.CountNPCS(ModContent.NPCType<PlagueHomingMissile>()) < (aboveGroundEnrage ? 8 : 5))
                        {
                            int num1062 = NPC.NewNPC((int)vector119.X, (int)vector119.Y, ModContent.NPCType<PlagueHomingMissile>(), 0, 0f, 0f, 0f, 0f, 255);
                            Main.npc[num1062].velocity.X = num1071;
                            Main.npc[num1062].velocity.Y = num1072;
                            Main.npc[num1062].netUpdate = true;
                        }
                    }
                }

                // Move closer if too far away
                if (num1060 > 600f)
                    Movement(100f, 350f, 450f, player);
                else
                    npc.velocity *= 0.9f;

                float playerLocation = vectorCenter.X - player.Center.X;
                npc.direction = playerLocation < 0 ? 1 : -1;
                npc.spriteDirection = npc.direction;

                if (npc.ai[2] > 2f)
                {
                    npc.ai[0] = -1f;
                    npc.ai[1] = 1f;
                    npc.netUpdate = true;
                }
            }

            // Stinger phase
            else if (npc.ai[0] == 3f)
            {
                Vector2 vector121 = new Vector2(npc.position.X + (float)(npc.width / 2) + (float)(80 * npc.direction), npc.position.Y + (float)npc.height * 1.2f);

                npc.ai[1] += 1f;
                bool flag104 = false;
                if (npc.Calamity().enraged > 0 || (CalamityMod.CalamityConfig.BossRushXerocCurse && CalamityWorld.bossRushActive))
                {
                    if (npc.ai[1] % 10f == 9f)
                        flag104 = true;
                }
                else if ((double)npc.life < (double)npc.lifeMax * 0.1 || death || aboveGroundEnrage)
                {
                    if (npc.ai[1] % 20f == 19f)
                        flag104 = true;
                }
                else if ((double)npc.life < (double)npc.lifeMax * 0.5)
                {
                    if (npc.ai[1] % 25f == 24f)
                        flag104 = true;
                }
                else if (npc.ai[1] % 30f == 29f)
                    flag104 = true;

                if (flag104 && vectorCenter.Y < player.position.Y)
                {
                    Main.PlaySound(2, (int)npc.position.X, (int)npc.position.Y, 42);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float projectileSpeed = revenge ? 6.5f : 6f;
                        if (jungleEnrage || npc.Calamity().enraged > 0 || (CalamityMod.CalamityConfig.BossRushXerocCurse && CalamityWorld.bossRushActive))
                            projectileSpeed += 10f;
                        if (CalamityWorld.bossRushActive)
                            projectileSpeed *= 1.5f;

                        float num1071 = player.position.X + (float)player.width * 0.5f - vector121.X;
                        float num1072 = player.position.Y + (float)player.height * 0.5f - vector121.Y;
                        float num1073 = (float)Math.Sqrt((double)(num1071 * num1071 + num1072 * num1072));
                        num1073 = projectileSpeed / num1073;
                        num1071 *= num1073;
                        num1072 *= num1073;

                        int num1074 = 40;
                        int num1075 = Main.rand.NextBool(2) ? ModContent.ProjectileType<PlagueStingerGoliath>() : ModContent.ProjectileType<PlagueStingerGoliathV2>();
                        if (expertMode)
                        {
                            num1074 = 32;
                            int damageBoost = death ? 5 : (int)(6f * (1f - (float)npc.life / (float)npc.lifeMax));
                            num1074 += damageBoost;

                            if (Main.rand.NextBool(6))
                            {
                                num1074 += 8;
                                num1075 = ModContent.ProjectileType<HiveBombGoliath>();
                            }
                        }
                        else
                        {
                            if (Main.rand.NextBool(9))
                            {
                                num1074 = 50;
                                num1075 = ModContent.ProjectileType<HiveBombGoliath>();
                            }
                        }
                        Projectile.NewProjectile(vector121.X, vector121.Y, num1071, num1072, num1075, num1074, 0f, Main.myPlayer, 0f, player.position.Y);
                    }
                }

                Movement(100f, 400f, 500f, player);

                float playerLocation = vectorCenter.X - player.Center.X;
                npc.direction = playerLocation < 0 ? 1 : -1;
                npc.spriteDirection = npc.direction;

                if (npc.ai[1] >= 300f)
                {
                    npc.ai[0] = -1f;
                    npc.ai[1] = 3f;
                    npc.netUpdate = true;
                }
            }

            // Missile charge
            else if (npc.ai[0] == 4f)
            {
				float num1044 = revenge ? 28f : 26f;
				if (CalamityWorld.bossRushActive)
					num1044 = 32f;

				int num1043 = 2;
                if (npc.ai[1] > (float)(2 * num1043) && npc.ai[1] % 2f == 0f)
                {
                    MissileCountdown = 0;
                    npc.ai[0] = -1f;
                    npc.ai[1] = 0f;
                    npc.ai[2] = 0f;
                    npc.netUpdate = true;
                    return;
                }

                // Charge
                if (npc.ai[1] % 2f == 0f)
                {
                    npc.TargetClosest(true);

                    float playerLocation = vectorCenter.X - player.Center.X;

                    if (Math.Abs(vectorCenter.Y - (player.Center.Y - 500f)) < 20f)
                    {
                        if (MissileCountdown == 1)
                        {
                            Main.PlaySound(2, (int)npc.position.X, (int)npc.position.Y, 116);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int speed = revenge ? 6 : 5;
								if (aboveGroundEnrage)
									speed = 8;
                                if (CalamityWorld.bossRushActive)
                                    speed = 12;
                                int damage = expertMode ? 48 : 60;

                                Vector2 baseVelocity = player.Center - vectorCenter;
                                baseVelocity.Normalize();
                                baseVelocity *= speed;

                                for (int i = 0; i < MissileProjectiles; i++)
                                {
                                    Vector2 spawn = vectorCenter;
                                    spawn.X += i * 27 - (MissileProjectiles * 12); // -96 to 93
                                    Vector2 velocity = baseVelocity.RotatedBy(MathHelper.ToRadians(-MissileAngleSpread / 2 + (MissileAngleSpread * i / (float)MissileProjectiles)));
                                    Projectile.NewProjectile(spawn.X, spawn.Y, velocity.X, velocity.Y, ModContent.ProjectileType<HiveBombGoliath>(), damage, 0f, Main.myPlayer, 0f, player.position.Y);
                                }
                            }
                        }

                        charging = true;

                        npc.ai[1] += 1f;
                        npc.ai[2] = 0f;

                        float num1045 = player.position.X + (float)(player.width / 2) - vectorCenter.X;
                        float num1046 = player.position.Y - 500f + (float)(player.height / 2) - vectorCenter.Y;
                        float num1047 = (float)Math.Sqrt((double)(num1045 * num1045 + num1046 * num1046));

                        num1047 = num1044 / num1047;
                        npc.velocity.X = num1045 * num1047;
                        npc.velocity.Y = num1046 * num1047;

                        npc.direction = playerLocation < 0 ? 1 : -1;
                        npc.spriteDirection = npc.direction;
                        return;
                    }

                    charging = false;

                    float num1048 = revenge ? 16f : 14f;
                    float num1049 = revenge ? 0.2f : 0.18f;
                    if (CalamityWorld.bossRushActive)
                    {
                        num1048 *= 1.5f;
                        num1049 *= 1.5f;
                    }

                    if (vectorCenter.Y < player.Center.Y - 500f)
                        npc.velocity.Y += num1049;
                    else
                        npc.velocity.Y -= num1049;

                    if (npc.velocity.Y < -num1048)
                        npc.velocity.Y = -num1048;
                    if (npc.velocity.Y > num1048)
                        npc.velocity.Y = num1048;

                    if (Math.Abs(vectorCenter.X - player.Center.X) > 600f)
                        npc.velocity.X += 0.15f * (float)npc.direction;
                    else if (Math.Abs(vectorCenter.X - player.Center.X) < 300f)
                        npc.velocity.X -= 0.15f * (float)npc.direction;
                    else
                        npc.velocity.X *= 0.8f;

                    if (npc.velocity.X < -20f)
                        npc.velocity.X = -20f;
                    if (npc.velocity.X > 20f)
                        npc.velocity.X = 20f;

                    npc.direction = playerLocation < 0 ? 1 : -1;
                    npc.spriteDirection = npc.direction;

					npc.netUpdate = true;

					if (npc.netSpam > 10)
						npc.netSpam = 10;
				}

                // Slow down after charge
                else
                {
                    if (npc.velocity.X < 0f)
                        npc.direction = -1;
                    else
                        npc.direction = 1;

                    npc.spriteDirection = npc.direction;

                    int num1050 = 600;
                    int num1051 = 1;

                    if (vectorCenter.X < player.Center.X)
                        num1051 = -1;
                    if (npc.direction == num1051 && Math.Abs(vectorCenter.X - player.Center.X) > (float)num1050)
                        npc.ai[2] = 1f;

                    if (npc.ai[2] != 1f)
                    {
                        charging = true;

						// Velocity fix if PBG slowed
						if (npc.velocity.Length() < num1044)
							npc.velocity.X = num1044 * npc.direction;

						npc.Calamity().newAI[0] += 1f;
						if (npc.Calamity().newAI[0] > 90f)
							npc.velocity.X *= 1.01f;

						return;
                    }

                    npc.TargetClosest(true);

                    npc.spriteDirection = npc.direction;

                    charging = false;

                    npc.velocity *= 0.9f;
                    float num1052 = revenge ? 0.12f : 0.1f;
                    if (npc.life < npc.lifeMax / 2 || death)
                    {
                        npc.velocity *= 0.98f;
                        num1052 += 0.05f;
                    }
                    if (npc.life < npc.lifeMax / 3 || death)
                    {
                        npc.velocity *= 0.98f;
                        num1052 += 0.05f;
                    }
                    if (npc.life < npc.lifeMax / 5 || death)
                    {
                        npc.velocity *= 0.98f;
                        num1052 += 0.05f;
                    }

                    if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < num1052)
                    {
                        npc.ai[2] = 0f;
                        npc.ai[1] += 1f;
						npc.Calamity().newAI[0] = 0f;
                    }

					npc.netUpdate = true;

					if (npc.netSpam > 10)
						npc.netSpam = 10;
				}
            }

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(23, -1, -1, null, npc.whoAmI, 0f, 0f, 0f, 0, 0, 0);
            }
        }

        private void Movement(float xPos, float yPos, float yPos2, Player player)
        {
            if (npc.position.Y > player.position.Y - yPos)
            {
                if (npc.velocity.Y > 0f)
                    npc.velocity.Y *= 0.98f;
                npc.velocity.Y -= (CalamityWorld.bossRushActive ? 0.2f : 0.15f);
                if (npc.velocity.Y > 5f)
                    npc.velocity.Y = 5f;
            }
            else if (npc.position.Y < player.position.Y - yPos2)
            {
                if (npc.velocity.Y < 0f)
                    npc.velocity.Y *= 0.98f;
                npc.velocity.Y += (CalamityWorld.bossRushActive ? 0.2f : 0.15f);
                if (npc.velocity.Y < -5f)
                    npc.velocity.Y = -5f;
            }
            if (npc.position.X + (float)(npc.width / 2) > player.position.X + (float)(player.width / 2) + xPos)
            {
                if (npc.velocity.X > 0f)
                    npc.velocity.X *= 0.98f;
                npc.velocity.X -= (CalamityWorld.bossRushActive ? 0.15f : 0.1f);
                if (npc.velocity.X > 8f)
                    npc.velocity.X = 8f;
            }
            if (npc.position.X + (float)(npc.width / 2) < player.position.X + (float)(player.width / 2) - xPos)
            {
                if (npc.velocity.X < 0f)
                    npc.velocity.X *= 0.98f;
                npc.velocity.X += (CalamityWorld.bossRushActive ? 0.15f : 0.1f);
                if (npc.velocity.X < -8f)
                    npc.velocity.X = -8f;
            }
        }

        public override bool CheckActive()
        {
            return canDespawn;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            for (int k = 0; k < 2; k++)
            {
                Dust.NewDust(npc.position, npc.width, npc.height, 46, hitDirection, -1f, 0, default, 1f);
            }
            if (npc.life <= 0)
            {
                Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/Pbg"), 2f);
                Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/Pbg2"), 2f);
                Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/Pbg3"), 2f);
                Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/Pbg4"), 2f);
                Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/Pbg5"), 2f);
                npc.position.X = npc.position.X + (float)(npc.width / 2);
                npc.position.Y = npc.position.Y + (float)(npc.height / 2);
                npc.width = 100;
                npc.height = 100;
                npc.position.X = npc.position.X - (float)(npc.width / 2);
                npc.position.Y = npc.position.Y - (float)(npc.height / 2);
                for (int num621 = 0; num621 < 40; num621++)
                {
                    int num622 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, 46, 0f, 0f, 100, default, 2f);
                    Main.dust[num622].velocity *= 3f;
                    if (Main.rand.NextBool(2))
                    {
                        Main.dust[num622].scale = 0.5f;
                        Main.dust[num622].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                    }
                }
                for (int num623 = 0; num623 < 70; num623++)
                {
                    int num624 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, 46, 0f, 0f, 100, default, 3f);
                    Main.dust[num624].noGravity = true;
                    Main.dust[num624].velocity *= 5f;
                    num624 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, 46, 0f, 0f, 100, default, 2f);
                    Main.dust[num624].velocity *= 2f;
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D texture = Main.npcTexture[npc.type];
            if (charging)
            {
                texture = ModContent.GetTexture("CalamityMod/NPCs/PlaguebringerGoliath/PlaguebringerGoliathChargeTex");
            }
            else
            {
                if (!flyingFrame2)
                {
                    texture = Main.npcTexture[npc.type];
                }
                else
                {
                    texture = ModContent.GetTexture("CalamityMod/NPCs/PlaguebringerGoliath/PlaguebringerGoliathAltTex");
                }
            }
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (npc.spriteDirection == 1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Color color24 = npc.GetAlpha(drawColor);
            Color color25 = Lighting.GetColor((int)((double)npc.position.X + (double)npc.width * 0.5) / 16, (int)(((double)npc.position.Y + (double)npc.height * 0.5) / 16.0));
            int num156 = Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type];
            int y3 = num156 * (int)npc.frameCounter;
            Rectangle rectangle = new Rectangle(0, y3, texture.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;
            int num157 = 8;
            int num158 = 2;
            int num159 = 1;
            float num160 = 0f;
            int num161 = num159;
            while (((num158 > 0 && num161 < num157) || (num158 < 0 && num161 > num157)) && Lighting.NotRetro)
            {
                Color color26 = npc.GetAlpha(color25);
                {
                    goto IL_6899;
                }
                IL_6881:
                num161 += num158;
                continue;
                IL_6899:
                float num164 = (float)(num157 - num161);
                if (num158 < 0)
                {
                    num164 = (float)(num159 - num161);
                }
                color26 *= num164 / ((float)NPCID.Sets.TrailCacheLength[npc.type] * 1.5f);
                Vector2 value4 = npc.oldPos[num161];
                float num165 = npc.rotation;
                Main.spriteBatch.Draw(texture, value4 + npc.Size / 2f - Main.screenPosition + new Vector2(0, npc.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color26, num165 + npc.rotation * num160 * (float)(num161 - 1) * -(float)spriteEffects.HasFlag(SpriteEffects.FlipHorizontally).ToDirectionInt(), origin2, npc.scale, spriteEffects, 0f);
                goto IL_6881;
            }
            var something = npc.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(texture, npc.Center - Main.screenPosition + new Vector2(0, npc.gfxOffY), npc.frame, color24, npc.rotation, npc.frame.Size() / 2, npc.scale, something, 0);
            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            npc.frameCounter += 1.0;
            if (npc.frameCounter > 4.0)
            {
                npc.frame.Y = npc.frame.Y + frameHeight;
                npc.frameCounter = 0.0;
            }
            if (npc.frame.Y >= frameHeight * 4)
            {
                npc.frame.Y = 0;
                if (!charging)
                {
                    flyingFrame2 = !flyingFrame2;
                }
            }
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public override void NPCLoot()
        {
            DropHelper.DropBags(npc);

            DropHelper.DropItemChance(npc, ModContent.ItemType<PlaguebringerGoliathTrophy>(), 10);
            DropHelper.DropItemCondition(npc, ModContent.ItemType<KnowledgePlaguebringerGoliath>(), true, !CalamityWorld.downedPlaguebringer);
            DropHelper.DropResidentEvilAmmo(npc, CalamityWorld.downedPlaguebringer, 4, 2, 1);

            // All other drops are contained in the bag, so they only drop directly on Normal
            if (!Main.expertMode)
            {
                // Materials
                DropHelper.DropItemSpray(npc, ModContent.ItemType<PlagueCellCluster>(), 10, 14);

                // Weapons
                DropHelper.DropItemChance(npc, ModContent.ItemType<VirulentKatana>(), 4); // Virulence
                DropHelper.DropItemChance(npc, ModContent.ItemType<DiseasedPike>(), 4);
                DropHelper.DropItemChance(npc, ModContent.ItemType<ThePlaguebringer>(), 4); // Pandemic
                DropHelper.DropItemChance(npc, ModContent.ItemType<Malevolence>(), 4);
                DropHelper.DropItemChance(npc, ModContent.ItemType<PestilentDefiler>(), 4);
                DropHelper.DropItemChance(npc, ModContent.ItemType<TheHive>(), 4);
                DropHelper.DropItemChance(npc, ModContent.ItemType<MepheticSprayer>(), 4); // Blight Spewer
                DropHelper.DropItemChance(npc, ModContent.ItemType<PlagueStaff>(), 4);
                DropHelper.DropItemChance(npc, ModContent.ItemType<TheSyringe>(), 4);
                DropHelper.DropItemChance(npc, ModContent.ItemType<FuelCellBundle>(), 4);
                DropHelper.DropItemChance(npc, ModContent.ItemType<InfectedRemote>(), 4);

                // Equipment
                DropHelper.DropItemChance(npc, ModContent.ItemType<BloomStone>(), 10);

                // Vanity
                DropHelper.DropItemChance(npc, ModContent.ItemType<PlaguebringerGoliathMask>(), 7);
            }

            // Mark PBG as dead
            CalamityWorld.downedPlaguebringer = true;
            CalamityMod.UpdateServerBoolean();
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.lifeMax = (int)(npc.lifeMax * 0.8f * bossLifeScale);
            npc.damage = (int)(npc.damage * 0.8f);
        }

        public override void OnHitPlayer(Player player, int damage, bool crit)
        {
            player.AddBuff(ModContent.BuffType<Plague>(), 300, true);
            if (CalamityWorld.revenge)
            {
                player.AddBuff(ModContent.BuffType<Horror>(), 180, true);
                player.AddBuff(ModContent.BuffType<MarkedforDeath>(), 180);
            }
        }
    }
}
