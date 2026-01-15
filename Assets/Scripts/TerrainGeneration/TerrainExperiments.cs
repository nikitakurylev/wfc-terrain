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
            RunBiomeSizeExperiment("biomeSize3biomes.csv");
            _settings = _sevenBiomes;
            RunBiomeSizeExperiment("biomeSize7biomes.csv");

            _settings = _threeBiomes;
            RunTerrainSizeExperiment("terrainSize3biomes.csv");
            _settings = _sevenBiomes;
            RunTerrainSizeExperiment("terrainSize7biomes.csv");
        }

        void RunBiomeSizeExperiment(string filename)
        {
            if(File.Exists(filename))
                File.Delete(filename);
        
            var file = File.CreateText(filename);
            file.WriteLine("biomeSize wfc perlin paint total");

            for (int i = 8; i <= 64; i += 2)
                RunExperimentAndWriteResult(file, i, 1024, i);
        
            file.Close();
        }

        void RunTerrainSizeExperiment(string filename)
        {
            if(File.Exists(filename))
                File.Delete(filename);
        
            var file = File.CreateText(filename);
            file.WriteLine("size wfc perlin paint total");

            for (int i = 16; i <= 1024; i += 16)
                RunExperimentAndWriteResult(file, i, i, 32);
        
            file.Close();
        }

        void RunExperimentAndWriteResult(StreamWriter file, int i, int size, int biomeSize)
        {
            float avgWfc = 0, avgPerlin = 0, avgPaint = 0, avgTotal = 0;
            for (int j = 0; j < TestsPerStep; j++)
            {
                _terrain.GenerateTerrain(new MockTerrainGenerationSettings()
                {
                    Size = size,
                    Biomes = _settings.Biomes,
                    BiomeSize = biomeSize,
                    InterpolationCurve = _settings.InterpolationCurve,
                    OctaveScales = _settings.OctaveScales
                });

                avgWfc += _terrain.WfcTime;
                avgPerlin += _terrain.PerlinTime;
                avgPaint += _terrain.PaintTime;
                avgTotal += _terrain.TotalTime;
            }

            avgWfc /= TestsPerStep;
            avgPerlin /= TestsPerStep;
            avgPaint /= TestsPerStep;
            avgTotal /= TestsPerStep;

            file.WriteLine($"{i} {avgWfc} {avgPerlin} {avgPaint} {avgTotal}");
            file.Flush();
        }
    }
}