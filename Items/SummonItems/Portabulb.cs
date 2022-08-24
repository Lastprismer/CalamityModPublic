﻿using CalamityMod.Events;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.SummonItems
{
    [LegacyName("BulbofDoom")]
    public class Portabulb : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
            DisplayName.SetDefault("Portabulb");
            Tooltip.SetDefault("Summons Plantera when used in the Jungle\n" +
                "Enrages outside the Underground Jungle\n" +
                "Not consumable");
			NPCID.Sets.MPAllowedEnemies[NPCID.Plantera] = true;
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 11; // Truffle Worm
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.rare = ItemRarityID.Lime;
            Item.consumable = false;
        }

		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
		{
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossItem;
		}

        public override bool CanUseItem(Player player)
        {
            return player.ZoneJungle && !NPC.AnyNPCs(NPCID.Plantera) && !BossRushEvent.BossRushActive;
        }

        public override bool? UseItem(Player player)
        {
            SoundEngine.PlaySound(SoundID.Roar, player.position);
            if (Main.netMode != NetmodeID.MultiplayerClient)
                NPC.SpawnOnPlayer(player.whoAmI, NPCID.Plantera);
            else
                NetMessage.SendData(MessageID.SpawnBoss, -1, -1, null, player.whoAmI, NPCID.Plantera);

            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.JungleSpores, 15).
                AddIngredient(ItemID.SoulofNight, 10).
                AddIngredient(ItemID.SoulofLight, 10).
                AddIngredient<MurkyPaste>(3).
                AddIngredient(ItemID.Vine).
                AddIngredient<TrapperBulb>().
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
