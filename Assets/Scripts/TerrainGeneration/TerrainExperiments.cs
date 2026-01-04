using System.IO;
using TerrainGeneration.ScriptableObjects;
using UnityEngine;

namespace TerrainGeneration
{
    public class TerrainExperiments : MonoBehaviour
    {
        private const string FileName = "test.csv";
        private const int TestsPerStep = 3;

        [SerializeField] private TerrainManager _terrain;
        [SerializeField] private TerrainGenerationSettings _settings;

        [InspectorButton("StartExperiments")] public bool _startExperiments;

        void StartExperiments()
        {
            if(File.Exists(FileName))
                File.Delete(FileName);
        
            var file = File.CreateText(FileName);
            file.WriteLine("# wfc perlin paint total");

            for (int i = 16; i <= 1024; i += 16)
            {

                float avgWfc = 0, avgPerlin = 0, avgPaint = 0, avgTotal = 0;
                for (int j = 0; j < TestsPerStep; j++)
                {
                    _terrain.GenerateTerrain(new MockTerrainGenerationSettings()
                    {
                        Size = i,
                        Biomes = _settings.Biomes,
                        BiomeSize = 32,
                        InterpolationCurve = _settings.InterpolationCurve,
                        OctaveScales = _settings.OctaveScales
                    });

                    avgWfc += _terrain.WfcTime;
                    avgPerlin += _terrain.PerlinTime;
                    avgPaint += _terrain.PaintTime;
                    avgTotal += _terrain.TotalTime;
                }

                file.WriteLine($"{i} {avgWfc} {avgPerlin} {avgPaint} {avgTotal}");
                file.Flush();
            }
        
            file.Close();
        }
    }
}