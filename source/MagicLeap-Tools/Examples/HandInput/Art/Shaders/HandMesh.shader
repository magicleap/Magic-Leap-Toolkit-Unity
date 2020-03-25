// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

Shader "Magic Leap/HandMesh"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags {"Queue"="geometry" "RenderType"="opaque" }
        LOD 200
        cull back
        Lighting Off
        zwrite on

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float3 viewDir : TEXCOORD5;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half3 worldNormal : TEXCOORD6;
                half3 viewDir : TEXCOORD5;
                float4 color : COLOR0;

                UNITY_VERTEX_OUTPUT_STEREO

            };

            float4 _Color;
            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));

                float dotProduct = 1 - dot(v.normal, viewDir);
                o.color = smoothstep(1 - 1, 1.0, dotProduct);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float fres = (i.color.r*2)-.7;
                float4 comp = _Color;
                comp *= saturate(fres);
                return comp;
            }
            ENDCG
        }

        UsePass "VertexLit/SHADOWCASTER"
    }
}