using System.Collections;
using System.Collections.Generic;
using ProceduralTerrainGeneration.Data;
using UnityEngine;

namespace ProceduralTerrainGeneration
{
	public static class HeightMapGenerator {

		public static MapOutputContainer GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre) {
			float[,] values = Noise.GenerateNoiseMap (width, height, settings.noiseSettings, sampleCentre);
			AnimationCurve heightCurve_threadsafe = new AnimationCurve (settings.heightCurve.keys);
			float minValue = float.MaxValue;
			float maxValue = float.MinValue;

			for (int i = 0; i < width; i++) {
				for (int j = 0; j < height; j++) {
					values [i, j] *= heightCurve_threadsafe.Evaluate (values [i, j]) * settings.heightMultiplier;

					if (values [i, j] > maxValue) {
						maxValue = values [i, j];
					}
					if (values [i, j] < minValue) {
						minValue = values [i, j];
					}
				}
			}

			return new MapOutputContainer(new HeightMap (values, minValue, maxValue));
		}

		public static MapOutputContainer GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre, BiomeMapSettings biomeMapSettings) {
			int offset = biomeMapSettings.smoothingRadius * 2;
			float[,] values = Noise.GenerateNoiseMap (width + offset, height + offset, settings.noiseSettings, sampleCentre);
			BiomeMap biomeMap = BiomeMapGenerator.GenerateBiomeMap (width, height, biomeMapSettings, sampleCentre, values);
			values = BiomeMapGenerator.removeOffset (values, biomeMapSettings.smoothingRadius);
			
			AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);
			AnimationCurve[] biomeCurves_threadsafe = new AnimationCurve[biomeMap.numBiomes];
			for (int i = 0; i < biomeMap.numBiomes; i++) {
				biomeCurves_threadsafe[i] = new AnimationCurve(biomeMapSettings.Biomes[i].heightCurve.keys);
			}
		
			float minValue = float.MaxValue;
			float maxValue = float.MinValue;

			for (int i = 0; i < width; i++) {
				for (int j = 0; j < height; j++) {
					int biomeIndex = biomeMap.biomeMapIndexes [i, j];
					float biomeScalar = biomeMap.heightMultMat[i, j];
					values [i, j] *= heightCurve_threadsafe.Evaluate (values [i, j]) * settings.heightMultiplier * biomeScalar;
					values [i, j] *= biomeCurves_threadsafe[biomeIndex].Evaluate (values [i, j]);
					if (values [i, j] > maxValue) {
						maxValue = values [i, j];
					}
					if (values [i, j] < minValue) {
						minValue = values [i, j];
					}
				}
			}

			return new MapOutputContainer(new HeightMap (values, minValue, maxValue), biomeMap);
		}
	}

	public struct HeightMap {
		public readonly float[,] values;
		public readonly float minValue;
		public readonly float maxValue;

		public HeightMap(float[,] values, float minValue, float maxValue)
		{
			this.values = values;
			this.minValue = minValue;
			this.maxValue = maxValue;
		}
	}

	public struct MapOutputContainer {
		public readonly HeightMap heightMap;
		public readonly BiomeMap biomeMap;

		public MapOutputContainer(HeightMap heightMap, BiomeMap biomeMap) {
			this.heightMap = heightMap;
			this.biomeMap = biomeMap;
		}

		public MapOutputContainer(HeightMap heightMap) {
			this.heightMap = heightMap;
			this.biomeMap = new BiomeMap ();
		}
	}
}