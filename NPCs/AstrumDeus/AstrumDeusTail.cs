﻿using System.IO;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Events;
using CalamityMod.NPCs.CalamityAIs.CalamityBossAIs;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.AstrumDeus
{
    public class AstrumDeusTail : ModNPC
    {
        public override LocalizedText DisplayName => CalamityUtils.GetText("NPCs.AstrumDeusHead.DisplayName");

        public static Asset<Texture2D> GlowTexture;
        public static Asset<Texture2D> GlowTexture2;

        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            NPCID.Sets.TrailingMode[NPC.type] = 1;
            if (!Main.dedServ)
            {
                GlowTexture = ModContent.Request<Texture2D>(Texture + "Glow", AssetRequestMode.AsyncLoad);
                GlowTexture2 = ModContent.Request<Texture2D>(Texture + "Glow2", AssetRequestMode.AsyncLoad);
            }
        }

        public override void SetDefaults()
        {
            NPC.GetNPCDamage();
            NPC.npcSlots = 5f;
            NPC.width = 52;
            NPC.height = 68;
            NPC.defense = 50;
            NPC.DR_NERD(0.4f);
            NPC.LifeMaxNERB(200000, 240000, 650000);
            double HPBoost = CalamityConfig.Instance.BossHealthBoost * 0.01;
            NPC.lifeMax += (int)(NPC.lifeMax * HPBoost);
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;

            if (BossRushEvent.BossRushActive)
                NPC.scale *= 1.5f;
            else if (CalamityWorld.death)
                NPC.scale *= 1.4f;
            else if (CalamityWorld.revenge)
                NPC.scale *= 1.35f;
            else if (Main.expertMode)
                NPC.scale *= 1.2f;

            NPC.alpha = 255;
            NPC.behindTiles = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = AstrumDeusHead.HitSound;
            NPC.DeathSound = AstrumDeusHead.DeathSound;
            NPC.netAlways = true;
            NPC.boss = true;
            NPC.dontCountMe = true;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToSickness = false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.dontTakeDamage);
            for (int i = 0; i < 4; i++)
                writer.Write(NPC.Calamity().newAI[i]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.dontTakeDamage = reader.ReadBoolean();
            for (int i = 0; i < 4; i++)
                NPC.Calamity().newAI[i] = reader.ReadSingle();
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }

        public override void AI()
        {
            AstrumDeusAI.VanillaAstrumDeusAI(NPC, Mod, false);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return true;

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            bool drawCyan = NPC.Calamity().newAI[3] >= (Main.getGoodWorld ? 300f : 600f);
            bool deathModeEnragePhase = Main.npc[(int)NPC.ai[2]].Calamity().newAI[0] == 3f;
            bool doubleWormPhase = NPC.Calamity().newAI[0] != 0f && !deathModeEnragePhase;

            Texture2D wormTexture = TextureAssets.Npc[NPC.type].Value;
            Vector2 halfSizeTex = new Vector2(TextureAssets.Npc[NPC.type].Value.Width / 2, TextureAssets.Npc[NPC.type].Value.Height / 2);

            Vector2 drawLocation = NPC.Center - screenPos;
            drawLocation -= new Vector2(wormTexture.Width, wormTexture.Height) * NPC.scale / 2f;
            drawLocation += halfSizeTex * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            spriteBatch.Draw(wormTexture, drawLocation, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTex, NPC.scale, spriteEffects, 0f);

            wormTexture = GlowTexture.Value;
            Color phaseColor = drawCyan ? Color.Cyan : Color.Orange;
            if (doubleWormPhase)
                wormTexture = drawCyan ? GlowTexture2.Value : wormTexture;

            Color wormColorLerp = Color.Lerp(Color.White, doubleWormPhase ? phaseColor : Color.Orange, 0.5f) * (deathModeEnragePhase ? 1f : NPC.Opacity);

            int timesToDraw = deathModeEnragePhase ? 3 : drawCyan ? 2 : 1;
            for (int i = 0; i < timesToDraw; i++)
                spriteBatch.Draw(wormTexture, drawLocation, NPC.frame, wormColorLerp, NPC.rotation, halfSizeTex, NPC.scale, spriteEffects, 0f);

            return false;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return !NPC.dontTakeDamage;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0 && !Main.zenithWorld) // I value people's computers
            {
                NPC.position.X = NPC.position.X + (NPC.width / 2);
                NPC.position.Y = NPC.position.Y + (NPC.height / 2);
                NPC.width = 50;
                NPC.height = 50;
                NPC.position.X = NPC.position.X - (NPC.width / 2);
                NPC.position.Y = NPC.position.Y - (NPC.height / 2);
                for (int i = 0; i < 5; i++)
                {
                    int purpleDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.PurpleCosmilite, 0f, 0f, 100, default, 2f);
                    Main.dust[purpleDust].velocity *= 3f;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[purpleDust].scale = 0.5f;
                        Main.dust[purpleDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                }
                for (int j = 0; j < 10; j++)
                {
                    int astralDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 100, default, 3f);
                    Main.dust[astralDust].noGravity = true;
                    Main.dust[astralDust].velocity *= 5f;
                    astralDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 100, default, 2f);
                    Main.dust[astralDust].velocity *= 2f;
                }
            }
        }

        public override bool CheckActive()
        {
            return false;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
                target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 75, true);
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance * bossAdjustment);
            NPC.damage = (int)(NPC.damage * NPC.GetExpertDamageMultiplier());
        }
    }
}
