using UnityEngine;
using System.Collections;
using ProceduralTerrainGeneration;
using ProceduralTerrainGeneration.Data;
using Unity.VisualScripting;

public static class TextureGenerator {

	public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) {
		Texture2D texture = new Texture2D (width, height);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels (colourMap);
		texture.Apply ();
		return texture;
	}

	public static Texture2D TextureFromIntMap(int[,] map, int min, int max, Color a, Color b) {
		int width = map.GetLength (0);
		int height = map.GetLength (1);

		Color[] colourMap = new Color[width * height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				colourMap [y * width + x] = Color.Lerp (a, b, Mathf.InverseLerp(min,max,map [x, y]));
			}
		}

		return TextureFromColourMap (colourMap, width, height);
	}
	public static Texture2D TextureFromFloatMap(float[,] map, float min, float max, Color a, Color b) {
		int width = map.GetLength (0);
		int height = map.GetLength (1);

		Color[] colourMap = new Color[width * height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				colourMap [y * width + x] = Color.Lerp (a, b, Mathf.InverseLerp(min,max,map [x, y]));
			}
		}

		return TextureFromColourMap (colourMap, width, height);
	}

	public static Texture2D TextureFromBiomeMap(BiomeMap map, BiomeMapSettings settings) {
		int width = map.biomeMapIndexes.GetLength (0);
		int height = map.biomeMapIndexes.GetLength (1);
		
		Color[] colourMap = new Color[width * height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				colourMap [y * width + x] = settings.Biomes[map.biomeMapIndexes[x,y]].biomeColour;
			}
		}

		return TextureFromColourMap (colourMap, width, height);
	}

}
