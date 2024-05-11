﻿using CalamityMod.Tiles.Crags;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Crags
{
    public class BrimstoneSlab : ModTile
    {
        private int subsheetWidth = 450;
        private int subsheetHeight = 198;

        public TileFraming.MergeFrameData tileAdjacency;
        public TileFraming.MergeFrameData secondTileAdjacency;

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithHell(Type);

            AddMapEntry(new Color(79, 55, 70));
            MineResist = 2f;
            MinPick = 100;
            HitSound = SoundID.Tink;
            DustType = 235;

            TileFraming.SetUpUniversalMerge(Type, ModContent.TileType<BrimstoneSlag>(), "CalamityMod/Tiles/Merges/BrimstoneSlagMerge", out tileAdjacency);
            TileFraming.SetUpUniversalMerge(Type, TileID.Ash, "CalamityMod/Tiles/Merges/AshMerge", out secondTileAdjacency);
        }

        public override bool CanExplode(int i, int j)
        {
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = i % 2 * subsheetWidth;
            frameYOffset = j % 2 * subsheetHeight;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            TileFraming.DrawUniversalMergeFrames(i, j, secondTileAdjacency, tileAdjacency);
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            TileFraming.GetAdjacencyData(i, j, ModContent.TileType<BrimstoneSlag>(), tileAdjacency);
            TileFraming.GetAdjacencyData(i, j, TileID.Ash, secondTileAdjacency);
            return TileFraming.BrimstoneFraming(i, j, resetFrame);
        }
    }
}
