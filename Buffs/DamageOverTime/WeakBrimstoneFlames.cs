﻿using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.DamageOverTime
{
    public class WeakBrimstoneFlames : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex) => player.Calamity().weakBrimstoneFlames = true;
    }
}
