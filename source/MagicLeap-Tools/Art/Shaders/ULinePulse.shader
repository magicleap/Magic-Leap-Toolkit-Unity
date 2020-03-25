// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

Shader "Magic Leap/Unlit/PointerLine" 
{
    Properties 
    {
        
        _Color1 ("Base Color", Color) = (1,1,1,1)
        _ColorR ("Fill Color", Color) = (1,1,1,1)
        _ColorP ("Pulse Color", Color) = (1,1,1,1)
        _PulseOffset ("Pulse Offset", Range(0,1)) = 0.0
        _PulseLength ("Pulse Length", Range(0,1)) = 0.0
        _TexOffset ("Fill", Range(0,1)) = 0.0
        _Fade ("Fade", Range(0,1)) = 1.0
    }

    SubShader 
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 200
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass 
        {  
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 3.0
                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 pos : POSITION;
                    float2 texcoord : TEXCOORD0;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f {
                    float4 pos : SV_POSITION;
                    half2 texcoord : TEXCOORD0;


                    UNITY_VERTEX_OUTPUT_STEREO

                };

                float _PulseOffset;
        		float _TexOffset;
        		float _PulseLength;
        		half4 _Color1;
        		half4 _ColorR;
        		half4 _ColorP;
        		float _Fade;

                v2f vert (appdata v)
                {
                    v2f o;

                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_OUTPUT(v2f, o);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                    o.pos = UnityObjectToClipPos (v.pos);
                    o.texcoord = v.texcoord;


                    return o;
                }
            
                fixed4 frag (v2f i) : SV_Target
                {
                	fixed pulseoffset=(_PulseOffset*2)-1;
                	fixed pulselength = saturate((_PulseLength*.5)+.5);
                	fixed invpulselength = 1 - _PulseLength;
                	invpulselength *=50;

                	//Fill Gradient
                	fixed fillgrad = i.texcoord.x - _TexOffset;
                	fillgrad = lerp(0,1,ceil(fillgrad));

                	//Pulse Gradient
                	fixed pulsefront = (i.texcoord.x - 1 + (pulselength-pulseoffset));
                	pulsefront = lerp(0,1,saturate(pulsefront*(10+invpulselength)));
                	fixed pulseback =  ((pulselength+pulseoffset) - i.texcoord.x);
                	pulseback = lerp(0,1,saturate(pulseback*(10+invpulselength)));
                	half pulsegrad = pulsefront*pulseback;

                	//Comp
                    fixed4 col = lerp(_ColorR, _Color1,fillgrad);
                    col = lerp(col,_ColorP,pulsegrad);
                    col.a *= _Fade;

                    return col;
                }
            ENDCG
        }
    }
}
