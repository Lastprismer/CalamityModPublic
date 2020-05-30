using CalamityMod.Items.Materials;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.World;
using CalamityMod.Projectiles.Typeless;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.SummonItems
{
    public class RuneofCos : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rune of Kos");
            Tooltip.SetDefault("A relic of the profaned flame\n" +
                "Contains the power hunted relentlessly by the sentinels of the cosmic devourer\n" +
                "When used in certain areas of the world it will unleash them\n" +
                "Not consumable");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 20;
            item.useAnimation = 45;
            item.useTime = 45;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.rare = 9;
            item.UseSound = SoundID.Item44;
            item.consumable = false;
            item.Calamity().customRarity = CalamityRarity.PureGreen;
        }

        public override bool CanUseItem(Player player)
        {
            return (player.ZoneSkyHeight || player.ZoneUnderworldHeight || player.ZoneDungeon) &&
                !NPC.AnyNPCs(ModContent.NPCType<StormWeaverHead>()) && !NPC.AnyNPCs(ModContent.NPCType<StormWeaverHeadNaked>()) && !NPC.AnyNPCs(ModContent.NPCType<CeaselessVoid>()) && !NPC.AnyNPCs(ModContent.NPCType<Signus>()) &&
				CalamityWorld.DoGSecondStageCountdown <= 0;
        }

        public override bool UseItem(Player player)
        {
            if (player.ZoneDungeon)
            {
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<CeaselessVoid>());
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					for (int num662 = 0; num662 < 2; num662++)
					{
						NPC.NewNPC((int)player.Center.X - 200, (int)player.Center.Y - 200, ModContent.NPCType<DarkEnergy>());
						NPC.NewNPC((int)player.Center.X + 200, (int)player.Center.Y - 200, ModContent.NPCType<DarkEnergy2>());
						NPC.NewNPC((int)player.Center.X, (int)player.Center.Y + 200, ModContent.NPCType<DarkEnergy3>());
					}
				}
			}
            else if (player.ZoneUnderworldHeight)
            {
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<Signus>());
            }
            else if (player.ZoneSkyHeight)
            {
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<StormWeaverHead>());
            }
            Main.PlaySound(SoundID.Roar, player.position, 0);
            return true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<UnholyEssence>(), 40);
            recipe.AddIngredient(ItemID.LunarBar, 10);
            recipe.AddIngredient(ItemID.FragmentSolar, 5);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
