using CalamityMod.Projectiles.Pets;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Pets
{
    public class BabyGhostBellBuff : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Baby Ghost Bell");
            Description.SetDefault("Be careful not to pop the bubble");
            Main.buffNoTimeDisplay[Type] = true;
            Main.lightPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 18000;
            player.Calamity().babyGhostBell = true;
            player.Calamity().lightStrength += 2;
            bool petProjectileNotSpawned = player.ownedProjectileCounts[ModContent.ProjectileType<BabyGhostBell>()] <= 0;
            if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer)
            {
                Projectile.NewProjectile(player.position.X + (float)(player.width / 2), player.position.Y + (float)(player.height / 2), 0f, 0f, ModContent.ProjectileType<BabyGhostBell>(), 0, 0f, player.whoAmI, 0f, 0f);
            }
        }
    }
}
