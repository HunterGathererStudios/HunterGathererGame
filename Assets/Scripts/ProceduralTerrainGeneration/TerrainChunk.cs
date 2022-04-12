using ProceduralTerrainGeneration;
using UnityEngine;

public class TerrainChunk {
	
	const float colliderGenerationDistanceThreshold = 5;
	public event System.Action<TerrainChunk, bool> onVisibilityChanged;
	public Vector2 coord;
	 
	GameObject meshObject;
	Vector2 sampleCentre;
	Bounds bounds;

	LODInfo[] detailLevels;
	int colliderLODIndex;

	HeightMap heightMap;
	bool heightMapReceived;
	int previousLODIndex = -1;
	bool hasSetCollider;
	float maxViewDst;

	BiomeMapSettings biomeMapSettings;
	HeightMapSettings heightMapSettings;
	MeshSettings meshSettings;
	Transform viewer;

	public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex ,Transform parent, Transform viewer) {
		this.coord = coord;
		this.heightMapSettings = heightMapSettings;
		this.detailLevels = detailLevels;
		this.colliderLODIndex = colliderLODIndex;
		this.meshSettings = meshSettings;
		this.viewer = viewer;

		sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
		Vector2 position = coord * meshSettings.meshWorldSize ;
		bounds = new Bounds(position,Vector2.one * meshSettings.meshWorldSize );
		
		meshObject = new GameObject("Terrain Chunk");

		meshObject.transform.position = new Vector3(position.x,0,position.y);
		meshObject.transform.parent = parent;
		SetVisible(false);

		maxViewDst = detailLevels [^1].visibleDstThreshold;

	}

	public void Load(bool useBiomes, BiomeMapSettings biomeMapSettings) {
		this.biomeMapSettings = biomeMapSettings;
		if (useBiomes) {
			ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap (meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCentre, biomeMapSettings), OnHeightMapReceived);
		} else {
			ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap (meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCentre), OnHeightMapReceived);
		}
	}

	void OnHeightMapReceived(object heightMapObject) {
		this.heightMap = (HeightMap)heightMapObject;
		heightMapReceived = true;
		GenerateVoxelMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMap, biomeMapSettings,
			meshSettings);
		UpdateTerrainChunk ();
	}

	void GenerateVoxelMap(int width, int height, HeightMap heightMap, BiomeMapSettings biomeMapSettings, MeshSettings meshSettings) {
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				GameObject voxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
				voxel.transform.parent = meshObject.transform;
				voxel.name = "Voxel";
				Vector2 position = sampleCentre + new Vector2(i, j) * meshSettings.meshScale;
				voxel.transform.position = new Vector3(position.x, heightMap.values[i,j], position.y);
			}
		}
	}

	void OnVoxelMapReceived(object voxelMapObject) {
		
	}

	Vector2 viewerPosition {
		get {
			return new Vector2 (viewer.position.x, viewer.position.z);
		}
	}


	public void UpdateTerrainChunk() {
		if (heightMapReceived) {
			float viewerDstFromNearestEdge = Mathf.Sqrt (bounds.SqrDistance (viewerPosition));

			bool wasVisible = IsVisible ();
			bool visible = viewerDstFromNearestEdge <= maxViewDst;

			// if (visible) {
			// 	int lodIndex = 0;
			//
			// 	for (int i = 0; i < detailLevels.Length - 1; i++) {
			// 		if (viewerDstFromNearestEdge > detailLevels [i].visibleDstThreshold) {
			// 			lodIndex = i + 1;
			// 		} else {
			// 			break;
			// 		}
			// 	}
			//
			// 	if (lodIndex != previousLODIndex) {
			// 		LODMesh lodMesh = lodMeshes [lodIndex];
			// 		if (lodMesh.hasMesh) {
			// 			previousLODIndex = lodIndex;
			// 			meshFilter.mesh = lodMesh.mesh;
			// 		} else if (!lodMesh.hasRequestedMesh) {
			// 			lodMesh.RequestMesh (heightMap, meshSettings);
			// 		}
			// 	}
			// }

			if (wasVisible != visible) {
				
				SetVisible (visible);
				if (onVisibilityChanged != null) {
					onVisibilityChanged (this, visible);
				}
			}
		}
	}

	public void SetVisible(bool visible) {
		meshObject.SetActive (visible);
	}

	public bool IsVisible() {
		return meshObject.activeSelf;
	}

}