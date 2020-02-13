using CalamityMod.CalPlayer;
using CalamityMod.Projectiles.Summon;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class SquirrelSquireBuff : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Squirrel Squire");
            Description.SetDefault("The squirrel squire will protect you");
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            CalamityPlayer modPlayer = player.Calamity();
            if (player.ownedProjectileCounts[ModContent.ProjectileType<SquirrelSquireMinion>()] > 0)
            {
                modPlayer.squirrel = true;
            }
            if (!modPlayer.squirrel)
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
            else
            {
                player.buffTime[buffIndex] = 18000;
            }
        }
    }
}
