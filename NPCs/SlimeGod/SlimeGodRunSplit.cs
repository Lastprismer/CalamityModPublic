﻿using CalamityMod.Events;
using CalamityMod.Items.TreasureBags;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.SlimeGod
{
    [AutoloadBossHead]
    public class SlimeGodRunSplit : ModNPC
    {
        private float bossLife;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crimulan Slime God");
            Main.npcFrameCount[NPC.type] = 6;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.LifeMaxNERB(1005, 1205, 80000);
            double HPBoost = CalamityConfig.Instance.BossHealthBoost * 0.01;
            NPC.lifeMax += (int)(NPC.lifeMax * HPBoost);
            NPC.GetNPCDamage();
            NPC.width = 150;
            NPC.height = 92;
            NPC.scale = 0.8f;
            NPC.defense = 10;
            NPC.knockBackResist = 0f;
            AnimationType = NPCID.KingSlime;
            NPC.value = Item.buyPrice(0, 1, 0, 0);
            NPC.alpha = 55;
            NPC.lavaImmune = false;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            Music = CalamityMod.Instance.GetMusicFromMusicMod("SlimeGod") ?? MusicID.Boss1;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToSickness = false;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,

				// Will move to localization whenever that is cleaned up.
				new FlavorTextBestiaryInfoElement("Flanking their creator and serving as its body, these are concentrated, swirling masses of the world’s evils, each utilizing the rot and corrosion to eat away and assimilate those who oppose it")
            });
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.localAI[1]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.localAI[1] = reader.ReadSingle();
        }

        public override void AI()
        {
            if (CalamityGlobalNPC.slimeGodRed < 0 || !Main.npc[CalamityGlobalNPC.slimeGodRed].active)
                CalamityGlobalNPC.slimeGodRed = NPC.whoAmI;

            bool malice = CalamityWorld.malice || BossRushEvent.BossRushActive;
            bool expertMode = Main.expertMode || BossRushEvent.BossRushActive;
            bool revenge = CalamityWorld.revenge || BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || NPC.localAI[1] == 1f || BossRushEvent.BossRushActive;
            NPC.Calamity().CurrentlyEnraged = !BossRushEvent.BossRushActive && malice;

            float lifeRatio = NPC.life / (float)NPC.lifeMax;

            NPC.defense = NPC.defDefense;
            NPC.damage = NPC.defDamage;
            if (NPC.localAI[1] == 1f)
            {
                NPC.defense = NPC.defDefense + 20;
                NPC.damage = NPC.defDamage + 22;
            }

            NPC.aiAction = 0;
            NPC.noTileCollide = false;
            NPC.noGravity = false;

            // Get a target
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            // Despawn safety, make sure to target another player if the current player target is too far away
            if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];

            if (NPC.ai[0] != 3f && (player.dead || !player.active))
            {
                NPC.TargetClosest();
                player = Main.player[NPC.target];
                if (player.dead || !player.active)
                {
                    NPC.ai[0] = 3f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.timeLeft < 1800)
                NPC.timeLeft = 1800;

            bool flag100 = false;
            bool hyperMode = NPC.localAI[1] == 1f;
            if (CalamityGlobalNPC.slimeGodPurple != -1)
            {
                if (Main.npc[CalamityGlobalNPC.slimeGodPurple].active)
                {
                    flag100 = true;
                }
            }
            if (CalamityGlobalNPC.slimeGod < 0 || !Main.npc[CalamityGlobalNPC.slimeGod].active)
            {
                NPC.localAI[1] = 0f;
                hyperMode = true;
                flag100 = false;
            }
            if (malice)
            {
                flag100 = false;
                hyperMode = true;
            }

            if (NPC.localAI[1] != 1f)
            {
                if (!flag100)
                    NPC.defense = NPC.defDefense * 2;
            }

            float distanceSpeedBoost = Vector2.Distance(player.Center, NPC.Center) * ((malice || hyperMode) ? 0.008f : 0.005f);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.localAI[0] += flag100 ? 0.5f : 1f;
                if (revenge)
                    NPC.localAI[0] += 0.5f;
                if (malice)
                    NPC.localAI[0] += 1f;

                if (NPC.localAI[0] >= 450f && Vector2.Distance(player.Center, NPC.Center) > 160f)
                {
                    NPC.localAI[0] = 0f;
                    float num179 = expertMode ? 10f : 8f;
                    Vector2 value9 = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                    float num180 = player.position.X + (float)player.width * 0.5f - value9.X;
                    float num181 = Math.Abs(num180) * 0.1f;
                    float num182 = player.position.Y + (float)player.height * 0.5f - value9.Y - num181;
                    float num183 = (float)Math.Sqrt((double)(num180 * num180 + num182 * num182));
                    num183 = num179 / num183;
                    num180 *= num183;
                    num182 *= num183;
                    int type = ModContent.ProjectileType<AbyssBallVolley2>();
                    int damage = NPC.GetProjectileDamage(type);
                    value9.X += num180;
                    value9.Y += num182;
                    num180 = player.position.X + (float)player.width * 0.5f - value9.X;
                    num182 = player.position.Y + (float)player.height * 0.5f - value9.Y;
                    num183 = (float)Math.Sqrt((double)(num180 * num180 + num182 * num182));
                    num183 = num179 / num183;
                    num180 *= num183;
                    num182 *= num183;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), value9.X, value9.Y, num180, num182, type, damage, 0f, Main.myPlayer, 0f, 0f);
                }
            }

            if (NPC.ai[0] == 0f)
            {
                if (NPC.velocity.Y == 0f)
                {
                    NPC.TargetClosest();
                    NPC.velocity.X *= 0.8f;
                    NPC.ai[1] += (!flag100 || hyperMode) ? 2f : 1f;

                    float jumpGateValue = 35f;
                    float velocityX = death ? 11.5f : revenge ? 10.5f : expertMode ? 9.5f : 7.5f;
                    if (revenge)
                    {
                        float moveBoost = death ? 11f * (1f - lifeRatio) : 7f * (1f - lifeRatio);
                        float speedBoost = death ? 2.25f * (1f - lifeRatio) : 1.5f * (1f - lifeRatio);
                        jumpGateValue -= moveBoost;
                        velocityX += speedBoost;
                    }

                    float distanceBelowTarget = NPC.position.Y - (player.position.Y + 80f);
                    float speedMult = 1f;
                    if (distanceBelowTarget > 0f)
                        speedMult += distanceBelowTarget * 0.002f;

                    if (speedMult > 2f)
                        speedMult = 2f;

                    float velocityY = 4f;
                    if (!Collision.CanHit(NPC.Center, 1, 1, player.Center, 1, 1))
                        velocityY += 2f;

                    if (NPC.ai[1] > jumpGateValue)
                    {
                        velocityY *= 0.75f;
                        velocityX *= 1.25f;
                        NPC.ai[1] = 0f;
                        NPC.velocity.Y -= velocityY * speedMult;
                        NPC.velocity.X = (velocityX + distanceSpeedBoost) * NPC.direction;
                        NPC.netUpdate = true;
                    }
                    else if (NPC.ai[1] >= jumpGateValue - 30f)
                        NPC.aiAction = 1;
                }
                else
                {
                    NPC.velocity.X *= 0.99f;
                    if (NPC.direction < 0 && NPC.velocity.X > -1f)
                        NPC.velocity.X = -1f;
                    if (NPC.direction > 0 && NPC.velocity.X < 1f)
                        NPC.velocity.X = 1f;
                }

                NPC.ai[2] += 1f;
                if (revenge)
                    NPC.ai[2] += death ? 1f * (1f - lifeRatio) : 0.5f * (1f - lifeRatio);

                if (NPC.velocity.Y == 0f)
                {
                    float phaseSwitchGateValue = 180f;
                    if (NPC.ai[2] >= phaseSwitchGateValue)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            switch ((int)NPC.localAI[2])
                            {
                                default:
                                case 1:
                                    NPC.ai[0] = 2f;
                                    break;

                                case 2:
                                    NPC.ai[0] = 1f;
                                    break;
                            }

                            if (NPC.ai[0] == 1f)
                            {
                                NPC.noTileCollide = true;
                                NPC.velocity.Y = death ? -11f : revenge ? -10f : expertMode ? -9f : -8f;
                            }

                            NPC.ai[1] = 0f;
                            NPC.ai[2] = 0f;
                            NPC.ai[3] = 0f;
                            NPC.netUpdate = true;
                        }
                    }
                    else if (NPC.ai[1] >= phaseSwitchGateValue - 30f)
                        NPC.aiAction = 1;
                }
            }
            else if (NPC.ai[0] == 1f)
            {
                NPC.noTileCollide = true;
                NPC.noGravity = true;

                if (NPC.velocity.X < 0f)
                    NPC.direction = -1;
                else
                    NPC.direction = 1;

                NPC.spriteDirection = NPC.direction;
                NPC.TargetClosest();
                Vector2 center40 = player.Center;
                center40.Y -= 350f;
                Vector2 vector272 = center40 - NPC.Center;

                if (NPC.ai[2] == 1f)
                {
                    NPC.ai[1] += 1f;
                    vector272 = player.Center - NPC.Center;
                    vector272.Normalize();
                    vector272 *= death ? 11f : revenge ? 10f : expertMode ? 9f : 8f;
                    NPC.velocity = (NPC.velocity * 4f + vector272) / 5f;

                    if (NPC.ai[1] > 12f)
                    {
                        NPC.ai[1] = 0f;
                        NPC.ai[0] = 1.1f;
                        NPC.ai[2] = 0f;
                        NPC.velocity = vector272;
                    }
                }
                else
                {
                    if (Math.Abs(NPC.Center.X - player.Center.X) < 40f && NPC.Center.Y < player.Center.Y - 300f)
                    {
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 1f;
                        return;
                    }

                    vector272.Normalize();
                    vector272 *= (death ? 13f : revenge ? 12f : expertMode ? 11f : 10f) + distanceSpeedBoost;
                    NPC.velocity = (NPC.velocity * 5f + vector272) / 6f;
                }
            }
            else if (NPC.ai[0] == 1.1f)
            {
                bool atTargetPosition = NPC.position.Y + NPC.height >= player.position.Y;
                if (NPC.ai[2] == 0f && (atTargetPosition || NPC.localAI[1] == 0f) && Collision.CanHit(NPC.Center, 1, 1, player.Center, 1, 1) && !Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                {
                    NPC.ai[2] = 1f;
                    NPC.netUpdate = true;
                }

                if (atTargetPosition || NPC.velocity.Y <= 0f)
                {
                    NPC.ai[1] += 1f;
                    if (NPC.ai[1] > 10f)
                    {
                        NPC.localAI[2] = NPC.ai[0] - 0.1f;
                        NPC.ai[0] = 0f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.ai[3] = 0f;
                        NPC.netUpdate = true;
                    }
                }
                else if (NPC.ai[2] == 0f)
                    NPC.noTileCollide = true;

                NPC.noGravity = true;

                NPC.velocity.Y += 0.5f;
                float velocityLimit = malice ? 22f : death ? 16f : revenge ? 15f : expertMode ? 14f : 13f;
                if (NPC.velocity.Y > velocityLimit)
                    NPC.velocity.Y = velocityLimit;
            }
            else if (NPC.ai[0] == 2f)
            {
                if (NPC.velocity.Y == 0f)
                {
                    NPC.TargetClosest();
                    NPC.velocity.X *= 0.8f;

                    NPC.ai[1] += 1f;
                    if (NPC.ai[1] > 15f)
                    {
                        NPC.ai[1] = 0f;

                        NPC.velocity.Y -= 4f;
                        if (player.position.Y + player.height < NPC.Center.Y)
                            NPC.velocity.Y -= 1.25f;
                        if (player.position.Y + player.height < NPC.Center.Y - 40f)
                            NPC.velocity.Y -= 1.5f;
                        if (player.position.Y + player.height < NPC.Center.Y - 80f)
                            NPC.velocity.Y -= 1.75f;
                        if (player.position.Y + player.height < NPC.Center.Y - 120f)
                            NPC.velocity.Y -= 2f;
                        if (player.position.Y + player.height < NPC.Center.Y - 160f)
                            NPC.velocity.Y -= 2.25f;
                        if (player.position.Y + player.height < NPC.Center.Y - 200f)
                            NPC.velocity.Y -= 2.5f;
                        if (!Collision.CanHit(NPC.Center, 1, 1, player.Center, 1, 1))
                            NPC.velocity.Y -= 2f;

                        NPC.velocity.X = ((death ? 12f : revenge ? 11f : expertMode ? 10f : 9f) + distanceSpeedBoost) * NPC.direction;
                        NPC.ai[2] += 1f;
                    }
                    else
                        NPC.aiAction = 1;
                }
                else
                {
                    NPC.velocity.X *= 0.98f;
                    float velocityLimit = (death ? 7f : revenge ? 6f : expertMode ? 5f : 4f) + distanceSpeedBoost;
                    if (NPC.direction < 0 && NPC.velocity.X > -velocityLimit)
                        NPC.velocity.X = -velocityLimit;
                    if (NPC.direction > 0 && NPC.velocity.X < velocityLimit)
                        NPC.velocity.X = velocityLimit;
                }

                if (NPC.ai[2] >= 3f && NPC.velocity.Y == 0f)
                {
                    NPC.localAI[2] = NPC.ai[0];
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[0] == 3f)
            {
                NPC.noTileCollide = true;
                NPC.alpha += 7;

                if (NPC.timeLeft > 10)
                    NPC.timeLeft = 10;

                if (NPC.alpha > 255)
                    NPC.alpha = 255;

                NPC.velocity.X *= 0.98f;
            }

            int num658 = Dust.NewDust(NPC.position, NPC.width, NPC.height, 260, NPC.velocity.X, NPC.velocity.Y, 255, new Color(0, 80, 255, 80), NPC.scale * 1.5f);
            Main.dust[num658].noGravity = true;
            Main.dust[num658].velocity *= 0.5f;

            if (bossLife == 0f && NPC.life > 0)
                bossLife = NPC.lifeMax;

            float num644 = 1f;
            if (NPC.life > 0)
            {
                float num659 = lifeRatio;
                num659 = num659 * 0.5f + 0.75f;
                num659 *= num644;

                if (num659 != NPC.scale)
                {
                    NPC.position.X = NPC.position.X + (float)(NPC.width / 2);
                    NPC.position.Y = NPC.position.Y + (float)NPC.height;
                    NPC.scale = num659 * 0.75f;
                    NPC.width = (int)(150f * NPC.scale);
                    NPC.height = (int)(92f * NPC.scale);
                    NPC.position.X = NPC.position.X - (float)(NPC.width / 2);
                    NPC.position.Y = NPC.position.Y - (float)NPC.height;
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int num660 = (int)((double)NPC.lifeMax * 0.15);
                    if ((float)(NPC.life + num660) < bossLife)
                    {
                        bossLife = (float)NPC.life;
                        int x = (int)(NPC.position.X + (float)Main.rand.Next(NPC.width - 32));
                        int y = (int)(NPC.position.Y + (float)Main.rand.Next(NPC.height - 32));
                        int num663 = ModContent.NPCType<SlimeSpawnCrimson>();
                        if (Main.rand.NextBool(3))
                        {
                            num663 = ModContent.NPCType<SlimeSpawnCrimson2>();
                        }
                        int num664 = NPC.NewNPC(NPC.GetSource_FromAI(), x, y, num663, 0, 0f, 0f, 0f, 0f, 255);
                        Main.npc[num664].SetDefaults(num663);
                        Main.npc[num664].velocity.X = (float)Main.rand.Next(-15, 16) * 0.1f;
                        Main.npc[num664].velocity.Y = (float)Main.rand.Next(-30, 1) * 0.1f;
                        Main.npc[num664].ai[0] = (float)(-1000 * Main.rand.Next(3));
                        Main.npc[num664].ai[1] = 0f;
                        if (Main.netMode == NetmodeID.Server && num664 < 200)
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, num664, 0f, 0f, 0f, 0, 0, 0);
                    }
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            Color lightColor = new Color(Main.DiscoR, 100, 150, NPC.alpha);
            Color newColor = NPC.localAI[1] == 1f ? lightColor : drawColor;
            return newColor;
        }

        public override void OnKill()
        {
            if (!CalamityWorld.revenge)
            {
                int heartAmt = Main.rand.Next(3) + 3;
                for (int i = 0; i < heartAmt; i++)
                    Item.NewItem(NPC.GetSource_Loot(), (int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height, ItemID.Heart);
            }

            if (SlimeGodCore.LastSlimeGodStanding())
                SlimeGodCore.RealOnKill(NPC);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot) => SlimeGodCore.DefineSlimeGodLoot(npcLoot);

        public override bool CheckActive()
        {
            if (CalamityGlobalNPC.slimeGod != -1)
            {
                if (Main.npc[CalamityGlobalNPC.slimeGod].active)
                    return false;
            }
            return true;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, 4, hitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                NPC.position.X = NPC.position.X + (float)(NPC.width / 2);
                NPC.position.Y = NPC.position.Y + (float)(NPC.height / 2);
                NPC.width = 50;
                NPC.height = 50;
                NPC.position.X = NPC.position.X - (float)(NPC.width / 2);
                NPC.position.Y = NPC.position.Y - (float)(NPC.height / 2);
                for (int num621 = 0; num621 < 40; num621++)
                {
                    int num622 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, 4, 0f, 0f, 100, default, 2f);
                    Main.dust[num622].velocity *= 3f;
                    if (Main.rand.NextBool(2))
                    {
                        Main.dust[num622].scale = 0.5f;
                        Main.dust[num622].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                    }
                }
                for (int num623 = 0; num623 < 70; num623++)
                {
                    int num624 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, 4, 0f, 0f, 100, default, 3f);
                    Main.dust[num624].noGravity = true;
                    Main.dust[num624].velocity *= 5f;
                    num624 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, 4, 0f, 0f, 100, default, 2f);
                    Main.dust[num624].velocity *= 2f;
                }
            }
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * bossLifeScale);
        }

        public override void OnHitPlayer(Player player, int damage, bool crit)
        {
            player.AddBuff(BuffID.Darkness, 240, true);
        }
    }
}
