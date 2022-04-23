using UnityEngine;

namespace ProceduralTerrainGeneration.Data {
    [CreateAssetMenu(menuName = "Terrain Data/Biome Map Settings")]
    public class BiomeMapSettings : UpdatableData
    {
        public TemperatureAndPrecipitationSettings temperatureSettings;
        public TemperatureAndPrecipitationSettings precipitationSettings;

        [Range (0, 10)]
        public int smoothingRadius;
        public Biome[] Biomes;

        [Range (0, 1)]
        public float oceanHeight;

        public float minHeight {
            get {
                float height = float.MaxValue;
                float bHeight;
                foreach (var biome in Biomes) {
                    bHeight = biome.heightCurve.Evaluate (0) * biome.heightMult;
                    if (bHeight < height)
                        height = bHeight;
                }
                return height;
            }
        }
    
        public float maxHeight {
            get {
                float height = float.MinValue;
                float bHeight;
                foreach (var biome in Biomes) {
                    bHeight = biome.heightCurve.Evaluate (1) * biome.heightMult;
                    if (bHeight > height)
                        height = bHeight;
                }
                return height;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate() {
            temperatureSettings.ValidateValues();
            precipitationSettings.ValidateValues();
            base.OnValidate ();
        }
#endif
    
    
    }
    [System.Serializable]
    public class TemperatureAndPrecipitationSettings : NoiseSettings {
        public Vector2 maxAndMinValues;
        //public float valueOffset;
    }
}

