using System;
using System.Runtime.Versioning;

namespace CyberShieldChatbot
{
    // This small helper organizes the app startup steps.
    // It checks the platform before trying to play audio and then starts the chatbot flow.
    public static class BotChat
    {
        [SupportedOSPlatform("windows")]
        public static void StartApplication()
        {
            Console.Title = "CyberShield Security Chatbot - Part 1";
            string audioPath = "greeting.wav";

            // Try to play a welcome sound, but only on Windows where the sound code is supported.
            if (OperatingSystem.IsWindows())
            {
#pragma warning disable CA1416
                try
                {
                    AudioPlayer.PlayGreeting(audioPath);
                }
                catch (Exception ex)
                {
                    // If the audio cannot play, show a gentle notice but continue running.
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[Notice] Audio playback failed: {ex.Message}");
                    Console.ResetColor();
                }
#pragma warning restore CA1416
            }

            // Create the pieces of the app and hand control to the chatbot engine.
            MediaHandler media = new MediaHandler(audioPath);
            ChatbotEngine bot = new ChatbotEngine(media);

            try
            {
                bot.InitializeSession();
                bot.RunMainLoop();
            }
            catch (Exception ex)
            {
                // If something fatal happens, show the message to the user.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"A fatal error occurred: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}