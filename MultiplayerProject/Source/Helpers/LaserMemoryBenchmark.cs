using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.Helpers.Factories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;

namespace MultiplayerProject.Source.Helpers
{
    /// <summary>
    /// Utility class to benchmark memory usage with and without Flyweight pattern
    /// Educational tool to demonstrate the effectiveness of the Flyweight pattern
    /// </summary>
    public class LaserMemoryBenchmark
    {
        private ContentManager _content;
        private Texture2D _laserTexture;

        public LaserMemoryBenchmark(ContentManager content)
        {
            _content = content;
            _laserTexture = content.Load<Texture2D>("laser");
        }

        /// <summary>
        /// Run comprehensive memory benchmark comparing WITH and WITHOUT flyweight
        /// </summary>
        public BenchmarkResult RunBenchmark(int laserCount = 1000)
        {
            // Force garbage collection before starting
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var result = new BenchmarkResult();
            result.LaserCount = laserCount;

            Console.WriteLine("=== LASER MEMORY BENCHMARK ===");
            Console.WriteLine($"Testing with {laserCount} lasers\n");

            // Test WITHOUT Flyweight (simulated old approach)
            Console.WriteLine("Testing WITHOUT Flyweight pattern...");
            result.WithoutFlyweight = MeasureMemoryWithoutFlyweight(laserCount);

            // Force cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Test WITH Flyweight (current implementation)
            Console.WriteLine("\nTesting WITH Flyweight pattern...");
            result.WithFlyweight = MeasureMemoryWithFlyweight(laserCount);

            // Calculate savings
            result.CalculateSavings();
            result.PrintResults();

            return result;
        }

        /// <summary>
        /// Simulate OLD approach: Each laser stores its own texture and properties
        /// </summary>
        private MemoryMeasurement MeasureMemoryWithoutFlyweight(int count)
        {
            var measurement = new MemoryMeasurement();
            var lasers = new List<LaserWithoutFlyweight>();

            // Measure before
            long memoryBefore = GC.GetTotalMemory(true);
            var stopwatch = Stopwatch.StartNew();

            // Create lasers WITHOUT flyweight (old way)
            for (int i = 0; i < count; i++)
            {
                var laser = new LaserWithoutFlyweight();
                laser.Texture = _laserTexture; // Each laser holds reference to texture
                laser.Position = new Vector2(i * 10, i * 10);
                laser.Rotation = (float)(i * Math.PI / 180);
                laser.Color = i % 3 == 0 ? Color.OrangeRed : (i % 3 == 1 ? Color.CornflowerBlue : Color.LightCyan);
                laser.Damage = i % 3 == 0 ? 15f : (i % 3 == 1 ? 8f : 7f);
                laser.Speed = i % 3 == 0 ? 30f : (i % 3 == 1 ? 25f : 40f);
                laser.Range = i % 3 == 0 ? 600f : (i % 3 == 1 ? 800f : 1200f);
                laser.Width = 46;
                laser.Height = 16;
                lasers.Add(laser);
            }

            stopwatch.Stop();

            // Measure after
            long memoryAfter = GC.GetTotalMemory(false);

            measurement.MemoryUsedBytes = memoryAfter - memoryBefore;
            measurement.CreationTimeMs = stopwatch.ElapsedMilliseconds;
            measurement.ObjectCount = lasers.Count;

            // Calculate per-object overhead
            measurement.MemoryPerObjectBytes = measurement.MemoryUsedBytes / count;

            Console.WriteLine($"  Memory Used: {FormatBytes(measurement.MemoryUsedBytes)}");
            Console.WriteLine($"  Per Laser: {FormatBytes(measurement.MemoryPerObjectBytes)}");
            Console.WriteLine($"  Creation Time: {measurement.CreationTimeMs}ms");

            lasers.Clear();
            return measurement;
        }

        /// <summary>
        /// Test TRUE flyweight approach: Lightweight lasers with NO Animation objects
        /// </summary>
        private MemoryMeasurement MeasureMemoryWithFlyweight(int count)
        {
            var measurement = new MemoryMeasurement();
            var lasers = new List<LightweightLaser>();

            // Initialize flyweight factory
            LaserFlyweightFactory.Instance.Initialize(_content);

            // Measure before
            long memoryBefore = GC.GetTotalMemory(true);
            var stopwatch = Stopwatch.StartNew();

            // TRUE flyweight: use shared flyweight, NO Animation objects created!
            ElementalType[] types = { ElementalType.Fire, ElementalType.Water, ElementalType.Electric };

            for (int i = 0; i < count; i++)
            {
                ElementalType type = types[i % 3];
                LaserFlyweight flyweight = LaserFlyweightFactory.Instance.GetFlyweight(type);
                
                // Create lightweight laser that only stores position, rotation, IDs
                // The flyweight (texture + properties) is shared!
                LightweightLaser laser = new LightweightLaser();
                Vector2 position = new Vector2(i * 10, i * 10);
                float rotation = (float)(i * Math.PI / 180);
                
                laser.Initialize(flyweight, position, rotation);  // ← Shared reference, not copied!
                lasers.Add(laser);
            }

            stopwatch.Stop();

            // Measure after
            long memoryAfter = GC.GetTotalMemory(false);

            measurement.MemoryUsedBytes = memoryAfter - memoryBefore;
            measurement.CreationTimeMs = stopwatch.ElapsedMilliseconds;
            measurement.ObjectCount = lasers.Count;
            measurement.FlyweightCount = 3; // Only 3 flyweight instances!

            // Calculate per-object overhead
            measurement.MemoryPerObjectBytes = measurement.MemoryUsedBytes / count;

            Console.WriteLine($"  Memory Used: {FormatBytes(measurement.MemoryUsedBytes)}");
            Console.WriteLine($"  Per Object: {FormatBytes(measurement.MemoryPerObjectBytes)}");
            Console.WriteLine($"  Flyweight Objects: {measurement.FlyweightCount}");
            Console.WriteLine($"  Creation Time: {measurement.CreationTimeMs}ms");

            lasers.Clear();
            return measurement;
        }

