﻿using Terraria.DataStructures;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class EternalBlizzard : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Eternal Blizzard");
            Tooltip.SetDefault("Fires an additional icicle arrow that shatters on impact");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults()
        {
            Item.damage = 80;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 38;
            Item.height = 62;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTurn = false;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.value = Item.buyPrice(0, 80, 0, 0);
            Item.rare = ItemRarityID.Yellow;
            Item.useAmmo = AmmoID.Arrow;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<IcicleArrowProj>();
            Item.shootSpeed = 11f;
            Item.Calamity().canFirePointBlankShots = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float SpeedX = velocity.X + (float)Main.rand.Next(-10, 11) * 0.05f;
            float SpeedY = velocity.Y + (float)Main.rand.Next(-10, 11) * 0.05f;
            int index = Projectile.NewProjectile(source, position.X, position.Y, SpeedX, SpeedY, ModContent.ProjectileType<IcicleArrowProj>(), (int)(damage * 0.7f), knockback, player.whoAmI);
            Main.projectile[index].noDropItem = true;

            return true;
        }
    }
}
