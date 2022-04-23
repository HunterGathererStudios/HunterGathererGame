using System;
using System.Collections;
using System.Collections.Generic;
using ProceduralTerrainGeneration.Data;
using UnityEngine;

namespace ProceduralTerrainGeneration {
	public static class BiomeMapGenerator {
		public static BiomeMap GenerateBiomeMap(int width, int height, BiomeMapSettings settings, Vector2 sampleCentre, float[,] heightMap) {
			int offset = settings.smoothingRadius * 2;
			int newWidth = width + offset;
			int newHeight = height + offset;

			float[,] temperatureMap = Noise.GenerateNoiseMap (newWidth, newHeight, settings.temperatureSettings, sampleCentre);
			float[,] precipitationMap = Noise.GenerateNoiseMap (newWidth, newHeight, settings.precipitationSettings, sampleCentre);

			int[,] map = new int[width + offset, height + offset];

			for (int x = 0; x < newWidth; x++) {
				for (int y = 0; y < newHeight; y++) {

					temperatureMap [x, y] = temperatureMap [x, y] * (settings.temperatureSettings.maxAndMinValues.x - settings.temperatureSettings.maxAndMinValues.y)  + settings.temperatureSettings.maxAndMinValues.y;
					precipitationMap[x, y] = precipitationMap [x, y] * (settings.precipitationSettings.maxAndMinValues.x - settings.precipitationSettings.maxAndMinValues.y)  + settings.precipitationSettings.maxAndMinValues.y;

					bool isOceanHeight = heightMap [x, y] < settings.oceanHeight;

					for (int biomeIndex = 0; biomeIndex < settings.Biomes.Length; biomeIndex++) {
						if (isOceanHeight) {
							if (settings.Biomes[biomeIndex].isOcean) {
								map [x, y] = biomeIndex;
								break;
							}
						} else if (IsInRange(settings.Biomes[biomeIndex],temperatureMap[x,y],precipitationMap[x,y])){
							map [x, y] = biomeIndex;
							break;
						}
						
					}
				}
			}
		
			float[,] heightMult = removeOffset (BlurHeightMultipliers (settings, map), settings.smoothingRadius);
			map = removeOffset (map, settings.smoothingRadius);
			
			//TODO change blur to HLSL
			return new BiomeMap (map, settings.Biomes.Length, heightMult, temperatureMap, precipitationMap);
		}

		public static bool IsInRange(Biome biome, float tempValue, float precipValue) {
			bool temp = biome.maxAndMinTemperature.x >= tempValue && biome.maxAndMinTemperature.y <= tempValue;
			bool rain = biome.maxAndMinPrecipitation.x >= precipValue && biome.maxAndMinPrecipitation.y <= precipValue;

			return temp && rain;
		}
		

		public static float[,] BlurHeightMultipliers(BiomeMapSettings settings, int[,] indexes) {
		
			int blurSize = settings.smoothingRadius;
			int gridSize = indexes.GetLength (0);
		
			int kernelSize = blurSize * 2 + 1;
			int kernelExtents = (kernelSize - 1) / 2;
		
			float[,] penaltiesHorizontalPass = new float[gridSize,gridSize];
			float[,] penaltiesVerticalPass = new float[gridSize,gridSize];
			float[,] values = new float[gridSize, gridSize];
		
			for (int y = 0; y < gridSize; y++) {
				for (int x = -kernelExtents; x <= kernelExtents; x++) {
					int sampleX = Mathf.Clamp (x, 0, kernelExtents);
					penaltiesHorizontalPass [0, y] += settings.Biomes[indexes[sampleX, y]].heightMult;
				}

				for (int x = 1; x < gridSize; x++) {
					int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSize);
					int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSize-1);

					penaltiesHorizontalPass [x, y] = penaltiesHorizontalPass [x - 1, y] - settings.Biomes[indexes[removeIndex, y]].heightMult + settings.Biomes[indexes[addIndex, y]].heightMult;
				}
			}
			
			for (int x = 0; x < gridSize; x++) {
				for (int y = -kernelExtents; y <= kernelExtents; y++) {
					int sampleY = Mathf.Clamp (y, 0, kernelExtents);
					penaltiesVerticalPass [x, 0] += penaltiesHorizontalPass [x, sampleY];
				}

				float blurredValues = penaltiesVerticalPass [x, 0] / (kernelSize * kernelSize);
				values [x, 0] = blurredValues;

				for (int y = 1; y < gridSize; y++) {
					int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSize);
					int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSize-1);

					penaltiesVerticalPass [x, y] = penaltiesVerticalPass [x, y-1] - penaltiesHorizontalPass [x,removeIndex] + penaltiesHorizontalPass [x, addIndex];
					blurredValues = penaltiesVerticalPass [x, y] / (kernelSize * kernelSize);
					values [x, y] = blurredValues;
				}
			}
			return values;
		}

		public static T[,] removeOffset<T> (T[,] values, int offset) {
			int width = values.GetLength (0) - offset * 2;
			int height = values.GetLength (1) - offset * 2;
			T[,] output = new T[width, height];
			for (int i = 0; i < width; i++) {
				for (int j = 0; j < height; j++) {
					output [i, j] = values [i + offset, j + offset];
				}
			}
			return output;
		}
	}


	public struct BiomeMap {
		public readonly int[,] biomeMapIndexes;
		public readonly int numBiomes;
		public readonly float[,] heightMultMat;
		public readonly float[,] temperatureMap;
		public readonly float[,] precipitationMap;

		public BiomeMap(int[,] biomeMapIndexes, int numBiomes, float[,] heightMultMat, float[,] temperatureMap, float[,] precipitationMap) {
			this.biomeMapIndexes = biomeMapIndexes;
			this.numBiomes = numBiomes;
			this.heightMultMat = heightMultMat;
			this.temperatureMap = temperatureMap;
			this.precipitationMap = precipitationMap;
		}

	}
}