        private string FormatBytes(long bytes)
        {
            if (bytes >= 1_000_000)
                return $"{bytes / 1_000_000.0:F2} MB";
            if (bytes >= 1_000)
                return $"{bytes / 1_000.0:F2} KB";
            return $"{bytes} bytes";
        }

        /// <summary>
        /// Simulated laser class WITHOUT flyweight pattern (for comparison)
        /// </summary>
        private class LaserWithoutFlyweight
        {
            public Texture2D Texture { get; set; } // Each laser stores texture reference
            public Vector2 Position { get; set; }
            public float Rotation { get; set; }
            public Color Color { get; set; }
            public float Damage { get; set; }
            public float Speed { get; set; }
            public float Range { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public bool Active { get; set; } = true;
            public string LaserID { get; set; } = Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// Container for memory measurement data
    /// </summary>
    public class MemoryMeasurement
    {
        public long MemoryUsedBytes { get; set; }
        public long MemoryPerObjectBytes { get; set; }
        public long CreationTimeMs { get; set; }
        public int ObjectCount { get; set; }
        public int FlyweightCount { get; set; } = 0;
    }

    /// <summary>
    /// Container for complete benchmark results
    /// </summary>
    public class BenchmarkResult
    {
        public int LaserCount { get; set; }
        public int ObjectCount { get; set; }  // Generic count for any object type
        public MemoryMeasurement WithoutFlyweight { get; set; }
        public MemoryMeasurement WithFlyweight { get; set; }
        
        public long MemorySavedBytes { get; private set; }
        public double MemorySavedPercentage { get; private set; }
        public long TimeSavedMs { get; private set; }

        public void CalculateSavings()
        {
            MemorySavedBytes = WithoutFlyweight.MemoryUsedBytes - WithFlyweight.MemoryUsedBytes;
            MemorySavedPercentage = (double)MemorySavedBytes / WithoutFlyweight.MemoryUsedBytes * 100.0;
            TimeSavedMs = WithoutFlyweight.CreationTimeMs - WithFlyweight.CreationTimeMs;
        }

        public void PrintResults(string objectType = "Lasers")
        {
            Console.WriteLine("\n=== BENCHMARK RESULTS ===");
            int count = ObjectCount > 0 ? ObjectCount : LaserCount;
            Console.WriteLine($"Total {objectType} Created: {count}");
            Console.WriteLine();
            
            Console.WriteLine("WITHOUT Flyweight:");
            Console.WriteLine($"  Total Memory: {FormatBytes(WithoutFlyweight.MemoryUsedBytes)}");
            Console.WriteLine($"  Per Object: {FormatBytes(WithoutFlyweight.MemoryPerObjectBytes)}");
            Console.WriteLine($"  Creation Time: {WithoutFlyweight.CreationTimeMs}ms");
            Console.WriteLine();
            
            Console.WriteLine("WITH Flyweight:");
            Console.WriteLine($"  Total Memory: {FormatBytes(WithFlyweight.MemoryUsedBytes)}");
            Console.WriteLine($"  Per Object: {FormatBytes(WithFlyweight.MemoryPerObjectBytes)}");
            Console.WriteLine($"  Flyweight Objects: {WithFlyweight.FlyweightCount}");
            Console.WriteLine($"  Creation Time: {WithFlyweight.CreationTimeMs}ms");
            Console.WriteLine();
            
            Console.WriteLine("SAVINGS:");
            Console.WriteLine($"  Memory Saved: {FormatBytes(MemorySavedBytes)} ({MemorySavedPercentage:F2}%)");
            Console.WriteLine($"  Time Difference: {TimeSavedMs}ms");
            Console.WriteLine();

            // Educational analysis
            Console.WriteLine("ANALYSIS:");
            if (MemorySavedPercentage > 0)
            {
                Console.WriteLine($"  ✓ Flyweight pattern REDUCED memory by {MemorySavedPercentage:F2}%");
                int objCount = ObjectCount > 0 ? ObjectCount : LaserCount;
                Console.WriteLine($"  ✓ With {objCount} {objectType.ToLower()}, saved {FormatBytes(MemorySavedBytes)}");
                Console.WriteLine($"  ✓ In a multiplayer game with many objects, this is significant!");
            }
            else
            {
                Console.WriteLine($"  ✗ Flyweight pattern INCREASED memory by {Math.Abs(MemorySavedPercentage):F2}%");
                Console.WriteLine($"  ✗ This suggests overhead from flyweight pattern exceeds benefits at this scale");
                Console.WriteLine($"  ✗ Flyweight works best with many objects sharing expensive resources");
            }

            Console.WriteLine("\nNote: Results may vary due to GC behavior and system state.");
            Console.WriteLine("Run multiple times for more accurate measurements.\n");
        }

        private string FormatBytes(long bytes)
        {
            if (bytes >= 1_000_000)
                return $"{bytes / 1_000_000.0:F2} MB";
            if (bytes >= 1_000)
                return $"{bytes / 1_000.0:F2} KB";
            return $"{bytes} bytes";
        }
    }
}
