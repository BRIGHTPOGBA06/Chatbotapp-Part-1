using System;
using System.IO;
using System.Threading;
using System.Runtime.Versioning;
using System.Media;

namespace CyberShieldChatbot
{
    // -------------------------
    // 1. Simple input helper
    // -------------------------
    // These methods keep asking the user until they type a valid answer.
    // This helps the rest of the program work with safe, expected values.
    public static class Validator
    {
        public static string GetValidStringInput(string prompt)
        {
            while (true)
            {
                try
                {
                    Console.Write(prompt);
                    string? input = Console.ReadLine();

                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        return input.Trim();
                    }

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Error] Input cannot be empty. Please try again.");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    // If something unexpected happens, show the message so the user knows.
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[Critical Error] An unexpected error occurred: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        public static int GetValidIntegerInput(string prompt, int min, int max)
        {
            while (true)
            {
                string input = GetValidStringInput(prompt);
                if (int.TryParse(input, out int result))
                {
                    if (result >= min && result <= max)
                    {
                        return result;
                    }
                }
                // Tell the user what range is allowed so they can try again correctly.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Error] Please enter a valid number between {min} and {max}.");
                Console.ResetColor();
            }
        }
    }

    // -------------------------
    // 2. Media and UI helper
    // -------------------------
    // This class finds the audio file and shows the ASCII header.
    // It logs audio errors quietly so the program can keep running.
    public class MediaHandler
    {
        private readonly string soundFilePath;
        private readonly string logPath;

        public MediaHandler(string path)
        {
            string fileName = string.IsNullOrWhiteSpace(path) ? "greeting.wav" : Path.GetFileName(path);
            string baseDir = AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();
            logPath = Path.Combine(baseDir, "audio_errors.log");

            string candidateInBase = Path.Combine(baseDir, fileName);

            if (File.Exists(candidateInBase))
            {
                soundFilePath = candidateInBase;
                return;
            }

            if (!string.IsNullOrWhiteSpace(path) && Path.IsPathRooted(path) && File.Exists(path))
            {
                soundFilePath = path;
                return;
            }

            soundFilePath = fileName;
        }

        [SupportedOSPlatform("windows")]
        public bool PlayVoiceGreeting(bool debugSync = false)
        {
            try
            {
                Console.WriteLine($"[Debug] Resolved sound path: {soundFilePath}");
                if (!File.Exists(soundFilePath))
                {
                    // If we can't find the audio, let the user know but continue.
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[Notice] Audio file not found. Skipping voice greeting playback.");
                    Console.ResetColor();
                    return false;
                }

                using SoundPlayer player = new SoundPlayer(soundFilePath);

                // Load audio now so any file issues show up before trying to play.
                player.Load();

                if (debugSync)
                {
                    player.PlaySync();
                    Console.WriteLine("PlaySync finished.");
                }
                else
                {
                    player.Play();
                    Console.WriteLine("Play started.");
                }

                return true;
            }
            catch (FileNotFoundException fnf)
            {
                // Keep a small log if audio is missing so you can debug later.
                LogAudioError(fnf);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Audio file not found: {fnf.Message}");
                Console.ResetColor();
                return false;
            }
            catch (InvalidOperationException inv)
            {
                // If playback fails for some reason, log it and continue.
                LogAudioError(inv);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[Notice] Audio playback failed (invalid operation): {inv.Message}");
                Console.ResetColor();
                return false;
            }
            catch (Exception ex)
            {
                // Catch anything unexpected so the app doesn't crash because of audio.
                LogAudioError(ex);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[Notice] Could not play audio: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        private void LogAudioError(Exception ex)
        {
            try
            {
                // Write a simple timestamped record to a file so issues can be reviewed later.
                string entry = $"{DateTime.UtcNow:O} | {ex.GetType().FullName} | {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";
                File.AppendAllText(logPath, entry);
            }
            catch
            {
                // If logging fails, don't crash the whole app for that reason.
            }
        }

        // Show a clear title banner and the current user's name.
        // This makes the console look friendly and tells the user what the app is.
        public void DisplayHeader(string userName)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("=====================================================================================");
            Console.WriteLine(); // Spacing line

            // Big readable title so the user sees the application name clearly.
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" ██████╗██╗   ██╗██████╗ ███████╗██████╗ ███████╗██╗  ██╗██╗███████╗██╗     ██████╗ ");
            Console.WriteLine("██╔════╝██║   ██║██╔══██╗██╔════╝██╔══██╗██╔════╝██║  ██║██║██╔════╝██║     ██╔══██╗");
            Console.WriteLine("██║     ██║   ██║██████╔╝█████╗  ██████╔╝███████╗███████║██║█████╗  ██║     ██║  ██║");
            Console.WriteLine("██║     ██║   ██║██╔══██╗██╔══╝  ██╔══██╗╚════██║██╔══██║██║██╔══╝  ██║     ██║  ██║");
            Console.WriteLine("╚██████╗╚██████╔╝██████╔╝███████╗██║  ██║███████║██║  ██║██║███████╗███████╗██████╔╝");
            Console.WriteLine(" ╚═════╝ ╚═════╝ ╚═════╝ ╚══════╝╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝╚═╝╚══════╝╚══════╝╚═════╝ ");

            Console.WriteLine(); // Spacing line
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=====================================================================================");

            if (!string.IsNullOrEmpty(userName))
            {
                // Show who is using the app so messages feel personal.
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("                         Active User: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{userName}                      ");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("=====================================================================================");
            }

            Console.ResetColor();
        }

    }

