using System;
using System.Linq;
using UnityEngine;

namespace ProceduralTerrainGeneration.Data {
	[CreateAssetMenu(menuName = "Terrain Data/Texture Data")]
	public class TextureData : UpdatableData {

		const int textureSize = 512;
		const TextureFormat textureFormat = TextureFormat.RGB565;

		public Layer[] layers;

		float savedMinHeight;
		float savedMaxHeight;
	
		ComputeBuffer buffer;
		public void ApplyToMaterial(Material material) {
		
			material.SetInt ("layerCount", layers.Length);
			material.SetColorArray ("baseColours", layers.Select(x => x.tint).ToArray());
			material.SetFloatArray ("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
			material.SetFloatArray ("baseBlends", layers.Select(x => x.blendStrength).ToArray());
			material.SetFloatArray ("baseColourStrength", layers.Select(x => x.tintStrength).ToArray());
			material.SetFloatArray ("baseTextureScales", layers.Select(x => x.textureScale).ToArray());
			Texture2DArray texturesArray = GenerateTextureArray (layers.Select (x => x.texture).ToArray ());
			material.SetTexture ("baseTextures", texturesArray);
    
			UpdateMeshHeights (material, savedMinHeight, savedMaxHeight);
		}

		public void ApplyToBiomeMaterial(Material material, BiomeMapSettings settings, MeshSettings meshSettings, BiomeMap map, Vector3 centre) {
			
			material.SetColorArray("biomeColours", settings.Biomes.Select(x => x.biomeColour).ToArray());
			material.SetInt("numBiomes",settings.Biomes.Length);
		
			int size = map.biomeMapIndexes.GetLength (0);
			float worldSize = meshSettings.meshWorldSize;
			float ratio = worldSize / size;
			
			material.SetFloat("worldSizeRatio",ratio);
			material.SetFloatArray("biomeOrigin",new []{centre.x - worldSize/2, centre.z - worldSize/2});
			material.SetInt("mapSize",size);

			ShaderMap[] shaderMap = ShaderMapFromBiomes (map);
			int bufferSize = ShaderMap.Size ();
			buffer = new ComputeBuffer (shaderMap.Length, bufferSize);
			if (Application.isEditor) {
				GC.SuppressFinalize(buffer);
			}
			buffer.SetData(shaderMap);
			material.SetBuffer("biomeMap",buffer);

			UpdateMeshHeights (material, savedMinHeight, savedMaxHeight);
		}

		public void UpdateMeshHeights(Material material, float minHeight, float maxHeight) {
			savedMinHeight = minHeight;
			savedMaxHeight = maxHeight;

			material.SetFloat ("minHeight", minHeight);
			material.SetFloat ("maxHeight", maxHeight);
		}

		Texture2DArray GenerateTextureArray(Texture2D[] textures) {
			Texture2DArray textureArray = new Texture2DArray (textureSize, textureSize, textures.Length, textureFormat, true);
			for (int i = 0; i < textures.Length; i++) {
				textureArray.SetPixels (textures [i].GetPixels (), i);
			}
			textureArray.Apply ();
			return textureArray;
		}

		private ShaderMap[] ShaderMapFromBiomes(BiomeMap biomeMap) {
			int width = biomeMap.biomeMapIndexes.GetLength (0);
			int height = biomeMap.biomeMapIndexes.GetLength (1);
		
			ShaderMap[] map = new ShaderMap[width * height];
			for (int j = 0; j < height; j++) {
				for (int i = 0; i < width; i++) {
					map[j * width + i] = new ShaderMap(biomeMap.biomeMapIndexes [i, j]);
				}
			}
			return map;
		}
	
		public struct ShaderMap {
			public int index;

			public ShaderMap(int index) {
				this.index = index;
			}

			public static int Size () {
				return sizeof(int);
			}
		}

		private void OnDisable() {
			buffer?.Dispose ();
		}


		[System.Serializable]
		public class Layer {
			public Texture2D texture;
			public Color tint;
			[Range(0,1)]
			public float tintStrength;
			[Range(0,1)]
			public float startHeight;
			[Range(0,1)]
			public float blendStrength;
			public float textureScale;
		}
		
	 
	}
}
