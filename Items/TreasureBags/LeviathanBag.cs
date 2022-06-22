﻿using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Vanity;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.NPCs.Leviathan;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.TreasureBags
{
    public class LeviathanBag : ModItem
    {
        public override int BossBagNPC => ModContent.NPCType<Anahita>();

        public override void SetStaticDefaults()
        {
            SacrificeTotal = 3;
            DisplayName.SetDefault("Treasure Bag (Leviathan and Anahita)");
            Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");
        }

        public override void SetDefaults()
        {
            Item.maxStack = 999;
            Item.consumable = true;
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Cyan;
            Item.expert = true;
        }

        public override bool CanRightClick() => true;

        public override void PostUpdate() => CalamityUtils.ForceItemIntoWorld(Item);

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return CalamityUtils.DrawTreasureBagInWorld(Item, spriteBatch, ref rotation, ref scale, whoAmI);
        }

        public override void OpenBossBag(Player player)
        {
            // IEntitySource my beloathed
            var s = player.GetSource_OpenItem(Item.type);

            // siren & levi are available PHM, so this check is necessary to keep vanilla consistency
            if (Main.hardMode)
				player.TryGettingDevArmor(s);

            // Weapons
            float w = DropHelper.BagWeaponDropRateFloat;
            DropHelper.DropEntireWeightedSet(s, player,
                DropHelper.WeightStack<Greentide>(w),
                DropHelper.WeightStack<Leviatitan>(w),
                DropHelper.WeightStack<AnahitasArpeggio>(w),
                DropHelper.WeightStack<Atlantis>(w),
                DropHelper.WeightStack<GastricBelcherStaff>(w),
                DropHelper.WeightStack<BrackishFlask>(w),
                DropHelper.WeightStack<LeviathanTeeth>(w),
                DropHelper.WeightStack<PearlofEnthrallment>(w)
            );

            // Equipment
            DropHelper.DropItem(s, player, ModContent.ItemType<LeviathanAmbergris>());
            DropHelper.DropItemChance(s, player, ModContent.ItemType<TheCommunity>(), 0.1f);

            // Vanity
            DropHelper.DropItemChance(s, player, ModContent.ItemType<LeviathanMask>(), 7);
            DropHelper.DropItemChance(s, player, ModContent.ItemType<AnahitaMask>(), 7);

            // Fishing
            DropHelper.DropItemChance(s, player, ItemID.HotlineFishingHook, 10);
            DropHelper.DropItemChance(s, player, ItemID.BottomlessBucket, 10);
            DropHelper.DropItemChance(s, player, ItemID.SuperAbsorbantSponge, 10);
            DropHelper.DropItemChance(s, player, ItemID.FishingPotion, 5, 5, 8);
            DropHelper.DropItemChance(s, player, ItemID.SonarPotion, 5, 5, 8);
            DropHelper.DropItemChance(s, player, ItemID.CratePotion, 5, 5, 8);
        }
    }
}
