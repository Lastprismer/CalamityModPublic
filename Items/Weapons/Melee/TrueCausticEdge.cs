using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using CalamityMod.Projectiles.Melee;

namespace CalamityMod.Items.Weapons.Melee
{
    public class TrueCausticEdge : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("True Caustic Edge");
            Tooltip.SetDefault("Fires a bouncing caustic beam\n" +
                "Inflicts on fire, poison, and venom");
        }

        public override void SetDefaults()
        {
            item.width = 64;
            item.damage = 42;
            item.melee = true;
            item.useAnimation = 28;
            item.useStyle = 1;
            item.useTime = 28;
            item.useTurn = true;
            item.knockBack = 5.75f;
            item.UseSound = SoundID.Item1;
            item.autoReuse = true;
            item.height = 74;
            item.value = Item.buyPrice(0, 36, 0, 0);
            item.rare = 5;
            item.shoot = ModContent.ProjectileType<TrueCausticEdgeProjectile>();
            item.shootSpeed = 16f;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "CausticEdge");
            recipe.AddIngredient(ItemID.FlaskofCursedFlames, 5);
            recipe.AddIngredient(ItemID.FlaskofPoison, 5);
            recipe.AddIngredient(ItemID.Deathweed, 3);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
            recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "CausticEdge");
            recipe.AddIngredient(ItemID.FlaskofIchor, 5);
            recipe.AddIngredient(ItemID.FlaskofPoison, 5);
            recipe.AddIngredient(ItemID.Deathweed, 3);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3))
            {
                int dust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 74);
            }
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Poisoned, 600);
            target.AddBuff(BuffID.OnFire, 300);
            target.AddBuff(BuffID.Venom, 300);
        }
    }
}
