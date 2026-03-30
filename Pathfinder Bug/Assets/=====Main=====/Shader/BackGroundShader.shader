Shader "Custom/BackgroundReveal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} 
        _Color ("Tint Color", Color) = (1,1,1,1) 
        _RevealProgress ("Reveal Progress", Range(0, 1.5)) = 0 // Controls the progress of the circular reveal effect.
        _RevealMaxRadius ("Max Reveal Radius (UV)", Float) = 0.7 // Defines the maximum radius for the reveal circle in UV space.
        _RevealCenter ("Reveal Center (UV)", Vector) = (0.5,0.5,0,0) // Specifies the UV coordinates for the center of the circular reveal.
        _Feather ("Feather Edge", Range(0.001, 0.2)) = 0.05 // Determines the softness of the reveal circle's edge.
        _RotationSpeed ("Rotation Speed (Degrees/Progress)", Float) = 360 // The total rotation in degrees applied as _RevealProgress goes from 0 to 1.
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100 
        Blend SrcAlpha OneMinusSrcAlpha 
        Cull Off 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag 
            #pragma multi_compile_fog

            #include "UnityCG.cginc" 
            struct appdata
            {
                float4 vertex : POSITION; 
                float2 uv : TEXCOORD0;   
            };
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)  
                float4 vertex : SV_POSITION; 
            };

            sampler2D _MainTex;
            float4 _MainTex_ST; 
            fixed4 _Color;     
            float _RevealProgress; 
            float _RevealMaxRadius; 
            float4 _RevealCenter;  
            float _Feather;    
            float _RotationSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);   
                UNITY_TRANSFER_FOG(o,o.vertex);       
                return o;
            }
            // Fragment Shader: Calculates the final color for each pixel.
            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate rotation angle based on _RevealProgress and _RotationSpeed.
                // Convert degrees to radians (degrees * PI / 180).
                float rotationAngle = _RevealProgress * _RotationSpeed * UNITY_PI / 180.0; 
                float cosAngle = cos(rotationAngle);
                float sinAngle = sin(rotationAngle);

                // --- Apply rotation to UV coordinates for texture sampling ---
                // Start with current fragment UV.
                float2 rotatedUV = i.uv;
                
                // Translate UVs so that the rotation center becomes the origin (0,0).
                rotatedUV -= _RevealCenter.xy;

                // Apply 2D rotation matrix to the translated UVs.
                float tempX = rotatedUV.x; // Store original X before modifying.
                rotatedUV.x = tempX * cosAngle - rotatedUV.y * sinAngle;
                rotatedUV.y = tempX * sinAngle + rotatedUV.y * cosAngle;

                // Translate UVs back to their original offset relative to the rotation center.
                rotatedUV += _RevealCenter.xy;

                // Sample the main texture using the newly rotated UV coordinates.
                fixed4 col = tex2D(_MainTex, rotatedUV) * _Color; 
                // --- End UV rotation for texture sampling ---


                // Calculates vector from current fragment UV to the reveal center.
                // This vector is used for the circular mask, and its calculation is separate
                // from the texture sampling UV rotation to ensure the mask's position is fixed.
                float2 distVec = i.uv - _RevealCenter.xy; 

                // Aspect Ratio Correction for Circular Reveal:
                // Adjusts the X component of the distance vector based on screen aspect ratio
                // to maintain a visually circular shape regardless of non-uniform scaling of the UI element.
                float screenAspect = _ScreenParams.x / _ScreenParams.y;
                distVec.x *= screenAspect;
                
                // Calculates the magnitude of the aspect-corrected distance vector, representing distance from center.
                float distance = length(distVec); 

                // Determines the effective radius for the current reveal progress.
                float currentActiveRadius = _RevealProgress * _RevealMaxRadius;

                // Generates a smooth mask value based on distance from the center and feathering.
                // The mask is 0 outside the feathered radius and 1 inside.
                float mask = smoothstep(currentActiveRadius - _Feather, currentActiveRadius + _Feather, distance);
                
                // Inverts the mask so that the revealed area has a mask value of 1.
                mask = 1.0 - mask;
                
                col.a *= mask; // Applies the mask to the alpha channel of the final color.

                UNITY_APPLY_FOG(i.fogCoord, col); // Applies Unity's fog effect.
                return col;
            }
            ENDCG
        }
    }
    FallBack "Standard" 
}