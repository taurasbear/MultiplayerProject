using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.GameObjects.Explosions;
using MultiplayerProject.Source.Helpers.Factories;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MultiplayerProject.Source.Helpers
{
    /// <summary>
    /// Benchmark for Explosion Flyweight pattern
    /// This should show better results than lasers because explosions truly share expensive resources
    /// </summary>
    public class ExplosionMemoryBenchmark
    {
        private ContentManager _content;
        private Texture2D _explosionTexture;

        public ExplosionMemoryBenchmark(ContentManager content)
        {
            _content = content;
            _explosionTexture = content.Load<Texture2D>("explosion");
        }

        /// <summary>
        /// Run benchmark comparing explosion creation WITH and WITHOUT flyweight
        /// </summary>
        public BenchmarkResult RunBenchmark(int explosionCount = 500)
        {
            // Force garbage collection before starting
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var result = new BenchmarkResult();
            result.ObjectCount = explosionCount;

            Console.WriteLine("=== EXPLOSION MEMORY BENCHMARK ===");
            Console.WriteLine($"Testing with {explosionCount} explosions\n");

            // Test WITHOUT Flyweight
            Console.WriteLine("Testing WITHOUT Flyweight pattern...");
            result.WithoutFlyweight = MeasureMemoryWithoutFlyweight(explosionCount);

            // Force cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Test WITH Flyweight
            Console.WriteLine("\nTesting WITH Flyweight pattern...");
            result.WithFlyweight = MeasureMemoryWithFlyweight(explosionCount);

            // Calculate savings
            result.CalculateSavings();
            result.PrintResults("Explosions");

            return result;
        }

        private MemoryMeasurement MeasureMemoryWithoutFlyweight(int count)
        {
            var measurement = new MemoryMeasurement();
            var animations = new List<Animation>();

            long memoryBefore = GC.GetTotalMemory(true);
            var stopwatch = Stopwatch.StartNew();

            // Simulate OLD way: each explosion creates its own Animation with full initialization
            for (int i = 0; i < count; i++)
            {
                Animation animation = new Animation();
                animation.Initialize(
                    _explosionTexture,
                    new Vector2(i * 10, i * 10),
                    0,
                    134,  // frameWidth
                    134,  // frameHeight
                    12,   // frameCount
                    30,   // frameTime
                    Color.White,
                    1.0f,
                    false
                );
                animations.Add(animation);
            }

            stopwatch.Stop();
            long memoryAfter = GC.GetTotalMemory(false);

            measurement.MemoryUsedBytes = memoryAfter - memoryBefore;
            measurement.CreationTimeMs = stopwatch.ElapsedMilliseconds;
            measurement.ObjectCount = animations.Count;
            measurement.MemoryPerObjectBytes = measurement.MemoryUsedBytes / count;

            Console.WriteLine($"  Memory Used: {FormatBytes(measurement.MemoryUsedBytes)}");
            Console.WriteLine($"  Per Explosion: {FormatBytes(measurement.MemoryPerObjectBytes)}");
            Console.WriteLine($"  Creation Time: {measurement.CreationTimeMs}ms");

            animations.Clear();
            return measurement;
        }

        private MemoryMeasurement MeasureMemoryWithFlyweight(int count)
        {
            var measurement = new MemoryMeasurement();
            var explosions = new List<LightweightExplosion>();

            // Initialize flyweight factory
            ExplosionFlyweightFactory.Instance.Initialize(_content);

            long memoryBefore = GC.GetTotalMemory(true);
            var stopwatch = Stopwatch.StartNew();

            // TRUE flyweight: use shared flyweight, NO Animation objects created!
            var flyweight = ExplosionFlyweightFactory.Instance.GetFlyweight();

            for (int i = 0; i < count; i++)
            {
                // Create lightweight explosion that only stores position, color, frame
                // The flyweight (texture + dimensions) is shared!
                LightweightExplosion explosion = new LightweightExplosion();
                explosion.Initialize(
                    flyweight,  // ← Shared reference, not copied!
                    new Vector2(i * 10, i * 10),
                    Color.White
                );
                explosions.Add(explosion);
            }

            stopwatch.Stop();
            long memoryAfter = GC.GetTotalMemory(false);

            measurement.MemoryUsedBytes = memoryAfter - memoryBefore;
            measurement.CreationTimeMs = stopwatch.ElapsedMilliseconds;
            measurement.ObjectCount = explosions.Count;
            measurement.FlyweightCount = 1; // Only 1 flyweight instance!
            measurement.MemoryPerObjectBytes = measurement.MemoryUsedBytes / count;

            Console.WriteLine($"  Memory Used: {FormatBytes(measurement.MemoryUsedBytes)}");
            Console.WriteLine($"  Per Explosion: {FormatBytes(measurement.MemoryPerObjectBytes)}");
            Console.WriteLine($"  Flyweight Objects: {measurement.FlyweightCount}");
            Console.WriteLine($"  Creation Time: {measurement.CreationTimeMs}ms");

            explosions.Clear();
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
    }
}
