﻿using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.Balancing;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Cooldowns;
using CalamityMod.Dusts;
using CalamityMod.EntitySources;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Prismatic;
using CalamityMod.Items.Armor.Reaver;
using CalamityMod.Items.Potions.Alcohol;
using CalamityMod.Items.VanillaArmorChanges;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.NPCs;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Particles;
using CalamityMod.Projectiles;
using CalamityMod.Projectiles.Healing;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.CalPlayer
{
    public partial class CalamityPlayer : ModPlayer
    {
        #region On Hit Anything
        public override void OnHitAnything(float x, float y, Entity victim)
        {
            rageCombatFrames = BalancingConstants.RageCombatDelayTime;

            if (AdamantiteSet)
            {
                adamantiteSetDefenseBoostInterpolant += 1.75f / AdamantiteArmorSetChange.TimeUntilBoostCompletelyDecays;
                adamantiteSetDefenseBoostInterpolant = MathHelper.Clamp(adamantiteSetDefenseBoostInterpolant, 0f, 1f);
                AdamantiteSetDecayDelay = AdamantiteArmorSetChange.TimeUntilDecayBeginsAfterAttacking;
            }
        }
        #endregion

        #region On Hit NPC
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)/* tModPorter If you don't need the Item, consider using OnHitNPC instead */
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            // Handle on-hit melee effects for the gem tech armor set.
            GemTechState.MeleeOnHitEffects(target);

            // Handle on-hit melee effects for the mythril armor set.
            MythrilArmorSetChange.OnHitEffects(target, damageDone, Player);

            if (witheringWeaponEnchant)
                witheringDamageDone += (int)(damageDone * (hit.Crit ? 2D : 1D));

            if (flamingItemEnchant)
                target.AddBuff(BuffType<VulnerabilityHex>(), VulnerabilityHex.AflameDuration);

            target.Calamity().IncreasedColdEffects_EskimoSet = eskimoSet;
            target.Calamity().IncreasedColdEffects_CryoStone = CryoStone;

            target.Calamity().IncreasedElectricityEffects_Transformer = transformer;

            target.Calamity().IncreasedHeatEffects_Fireball = fireball;
            target.Calamity().IncreasedHeatEffects_CinnamonRoll = cinnamonRoll;
            target.Calamity().IncreasedHeatEffects_FlameWakerBoots = flameWakerBoots;
            target.Calamity().IncreasedHeatEffects_HellfireTreads = hellfireTreads;

            target.Calamity().IncreasedSicknessEffects_ToxicHeart = toxicHeart;

            target.Calamity().IncreasedSicknessAndWaterEffects_EvergreenGin = evergreenGin;

            switch (item.type)
            {
                case ItemID.CobaltSword:
                    target.Calamity().cobaltNerfTimer = CalamityGlobalNPC.cobaltAndMythrilNerfTime;
                    break;

                case ItemID.PalladiumSword:
                    if (Player.moonLeech)
                        return;
                    Player.lifeRegenTime += 2;
                    break;

                case ItemID.MythrilSword:
                    target.Calamity().mythrilNerfTimer = CalamityGlobalNPC.cobaltAndMythrilNerfTime;
                    break;

                case ItemID.OrichalcumSword:
                    if (Player.petalTimer > 0)
                        Player.petalTimer /= 2;
                    break;

                case ItemID.AdamantiteSword:
                    float slowDownMult = 0.5f;
                    if (CalamityLists.enemyImmunityList.Contains(target.type) || target.boss)
                        slowDownMult = 0.95f;
                    target.velocity *= slowDownMult;
                    break;

                case ItemID.CandyCaneSword:

                    if (Player.moonLeech || Player.lifeSteal <= 0f || target.lifeMax <= 5)
                        return;

                    int heal = 2;
                    Player.lifeSteal -= heal;
                    Player.statLife += heal;
                    Player.HealEffect(heal);
                    if (Player.statLife > Player.statLifeMax2)
                        Player.statLife = Player.statLifeMax2;

                    break;

                case ItemID.DeathSickle:
                    target.AddBuff(BuffType<WhisperingDeath>(), 120);
                    break;

                case ItemID.BeeKeeper:
                case ItemID.BladeofGrass:
                    target.AddBuff(BuffID.Poisoned, 240);
                    break;

                case ItemID.FieryGreatsword:
                    target.AddBuff(BuffID.OnFire3, 180);
                    break;

                case ItemID.IceSickle:
                case ItemID.Frostbrand:
                    target.AddBuff(BuffID.Frostburn2, 300);
                    break;

                case ItemID.IceBlade:
                    target.AddBuff(BuffID.Frostburn, 120);
                    break;

                case (>= ItemID.BluePhaseblade and <= ItemID.YellowPhaseblade) or ItemID.OrangePhaseblade:
                case (>= ItemID.BluePhasesaber and <= ItemID.YellowPhasesaber) or ItemID.OrangePhasesaber:
                    // TODO: find an EPIC lightsaber sound
                    break;
            }

            if (flameWakerBoots)
                target.AddBuff(BuffID.OnFire, 120);

            if (hellfireTreads)
            {
                if (Main.rand.NextBool(4))
                    target.AddBuff(BuffID.OnFire3, 360);
                else if (Main.rand.NextBool())
                    target.AddBuff(BuffID.OnFire3, 240);
                else
                    target.AddBuff(BuffID.OnFire3, 120);
            }

            ItemLifesteal(target, item, damageDone);
            ItemOnHit(item, damageDone, target.Center, hit.Crit, target.IsAnEnemy(false, true));
            NPCDebuffs(target, item.CountsAsClass<MeleeDamageClass>(), item.CountsAsClass<RangedDamageClass>(), item.CountsAsClass<MagicDamageClass>(), item.CountsAsClass<SummonDamageClass>(), item.CountsAsClass<ThrowingDamageClass>(), item.CountsAsClass<SummonMeleeSpeedDamageClass>());

            // Shattered Community tracks all damage dealt with Rage Mode (ignoring dummies).
            if (target.type == NPCID.TargetDummy || target.type == NPCType<SuperDummyNPC>())
                return;

            if (rageModeActive && shatteredCommunity)
                Player.GetModPlayer<ShatteredCommunityPlayer>().AccumulateRageDamage(damageDone);
        }
        #endregion

        #region On Hit NPC With Proj
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)/* tModPorter If you don't need the Projectile, consider using OnHitNPC instead */
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            CalamityGlobalNPC cgn = target.Calamity();

            // Handle on-hit melee effects for the gem tech armor set.
            if (proj.CountsAsClass<MeleeDamageClass>())
                GemTechState.MeleeOnHitEffects(target);

            // Handle on-hit ranged effects for the gem tech armor set.
            if (proj.CountsAsClass<RangedDamageClass>() && proj.type != ModContent.ProjectileType<GemTechGreenFlechette>())
                GemTechState.RangedOnHitEffects(target, proj.damage);

            // Handle on-hit projectiles effects for the mythril armor set.
            if (proj.type != ModContent.ProjectileType<MythrilFlare>())
                MythrilArmorSetChange.OnHitEffects(target, damageDone, Player);

            if (witheringWeaponEnchant)
                witheringDamageDone += (int)(damageDone * (hit.Crit ? 2D : 1D));

            cgn.IncreasedColdEffects_EskimoSet = eskimoSet;
            cgn.IncreasedColdEffects_CryoStone = CryoStone;

            cgn.IncreasedElectricityEffects_Transformer = transformer;

            cgn.IncreasedHeatEffects_Fireball = fireball;
            cgn.IncreasedHeatEffects_CinnamonRoll = cinnamonRoll;
            cgn.IncreasedHeatEffects_HellfireTreads = hellfireTreads;

            cgn.IncreasedSicknessEffects_ToxicHeart = toxicHeart;

            cgn.IncreasedSicknessAndWaterEffects_EvergreenGin = evergreenGin;

            switch (proj.type)
            {
                case ProjectileID.CobaltNaginata:
                    target.Calamity().cobaltNerfTimer = CalamityGlobalNPC.cobaltAndMythrilNerfTime;
                    break;

                case ProjectileID.PalladiumPike:
                    if (Player.moonLeech)
                        return;
                    Player.lifeRegenTime += 2;
                    break;

                case ProjectileID.MythrilHalberd:
                    target.Calamity().mythrilNerfTimer = CalamityGlobalNPC.cobaltAndMythrilNerfTime;
                    break;

                case ProjectileID.OrichalcumHalberd:
                    if (Player.petalTimer > 0)
                        Player.petalTimer /= 2;
                    break;

                case ProjectileID.AdamantiteGlaive:
                    float slowDownMult = 0.5f;
                    if (CalamityLists.enemyImmunityList.Contains(target.type) || target.boss)
                        slowDownMult = 0.95f;
                    target.velocity *= slowDownMult;
                    break;

                case ProjectileID.FruitcakeChakram:

                    if (Player.moonLeech || Main.player[Main.myPlayer].lifeSteal <= 0f || target.lifeMax <= 5)
                        return;

                    int heal = 2;
                    Main.player[Main.myPlayer].lifeSteal -= heal;
                    Player.statLife += heal;
                    Player.HealEffect(heal);
                    if (Player.statLife > Player.statLifeMax2)
                        Player.statLife = Player.statLifeMax2;

                    break;

                case ProjectileID.ObsidianSwordfish:
                    target.AddBuff(BuffID.OnFire3, 180);
                    break;

                case ProjectileID.GolemFist:
                    target.AddBuff(BuffType<ArmorCrunch>(), 180);
                    break;

                case ProjectileID.DeathSickle:
                    target.AddBuff(BuffType<WhisperingDeath>(), 60);
                    break;

                case ProjectileID.Bee:
                case ProjectileID.GiantBee:
                case ProjectileID.BladeOfGrass:
                    target.AddBuff(BuffID.Poisoned, 120);
                    break;

                case ProjectileID.Wasp:
                    target.AddBuff(BuffID.Venom, 60);
                    break;

                case ProjectileID.BoneArrow:
                    target.AddBuff(BuffType<Crumbling>(), 300);
                    break;

                case ProjectileID.NorthPoleWeapon:
                    target.AddBuff(BuffID.Frostburn, 300);
                    break;

                case ProjectileID.IceSickle:
                case ProjectileID.FrostArrow: // Ice Bow
                case ProjectileID.NorthPoleSpear:
                    target.AddBuff(BuffID.Frostburn2, 180);
                    break;

                case ProjectileID.Blizzard: // Blizzard Staff
                case ProjectileID.NorthPoleSnowflake:
                    target.AddBuff(BuffID.Frostburn2, 120);
                    break;

                case ProjectileID.IceBolt: // Ice Blade
                case ProjectileID.FrostDaggerfish:
                    target.AddBuff(BuffID.Frostburn, 60);
                    break;
            }

            if (flameWakerBoots)
                target.AddBuff(BuffID.OnFire, 120);

            if ((proj.arrow && Player.hasMoltenQuiver) || hellfireTreads)
            {
                if (Main.rand.NextBool(4))
                    target.AddBuff(BuffID.OnFire3, 360);
                else if (Main.rand.NextBool())
                    target.AddBuff(BuffID.OnFire3, 240);
                else
                    target.AddBuff(BuffID.OnFire3, 120);
            }

            if (!proj.npcProj && !proj.trap && proj.friendly)
            {
                if ((plaguebringerCarapace || uberBees) && CalamityLists.friendlyBeeList.Contains(proj.type))
                    target.AddBuff(BuffType<Plague>(), 300);

                // All projectiles fired from Soma Prime are marked using CalamityGlobalProjectile
                CalamityGlobalProjectile cgp = proj.Calamity();
                if (cgp.appliesSomaShred)
                {
                    target.AddBuff(BuffType<Shred>(), 320);
                    // This information cannot be transferred through the buff, but is necessary to calculate damage
                    cgn.somaShredApplicator = Player.whoAmI;
                }

                // Similarly, all shots from Animosity are also marked
                if (cgp.brimstoneBullets)
                {
                    target.AddBuff(BuffType<BrimstoneFlames>(), 90);

                    // Music easter egg in GFB
                    if (Main.zenithWorld)
                        GungeonMusicSystem.GUN();
                }

                if (cgp.fireBullet)
                {
                    target.AddBuff(BuffID.OnFire3, 60);
                    if (proj.numHits == 0)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            CritSpark spark = new CritSpark(proj.Center, proj.velocity.RotatedByRandom(0.4) * Main.rand.NextFloat(0.8f, 1.5f), Main.rand.NextBool() ? Color.Orange : Color.OrangeRed, Color.Yellow, Main.rand.NextFloat(0.4f, 0.6f), 15, Main.rand.NextFloat(-2f, 2f), 1.5f);
                            GeneralParticleHandler.SpawnParticle(spark);
                        }
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.3f, Pitch = 1f }, proj.Center);
                    }
                }

                if (cgp.iceBullet)
                {
                    target.AddBuff(BuffID.Frostburn2, 60);
                    if (proj.numHits == 0)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            CritSpark spark = new CritSpark(proj.Center, proj.velocity.RotatedByRandom(0.4) * Main.rand.NextFloat(0.8f, 1.5f), Main.rand.NextBool() ? Color.DeepSkyBlue : Color.LightSkyBlue, Color.White, Main.rand.NextFloat(0.4f, 0.6f), 15, Main.rand.NextFloat(-2f, 2f), 1.5f);
                            GeneralParticleHandler.SpawnParticle(spark);
                        }
                        SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.3f, Pitch = 0.8f }, proj.Center);
                    }
                }

                if (cgp.shockBullet)
                {
                    target.AddBuff(BuffID.Electrified, 180);

                    if (proj.numHits == 0)
                    {
                        CustomPulse spark = new CustomPulse(proj.Center, Vector2.Zero, Color.Turquoise, "CalamityMod/Particles/PlasmaExplosion", new Vector2(1, 1), Main.rand.NextFloat(-2f, 2f), 0.005f, Main.rand.NextFloat(0.048f, 0.055f), 14);
                        GeneralParticleHandler.SpawnParticle(spark);
                        int points = 6;
                        float radians = MathHelper.TwoPi / points;
                        Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
                        float rotRando = Main.rand.NextFloat(0.1f, 2.5f);
                        for (int k = 0; k < points; k++)
                        {
                            Vector2 velocity = spinningPoint.RotatedBy(radians * k).RotatedBy(-0.45f * rotRando);
                            SparkParticle subTrail = new SparkParticle(proj.Center + velocity * 4.5f, velocity * 8, false, 13, 0.85f, Color.Turquoise);
                            GeneralParticleHandler.SpawnParticle(subTrail);
                        }
                        for (int i = 0; i <= 12; i++)
                        {
                            Dust dust2 = Dust.NewDustPerfect(proj.Center, 278, new Vector2(4, 4).RotatedByRandom(100f) * Main.rand.NextFloat(0.1f, 2.9f));
                            dust2.noGravity = false;
                            dust2.scale = Main.rand.NextFloat(0.3f, 0.9f);
                            dust2.color = Color.Turquoise;
                        }

                        int onHitDamage = Player.CalcIntDamage<RangedDamageClass>(0.2f * proj.damage);
                        Player.ApplyDamageToNPC(target, onHitDamage, 0f, 0, false);

                        SoundStyle hitSound = new("CalamityMod/Sounds/Item/ElectricHit");
                        SoundEngine.PlaySound(hitSound with { Volume = 0.2f, Pitch = 0.7f, PitchVariance = 0.2f }, proj.Center);
                    }
                }

                if ((cgp.pearlBullet1 || cgp.pearlBullet2 || cgp.pearlBullet3) && proj.numHits == 0)
                {
                    Color color = cgp.pearlBullet1 ? Color.LightBlue : cgp.pearlBullet2 ? Color.LightPink : Color.Khaki;
                    Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
                    float radians = MathHelper.TwoPi / 3;

                    Vector2 Position = target.Center + spinningPoint.RotatedBy(radians * (cgp.pearlBullet1 ? 0 : cgp.pearlBullet2 ? 1 : 2)).RotatedBy(-0.45f) * 55;
                    int bulletType = (cgp.pearlBullet1 ? 0 : cgp.pearlBullet2 ? 1 : 2);

                    CustomPulse spark = new CustomPulse(target.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/HighResHollowCircleHardEdge", new Vector2(1, 1), Main.rand.NextFloat(-2f, 2f), 0.005f, 0.035f + 0.018f * bulletType, 14 + bulletType);
                    GeneralParticleHandler.SpawnParticle(spark);
                    CustomPulse spark2 = new CustomPulse(Position, Vector2.Zero, color, "CalamityMod/Particles/HighResFoggyCircleHardEdge", new Vector2(1, 1), Main.rand.NextFloat(-2f, 2f), 0.005f, 0.06f, 17);
                    GeneralParticleHandler.SpawnParticle(spark2);

                    int points = 6;
                    radians = MathHelper.TwoPi / points;
                    spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
                    float rotRando = Main.rand.NextFloat(0.1f, 2.5f);
                    for (int k = 0; k < points; k++)
                    {
                        Vector2 velocity = spinningPoint.RotatedBy(radians * k).RotatedBy(-0.45f * rotRando);
                        Particle subTrail = new GlowSparkParticle(Position + velocity * 10f, velocity * 15, false, 12, 0.03f, color, new Vector2(1.35f, 0.5f), true);
                        GeneralParticleHandler.SpawnParticle(subTrail);
                    }

                    int pearls = (int)(MathHelper.Clamp(7 - (int)(proj.numHits * 0.5f), 2, 7));
                    for (int k = 0; k < pearls; k++)
                    {
                        Vector2 velocity = new Vector2(1, 1).RotatedByRandom(100) * Main.rand.NextFloat(0.7f, 1.2f);
                        PearlParticle subTrail = new PearlParticle(Position + velocity * 11f, velocity * 10, true, 50, 0.85f, color, 0.95f, Main.rand.NextFloat(2, -2), true);
                        GeneralParticleHandler.SpawnParticle(subTrail);
                    }
                    int dusts = (int)(MathHelper.Clamp(10 - (int)(proj.numHits * 0.5f), 2, 10));
                    for (int i = 0; i <= dusts; i++)
                    {
                        Dust dust2 = Dust.NewDustPerfect(Position, 278, new Vector2(5, 5).RotatedByRandom(100f) * Main.rand.NextFloat(0.1f, 2.9f));
                        dust2.noGravity = false;
                        dust2.scale = Main.rand.NextFloat(0.3f, 0.8f);
                        dust2.color = color;
                    }

                    int onHitDamage = Player.CalcIntDamage<RangedDamageClass>(0.2f * proj.damage);
                    Player.ApplyDamageToNPC(target, onHitDamage, 0f, 0, false);

                    SoundStyle hitSound = new("CalamityMod/Sounds/Item/HadalUrnClose");
                    SoundEngine.PlaySound(hitSound with { Volume = 0.4f, Pitch = 0.4f, PitchVariance = 0.2f }, proj.Center);
                }


                if (cgp.lifeBullet && proj.numHits == 0)
                {
                    int points = 10;
                    for (int k = 0; k < points; k++)
                    {
                        Vector2 velocity = proj.velocity.RotatedByRandom(0.7f) * Main.rand.NextFloat(0.3f, 0.8f);
                        LineParticle orb = new LineParticle(proj.Center + velocity * 1.5f, velocity * Main.rand.NextFloat(1f, 2f), false, 18, Main.rand.NextFloat(0.4f, 0.7f), Color.White * 0.85f);
                        GeneralParticleHandler.SpawnParticle(orb);
                    }

                    int heal = (int)Math.Round(hit.Damage * 0.035);
                    if (Main.player[Main.myPlayer].lifeSteal <= 0f || heal <= 0 || target.lifeMax <= 5)
                        return;

                    CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Main.player[proj.owner], heal, ModContent.ProjectileType<AltTransfusionTrail>(), BalancingConstants.LifeStealRange);
                }

                if ((cgp.betterLifeBullet1 || cgp.betterLifeBullet2) && proj.numHits == 0)
                {
                    int points = 12;
                    for (int k = 0; k < points; k++)
                    {
                        int randomColor = Main.rand.Next(1, 3 + 1);
                        Color color = randomColor == 1 ? Color.LightBlue : randomColor == 2 ? Color.LightPink : Color.Khaki;

                        Vector2 velocity = proj.velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(0.3f, 0.8f);
                        LineParticle orb = new LineParticle(proj.Center + velocity * 1.5f, velocity * Main.rand.NextFloat(1f, 3f), false, 18, Main.rand.NextFloat(0.4f, 0.7f), color);
                        GeneralParticleHandler.SpawnParticle(orb);
                    }

                    int heal = (int)Math.Round(hit.Damage * 0.01);
                    if (Main.player[Main.myPlayer].lifeSteal <= 0f || heal <= 0 || target.lifeMax <= 5)
                        return;

                    for (int i = 0; i <= 2; i++)
                    {
                        CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Main.player[proj.owner], heal, ModContent.ProjectileType<AltTransfusionTrail>(), BalancingConstants.LifeStealRange);
                    }
                }

                ProjLifesteal(target, proj, damageDone, hit.Crit);
                ProjOnHit(proj, target.Center, hit.Crit, target.IsAnEnemy(false));
                NPCDebuffs(target, proj.CountsAsClass<MeleeDamageClass>(), proj.CountsAsClass<RangedDamageClass>(), proj.CountsAsClass<MagicDamageClass>(), proj.CountsAsClass<SummonDamageClass>(), proj.CountsAsClass<ThrowingDamageClass>(), proj.CountsAsClass<SummonMeleeSpeedDamageClass>(), true, proj.noEnchantments);

                // Shattered Community tracks all damage dealt with Rage Mode (ignoring dummies).
                if (target.type == NPCID.TargetDummy || target.type == NPCType<SuperDummyNPC>())
                    return;

                if (rageModeActive && shatteredCommunity)
                    Player.GetModPlayer<ShatteredCommunityPlayer>().AccumulateRageDamage(damageDone);
            }
        }
        #endregion

        #region Item
        public void ItemOnHit(Item item, int damage, Vector2 position, bool crit, bool npcCheck)
        {
            var source = Player.GetSource_ItemUse(item);
            if (!item.CountsAsClass<MeleeDamageClass>() && Player.meleeEnchant == 7)
                Projectile.NewProjectile(source, position, Player.velocity, ProjectileID.ConfettiMelee, 0, 0f, Player.whoAmI);

            if (reaverDefense)
                Player.lifeRegenTime += 1;

            if (npcCheck)
            {
                if (item.CountsAsClass<MeleeDamageClass>() && hideOfDeus && hideOfDeusTimer == 0)
                {
                    hideOfDeusTimer = 10;
                    int bulwarkStarDamage = (int)Player.GetTotalDamage<MeleeDamageClass>().ApplyTo(320);
                    bulwarkStarDamage = Player.ApplyArmorAccDamageBonusesTo(bulwarkStarDamage);

                    for (int n = 0; n < 3; n++)
                        CalamityUtils.ProjectileRain(source, Player.Center, 400f, 100f, 500f, 800f, 29f, ProjectileType<AstralStar>(), bulwarkStarDamage, 5f, Player.whoAmI);
                }
                if (astralStarRain && crit && astralStarRainCooldown <= 0)
                {
                    astralStarRainCooldown = 60;
                    for (int n = 0; n < 3; n++)
                    {
                        int projectileType = Utils.SelectRandom(Main.rand, new int[]
                        {
                            ProjectileType<AstralStar>(),
                            ProjectileID.BeeCloakStar,
                            ProjectileID.StarCloakStar,
                            ProjectileID.StarCannonStar
                        });

                        int astralStarDamage = (int)Player.GetBestClassDamage().ApplyTo(120);
                        astralStarDamage = Player.ApplyArmorAccDamageBonusesTo(astralStarDamage);

                        Projectile star = CalamityUtils.ProjectileRain(source, position, 400f, 100f, 500f, 800f, 12f, projectileType, astralStarDamage, 5f, Player.whoAmI);
                        if (star.whoAmI.WithinBounds(Main.maxProjectiles))
                            star.DamageType = DamageClass.Generic;
                    }
                }
            }

            if (item.CountsAsClass<MeleeDamageClass>())
            {
                if (npcCheck)
                {
                    if (ataxiaGeyser && Player.ownedProjectileCounts[ProjectileType<ChaoticGeyser>()] < 3)
                    {
                        // Ataxia True Melee Geysers: 15%, softcap starts at 300 base damage
                        int geyserDamage = CalamityUtils.DamageSoftCap(damage * 0.15, 45);
                        geyserDamage = Player.ApplyArmorAccDamageBonusesTo(geyserDamage);

                        Projectile.NewProjectile(source, position, Vector2.Zero, ProjectileType<ChaoticGeyser>(), geyserDamage, 2f, Player.whoAmI, 0f, 0f);
                    }
                    if (soaring)
                    {
                        double useTimeMultiplier = 0.85 + (item.useTime * item.useAnimation / 3600D); //28 * 28 = 784 is average so that equals 784 / 3600 = 0.217777 + 1 = 21.7% boost
                        double wingTimeFraction = Player.wingTimeMax / 20D;

                        // TODO -- this scaling function is probably totally screwed. What is it supposed to do?
                        double meleeStatMultiplier = (double)(Player.GetTotalDamage<MeleeDamageClass>().Additive * (float)(Player.GetTotalCritChance<MeleeDamageClass>() / 10f));

                        if (Player.wingTime < Player.wingTimeMax)
                            Player.wingTime += (int)(useTimeMultiplier * (wingTimeFraction + meleeStatMultiplier));

                        if (Player.wingTime > Player.wingTimeMax)
                            Player.wingTime = Player.wingTimeMax;
                    }
                    if (bloodflareMelee && item.CountsAsClass<MeleeDamageClass>() && bloodflareMeleeHits < 15 && !bloodflareFrenzy && !Player.HasCooldown(BloodflareFrenzy.ID))
                        bloodflareMeleeHits++;
                }
            }
        }
        #endregion

        #region Proj On Hit
        public void ProjOnHit(Projectile proj, Vector2 position, bool crit, bool npcCheck)
        {
            CalamityGlobalProjectile modProj = proj.Calamity();
            var source = proj.GetSource_FromThis();
            bool hasClass = proj.CountsAsClass<MeleeDamageClass>() || proj.CountsAsClass<RangedDamageClass>() || proj.CountsAsClass<MagicDamageClass>() || proj.CountsAsClass<SummonDamageClass>() || proj.CountsAsClass<ThrowingDamageClass>();

            //flask of party affects all types of weapons, !proj.CountsAsClass<MeleeDamageClass>() is to prevent double flask effects
            if (!proj.CountsAsClass<MeleeDamageClass>() && !proj.CountsAsClass<SummonMeleeSpeedDamageClass>() && Player.meleeEnchant == 7)
                Projectile.NewProjectile(source, position, proj.velocity, ProjectileID.ConfettiMelee, 0, 0f, proj.owner);

            if (alchFlask && AlchFlaskCooldown == 0 && proj.type != ModContent.ProjectileType<BasicPlagueBee>())
            {
                int seekerDamage = (int)Player.GetBestClassDamage().ApplyTo(10);
                seekerDamage = Player.ApplyArmorAccDamageBonusesTo(seekerDamage);
                Vector2 seekerVelocity = new Vector2(5, 5).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1.2f);

                Projectile bee = Projectile.NewProjectileDirect(source, position, seekerVelocity, ModContent.ProjectileType<BasicPlagueBee>(), seekerDamage, 0f, Player.whoAmI, -20, 30, 2);
                bee.ArmorPenetration = 20;
                bee.penetrate = 2;
                bee.extraUpdates = 1;
                AlchFlaskCooldown = 7;
            }

            bool lifeAndShieldCondition = Player.statLife >= Player.statLifeMax2 && (!HasAnyEnergyShield || TotalEnergyShielding >= TotalMaxShieldDurability);
            if (theBee && lifeAndShieldCondition)
                SoundEngine.PlaySound(SoundID.Item110, proj.Center);

            if (reaverDefense)
                Player.lifeRegenTime += 1;

            if (npcCheck)
            {
                if (astralStarRain && crit && astralStarRainCooldown <= 0)
                {
                    astralStarRainCooldown = 60;
                    for (int n = 0; n < 3; n++)
                    {
                        int projectileType = Utils.SelectRandom(Main.rand, new int[]
                        {
                            ProjectileType<AstralStar>(),
                            ProjectileID.BeeCloakStar,
                            ProjectileID.StarCloakStar,
                            ProjectileID.StarCannonStar
                        });

                        int astralStarDamage = (int)Player.GetBestClassDamage().ApplyTo(120);
                        astralStarDamage = Player.ApplyArmorAccDamageBonusesTo(astralStarDamage);

                        Projectile star = CalamityUtils.ProjectileRain(source, position, 400f, 100f, 500f, 800f, 25f, projectileType, astralStarDamage, 5f, Player.whoAmI);
                        if (star.whoAmI.WithinBounds(Main.maxProjectiles))
                            star.DamageType = DamageClass.Generic;
                    }
                }
            }

            if (abaddon && crit && AbaddonCooldown <= 0)
            {
                AbaddonCooldown = 15;
                int AbaddonExploDamage = CalamityUtils.DamageSoftCap(proj.damage * 0.03f, 25);
                Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<AbaddonCrit>(), AbaddonExploDamage, 0f, Player.whoAmI);
            }

            if (proj.CountsAsClass<MeleeDamageClass>())
                MeleeOnHit(proj, modProj, position, crit, npcCheck);
            if (proj.CountsAsClass<RangedDamageClass>())
                RangedOnHit(proj, modProj, position, crit, npcCheck);
            if (proj.CountsAsClass<MagicDamageClass>())
                MagicOnHit(proj, modProj, position, crit, npcCheck);
            if (proj.CountsAsClass<SummonDamageClass>() && !proj.CountsAsClass<SummonMeleeSpeedDamageClass>())
                SummonOnHit(proj, modProj, position, crit, npcCheck);
            if (proj.CountsAsClass<ThrowingDamageClass>())
                RogueOnHit(proj, modProj, position, crit, npcCheck);
        }

        #region Melee
        private void MeleeOnHit(Projectile proj, CalamityGlobalProjectile modProj, Vector2 position, bool crit, bool npcCheck)
        {
            var source = proj.GetSource_FromThis();
            Item heldItem = Player.ActiveItem();

            if (proj.IsTrueMelee())
            {
                if (soaring)
                {
                    double useTimeMultiplier = 0.85 + (heldItem.useTime * heldItem.useAnimation / 3600D); //28 * 28 = 784 is average so that equals 784 / 3600 = 0.217777 + 1 = 21.7% boost
                    double wingTimeFraction = Player.wingTimeMax / 20D;

                    // TODO -- this scaling function is probably totally screwed. What is it supposed to do?
                    double meleeStatMultiplier = (double)(Player.GetTotalDamage<MeleeDamageClass>().Additive * (float)(Player.GetTotalCritChance<MeleeDamageClass>() / 10f));

                    if (Player.wingTime < Player.wingTimeMax)
                        Player.wingTime += (int)(useTimeMultiplier * (wingTimeFraction + meleeStatMultiplier));

                    if (Player.wingTime > Player.wingTimeMax)
                        Player.wingTime = Player.wingTimeMax;
                }
                if (hideOfDeus && hideOfDeusTimer == 0)
                {
                    hideOfDeusTimer = 10;
                    int bulwarkStarDamage = (int)Player.GetTotalDamage<MeleeDamageClass>().ApplyTo(320);
                    bulwarkStarDamage = Player.ApplyArmorAccDamageBonusesTo(bulwarkStarDamage);

                    for (int n = 0; n < 3; n++)
                        CalamityUtils.ProjectileRain(source, Player.Center, 400f, 100f, 500f, 800f, 29f, ProjectileType<AstralStar>(), bulwarkStarDamage, 5f, Player.whoAmI);
                }
            }

            if (npcCheck)
            {
                if (ataxiaGeyser && Player.ownedProjectileCounts[ProjectileType<ChaoticGeyser>()] < 3)
                {
                    // Ataxia Melee Geysers: 15%, softcap starts at 240 base damage
                    int geyserDamage = CalamityUtils.DamageSoftCap(proj.damage * 0.15, 36);
                    geyserDamage = Player.ApplyArmorAccDamageBonusesTo(geyserDamage);

                    Projectile.NewProjectile(source, proj.Center, Vector2.Zero, ProjectileType<ChaoticGeyser>(), geyserDamage, 0f, Player.whoAmI, 0f, 0f);
                }
                if (bloodflareMelee && proj.IsTrueMelee() && bloodflareMeleeHits < 15 && !bloodflareFrenzy && !Player.HasCooldown(BloodflareFrenzy.ID))
                    bloodflareMeleeHits++;
            }
        }
        #endregion

        #region Ranged
        private void RangedOnHit(Projectile proj, CalamityGlobalProjectile modProj, Vector2 position, bool crit, bool npcCheck)
        {
            var source = proj.GetSource_FromThis();

            if (npcCheck)
            {
                if (tarraRanged && proj.CountsAsClass<RangedDamageClass>() && tarraRangedCooldown <= 0)
                {
                    tarraRangedCooldown = 60;
                    for (int l = 0; l < 2; l++)
                    {
                        Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                        int leafDamage = Player.ApplyArmorAccDamageBonusesTo(0.25f * proj.damage);
                        int leaf = Projectile.NewProjectile(source, position, velocity, ProjectileID.Leaf, leafDamage, 0f, Player.whoAmI);
                        if (leaf.WithinBounds(Main.maxProjectiles))
                        {
                            Main.projectile[leaf].DamageType = DamageClass.Generic;
                            Main.projectile[leaf].netUpdate = true;
                        }
                    }
                    if (Player.ownedProjectileCounts[ProjectileType<TarraEnergy>()] < 2)
                    {
                        for (int projCount = 0; projCount < 2; projCount++)
                        {
                            Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                            int energyDamage = Player.ApplyArmorAccDamageBonusesTo(0.33f * proj.damage);
                            Projectile.NewProjectile(source, proj.Center, velocity, ProjectileType<TarraEnergy>(), energyDamage, 0f, proj.owner);
                        }
                    }
                }
            }
        }
        #endregion

        #region Magic
        private void MagicOnHit(Projectile proj, CalamityGlobalProjectile modProj, Vector2 position, bool crit, bool npcCheck)
        {
            var source = proj.GetSource_FromThis();
            if (ataxiaMage && ataxiaDmg <= 0)
            {
                int orbDamage = Player.ApplyArmorAccDamageBonusesTo(proj.damage * 0.6f);

                CalamityUtils.SpawnOrb(proj, orbDamage, ProjectileType<HydrothermicSphere>(), 800f, 20f);
                int cooldown = (int)(orbDamage * 0.5);
                ataxiaDmg += cooldown;
            }
            if (tarraMage && crit)
            {
                tarraCrits++;
            }
            if (npcCheck)
            {
                if (bloodflareMage && bloodflareMageCooldown <= 0 && crit)
                {
                    bloodflareMageCooldown = 120;
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                        // Bloodflare Mage Fireballs: 3 x 50%, softcap starts at 500 base damage to not overly punish slow weapons
                        int bloodflareFireballDamage = Player.ApplyArmorAccDamageBonusesTo(CalamityUtils.DamageSoftCap(proj.damage * 0.5, 250));

                        int fire = Projectile.NewProjectile(source, position, velocity, ProjectileID.BallofFire, bloodflareFireballDamage, 0f, Player.whoAmI);
                        if (fire.WithinBounds(Main.maxProjectiles))
                        {
                            Main.projectile[fire].DamageType = DamageClass.Generic;
                            Main.projectile[fire].netUpdate = true;
                        }
                    }
                }
            }
            if (silvaMage && silvaMageCooldown <= 0 && (proj.penetrate == 1 || proj.timeLeft <= 5))
            {
                silvaMageCooldown = 300;
                SoundEngine.PlaySound(SoundID.Zombie103, proj.Center); //So scuffed, just because zombie sounds werent ported normally
                // Silva Mage Blasts: 800 + 60%, softcap on the whole combined thing starts at 1400
                int silvaBurstDamage = Player.ApplyArmorAccDamageBonusesTo(CalamityUtils.DamageSoftCap(800.0 + 0.6 * proj.damage, 1400));
                Projectile.NewProjectile(source, proj.Center, Vector2.Zero, ProjectileType<SilvaBurst>(), silvaBurstDamage, 8f, Player.whoAmI);
            }
        }
        #endregion

        #region Summon
        private void SummonOnHit(Projectile proj, CalamityGlobalProjectile modProj, Vector2 position, bool crit, bool npcCheck)
        {
            var source = proj.GetSource_FromThis();

            if (npcCheck)
            {
                if (phantomicArtifact)
                {
                    int restoreBuff = BuffType<PhantomicRegen>();
                    int empowerBuff = BuffType<PhantomicEmpowerment>();
                    int shieldBuff = BuffType<Buffs.StatBuffs.PhantomicShield>();
                    int buffType = Utils.SelectRandom(Main.rand, new int[]
                    {
                        restoreBuff,
                        empowerBuff,
                        shieldBuff
                    });
                    Player.AddBuff(buffType, 60);
                    if (buffType == restoreBuff)
                    {
                        if (phantomicHeartRegen == 1000 && Player.ownedProjectileCounts[ProjectileType<PhantomicHeart>()] == 0 && Main.rand.NextBool(20))
                        {
                            Vector2 target = proj.Center;
                            target.Y += Main.rand.Next(-50, 50);
                            target.X += Main.rand.Next(-50, 50);
                            Projectile.NewProjectile(source, target, Vector2.Zero, ProjectileType<PhantomicHeart>(), 0, 0f, Player.whoAmI);
                        }
                    }
                    else if (buffType == empowerBuff)
                    {
                        if (Player.ownedProjectileCounts[ProjectileType<PhantomicDagger>()] < 3 && Main.rand.NextBool(10))
                        {
                            int damage = (int)Player.GetTotalDamage<SummonDamageClass>().ApplyTo(100);
                            damage = Player.ApplyArmorAccDamageBonusesTo(damage);
                            Projectile.NewProjectile(source, proj.position, proj.velocity, ProjectileType<PhantomicDagger>(), damage, 1f, Player.whoAmI);
                        }
                    }
                    else
                    {
                        if (Player.ownedProjectileCounts[ProjectileType<Projectiles.Summon.PhantomicShield>()] == 0 && phantomicBulwarkCooldown == 0)
                            Projectile.NewProjectile(source, Player.position, Vector2.Zero, ProjectileType<Projectiles.Summon.PhantomicShield>(), 0, 0f, Player.whoAmI);
                    }
                }
                else if (hallowedRune)
                {
                    int buffType = Utils.SelectRandom(Main.rand, new int[]
                    {
                        BuffType<HallowedRunePower>(),
                        BuffType<HallowedRuneRegeneration>(),
                        BuffType<HallowedRuneDefense>()
                    });
                    Player.AddBuff(buffType, 60);
                }
                else if (sGlyph)
                {
                    int buffType = Utils.SelectRandom(Main.rand, new int[]
                    {
                        BuffType<SpiritPower>(),
                        BuffType<SpiritRegen>(),
                        BuffType<SpiritDefense>()
                    });
                    Player.AddBuff(buffType, 60);
                }
            }

            // Fearmonger set gains +10 frames (max 90) of regen when any minion lands any hit
            if (fearmongerSet)
            {
                fearmongerRegenFrames += 10;
                if (fearmongerRegenFrames > 90)
                    fearmongerRegenFrames = 90;
            }

            //Priorities: Nucleogenesis => Starbuster Core => Nuclear Rod => Jelly-Charged Battery
            List<int> summonExceptionList = new List<int>()
            {
                ProjectileType<EnergyOrb>(),
                ProjectileType<IrradiatedAura>(),
                ProjectileType<SummonAstralExplosion>(),
                ProjectileType<ApparatusExplosion>(),
                ProjectileType<HallowedStarSummon>()
            };

            if (summonExceptionList.TrueForAll(x => proj.type != x))
            {
                if (summonProjCooldown <= 0)
                {
                    if (nucleogenesis)
                    {
                        int apparatusDamage = (int)Player.GetTotalDamage<SummonDamageClass>().ApplyTo(60);
                        apparatusDamage = Player.ApplyArmorAccDamageBonusesTo(apparatusDamage);
                        Projectile.NewProjectile(source, proj.Center, Vector2.Zero, ProjectileType<ApparatusExplosion>(), apparatusDamage, 4f, proj.owner);
                        summonProjCooldown = 100f;
                    }
                    else if (starbusterCore)
                    {
                        int starburstDamage = (int)Player.GetTotalDamage<SummonDamageClass>().ApplyTo(40);
                        starburstDamage = Player.ApplyArmorAccDamageBonusesTo(starburstDamage);
                        Projectile.NewProjectile(source, proj.Center, Vector2.Zero, ProjectileType<SummonAstralExplosion>(), starburstDamage, 3.5f, proj.owner);
                        summonProjCooldown = 60f;
                    }
                    else if (nuclearFuelRod)
                    {
                        int nuclearDamage = (int)Player.GetTotalDamage<SummonDamageClass>().ApplyTo(20);
                        nuclearDamage = Player.ApplyArmorAccDamageBonusesTo(nuclearDamage);
                        Projectile.NewProjectile(source, proj.Center, Vector2.Zero, ProjectileType<IrradiatedAura>(), nuclearDamage, 0f, proj.owner);
                        summonProjCooldown = 60f;
                    }
                    else if (jellyChargedBattery)
                    {
                        int batteryDamage = (int)Player.GetTotalDamage<SummonDamageClass>().ApplyTo(15);
                        batteryDamage = Player.ApplyArmorAccDamageBonusesTo(batteryDamage);
                        CalamityUtils.SpawnOrb(proj, batteryDamage, ProjectileType<EnergyOrb>(), 800f, 15f);
                        summonProjCooldown = 60f;
                    }
                }

                if (hallowedPower)
                {
                    if (hallowedRuneCooldown <= 0)
                    {
                        hallowedRuneCooldown = 180;
                        Vector2 spawnPosition = position - new Vector2(0f, 920f).RotatedByRandom(0.3f);
                        float speed = Main.rand.NextFloat(17f, 23f);
                        int hallowedDamage = (int)Player.GetTotalDamage<SummonDamageClass>().ApplyTo(50);
                        hallowedDamage = Player.ApplyArmorAccDamageBonusesTo(hallowedDamage);
                        Projectile.NewProjectile(source, spawnPosition, Vector2.Normalize(position - spawnPosition) * speed, ProjectileType<HallowedStarSummon>(), hallowedDamage, 3f, proj.owner);
                    }
                }
            }
        }
        #endregion

        #region Rogue
        private void RogueOnHit(Projectile proj, CalamityGlobalProjectile modProj, Vector2 position, bool crit, bool npcCheck)
        {
            var spawnSource = proj.GetSource_FromThis();
            int Type = ProjectileType<DragonScalesInfernado>();
            if (modProj.stealthStrike && dragonScales && Main.projectile.Count(proj => proj.type == Type && proj.active) < 1)
            {
                int damage = (int)Player.GetTotalDamage<RogueDamageClass>().ApplyTo(DragonScales.TornadoBaseDamage);
                damage = Player.ApplyArmorAccDamageBonusesTo(damage);
                int projectileIndex = Projectile.NewProjectile(spawnSource, proj.Center.X, proj.Center.Y, 0f, 0f, ProjectileType<DragonScalesInfernado>(), damage, 15f, Main.myPlayer, 10f, 9f); //First overload seems to deal with timing, second is segment amount
                if (projectileIndex.WithinBounds(Main.maxProjectiles))
                    Main.projectile[projectileIndex].netUpdate = true;
            }

            if (crit && tarraThrowing && tarraThrowingCrits < 50 && !tarragonImmunity && !Player.HasCooldown(Cooldowns.TarragonImmunity.ID))
                tarraThrowingCrits++;

            if (xerocSet && xerocDmg <= 0 && Player.ownedProjectileCounts[ProjectileType<EmpyreanEmber>()] < 3 && Player.ownedProjectileCounts[ProjectileType<EmpyreanBlast>()] < 3)
            {
                switch (Main.rand.Next(5))
                {
                    case 0:
                        // Exodus Rogue Stars: 80%
                        int starDamage = Player.ApplyArmorAccDamageBonusesTo(proj.damage * 0.8f);
                        CalamityUtils.SpawnOrb(proj, starDamage, ProjectileType<EmpyreanStellarDetritus>(), 800f, Main.rand.Next(15, 30));
                        xerocDmg += (int)(starDamage * 0.5f);
                        break;

                    case 1:
                        // Exodus Rogue Orbs: 60%
                        int orbDamage = Player.ApplyArmorAccDamageBonusesTo(proj.damage * 0.6f);
                        CalamityUtils.SpawnOrb(proj, orbDamage, ProjectileType<EmpyreanMarble>(), 800f, 30f);
                        xerocDmg += (int)(orbDamage * 0.5f);
                        break;

                    case 2:
                        // Exodus Rogue Fire: 15%
                        int fireDamage = Player.ApplyArmorAccDamageBonusesTo(proj.damage * 0.15f);
                        Projectile.NewProjectile(spawnSource, proj.Center, Vector2.Zero, ProjectileType<EmpyreanEmber>(), fireDamage, 0f, proj.owner, 0f, 0f);
                        break;

                    case 3:
                        // Exodus Rogue Blast: 20%
                        int blastDamage = Player.ApplyArmorAccDamageBonusesTo(proj.damage * 0.2f);
                        Projectile.NewProjectile(spawnSource, proj.Center, Vector2.Zero, ProjectileType<EmpyreanBlast>(), blastDamage, 0f, proj.owner, 0f, 0f);
                        break;

                    case 4:
                        // Exodus Rogue Bubble: 60%
                        int bubbleDamage = Player.ApplyArmorAccDamageBonusesTo(proj.damage * 0.6f);
                        CalamityUtils.SpawnOrb(proj, bubbleDamage, ProjectileType<EmpyreanGlob>(), 800f, 15f);
                        xerocDmg += (int)(bubbleDamage * 0.5);
                        break;

                    default:
                        break;
                }
            }

            if (modProj.stealthStrike && rogueCrownCooldown <= 0 && modProj.stealthStrikeHitCount < 3)
            {
                bool spawnedFeathers = false;
                if (nanotech)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 source = new Vector2(position.X + Main.rand.Next(-201, 201), Main.screenPosition.Y - 600f - Main.rand.Next(50));
                        Vector2 velocity = (position - source) / 40f;
                        int damage = (int)Player.GetTotalDamage<RogueDamageClass>().ApplyTo(120);
                        damage = Player.ApplyArmorAccDamageBonusesTo(damage);
                        Projectile.NewProjectile(spawnSource, source, velocity, ProjectileType<NanoFlare>(), damage, 3f, proj.owner);
                    }
                }
                else if (moonCrown)
                {
                    int lunarFlareDamage = (int)Player.GetTotalDamage<RogueDamageClass>().ApplyTo(MoonstoneCrown.BaseDamage);
                    lunarFlareDamage = Player.ApplyArmorAccDamageBonusesTo(lunarFlareDamage);
                    float lunarFlareKB = 3f;

                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 source = new Vector2(position.X + Main.rand.Next(-201, 201), Main.screenPosition.Y - 600f - Main.rand.Next(50));
                        Vector2 velocity = (position - source) / 10f;
                        int flare = Projectile.NewProjectile(spawnSource, source, velocity, ProjectileID.LunarFlare, lunarFlareDamage, lunarFlareKB, proj.owner);
                        if (flare.WithinBounds(Main.maxProjectiles))
                            Main.projectile[flare].DamageType = DamageClass.Generic;
                    }
                }
                else if (featherCrown)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 source = new Vector2(position.X + Main.rand.Next(-201, 201), Main.screenPosition.Y - 600f - Main.rand.Next(50));
                        float speedX = (position.X - source.X) / 30f;
                        float speedY = (position.Y - source.Y) * 8;
                        Vector2 velocity = new Vector2(speedX, speedY);

                        int featherDamage = (int)Player.GetTotalDamage<RogueDamageClass>().ApplyTo(25);
                        featherDamage = Player.ApplyArmorAccDamageBonusesTo(featherDamage);

                        int feather = Projectile.NewProjectile(spawnSource, source, velocity, ProjectileType<StickyFeather>(), featherDamage, 3f, proj.owner);
                        if (feather.WithinBounds(Main.maxProjectiles))
                        {
                            Main.projectile[feather].DamageType = DamageClass.Generic;
                            Main.projectile[feather].extraUpdates += 3;
                        }
                    }
                    spawnedFeathers = true;
                }
                rogueCrownCooldown = spawnedFeathers ? 15 : 60;
            }

            if (forbiddenCirclet && modProj.stealthStrike && forbiddenCooldown <= 0 && modProj.stealthStrikeHitCount < 3)
            {
                for (int index2 = 0; index2 < 6; index2++)
                {
                    float xVector = Main.rand.Next(-35, 36) * 0.02f;
                    float yVector = Main.rand.Next(-35, 36) * 0.02f;
                    xVector *= 10f;
                    yVector *= 10f;
                    int damage = (int)Player.GetTotalDamage<RogueDamageClass>().ApplyTo(40);
                    damage = Player.ApplyArmorAccDamageBonusesTo(damage);

                    int eater = Projectile.NewProjectile(spawnSource, proj.Center.X, proj.Center.Y, xVector, yVector, ProjectileType<ForbiddenCircletEater>(), damage, proj.knockBack, proj.owner);
                    if (eater.WithinBounds(Main.maxProjectiles))
                        Main.projectile[eater].DamageType = DamageClass.Generic;
                    forbiddenCooldown = 15;
                }
            }

            if (titanHeartSet && modProj.stealthStrike && titanCooldown <= 0 && modProj.stealthStrikeHitCount < 3)
            {
                int damage = (int)Player.GetTotalDamage<RogueDamageClass>().ApplyTo(40);
                damage = Player.ApplyArmorAccDamageBonusesTo(damage);

                Projectile.NewProjectile(spawnSource, proj.Center, Vector2.Zero, ProjectileType<SabatonBoom>(), damage, proj.knockBack, proj.owner, 1f, 0f);
                SoundEngine.PlaySound(SoundID.Item14, proj.Center);
                for (int dustexplode = 0; dustexplode < 360; dustexplode++)
                {
                    Vector2 dustd = new Vector2(17f, 17f).RotatedBy(MathHelper.ToRadians(dustexplode));
                    int d = Dust.NewDust(proj.Center, proj.width, proj.height, Main.rand.NextBool() ? DustType<AstralBlue>() : DustType<AstralOrange>(), dustd.X, dustd.Y, 100, default, 1f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].position = proj.Center;
                    Main.dust[d].velocity *= 0.1f;
                }
                titanCooldown = 15;
            }

            if (corrosiveSpine && modProj.stealthStrikeHitCount < 3 && (Player.ownedProjectileCounts[ProjectileType<Corrocloud1>()] + Player.ownedProjectileCounts[ProjectileType<Corrocloud2>()] + Player.ownedProjectileCounts[ProjectileType<Corrocloud3>()]) < 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (Main.rand.NextBool())
                    {
                        int type = -1;
                        switch (Main.rand.Next(15))
                        {
                            case 0:
                                type = ProjectileType<Corrocloud1>();
                                break;
                            case 1:
                                type = ProjectileType<Corrocloud2>();
                                break;
                            case 2:
                                type = ProjectileType<Corrocloud3>();
                                break;
                        }

                        if (type != -1)
                        {
                            int damage = (int)Player.GetTotalDamage<RogueDamageClass>().ApplyTo(22);
                            damage = Player.ApplyArmorAccDamageBonusesTo(damage);

                            float speed = Main.rand.NextFloat(5f, 11f);
                            int cloud = Projectile.NewProjectile(spawnSource, position, Vector2.One.RotatedByRandom(MathHelper.TwoPi) * speed, type, damage, proj.knockBack, Player.whoAmI);
                            if (cloud.WithinBounds(Main.maxProjectiles))
                                Main.projectile[cloud].DamageType = DamageClass.Generic;
                        }
                    }
                }
            }

            if (shadow && shadowPotCooldown <= 0 && modProj.stealthStrikeHitCount < 3)
            {
                int randrot = Main.rand.Next(-30, 391);
                Vector2 SoulSpeed = new Vector2(13f, 13f).RotatedBy(MathHelper.ToRadians(randrot));
                int soul = Projectile.NewProjectile(spawnSource, proj.Center, SoulSpeed, ProjectileType<PenumbraSoul>(), (int)(proj.damage * 0.1), 3f, proj.owner, 0f, 0f);
                if (soul.WithinBounds(Main.maxProjectiles))
                    Main.projectile[soul].DamageType = DamageClass.Generic;
                shadowPotCooldown = 30;
            }

            if (raiderTalisman && modProj.stealthStrike)
            {
                raiderCritBonus = RaidersTalisman.RaiderBonus;
                if (raiderSoundCooldown <= 0)
                {
                    SoundEngine.PlaySound(RaidersTalisman.StealthHitSound, Player.Center);
                    raiderSoundCooldown = 60;
                }
            }

            if (npcCheck)
            {
                // Umbraphile cannot trigger off of itself. It is guaranteed on stealth strikes and 20% chance otherwise.
                if (umbraphileSet && ((modProj.stealthStrike && modProj.stealthStrikeHitCount < 3) || Main.rand.NextBool(5)))
                {
                    // Umbraphile Rogue Blasts: 20%, softcap starts at 50 base damage
                    int umbraBlastDamage = CalamityUtils.DamageSoftCap(proj.damage * 0.20, 50);
                    umbraBlastDamage = Player.ApplyArmorAccDamageBonusesTo(umbraBlastDamage);

                    Projectile.NewProjectile(spawnSource, proj.Center, Vector2.Zero, ProjectileType<UmbraphileBoom>(), umbraBlastDamage, 0f, Player.whoAmI);
                }
                if (electricianGlove && modProj.stealthStrike && modProj.stealthStrikeHitCount < 3)
                {
                    for (int s = 0; s < 3; s++)
                    {
                        Vector2 velocity = CalamityUtils.RandomVelocity(50f, 30f, 60f);
                        int damage = (int)Player.GetTotalDamage<RogueDamageClass>().ApplyTo(9);
                        damage = Player.ApplyArmorAccDamageBonusesTo(damage);

                        int spark = Projectile.NewProjectile(spawnSource, position, velocity, ProjectileType<EGloveSpark>(), damage, 0f, Player.whoAmI);
                        if (spark.WithinBounds(Main.maxProjectiles))
                        {
                            Main.projectile[spark].DamageType = DamageClass.Generic;
                            Main.projectile[spark].localNPCHitCooldown = -1;
                        }
                    }
                }
            }
            modProj.stealthStrikeHitCount++;
        }
        #endregion
        #endregion

        #region Debuffs
        public void NPCDebuffs(NPC target, bool melee, bool ranged, bool magic, bool summon, bool rogue, bool whip, bool proj = false, bool noFlask = false)
        {
            if (melee && !noFlask) // Prevents Deep Sea Dumbell from snagging true melee debuff memes
            {
                if (eGauntlet)
                {
                    CalamityUtils.Inflict246DebuffsNPC(target, ModContent.BuffType<ElementalMix>());
                }
                if (ataxiaFire)
                {
                    CalamityUtils.Inflict246DebuffsNPC(target, BuffID.OnFire3, 4f);
                }
            }
            if ((melee || rogue || whip) && !noFlask)
            {
                if (flaskCrumbling)
                {
                    CalamityUtils.Inflict246DebuffsNPC(target, BuffType<Crumbling>());
                }
                if (flaskBrimstone)
                {
                    CalamityUtils.Inflict246DebuffsNPC(target, BuffType<BrimstoneFlames>(), 4f);
                }
                if (flaskHoly)
                {
                    target.AddBuff(BuffType<HolyFlames>(), 180, false);
                }
            }
            if (rogue && !noFlask)
            {
                switch (Player.meleeEnchant)
                {
                    case 1:
                        target.AddBuff(BuffID.Venom, 60 * Main.rand.Next(5, 10), false);
                        break;
                    case 2:
                        target.AddBuff(BuffID.CursedInferno, 60 * Main.rand.Next(3, 7), false);
                        break;
                    case 3:
                        target.AddBuff(BuffID.OnFire, 60 * Main.rand.Next(3, 7), false);
                        break;
                    case 5:
                        target.AddBuff(BuffID.Ichor, 60 * Main.rand.Next(10, 20), false);
                        break;
                    case 6:
                        target.AddBuff(BuffID.Confused, 60 * Main.rand.Next(1, 4), false);
                        break;
                    case 8:
                        target.AddBuff(BuffID.Poisoned, 60 * Main.rand.Next(5, 10), false);
                        break;
                    case 4:
                        target.AddBuff(BuffID.Midas, 120, false);
                        break;
                }
                if (titanHeartMask)
                {
                    target.AddBuff(BuffType<AstralInfectionDebuff>(), 120);
                }
                if (corrosiveSpine)
                {
                    target.AddBuff(BuffID.Poisoned, 240);
                }
            }
            if (summon)
            {
                if (pSoulArtifact && !profanedCrystal)
                    target.AddBuff(BuffType<HolyFlames>(), 300);

                if (profanedCrystalBuffs)
                {
                    bool empowered = pscState == (int)ProfanedSoulCrystal.ProfanedSoulCrystalState.Empowered;
                    if (empowered || Main.dayTime)
                        target.AddBuff(BuffType<HolyFlames>(), 600);
                    if (empowered || !Main.dayTime)
                        target.AddBuff(BuffType<Nightwither>(), 600);
                }

                if (divineBless)
                    target.AddBuff(BuffType<BanishingFire>(), 60);

                if (shadowMinions)
                    target.AddBuff(BuffID.ShadowFlame, 180);

                if (voltaicJelly)
                {
                    //100% chance for Star Tainted Generator or Nucleogenesis
                    //20% chance for Voltaic Jelly
                    if (Main.rand.NextBool(starTaintedGenerator ? 1 : 5))
                    {
                        target.AddBuff(BuffID.Electrified, 60);
                    }
                }

                if (starTaintedGenerator)
                {
                    target.AddBuff(BuffType<AstralInfectionDebuff>(), 180);
                    target.AddBuff(BuffType<Irradiated>(), 180);
                }
            }
            if (voidOfExtinction)
                CalamityUtils.Inflict246DebuffsNPC(target, BuffType<BrimstoneFlames>());
            if (frostFlare)
                CalamityUtils.Inflict246DebuffsNPC(target, BuffID.Frostburn2);
            if (omegaBlueChestplate)
                target.AddBuff(BuffType<CrushDepth>(), 180);
            if (sulphurSet)
                target.AddBuff(BuffID.Poisoned, 60);
            if (abyssalAmulet)
            {
                CalamityUtils.Inflict246DebuffsNPC(target, BuffType<RiptideDebuff>());
            }
            if (lumenousAmulet)
            {
                CalamityUtils.Inflict246DebuffsNPC(target, BuffType<CrushDepth>());
            }
            if (alchFlask)
            {
                CalamityUtils.Inflict246DebuffsNPC(target, BuffType<Plague>());
            }
            if (vexation)
            {
                if ((Player.armor[0].type == ItemType<ReaverHeadTank>() || Player.armor[0].type == ItemType<ReaverHeadExplore>() ||
                    Player.armor[0].type == ItemType<ReaverHeadMobility>()) && Player.armor[1].type == ItemType<ReaverScaleMail>() &&
                    Player.armor[2].type == ItemType<ReaverCuisses>())
                {
                    target.AddBuff(BuffID.Venom, 120, false);
                }
            }
        }
        #endregion

        #region Lifesteal
        public void ProjLifesteal(NPC target, Projectile proj, int damage, bool crit)
        {
            CalamityGlobalProjectile modProj = proj.Calamity();

            if (bloodflareSet && !target.IsAnEnemy(false) && !Player.moonLeech && target.lifeMax > 5)
            {
                if ((target.life < target.lifeMax * 0.5) && bloodflareHeartTimer <= 0)
                {
                    bloodflareHeartTimer = 300;
                    Item.NewItem(target.GetSource_Loot(), target.Hitbox, ItemID.Heart);
                }
            }

            if (gladiatorSword && target.IsAnEnemy(false) && target.life <= 0 && target.Calamity().gladiatorOnKill && target.lifeMax > 5)
            {
                target.Calamity().gladiatorOnKill = false;
                Projectile.NewProjectile(Player.GetSource_FromThis(), target.Center.X, target.Center.Y, target.velocity.X / 2, target.velocity.Y / 2, ModContent.ProjectileType<GladiatorHealOrb>(), 0, 0f);
            }

            if (Main.player[Main.myPlayer].lifeSteal > 0f && !Player.moonLeech && target.lifeMax > 5)
            {
                // Increases the degree to which Spectre Healing set contributes to the lifesteal cap
                if (Player.ghostHeal && proj.CountsAsClass<MagicDamageClass>())
                {
                    // This doesn't use Math.Round because it doesn't in vanilla
                    float cooldownMult = 0.2f;
                    cooldownMult -= proj.numHits * 0.05f;
                    if (cooldownMult < 0f)
                        cooldownMult = 0f;

                    float cooldown = damage * cooldownMult;
                    Main.player[Main.myPlayer].lifeSteal -= cooldown;
                }

                if (vampiricTalisman && proj.CountsAsClass<RogueDamageClass>() && crit)
                {
                    int heal = (int)Math.Round(damage * 0.015);
                    if (heal > BalancingConstants.LifeStealCap)
                        heal = BalancingConstants.LifeStealCap;

                    if (CalamityGlobalProjectile.CanSpawnLifeStealProjectile(1f, heal))
                        CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, heal, ProjectileID.VampireHeal, BalancingConstants.LifeStealRange, BalancingConstants.LifeStealAccessoryCooldownMultiplier);
                }

                if (bloodyGlove && proj.CountsAsClass<RogueDamageClass>() && modProj.stealthStrike)
                    CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, 2, ProjectileID.VampireHeal, BalancingConstants.LifeStealRange, BalancingConstants.LifeStealAccessoryCooldownMultiplier);

                if (target.IsAnEnemy(false))
                {
                    if (bloodflareThrowing && proj.CountsAsClass<ThrowingDamageClass>() && crit)
                        CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, 2, ProjectileID.VampireHeal, BalancingConstants.LifeStealRange, BalancingConstants.LifeStealSetBonusCooldownMultiplier);

                    if (bloodflareMelee && proj.IsTrueMelee())
                        CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, 2, ProjectileID.VampireHeal, BalancingConstants.LifeStealRange, BalancingConstants.LifeStealSetBonusCooldownMultiplier);
                }

                if (proj.CountsAsClass<MagicDamageClass>() && Player.ActiveItem().CountsAsClass<MagicDamageClass>())
                {
                    if (manaOverloader)
                    {
                        double healMult = 0.2;
                        healMult -= proj.numHits * healMult * 0.25;
                        int heal = (int)Math.Round(damage * healMult * (Player.statMana / (double)Player.statManaMax2));
                        if (heal > BalancingConstants.LifeStealCap)
                            heal = BalancingConstants.LifeStealCap;

                        if (CalamityGlobalProjectile.CanSpawnLifeStealProjectile(healMult, heal))
                            CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, heal, ProjectileType<ManaPolarizerHealOrb>(), BalancingConstants.LifeStealRange, BalancingConstants.LifeStealAccessoryCooldownMultiplier);
                    }
                }

                if (silvaSet)
                {
                    double healMult = 0.1;
                    healMult -= proj.numHits * healMult * 0.5;
                    int heal = (int)Math.Round(damage * healMult);
                    if (heal > BalancingConstants.LifeStealCap)
                        heal = BalancingConstants.LifeStealCap;

                    if (CalamityGlobalProjectile.CanSpawnLifeStealProjectile(healMult, heal))
                        CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, heal, ProjectileType<SilvaOrb>(), BalancingConstants.LifeStealRange, BalancingConstants.LifeStealSetBonusCooldownMultiplier);
                }
                else if (proj.CountsAsClass<MagicDamageClass>() && Player.ActiveItem().CountsAsClass<MagicDamageClass>())
                {
                    if (tarraMage)
                    {
                        double healMult = 0.1;
                        healMult -= proj.numHits * healMult * 0.5;
                        int heal = (int)Math.Round(damage * healMult);
                        if (heal > BalancingConstants.LifeStealCap)
                            heal = BalancingConstants.LifeStealCap;

                        if (CalamityGlobalProjectile.CanSpawnLifeStealProjectile(healMult, heal))
                            CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, heal, ProjectileType<ReaverHealOrb>(), BalancingConstants.LifeStealRange, BalancingConstants.LifeStealSetBonusCooldownMultiplier);
                    }
                    else if (ataxiaMage)
                    {
                        double healMult = 0.1;
                        healMult -= proj.numHits * healMult * 0.5;
                        int heal = (int)Math.Round(damage * healMult);
                        if (heal > BalancingConstants.LifeStealCap)
                            heal = BalancingConstants.LifeStealCap;

                        if (CalamityGlobalProjectile.CanSpawnLifeStealProjectile(healMult, heal))
                            CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, heal, ProjectileType<HydrothermicHealOrb>(), BalancingConstants.LifeStealRange, BalancingConstants.LifeStealSetBonusCooldownMultiplier);
                    }
                }

                if (reaverDefense)
                {
                    double healMult = 0.1;
                    healMult -= proj.numHits * healMult * 0.5;
                    int heal = (int)Math.Round(damage * healMult);
                    if (heal > BalancingConstants.LifeStealCap)
                        heal = BalancingConstants.LifeStealCap;

                    if (CalamityGlobalProjectile.CanSpawnLifeStealProjectile(healMult, heal))
                        CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, heal, ProjectileType<ReaverHealOrb>(), BalancingConstants.LifeStealRange, BalancingConstants.LifeStealReaverTankCooldownMultiplier);
                }

                if (proj.CountsAsClass<ThrowingDamageClass>())
                {
                    if (xerocSet && xerocDmg <= 0 && Player.ownedProjectileCounts[ProjectileType<EmpyreanEmber>()] < 3 && Player.ownedProjectileCounts[ProjectileType<EmpyreanBlast>()] < 3)
                    {
                        double healMult = 0.1;
                        healMult -= proj.numHits * healMult * 0.5;
                        int heal = (int)Math.Round(damage * healMult);
                        if (heal > BalancingConstants.LifeStealCap)
                            heal = BalancingConstants.LifeStealCap;

                        if (CalamityGlobalProjectile.CanSpawnLifeStealProjectile(healMult, heal))
                            CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, heal, ProjectileType<EmpyreanHealOrb>(), BalancingConstants.LifeStealRange, BalancingConstants.LifeStealSetBonusCooldownMultiplier);
                    }
                }
            }
        }

        public void ItemLifesteal(NPC target, Item item, int damage)
        {
            if (bloodflareSet && target.IsAnEnemy(false) && target.lifeMax > 5)
            {
                if ((target.life < target.lifeMax * 0.5) && bloodflareHeartTimer <= 0)
                {
                    bloodflareHeartTimer = 300;
                    Item.NewItem(target.GetSource_Loot(), target.Hitbox, ItemID.Heart);
                }
            }

            if (bloodflareMelee && item.CountsAsClass<MeleeDamageClass>() && target.lifeMax > 5)
            {
                if (target.IsAnEnemy(false) && Main.player[Main.myPlayer].lifeSteal > 0f && !Player.moonLeech)
                {
                    int heal = 4;
                    if (!Main.player[Main.myPlayer].moonLeech)
                    {
                        Main.player[Main.myPlayer].lifeSteal -= heal * BalancingConstants.LifeStealSetBonusCooldownMultiplier;

                        float lowestHealthCheck = 0f;
                        int healTarget = Player.whoAmI;
                        foreach (Player otherPlayer in Main.ActivePlayers)
                        {
                            if (!otherPlayer.dead && ((!Player.hostile && !otherPlayer.hostile) || Player.team == otherPlayer.team))
                            {
                                float playerDist = Vector2.Distance(target.Center, otherPlayer.Center);
                                if (playerDist < BalancingConstants.LifeStealRange && (otherPlayer.statLifeMax2 - otherPlayer.statLife) > lowestHealthCheck)
                                {
                                    lowestHealthCheck = otherPlayer.statLifeMax2 - otherPlayer.statLife;
                                    healTarget = otherPlayer.whoAmI;
                                }
                            }
                        }

                        // https://github.com/tModLoader/tModLoader/wiki/IEntitySource#detailed-list
                        var source = Player.GetSource_FromThis(ReaverHeadTank.HealOrbEntitySourceContext);
                        Projectile.NewProjectile(source, target.Center, Vector2.Zero, ProjectileID.VampireHeal, 0, 0f, Player.whoAmI, healTarget, heal);
                    }
                }
            }

            if (gladiatorSword && target.IsAnEnemy(false) && target.life <= 0 && target.Calamity().gladiatorOnKill && target.lifeMax > 5)
            {
                target.Calamity().gladiatorOnKill = false;
                Projectile.NewProjectile(Player.GetSource_FromThis(), target.Center, target.velocity * 0.5f, ModContent.ProjectileType<GladiatorHealOrb>(), 0, 0f);
            }

            if (reaverDefense)
            {
                if (Main.player[Main.myPlayer].lifeSteal > 0f && !Player.moonLeech && target.lifeMax > 5)
                {
                    double healMult = 0.1;
                    int heal = (int)Math.Round(damage * healMult);
                    if (heal > BalancingConstants.LifeStealCap)
                        heal = BalancingConstants.LifeStealCap;

                    if (heal > 0 && !Main.player[Main.myPlayer].moonLeech)
                    {
                        Main.player[Main.myPlayer].lifeSteal -= heal * BalancingConstants.LifeStealReaverTankCooldownMultiplier;

                        float lowestHealthCheck = 0f;
                        int healTarget = Player.whoAmI;
                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            Player otherPlayer = Main.player[i];
                            if (otherPlayer.active && !otherPlayer.dead && ((!Player.hostile && !otherPlayer.hostile) || Player.team == otherPlayer.team))
                            {
                                float playerDist = Vector2.Distance(target.Center, otherPlayer.Center);
                                if (playerDist < BalancingConstants.LifeStealRange && (otherPlayer.statLifeMax2 - otherPlayer.statLife) > lowestHealthCheck)
                                {
                                    lowestHealthCheck = otherPlayer.statLifeMax2 - otherPlayer.statLife;
                                    healTarget = otherPlayer.whoAmI;
                                }
                            }
                        }

                        // https://github.com/tModLoader/tModLoader/wiki/IEntitySource#detailed-list
                        var source = Player.GetSource_FromThis(ReaverHeadTank.HealOrbEntitySourceContext);
                        Projectile.NewProjectile(source, target.Center, Vector2.Zero, ProjectileType<ReaverHealOrb>(), 0, 0f, Player.whoAmI, healTarget, heal);
                    }
                }
            }
        }
        #endregion

        #region The Horseman's Blade
        public static void HorsemansBladeOnHit(Player player, int targetIdx, int damage, float knockback, int extraUpdateAmt = 0, int type = ProjectileID.FlamingJack)
        {
            int logicCheckScreenHeight = Main.LogicCheckScreenHeight;
            int logicCheckScreenWidth = Main.LogicCheckScreenWidth;
            int x = Main.rand.Next(100, 300);
            int y = Main.rand.Next(100, 300);
            switch (Main.rand.Next(4))
            {
                case 0:
                    x -= logicCheckScreenWidth / 2 + x;
                    break;
                case 1:
                    x += logicCheckScreenWidth / 2 - x;
                    break;
                case 2:
                    y -= logicCheckScreenHeight / 2 + y;
                    break;
                case 3:
                    y += logicCheckScreenHeight / 2 - y;
                    break;
                default:
                    break;
            }
            x += (int)player.position.X;
            y += (int)player.position.Y;
            float speed = 8f;
            Vector2 spawnPos = new Vector2(x, y);
            Vector2 velocity = Main.npc[targetIdx].DirectionFrom(spawnPos);
            velocity *= speed;

            var source = player.GetSource_ItemUse(player.ActiveItem());
            int projectile = Projectile.NewProjectile(source, spawnPos, velocity, type, damage, knockback, player.whoAmI, targetIdx, 0f);
            Main.projectile[projectile].extraUpdates += extraUpdateAmt;
        }
        #endregion
    }
}
