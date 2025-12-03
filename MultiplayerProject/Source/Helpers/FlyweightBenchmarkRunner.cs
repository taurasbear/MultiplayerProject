using MultiplayerProject.Source.Helpers;
using Microsoft.Xna.Framework.Content;
using System;

namespace MultiplayerProject.Source
{
    /// <summary>
    /// Simple utility to run the Flyweight benchmark from anywhere in the code
    /// Usage: FlyweightBenchmarkRunner.RunBenchmark(contentManager, 1000);
    /// </summary>
    public static class FlyweightBenchmarkRunner
    {
        private static bool _hasRun = false;

        /// <summary>
        /// Run benchmarks for both lasers and explosions
        /// </summary>
        /// <param name="content">ContentManager for loading textures</param>
        /// <param name="objectCount">Number of objects to test with (default: 500)</param>
        public static void RunBenchmark(ContentManager content, int objectCount = 500)
        {
            // Prevent running multiple times (happens when server + clients all launch)
            if (_hasRun)
                return;
            
            _hasRun = true;

            try
            {
                Console.WriteLine("\n========================================");
                Console.WriteLine("FLYWEIGHT PATTERN BENCHMARKS");
                Console.WriteLine("========================================\n");

                int[] testCounts = { 100, 500, 1000 };

                // Benchmark explosions
                Console.WriteLine("--- EXPLOSION BENCHMARKS ---\n");
                foreach (int count in testCounts)
                {
                    Console.WriteLine($"Testing with {count} explosions...");
                    var explosionBenchmark = new ExplosionMemoryBenchmark(content);
                    explosionBenchmark.RunBenchmark(count);
                    Console.WriteLine();
                }

                Console.WriteLine("\n--- LASER BENCHMARKS ---\n");
                foreach (int count in testCounts)
                {
                    Console.WriteLine($"Testing with {count} lasers...");
                    var laserBenchmark = new LaserMemoryBenchmark(content);
                    laserBenchmark.RunBenchmark(count);
                    Console.WriteLine();
                }

                Console.WriteLine("\n========================================");
                Console.WriteLine("ALL BENCHMARKS COMPLETE");
                Console.WriteLine("========================================\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to run benchmark: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
