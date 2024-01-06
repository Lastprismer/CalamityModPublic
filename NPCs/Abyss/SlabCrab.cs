﻿using CalamityMod.BiomeManagers;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Placeables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.Abyss
{
    public class SlabCrab : ModNPC
    {
        enum AIState
        {
            Hiding = 0,
            IdleAnim = 1,
            Enraged = 2,
            Active = 3
        }

        public Player Target => Main.player[NPC.target];
        public ref float CurrentPhase => ref NPC.ai[0];
        public ref float AITimer => ref NPC.ai[1];
        public ref float HopTimer => ref NPC.ai[2];
        public ref float CalmDownTimer => ref NPC.ai[3];
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 23;
        }

        public override void SetDefaults()
        {
            NPC.width = 44;
            NPC.height = 30;

            NPC.damage = 20;
            NPC.lifeMax = 300;

            NPC.aiStyle = AIType = -1;

            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 0, 2, 0);
            NPC.lavaImmune = true;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.chaseable = false;
            NPC.knockBackResist = 0f;
            NPC.defense = 999998;
            NPC.HitSound = SoundID.NPCHit33;
            NPC.DeathSound = SoundID.NPCDeath36;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<SlabCrabBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            NPC.GravityIgnoresLiquid = true;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<AbyssLayer1Biome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.SlabCrab")
            });
        }

        public override void AI()
        {
            NPC.direction = Math.Sign(NPC.velocity.X);
            NPC.spriteDirection = -NPC.direction;
            NPC.localAI[0]++;
            if (NPC.localAI[0] < 90)
                return;
            // Enables expert scaling, if damage is 0 in set defaults expert scaling will not happen.
            NPC.damage = 0;
            switch (CurrentPhase)
            {
                case (int)AIState.Hiding:
                    NPC.ShowNameOnHover = false;
                    NPC.chaseable = false;
                    NPC.defense = 999998;
                    NPC.velocity.X = 0;
                    AITimer++;
                    // if the block below it is mined, instantly start running
                    //if (NPC.velocity.Y > 0)
                    {
                        //ChangePhase((int)AIState.Active);
                    }
                    // randomly start looking around after a bit
                    if (AITimer > 300 && Main.rand.NextBool(420))
                    {
                        ChangePhase((int)AIState.IdleAnim);
                    }
                    break;
                case (int)AIState.IdleAnim:
                    NPC.ShowNameOnHover = true;
                    NPC.chaseable = false;
                    NPC.defense = 999998;
                    AITimer++;
                    NPC.velocity.X = 0;
                    // if the block below it is mined, instantly start running
                    if (NPC.velocity.Y > 0)
                    {
                        ChangePhase((int)AIState.Active);
                    }
                    NPC.TargetClosest();
                    // if a player gets too close while it's looking, pop out
                    if (Target.Distance(NPC.Center) < 480 && Collision.CanHitLine(NPC.Center, 1, 1, Target.Center, 1, 1))
                    {
                        ChangePhase((int)AIState.Enraged);
                    }
                    // if the animation finishes, go back to stone mode
                    if (AITimer > 90)
                    {
                        ChangePhase((int)AIState.Hiding);
                    }
                    break;
                case (int)AIState.Enraged:
                    NPC.ShowNameOnHover = true;
                    AITimer++;
                    NPC.defense = 10;
                    NPC.knockBackResist = 0f;
                    NPC.chaseable = false;
                    // give the animation time to play out then start attacking
                    if (AITimer > 24)
                    {
                        ChangePhase((int)AIState.Active);
                    }
                    break;
                case (int)AIState.Active:
                    NPC.ShowNameOnHover = true;
                    NPC.TargetClosest();
                    NPC.defense = 10;
                    NPC.knockBackResist = 1f;
                    NPC.damage = 20;
                    NPC.chaseable = true;
                    bool outofRange = ((Target.Center.Distance(NPC.Center) > 600) || (Target.Center.Distance(NPC.Center) > 320 && !Collision.CanHitLine(NPC.Center, 1, 1, Target.Center, 1, 1)));
                    if (outofRange)
                    {
                        CalmDownTimer++;
                    }
                    else if (CalmDownTimer > 0)
                    {
                        CalmDownTimer--;
                    }
                    if (NPC.velocity.Y == 0f)
                    {
                        AITimer++;
                        NPC.knockBackResist = 0.6f;
                        NPC.TargetClosest(true);
                        NPC.velocity.X *= 0.85f;

                        float hopRate = MathHelper.Lerp(25f, 10f, 1f - NPC.life / (float)NPC.lifeMax);
                        float lungeForwardSpeed = 6f;
                        float jumpSpeed = 7f;
                        if (Collision.CanHit(NPC.Center, 1, 1, Target.Center, 1, 1))
                            lungeForwardSpeed *= 1.2f;

                        if (Main.netMode != NetmodeID.MultiplayerClient && AITimer > hopRate)
                        {
                            HopTimer++;

                            // Make a bigger leap every 3 hops.
                            if (HopTimer % 3f == 2f)
                                lungeForwardSpeed *= 1.5f;

                            AITimer = 0f;
                            NPC.velocity.Y -= jumpSpeed;
                            NPC.velocity.X = lungeForwardSpeed * NPC.direction;
                            NPC.netUpdate = true;
                        }
                    }
                    else
                    {
                        NPC.knockBackResist = 0.2f;
                        NPC.velocity.X *= 0.995f;
                    }
                    // go back to hiding if on the ground, has been angri for over 5 seconds, and is far enough from the player (distance reduced if no line of sight)
                    if (CalmDownTimer > 300 && outofRange && NPC.velocity.Y == 0 && Main.rand.NextBool(180))
                    {
                        ChangePhase(0);
                    }
                    break;
            }
            if (NPC.velocity.Y > 0)
            {
                NPC.velocity.Y *= 1.1f;
            }
        }

        public void ChangePhase(float ai0, float ai1 = -1, float ai2 = -1, float ai3 = -1)
        {
            CurrentPhase = ai0;
            AITimer = ai1 == -1 ? 0 : ai1;
            HopTimer = ai2 == -1 ? 0 : ai2;    
            CalmDownTimer = ai3 == -1 ? 0 : ai3;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            // Don't draw the bar if in stealth mode.
            if (CurrentPhase < (int)AIState.Enraged)
                return false;
            return null;
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneAbyssLayer1 && spawnInfo.Water)
            {
                return SpawnCondition.OceanMonster.Chance;
            }
            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<SulphurousShale>(), 5, 10, 30);
        }

        public override void FindFrame(int frameHeight)
        {
            // walkie
            if (NPC.IsABestiaryIconDummy || CurrentPhase == (int)AIState.Active)
            {
                if (NPC.velocity.Y == 0)
                {
                    if (NPC.frameCounter++ % 6 == 0)
                    {
                        NPC.frame.Y += frameHeight;
                    }
                }
                if (NPC.frame.Y > frameHeight * 22 || NPC.frame.Y < frameHeight * 19)
                {
                    NPC.frame.Y = frameHeight * 19;
                }
                return;
            }
            switch (CurrentPhase)
            {
                case (int)AIState.Hiding:
                    NPC.frame.Y = 0;
                    break;
                case (int)AIState.IdleAnim:
                    if (NPC.frameCounter++ % 6 == 0)
                    {
                        NPC.frame.Y += frameHeight;
                    }
                    if (NPC.frame.Y > frameHeight * 14)
                    {
                        NPC.frame.Y = frameHeight * 0;
                    }
                    break;
                case (int)AIState.Enraged:
                    if (NPC.frame.Y < frameHeight * 18)
                    {
                        NPC.frameCounter++;
                    }
                    if (NPC.frame.Y > frameHeight * 18 || NPC.frame.Y < frameHeight * 15)
                    {
                        NPC.frame.Y = frameHeight * 15;
                    }
                    if (NPC.frameCounter == 6)
                    {
                        NPC.frame.Y += frameHeight;
                        NPC.frameCounter = 0;
                    }
                    break;
                default:
                    if (NPC.frameCounter++ % 6 == 0)
                    {
                        NPC.frame.Y += frameHeight;
                    }
                    if (NPC.frame.Y > frameHeight * 22 || NPC.frame.Y < frameHeight * 19)
                    {
                        NPC.frame.Y = frameHeight * 19;
                    }
                    break;
            }
        }

        public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (CurrentPhase < (int)AIState.Enraged)
            {
                ChangePhase((int)AIState.Enraged);
            }
        }

        public override bool? CanBeHitByItem(Player player, Item item) => CurrentPhase > (int)AIState.IdleAnim || (item.pick > 0 && new Rectangle((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, 20, 20).Intersects(NPC.getRect())); // can only be hit by pickaxes while hidden

        public override bool? CanBeHitByProjectile(Projectile projectile) => CurrentPhase > (int)AIState.IdleAnim;

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Water, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 15; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Water, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
                target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 90);
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!NPC.IsABestiaryIconDummy)
            {
                Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/NPCs/Abyss/SlabCrabGlow").Value;

                var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                Main.EntitySpriteDraw(tex, NPC.Center - Main.screenPosition + new Vector2(0, NPC.gfxOffY + 4),
                NPC.frame, Color.White * 0.5f, NPC.rotation, NPC.frame.Size() / 2f, NPC.scale, effects, 0);
            }
        }
    }
}
