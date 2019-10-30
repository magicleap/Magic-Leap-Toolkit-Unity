// ---------------------------------------------------------------------
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

Shader "Magic Leap/Unlit/PointerLine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "White" {}
        _ScrollTex ("Pulse Texture", 2D) = "White" {}
        _Color1 ("Base Color",Color)= (1,0,0,1)
        _ColorR ("Fill Color ",Color)= (1,0,0,1)
        _POffset ("Pulse Offset", Range(0,1)) = 0.0
        _PulseLength ("Pulse Length", Range(1,20)) = 0.0
        _TexOffset ("Fill", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType" = "Opaque" }
        LOD 200
        ZWrite on
        Lighting Off
        Cull back
        Fog { Mode Off}
        CGPROGRAM
        #pragma surface surf Unlit   halfasview novertexlights exclude_path:prepass noambient noforwardadd nolightmap nodirlightmap

        half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half3 viewDir)
        {
            half3 h = normalize (lightDir + viewDir);
            half4 c;
            c.rgb = s.Albedo;
            c.a = s.Alpha;
            return c;
        }   
        
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_ScrollTex;
        };
      
        sampler2D _MainTex;
        sampler2D _ScrollTex;
        float _POffset;
        float _TexOffset;
        float _PulseLength;
        half3 _Color1;
        half3 _ColorR;

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed2 texoffset = IN.uv_MainTex;
            texoffset += float2(((_TexOffset*.5)*-1)+1.5,0);
            half4 maintex =tex2D (_MainTex, IN.uv_MainTex);
            half offsettex =tex2D (_MainTex, texoffset).r;
            half3 maincolor = lerp(_Color1,_ColorR,offsettex);
            half pulseoffset = lerp(1,_PulseLength*-1,_POffset);
            fixed2 scrolledpulse = float2(IN.uv_ScrollTex.x*_PulseLength,IN.uv_ScrollTex.y);
            scrolledpulse += float2( pulseoffset,0);
            half pulse = tex2D (_ScrollTex, scrolledpulse).r;
            o.Emission = (maincolor)+(pulse);
        }

        ENDCG
    } 

    Fallback "Diffuse"
}