using UnityEngine;
using System.Collections;
using ProceduralTerrainGeneration;
using ProceduralTerrainGeneration.Data;

public class MapPreview : MonoBehaviour {

	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;


	public enum DrawMode {NoiseMap, Mesh, FalloffMap, Biomes, BiomeMesh, BiomeMapHeightMult, Temperature, Precipitation};
	public DrawMode drawMode;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureData textureData;
	public BiomeMapSettings biomeMapSettings;

	public Material terrainMaterial;



	[Range(0,MeshSettings.numSupportedLODs-1)]
	public int editorPreviewLOD;
	public bool autoUpdate;
	
	private BiomeMap biomeMap;

	public void DrawMapInEditor() {
		float minHeight = (drawMode.Equals(DrawMode.BiomeMesh)) ?  biomeMapSettings.minHeight * heightMapSettings.minHeight : heightMapSettings.minHeight;
		float maxHeight = (drawMode.Equals(DrawMode.BiomeMesh)) ?  biomeMapSettings.maxHeight * heightMapSettings.maxHeight : heightMapSettings.maxHeight;

		HeightMap heightMap = HeightMapGenerator.GenerateHeightMap (meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, Vector2.zero).heightMap;
		MapOutputContainer mapOutputContainer = HeightMapGenerator.GenerateHeightMap (meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, Vector2.zero, biomeMapSettings);
		HeightMap biomeHeightMap = mapOutputContainer.heightMap;
		biomeMap = mapOutputContainer.biomeMap;

		if (terrainMaterial.shader.Equals (Shader.Find ("Custom/Terrain"))) {
			textureData.ApplyToMaterial (terrainMaterial);
		} else if (terrainMaterial.shader.Equals (Shader.Find ("Custom/BiomeTerrain"))) {
			textureData.ApplyToBiomeMaterial (terrainMaterial, biomeMapSettings, meshSettings, biomeMap, new Vector3 (0, 0, 0));
		}
		textureData.UpdateMeshHeights (terrainMaterial, minHeight, maxHeight);
		
		switch (drawMode) {
			case DrawMode.NoiseMap:
				DrawTexture (TextureGenerator.TextureFromFloatMap(heightMap.values,heightMap.minValue, heightMap.maxValue, Color.black, Color.white));
				break;
			case DrawMode.Mesh:
				DrawMesh (MeshGenerator.GenerateTerrainMesh (heightMap.values,meshSettings, editorPreviewLOD));
				break;
			case DrawMode.FalloffMap:
				HeightMap falloffMap = new HeightMap (FalloffGenerator.GenerateFalloffMap (meshSettings.numVertsPerLine), 0, 1);
				DrawTexture(TextureGenerator.TextureFromFloatMap(falloffMap.values, falloffMap.minValue, falloffMap.maxValue, Color.black, Color.white));
				break;
			case DrawMode.Biomes:
				DrawTexture (TextureGenerator.TextureFromBiomeMap (biomeMap, biomeMapSettings));
				break;
			case DrawMode.BiomeMesh:
				DrawMesh(MeshGenerator.GenerateTerrainMesh(biomeHeightMap.values,meshSettings,editorPreviewLOD));
				break;
			case DrawMode.BiomeMapHeightMult:
				DrawTexture(TextureGenerator.TextureFromFloatMap(biomeMap.heightMultMat, biomeMapSettings.minHeight, biomeMapSettings.maxHeight, Color.black, Color.white));
				break;
			case DrawMode.Temperature:
				float maxTemp = biomeMapSettings.temperatureSettings.maxAndMinValues.x;
				float minTemp = biomeMapSettings.temperatureSettings.maxAndMinValues.y;
				DrawTexture(TextureGenerator.TextureFromFloatMap(biomeMap.temperatureMap, minTemp, maxTemp, Color.blue, Color.red));
				break;
			case DrawMode.Precipitation:
				float maxRain = biomeMapSettings.precipitationSettings.maxAndMinValues.x;
				float minRain = biomeMapSettings.precipitationSettings.maxAndMinValues.y;
				DrawTexture(TextureGenerator.TextureFromFloatMap(biomeMap.precipitationMap, minRain, maxRain, Color.black, Color.cyan));
				break;
		}
	}


	public void DrawTexture(Texture2D texture) {
		textureRender.sharedMaterial.mainTexture = texture;
		textureRender.transform.localScale = new Vector3 (texture.width, 1, texture.height) /10f;

		textureRender.gameObject.SetActive (true);
		meshFilter.gameObject.SetActive (false);
	}

	public void DrawMesh(MeshData meshData) {
		meshFilter.sharedMesh = meshData.CreateMesh ();

		textureRender.gameObject.SetActive (false);
		meshFilter.gameObject.SetActive (true);
	}



	void OnValuesUpdated() {
		if (!Application.isPlaying) {
			DrawMapInEditor ();
		}
	}

	void OnTextureValuesUpdated() {
		if (terrainMaterial.shader.Equals (Shader.Find ("Custom/Terrain"))) {
			textureData.ApplyToMaterial (terrainMaterial);
		} else if (terrainMaterial.shader.Equals (Shader.Find ("Custom/BiomeTerrain"))) {
			textureData.ApplyToBiomeMaterial (terrainMaterial, biomeMapSettings, meshSettings, biomeMap, new Vector3 (0, 0, 0));
		}
	}

	void OnValidate() {

		if (meshSettings != null) {
			meshSettings.OnValuesUpdated -= OnValuesUpdated;
			meshSettings.OnValuesUpdated += OnValuesUpdated;
		}
		if (heightMapSettings != null) {
			heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
			heightMapSettings.OnValuesUpdated += OnValuesUpdated;
		}
		if (textureData != null) {
			textureData.OnValuesUpdated -= OnTextureValuesUpdated;
			textureData.OnValuesUpdated += OnTextureValuesUpdated;
		}
		if (biomeMapSettings!= null) {
			biomeMapSettings.OnValuesUpdated -= OnValuesUpdated;
			biomeMapSettings.OnValuesUpdated += OnValuesUpdated;

			for (int i = 0; i < biomeMapSettings.Biomes.Length; i++) {
				biomeMapSettings.Biomes [i].OnValuesUpdated -= OnValuesUpdated;
				biomeMapSettings.Biomes [i].OnValuesUpdated += OnValuesUpdated;
			}
		}

	}

}
