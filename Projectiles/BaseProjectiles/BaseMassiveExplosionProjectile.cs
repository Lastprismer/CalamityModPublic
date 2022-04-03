using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.BaseProjectiles
{
    public abstract class BaseMassiveExplosionProjectile : ModProjectile
    {
        public ref float CurrentRadius => ref Projectile.ai[0];
        public ref float MaxRadius => ref Projectile.ai[1];
        public virtual bool UsesScreenshake { get; } = false;
        public virtual float GetScreenshakePower(float pulseCompletionRatio) => 0f;
        public virtual float Fadeout(float completion) => (1f - (float)Math.Sqrt(completion)) * 0.7f;
        public abstract int Lifetime { get; }
        public abstract Color GetCurrentExplosionColor(float pulseCompletionRatio);

        // Use the invisible projectile by default. This can be overridden in child types if desired.
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void AI()
        {
            if (UsesScreenshake)
            {
                float screenShakePower = GetScreenshakePower(Projectile.timeLeft / (float)Lifetime) * Utils.InverseLerp(1300f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                if (Main.LocalPlayer.Calamity().GeneralScreenShakePower < screenShakePower)
                    Main.LocalPlayer.Calamity().GeneralScreenShakePower = screenShakePower;
            }

            // Expand outward.
            CurrentRadius = MathHelper.Lerp(CurrentRadius, MaxRadius, 0.25f);
            Projectile.scale = MathHelper.Lerp(1.2f, 5f, Utils.InverseLerp(Lifetime, 0f, Projectile.timeLeft, true));

            // Adjust the hitbox.
            CalamityGlobalProjectile.ExpandHitboxBy(Projectile, (int)(CurrentRadius * Projectile.scale), (int)(CurrentRadius * Projectile.scale));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            float pulseCompletionRatio = Utils.InverseLerp(Lifetime, 0f, Projectile.timeLeft, true);
            Vector2 scale = new Vector2(1.5f, 1f);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + Projectile.Size * scale * 0.5f;
            Rectangle drawArea = new Rectangle(0, 0, Projectile.width, Projectile.height);
            Color fadeoutColor = new Color(new Vector4(Fadeout(pulseCompletionRatio)) * Projectile.Opacity);
            DrawData drawData = new DrawData(ModContent.Request<Texture2D>("Terraria/Misc/Perlin"), drawPosition, drawArea, fadeoutColor, Projectile.rotation, Projectile.Size, scale, SpriteEffects.None, 0);

            GameShaders.Misc["ForceField"].UseColor(GetCurrentExplosionColor(pulseCompletionRatio));
            GameShaders.Misc["ForceField"].Apply(drawData);
            drawData.Draw(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
