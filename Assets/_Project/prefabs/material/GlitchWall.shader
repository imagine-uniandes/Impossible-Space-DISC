Shader "Custom/GlitchWall"
{
    Properties
    {
        _GlitchColor ("Glitch Color", Color) = (0, 1, 0.4, 1)
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.6
        _BlockSpeed ("Block Glitch Speed", Range(0, 5)) = 2.0
        _ScanlineSpeed ("Scanline Speed", Range(0, 10)) = 3.0
        _ScanlineDensity ("Scanline Density", Range(10, 200)) = 80.0
        _GridScale ("Grid Scale", Range(1, 50)) = 20.0
        _EmissionStrength ("Emission Strength", Range(0, 3)) = 1.2
        _ChromaShift ("Chromatic Aberration", Range(0, 0.05)) = 0.008
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _GlitchColor;
                float _GlitchIntensity;
                float _BlockSpeed;
                float _ScanlineSpeed;
                float _ScanlineDensity;
                float _GridScale;
                float _EmissionStrength;
                float _ChromaShift;
            CBUFFER_END

            // Hash function for pseudo-random numbers
            float hash(float2 p)
            {
                p = frac(p * float2(234.34, 435.345));
                p += dot(p, p + 34.23);
                return frac(p.x * p.y);
            }

            float hash1(float n)
            {
                return frac(sin(n) * 43758.5453);
            }

            // Glitch block displacement
            float2 glitchBlock(float2 uv, float time)
            {
                float blockY = floor(uv.y * 20.0);
                float timeSlot = floor(time * _BlockSpeed * 2.0);
                float rand = hash(float2(blockY, timeSlot));
                float rand2 = hash(float2(blockY + 1.0, timeSlot));

                float glitchActive = step(0.92 - _GlitchIntensity * 0.4, rand);
                float offsetX = (rand2 - 0.5) * 0.08 * _GlitchIntensity * glitchActive;
                return float2(uv.x + offsetX, uv.y);
            }

            // Scanline effect
            float scanline(float2 uv, float time)
            {
                float scanVal = sin((uv.y - time * _ScanlineSpeed * 0.05) * _ScanlineDensity * 3.14159);
                return (scanVal * 0.5 + 0.5) * 0.15 + 0.85;
            }

            // Digital grid / circuit pattern
            float digitalGrid(float2 uv, float time)
            {
                float2 grid = frac(uv * _GridScale);
                float lines = step(0.95, grid.x) + step(0.95, grid.y);
                lines = min(lines, 1.0);

                // Animate some cells blinking
                float2 cell = floor(uv * _GridScale);
                float cellRand = hash(cell);
                float blink = step(0.7, sin(time * cellRand * 5.0 + cellRand * 6.28));
                float cellFill = step(0.85, cellRand) * blink * 0.4;

                return saturate(lines * 0.5 + cellFill);
            }

            // Vertical flickering bands
            float flickerBands(float2 uv, float time)
            {
                float band = floor(uv.y * 8.0);
                float t = floor(time * _BlockSpeed * 3.0);
                float rand = hash(float2(band, t));
                return 1.0 - step(0.97 - _GlitchIntensity * 0.3, rand) * 0.35;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float time = _Time.y;
                float2 uv = IN.uv;

                // Apply block glitch displacement
                float2 glitchedUV = glitchBlock(uv, time);

                // Chromatic aberration: shift R and B channels slightly
                float2 uvR = glitchedUV + float2(_ChromaShift, 0);
                float2 uvB = glitchedUV - float2(_ChromaShift, 0);

                float gridR = digitalGrid(uvR, time);
                float gridG = digitalGrid(glitchedUV, time);
                float gridB = digitalGrid(uvB, time);

                // Scanlines
                float scan = scanline(uv, time);

                // Flicker
                float flicker = flickerBands(uv, time);

                // Base dark background with subtle noise
                float noise = hash(float2(uv.x * 100.0 + time * 0.3, uv.y * 100.0)) * 0.04;

                // Compose color with tint
                float3 col;
                col.r = gridR * _GlitchColor.r * 0.6;
                col.g = gridG * _GlitchColor.g;
                col.b = gridB * _GlitchColor.b * 0.7;

                col *= scan * flicker;
                col += noise;

                // Emission boost
                col *= _EmissionStrength;

                // Occasional full-row bright flash
                float flashRow = floor(uv.y * 30.0);
                float flashT = floor(time * 10.0);
                float flashRand = hash(float2(flashRow, flashT));
                float flash = step(0.98, flashRand) * 0.6 * _GlitchIntensity;
                col += flash * _GlitchColor.rgb;

                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
