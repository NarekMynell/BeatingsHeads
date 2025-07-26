Shader "Hidden/DecalBlend"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _DecalTex ("Decal Texture", 2D) = "white" {}
        _DecalCenter ("Decal Center", Vector) = (0.5, 0.5, 0, 0)
        _DecalSize ("Decal Size", Vector) = (0.1, 0.1, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _DecalTex;
            float2 _DecalCenter;
            float2 _DecalSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Հավելում է դեկալը եթե UV-ն [0,1] է
            fixed4 SampleDecal(float2 uv, float2 center, float2 size)
            {
                float2 decalUV = (uv - center) / size + 0.5;
                if (decalUV.x < 0.0 || decalUV.x > 1.0 || decalUV.y < 0.0 || decalUV.y > 1.0)
                    return fixed4(0,0,0,0);
                return tex2D(_DecalTex, decalUV);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);

                fixed result = 0;

                result += SampleDecal(i.uv, _DecalCenter, _DecalSize).r;

                result += SampleDecal(i.uv, _DecalCenter - float2(1.0, 0.0), _DecalSize).r;
                result += SampleDecal(i.uv, _DecalCenter + float2(1.0, 0.0), _DecalSize).r;

                result += SampleDecal(i.uv, _DecalCenter - float2(0.0, 1.0), _DecalSize).r;
                result += SampleDecal(i.uv, _DecalCenter + float2(0.0, 1.0), _DecalSize).r;

                result += SampleDecal(i.uv, _DecalCenter - float2(1.0, 1.0), _DecalSize).r;
                result += SampleDecal(i.uv, _DecalCenter + float2(1.0, 1.0), _DecalSize).r;
                result += SampleDecal(i.uv, _DecalCenter + float2(1.0, -1.0), _DecalSize).r;
                result += SampleDecal(i.uv, _DecalCenter - float2(1.0, -1.0), _DecalSize).r;

                // Կարող ես ընտրել max, add կամ lerp blend
                color.r = max(color.r, result); // ✔️ պահում է իրական արժեքը, բայց չի clipped

                return color;
            }

            ENDCG
        }
    }
}
