﻿using Terraria.DataStructures;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class LeonidProgenitor : RogueWeapon
    {
        public static readonly Color blueColor = new Color(48, 208, 255);
        public static readonly Color purpleColor = new Color(208, 125, 218);

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Leonid Progenitor");
            Tooltip.SetDefault("Throws a bombshell that explodes, summoning a meteor to impact the site\n" +
                "Right click to throw a spread of gravity affected comets that explode, leaving behind a star\n" +
                "Stealth strikes lob a bombshell that additionally splits into comets on hit");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SafeSetDefaults()
        {
            Item.damage = 64;
            Item.Calamity().rogue = true;
            Item.knockBack = 3f;
            Item.useTime = Item.useAnimation = 15;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<LeonidProgenitorBombshell>();
            Item.shootSpeed = 12f;

            Item.width = 32;
            Item.height = 48;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.UseSound = SoundID.Item61;

            Item.value = CalamityGlobalItem.Rarity7BuyPrice;
            Item.rare = ItemRarityID.Lime;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.Calamity().StealthStrikeAvailable() || player.altFunctionUse != 2)
            {
                Item.UseSound = SoundID.Item61;
                Item.shoot = ModContent.ProjectileType<LeonidProgenitorBombshell>();
            }
            else
            {
                Item.UseSound = SoundID.Item88;
                Item.shoot = ModContent.ProjectileType<LeonidCometSmall>();
            }
            return base.CanUseItem(player);
        }

        public override float SafeSetUseTimeMultiplier(Player player)
        {
            if (player.Calamity().StealthStrikeAvailable() || player.altFunctionUse != 2)
                return 1f;
            return 0.75f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float stealthDamageFactor = player.Calamity().StealthStrikeAvailable() ? 1.25f : 1f;

            if (player.Calamity().StealthStrikeAvailable() || player.altFunctionUse != 2)
            {
                int bomb = Projectile.NewProjectile(source, position, velocity, type, (int)(damage * stealthDamageFactor), knockback, player.whoAmI);
                if (bomb.WithinBounds(Main.maxProjectiles))
                    Main.projectile[bomb].Calamity().stealthStrike = player.Calamity().StealthStrikeAvailable();
                return false;
            }
            else
            {
                float dmgMult = 0.5f;
                for (float i = -2.5f; i < 3f; ++i)
                {
                    Vector2 perturbedSpeed = velocity.RotatedBy(MathHelper.ToRadians(i));
                    Projectile.NewProjectile(source, position, perturbedSpeed, type, (int)(damage * dmgMult * stealthDamageFactor), knockback, player.whoAmI);
                }
            }
            return false;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Rogue/LeonidProgenitorGlow").Value);
        }
    }
}
