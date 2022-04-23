Shader "Custom/BiomeTerrain" {
    Properties {
		testTexture("Texture", 2D) = "white"{}
    }
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.5
		
		const static int maxNumBiomes = 10;
		/*const static int maxLayerCount = 8;
		const static int maxMapSize = 2809;
		const static float epsilon = 1E-4;*/

		int numBiomes;
		int mapSize;
		float3 biomeColours[maxNumBiomes];
		float2 biomeOrigin;

		UNITY_DECLARE_TEX2D(biomeMap);
		
		float worldSizeRatio;

		float minHeight;
		float maxHeight;

		struct Input {
			float3 worldPos;
			float3 worldNormal;
		};

		float inverseLerp(float a, float b, float value) {
			return saturate((value-a)/(b-a));
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			int biomeX = round((IN.worldPos.x - biomeOrigin.x) / worldSizeRatio);
			int biomeY = round((IN.worldPos.z - biomeOrigin.y) / worldSizeRatio);
			float2 uv = (biomeX, biomeY);
			float4 biomeColour = UNITY_SAMPLE_TEX2D(biomeMap, uv);
			o.Albedo = biomeColour;

			//o.Albedo = o.Albedo * float3 (IN.worldPos.x, IN.worldPos.y, IN.worldPos.z); 
		}
        ENDCG
    }
    FallBack "Diffuse"
}