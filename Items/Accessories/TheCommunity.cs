﻿using CalamityMod.CalPlayer;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class TheCommunity : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
            DisplayName.SetDefault("The Community");
            Tooltip.SetDefault("The heart of (most of) the Terraria community\n" +
                "Starts off with weak buffs to all of your stats\n" +
                "The stat buffs become more powerful as you progress\n" +
                "Reduces the DoT effects of harmful debuffs inflicted on you\n" +
                "Thank you to all of my supporters who made this mod a reality");
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 10));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 64;
            Item.value = CalamityGlobalItem.Rarity7BuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
            Item.Calamity().customRarity = CalamityRarity.Rainbow;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.community = true;
        }

        // Community and Shattered Community are mutually exclusive
        public override bool CanEquipAccessory(Player player, int slot, bool modded) => !player.Calamity().shatteredCommunity;

        // Returns the total power of the Community, from 0.0 to 1.0 (0% to 100%), based on bosses defeated in the world.
        private static readonly int TotalCountedBosses = 42;
        internal static float CalculatePower()
        {
            int numBosses = 0;
            numBosses += NPC.downedSlimeKing.ToInt();
            numBosses += DownedBossSystem.downedDesertScourge.ToInt();
            numBosses += NPC.downedBoss1.ToInt();
            numBosses += DownedBossSystem.downedCrabulon.ToInt();
            numBosses += NPC.downedBoss2.ToInt();
            numBosses += (DownedBossSystem.downedHiveMind || DownedBossSystem.downedPerforator).ToInt();
            numBosses += NPC.downedQueenBee.ToInt();
            numBosses += NPC.downedBoss3.ToInt();
            numBosses += NPC.downedDeerclops.ToInt();
            numBosses += DownedBossSystem.downedSlimeGod.ToInt(); // 10
            numBosses += Main.hardMode.ToInt();
            numBosses += NPC.downedQueenSlime.ToInt();
            numBosses += DownedBossSystem.downedCryogen.ToInt();
            numBosses += NPC.downedMechBoss1.ToInt();
            numBosses += DownedBossSystem.downedAquaticScourge.ToInt();
            numBosses += NPC.downedMechBoss2.ToInt();
            numBosses += DownedBossSystem.downedBrimstoneElemental.ToInt();
            numBosses += NPC.downedMechBoss3.ToInt();
            numBosses += DownedBossSystem.downedCalamitas.ToInt();
            numBosses += NPC.downedPlantBoss.ToInt(); // 20
            numBosses += DownedBossSystem.downedLeviathan.ToInt();
            numBosses += DownedBossSystem.downedAstrumAureus.ToInt();
            numBosses += NPC.downedGolemBoss.ToInt();
            numBosses += DownedBossSystem.downedPlaguebringer.ToInt();
            numBosses += NPC.downedFishron.ToInt();
            numBosses += NPC.downedEmpressOfLight.ToInt();
            numBosses += DownedBossSystem.downedRavager.ToInt();
            numBosses += NPC.downedAncientCultist.ToInt();
            numBosses += DownedBossSystem.downedAstrumDeus.ToInt();
            numBosses += NPC.downedMoonlord.ToInt(); // 30
            numBosses += DownedBossSystem.downedGuardians.ToInt();
            numBosses += DownedBossSystem.downedDragonfolly.ToInt();
            numBosses += DownedBossSystem.downedProvidence.ToInt();
            numBosses += DownedBossSystem.downedCeaselessVoid.ToInt();
            numBosses += DownedBossSystem.downedStormWeaver.ToInt();
            numBosses += DownedBossSystem.downedSignus.ToInt();
            numBosses += DownedBossSystem.downedPolterghast.ToInt();
            numBosses += DownedBossSystem.downedBoomerDuke.ToInt();
            numBosses += DownedBossSystem.downedDoG.ToInt();
            numBosses += DownedBossSystem.downedYharon.ToInt(); // 40
            numBosses += DownedBossSystem.downedExoMechs.ToInt();
            numBosses += DownedBossSystem.downedSCal.ToInt();
            return numBosses / (float)TotalCountedBosses;
        }
    }
}
