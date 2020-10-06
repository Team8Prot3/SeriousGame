Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _Radius("Radius", float) = 10.0
        _MainTex("MainTex (Sprite)", 2D) = "white" {}
    }
        SubShader{
         Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
         LOD 100

         ZWrite Off
         Blend SrcAlpha OneMinusSrcAlpha

         Pass {
             CGPROGRAM
                 #pragma vertex vert
                 #pragma fragment frag

                 #include "UnityCG.cginc"

                 struct appdata_t {
                     float4 vertex : POSITION;
                     float2 texcoord : TEXCOORD0;
                     float4 color: COLOR;
                 };

                 struct v2f {
                     float4 vertex : SV_POSITION;
                     half2 texcoord : TEXCOORD0;
                     float4 color: COLOR;
                 };

                 sampler2D _MainTex;
                 float4 _MainTex_ST;
                 float _Radius;

                 // vertex function
                 v2f vert(appdata_t v)
                 {
                     v2f o;
                     o.vertex = UnityObjectToClipPos(v.vertex); // tranforms the vertex from object space to the screen
                     o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                     o.color = v.color;

                     float dist = distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, v.vertex));
                     float alpha = smoothstep(0, _Radius, dist);
                     o.color.a = 1 - alpha;
                     return o;
                 }

                 // fragment function
                 fixed4 frag(v2f i) : SV_Target
                 {
                     fixed4 main = tex2D(_MainTex, i.texcoord);
                     return fixed4(main.r, main.g, main.b, (main.a * i.color.a));
                 }
             ENDCG
         }
    }
}
