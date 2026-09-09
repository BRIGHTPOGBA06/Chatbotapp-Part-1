using System;
using System.Media;
using System.Runtime.Versioning;

namespace CyberShieldChatbot
{
    // Small utility to play a WAV file on Windows.
    // We keep audio code here so other parts of the app can call it without knowing the details.
    public static class AudioPlayer
    {
        [SupportedOSPlatform("windows")]
        public static void PlayGreeting(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                // If no path is given, don't try to play anything.
                return;
            }

            try
            {
                using SoundPlayer player = new SoundPlayer(path);
                // Try to load the file now so any problems are caught early.
                player.Load();
                player.Play();
            }
            catch (Exception ex)
            {
                // Keep audio errors non-fatal and show a friendly message.
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[Notice] Audio playback failed: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}