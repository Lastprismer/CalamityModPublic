sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;

float4 PixelShaderFunction(float4 sampleColor : TEXCOORD, float2 coords : TEXCOORD0) : COLOR0
{
    float frameY = (coords.y * uImageSize0.y - uSourceRect.y) / uSourceRect.w; // Gets a 0-1 representation of the y position on a given frame, with 0 being the top, and 1 being the bottom.
    float2 swirlOffset = float2(sin(uTime * 0.96 + 1.754) * 0.31, sin(uTime * 0.96) * 0.16) * uSaturation;
    float4 color = tex2D(uImage0, coords);
    float4 noiseColor = tex2D(uImage1, frac(float2(coords.x, frameY + uTime * 0.36) + swirlOffset - uWorldPosition * 0.0006) * 0.26);
    float4 flameColor = tex2D(uImage1, frac(float2(coords.x, frameY + uTime * 0.44)) * 0.1);
    float brightnessFactor = noiseColor.r * 2;
    
    float normalizedDistanceFromCenter = distance(float2(coords.x, frameY), float2(0.5, 0.5)) * 2;
    
    // Cause "ring" fades based on distance.
    float fadeToNormal = 1 - saturate((normalizedDistanceFromCenter - 0.67) / 0.3);
    
    // Cause noise-based stars to appear all across the sprite.
    float3 starColor = lerp(uColor * 1.1, uSecondaryColor, sin(brightnessFactor * 6.283) * 0.5 + 0.5);
    
    // Prepare a dull, bluish-greyish background across the sprite for contrast.
    color = lerp(color, float4(43 / 255.0, 58 / 255.0, 92 / 255.0, 1) * color.a, fadeToNormal * 0.45);
    
    // Fade to the secondary color based on the flame color map. This helps the overall result more vibrant.
    color = lerp(color, float4(uSecondaryColor, 1) * color.a, flameColor.r * 0.5);
    
    // And create stars based on a noise texture that rise upward.
    color = lerp(color, float4(starColor, 1) * color.a, fadeToNormal * pow(brightnessFactor, 6) * 0.15);
    return color * (1 + brightnessFactor);
}
technique Technique1
{
    pass DyePass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}