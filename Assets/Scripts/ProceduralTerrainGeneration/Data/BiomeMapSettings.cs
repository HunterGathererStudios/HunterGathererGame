using ProceduralTerrainGeneration;
using UnityEngine;

[CreateAssetMenu()]
public class BiomeMapSettings : UpdatableData
{
    public NoiseSettings noiseSettings;
    [Range (0, 10)]
    public int smoothingRadius;
    public Biome[] Biomes;
    
#if UNITY_EDITOR
    protected override void OnValidate() {
        noiseSettings.ValidateValues ();
        base.OnValidate ();
    }
#endif
    
    
}
[System.Serializable]
public class Biome {
    [Range(0,1)]
    public float startValue;
    public float heightMult = 1;
    public AnimationCurve heightCurve;
    public Material material;
}
