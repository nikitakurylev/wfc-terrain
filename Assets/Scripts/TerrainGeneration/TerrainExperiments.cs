using System.Collections;
using System.IO;
using TerrainGeneration.ScriptableObjects;
using UnityEngine;

namespace TerrainGeneration
{
    public class TerrainExperiments : MonoBehaviour
    {
        private const int TestsPerStep = 5;

        [SerializeField] private TerrainManager _terrain;
        [SerializeField] private TerrainGenerationSettings _threeBiomes;
        [SerializeField] private TerrainGenerationSettings _sevenBiomes;
        
        private TerrainGenerationSettings _settings;

        [InspectorButton("StartExperiments", ButtonWidth = 120)] public bool _startExperiments;

        void StartExperiments()
        {
            _settings = _threeBiomes;
            RunBiomeSizeExperiment("biomeSize3biomes-linear.csv", InterpolatorType.Linear);
            _settings = _sevenBiomes;
            RunBiomeSizeExperiment("biomeSize7biomes-linear.csv", InterpolatorType.Linear);
            RunBiomeSizeExperiment("biomeSize7biomes-barycentric.csv", InterpolatorType.Barycentric);
            RunBiomeSizeExperiment("biomeSize7biomes-sibson.csv", InterpolatorType.Sibson);

            _settings = _threeBiomes;
            RunTerrainSizeExperiment("terrainSize3biomes-linear.csv", InterpolatorType.Linear);
            _settings = _sevenBiomes;
            RunTerrainSizeExperiment("terrainSize7biomes-linear.csv", InterpolatorType.Linear);
            RunTerrainSizeExperiment("terrainSize7biomes-barycentric.csv", InterpolatorType.Barycentric);
            RunTerrainSizeExperiment("terrainSize7biomes-sibson.csv", InterpolatorType.Sibson);
        }

        void RunBiomeSizeExperiment(string filename, InterpolatorType interpolatorType)
        {
            if(File.Exists(filename))
                File.Delete(filename);
        
            var file = File.CreateText(filename);
            file.WriteLine("biomeSize wfc perlin total");

            for (int i = 16; i <= 64; i += 4)
                RunExperimentAndWriteResult(file, i, 512, i, interpolatorType);

            file.Close();
        }

        void RunTerrainSizeExperiment(string filename, InterpolatorType interpolatorType)
        {
            if(File.Exists(filename))
                File.Delete(filename);
        
            var file = File.CreateText(filename);
            file.WriteLine("size wfc perlin total");

            for (int i = 128; i <= 1024; i += 64)
                RunExperimentAndWriteResult(file, i, i, 32, interpolatorType);
        
            file.Close();
        }

        void RunExperimentAndWriteResult(StreamWriter file, int i, int size, int biomeSize, InterpolatorType interpolatorType)
        {
            float avgWfc = 0, avgPerlin = 0, avgTotal = 0;
            for (int j = 0; j < TestsPerStep; j++)
            {
                _terrain.GenerateTerrain(new MockTerrainGenerationSettings()
                {
                    Size = size,
                    Biomes = _settings.Biomes,
                    BiomeSize = biomeSize,
                    OctaveScales = _settings.OctaveScales,
                    InterpolatorType = interpolatorType
                });

                avgWfc += _terrain.WfcTime;
                avgPerlin += _terrain.PerlinTime;
                avgTotal += _terrain.TotalTime;
            }

            avgWfc /= TestsPerStep;
            avgPerlin /= TestsPerStep;
            avgTotal /= TestsPerStep;

            file.WriteLine($"{i} {avgWfc} {avgPerlin} {avgTotal}");
            file.Flush();
        }
    }
}