using System;
using MultiplayerProject.Source.Helpers;

namespace MultiplayerProject
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    { 
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Add additional cleanup handlers for unexpected termination
            Console.CancelKeyPress += (sender, e) => AudioManager.ForceCleanup();
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => AudioManager.ForceCleanup();

            try
            {
                using (var app = new Application())
                    app.Run();
            }
            finally
            {
                // Ensure cleanup happens even if the application crashes
                AudioManager.ForceCleanup();
            }
        }
    }
}
