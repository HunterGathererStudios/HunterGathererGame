using System;
using ProceduralTerrainGeneration.Data;
using UnityEngine;

namespace General {
	public class Blurring {
			public static float[,] GaussianBlurKernel(int length, float weight) {
        		float[,] kernel = new float[length, length];
        		float kernelSum = 0;
        		int foff = (length - 1) / 2;
        		float distance = 0;
        		float constant = 1f / (2 * (float)Math.PI * weight * weight);
        		for (int y = -foff; y <= foff; y++) {
        			for (int x = -foff; x <= foff; x++) {
        				distance = ((y * y) + (x * x)) / (2 * weight * weight);
        				kernel [y + foff, x + foff] = constant * (float)Math.Exp (-distance);
        				kernelSum += kernel [y + foff, x + foff];
        			}
        		}
        		for (int y = 0; y < length; y++) {
        			for (int x = 0; x < length; x++) {
        				kernel [y, x] = kernel [y, x] / kernelSum;
        			}
        		}
        		return kernel;
        	}
        
        	public static float[,] MatrixConvolution(int[,] values, float[,] kernel, BiomeMapSettings settings) {
        		int size = values.GetLength (0);
        		int kernelMidPoint = kernel.GetLength (0) / 2 - 1;
        		float[,] output = new float[size, size];
        
        		for (int x = 0; x < size; x++) {
        			for (int y = 0; y < size; y++) {
        				float accumulator = 0;
        
        				for (int i = -kernelMidPoint; i < kernelMidPoint; i++) {
        					for (int j = -kernelMidPoint; j < kernelMidPoint; j++) {
        						try {
        							accumulator += kernel [kernelMidPoint + i, kernelMidPoint + j] * settings.Biomes[values [x + i, y + j]].heightMult;
        						}
        						catch (Exception e) {
        							Console.WriteLine (e);
        						} //TODO Change edge handling to extend boundaries of map
        					}
        				}
        				output [x, y] = accumulator;
        			
        			}
        		}
        		return output;
        	}

            public static float[,] BoxBlurMatrix(float[,] values, int blurSize) {
	            int gridSizeX = values.GetLength (0);
	            int gridSizeY = values.GetLength (1);
	            
	            
	            int kernelSize = blurSize * 2 + 1;
	            int kernelExtents = (kernelSize - 1) / 2;

	            float[,] penaltiesHorizontalPass = new float[gridSizeX,gridSizeY];
	            float[,] penaltiesVerticalPass = new float[gridSizeX,gridSizeY];

	            for (int y = 0; y < gridSizeY; y++) {
		            for (int x = -kernelExtents; x <= kernelExtents; x++) {
			            int sampleX = Mathf.Clamp (x, 0, kernelExtents);
			            penaltiesHorizontalPass [0, y] += values [sampleX, y];
		            }

		            for (int x = 1; x < gridSizeX; x++) {
			            int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
			            int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX-1);

			            penaltiesHorizontalPass [x, y] = penaltiesHorizontalPass [x - 1, y] - values [removeIndex, y] + values [addIndex, y];
		            }
	            }
			
	            for (int x = 0; x < gridSizeX; x++) {
		            for (int y = -kernelExtents; y <= kernelExtents; y++) {
			            int sampleY = Mathf.Clamp (y, 0, kernelExtents);
			            penaltiesVerticalPass [x, 0] += penaltiesHorizontalPass [x, sampleY];
		            }

		            float blurredPenalty = penaltiesVerticalPass [x, 0] / (kernelSize * kernelSize);
		            values [x, 0] = blurredPenalty;

		            for (int y = 1; y < gridSizeY; y++) {
			            int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
			            int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY-1);

			            penaltiesVerticalPass [x, y] = penaltiesVerticalPass [x, y-1] - penaltiesHorizontalPass [x,removeIndex] + penaltiesHorizontalPass [x, addIndex];
			            blurredPenalty = penaltiesVerticalPass [x, y] / (kernelSize * kernelSize);
			            values [x, y] = blurredPenalty;
		            }
	            }


	            return values;
            }
	}
}