    // -------------------------
    // 3. Main chatbot engine
    // -------------------------
    // This class contains the main features of the chatbot:
    // starting a session, showing menus, and responding to user choices.
    public class ChatbotEngine
    {
        private readonly MediaHandler _mediaHandler;
        private string? userName;
        private string UserName => string.IsNullOrWhiteSpace(userName) ? "Guest" : userName!;

        public ChatbotEngine(MediaHandler media)
        {
            _mediaHandler = media ?? throw new ArgumentNullException(nameof(media));
        }

        public void InitializeSession()
        {
            _mediaHandler.DisplayHeader(string.Empty);

            // Only try to play audio on Windows systems.
            if (OperatingSystem.IsWindows())
            {
                // We suppress the analyzer warning because we check the platform at runtime.
#pragma warning disable CA1416
                _mediaHandler.PlayVoiceGreeting();
#pragma warning restore CA1416
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[CyberShield Bot]: Hello! Welcome to your personal Cybersecurity Assistant.");
            Console.ResetColor();

            userName = Validator.GetValidStringInput("\n[CyberShield Bot]: What is your name? ");
            userName = userName?.Trim();

            // Show the header again now that we know the user's name.
            _mediaHandler.DisplayHeader(UserName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[CyberShield Bot]: Great to meet you, **{UserName}**! Let's keep your data secure.\n");
            Console.ResetColor();
            Thread.Sleep(1000);
        }

        public void RunMainLoop()
        {
            bool running = true;
            while (running)
            {
                DisplayMainMenu();
                int choice = Validator.GetValidIntegerInput("\nSelect an option (1-4): ", 1, 4);

                Console.Clear();
                _mediaHandler.DisplayHeader(UserName);

                switch (choice)
                {
                    case 1:
                        ProvideTopicResponse("Password Safety");
                        break;
                    case 2:
                        ProvideTopicResponse("Phishing Awareness");
                        break;
                    case 3:
                        ProvideTopicResponse("Safe Browsing");
                        break;
                    case 4:
                        running = false;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"\n[CyberShield Bot]: Goodbye, {UserName}! Stay safe online.");
                        Console.ResetColor();
                        break;
                }

                if (running)
                {
                    // Wait for the user to acknowledge before returning to the menu.
                    Console.WriteLine("\nPress any key to return to the main menu...");
                    Console.ReadKey();
                }
            }
        }

        private void DisplayMainMenu()
        {
            // Show a simple menu so the user can pick what to learn about.
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n--- MAIN MENU ({UserName}'s Portal) ---");
            Console.ResetColor();
            Console.WriteLine("1. Learn about Password Safety");
            Console.WriteLine("2. Learn about Phishing Awareness");
            Console.WriteLine("3. Learn about Safe Browsing");
            Console.WriteLine("4. Exit Application");
        }

        private void ProvideTopicResponse(string topic)
        {
            // Print short, friendly tips for each topic so the user can learn quickly.
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"=== TOPIC: {topic.ToUpper()} ===\n");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.White;
            switch (topic)
            {
                case "Password Safety":
                    Console.WriteLine($"{UserName}, robust passwords are your first line of defense!");
                    Console.WriteLine("- Use at least 12-16 characters mixing upper, lower, numbers, and symbols.");
                    Console.WriteLine("- Avoid using predictable info like birthdays or pet names.");
                    Console.WriteLine("- Never reuse passwords across multiple sensitive accounts.");
                    break;
                case "Phishing Awareness":
                    Console.WriteLine($"Watch out, {UserName}! Phishing attacks trick you into revealing data.");
                    Console.WriteLine("- Check sender email addresses carefully for subtle misspellings.");
                    Console.WriteLine("- Never click unexpected links or download attachments from unknown sources.");
                    Console.WriteLine("- Banks and legitimate institutions will never ask for your password via email.");
                    break;
                case "Safe Browsing":
                    Console.WriteLine($"Browsing safely protects your hardware and identity, {UserName}:");
                    Console.WriteLine("- Always look for 'https://' and the padlock icon in your URL bar.");
                    Console.WriteLine("- Keep your web browsers and operating systems fully updated.");
                    Console.WriteLine("- Use reputable ad-blockers and avoid suspicious download portals.");
                    break;
            }
            Console.ResetColor();
        }
    }
}