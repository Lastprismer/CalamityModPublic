﻿using Terraria.DataStructures;
using CalamityMod.CalPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.Utilities;
using CalamityMod.Prefixes;

namespace CalamityMod.Items.Weapons.Rogue
{
    public abstract class RogueWeapon : ModItem
    {
        public virtual void SafeSetDefaults()
        {
        }

        public float StealthStrikePrefixBonus;

        public RogueWeapon()
        {
            StealthStrikePrefixBonus = 1f;
        }

        public override ModItem Clone(Item itemClone)
        {
            RogueWeapon myClone = (RogueWeapon)base.Clone(itemClone);
            myClone.StealthStrikePrefixBonus = StealthStrikePrefixBonus;
            return myClone;
        }

        public override int ChoosePrefix(UnifiedRandom rand)
        {
            WeightedRandom<string> newPrefix = new WeightedRandom<string>();
            newPrefix.Add("CalamityMod/Pointy", 1);
            newPrefix.Add("CalamityMod/Sharp", 1);
            newPrefix.Add("CalamityMod/Feathered", 1);
            newPrefix.Add("CalamityMod/Sleek", 1);
            newPrefix.Add("CalamityMod/Hefty", 1);
            newPrefix.Add("CalamityMod/Mighty", 1);
            newPrefix.Add("CalamityMod/Glorious", 1);
            newPrefix.Add("CalamityMod/Serrated", 1);
            newPrefix.Add("CalamityMod/Vicious", 1);
            newPrefix.Add("CalamityMod/Lethal", 1);
            newPrefix.Add("CalamityMod/Flawless", 1);
            newPrefix.Add("CalamityMod/Radical", 1);
            newPrefix.Add("CalamityMod/Blunt", 1);
            newPrefix.Add("CalamityMod/Flimsy", 1);
            newPrefix.Add("CalamityMod/Unbalanced", 1);
            newPrefix.Add("CalamityMod/Atrocious", 1);
            return ModContent.Find<ModPrefix>(newPrefix.Get()).Type;
        }

        public override bool PreReforge()
        {
            StealthStrikePrefixBonus = 1f;
            return true;
        }

        public override bool? PrefixChance(int pre, UnifiedRandom rand)
        {
            if (Item.maxStack > 1)
            {
                return false;
            }
            return null;
        }

        public sealed override void SetDefaults()
        {
            SafeSetDefaults();
            // item.melee = false /* tModPorter - this is redundant, for more info see https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide#damage-classes */ ;
            // item.ranged = false /* tModPorter - this is redundant, for more info see https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide#damage-classes */ ;
            // item.magic = false /* tModPorter - this is redundant, for more info see https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide#damage-classes */ ;
            Item.DamageType = DamageClass.Throwing;
            // item.summon = false /* tModPorter - this is redundant, for more info see https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide#damage-classes */ ;
        }

        // Add both the player's dedicated rogue damage and stealth strike damage as applicable.
        // Rogue weapons are internally throwing so they already benefit from throwing damage boosts.
        // 5E-06 to prevent downrounding is not needed anymore, added by TML itself
        public sealed override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            CalamityPlayer mp = player.Calamity();

            SafeModifyWeaponDamage(player, ref damage);

            // Both regular rogue damage stat and stealth damage are added to the weapon simultaneously.
            damage += mp.throwingDamage + mp.stealthDamage - 1f;

            // Boost (or lower) the weapon's damage if it has a stealth strike available and an associated prefix
            if (mp.StealthStrikeAvailable() && Item.prefix > 0)
                damage *= StealthStrikePrefixBonus;
        }

        public virtual void SafeModifyWeaponDamage(Player player, ref StatModifier damage) { }

        // Simply add the player's dedicated rogue crit chance.
        // Rogue crit isn't boosted by Calamity universal crit boosts, so this won't double-add universal crit.
        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            crit += player.Calamity().throwingCrit;
        }

        public sealed override float UseTimeMultiplier(Player player)
        {
            float baseMultiplier = SafeSetUseTimeMultiplier(player);
            float rogueAS = baseMultiplier == -1f ? 1f : baseMultiplier;
            if (Item.useTime == Item.useAnimation)
            {
                rogueAS += player.Calamity().rogueUseSpeedFactor;
            }
            return rogueAS;
        }

        public virtual float SafeSetUseTimeMultiplier(Player player) => -1f;

        public virtual void SafeModifyTooltips(List<TooltipLine> tooltips)
        {
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine damageTooltip = tooltips.FirstOrDefault(x => x.Name == "Damage" && x.Mod == "Terraria");
            if (damageTooltip != null)
            {
                // Replace the word "throwing" with "rogue" in the item's damage line.
                string text = damageTooltip.Text.Replace(" throwing ", " rogue ");

                // Split visible damage into stealth and non-stealth damage values if the player has the stealth mechanic available to them.
                Player p = Main.LocalPlayer;
                CalamityPlayer mp = p.Calamity();
                if (mp.rogueStealthMax > 0f)
                {
                    int damageNumberSubstringIndex = text.IndexOf(' ');
                    if (damageNumberSubstringIndex >= 0)
                    {
                        string restOfTooltip = text.Substring(damageNumberSubstringIndex);
                        int damageWithStealth = int.Parse(text.Substring(0, damageNumberSubstringIndex));

                        int damageWithoutStealth = (int)(Item.damage * (p.GetDamage<GenericDamageClass>().Additive + p.GetDamage(DamageClass.Throwing).Additive + mp.throwingDamage - 2f));
                        text = damageWithoutStealth + restOfTooltip + " : " + damageWithStealth + " stealth strike damage";
                    }
                }

                damageTooltip.Text = text;
            }

            // Add a tooltip line for the stealth strike damage bonus of the item's prefix, if applicable.
            if (Item.prefix > 0)
            {
                float ssDmgBoost = StealthStrikePrefixBonus - 1f;
                if (ssDmgBoost != 0f)
                {
                    bool badModifier = ssDmgBoost < 0f;
                    string txt = (badModifier ? "-" : "+") + Math.Round(Math.Abs(ssDmgBoost) * 100f) + "% stealth strike damage";
                    TooltipLine stealthTooltip = new TooltipLine(Mod, "PrefixSSDmg", txt)
                    {
                        IsModifier = true,
                        IsModifierBad = badModifier
                    };
                    tooltips.Add(stealthTooltip);
                }
            }

            SafeModifyTooltips(tooltips);
        }

        public override bool ConsumeItem(Player player) => Main.rand.NextFloat() <= player.Calamity().throwingAmmoCost;
    }
}
