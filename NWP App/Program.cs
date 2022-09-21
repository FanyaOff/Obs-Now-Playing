using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace NWP_App
{
    internal class Program
    {
        static string values;
        private const string logFile = "latest.log";

        static void Main(string[] args)
        {
            if (File.Exists(logFile))
            {
                File.Delete(logFile);
                File.WriteAllText(logFile, "----- NWP Log File ----\n");
            } else
                File.WriteAllText(logFile, "----- NWP Log File ----\n");

            Console.WriteLine("Starting check thread.");
            Console.WriteLine("If you want add this to obs, add new source Text, check the box named by Read from file and select created file");
            updateFile().GetAwaiter().GetResult();
        }

        public async static Task updateFile()
        {
            while (true)
            {
                try
                {
                exit:
                    var gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
                    var mediaProperties = await GetMediaProperties(gsmtcsm.GetCurrentSession());
                    string newValues = $"{mediaProperties.Artist} - {mediaProperties.Title}";
                    if (values != newValues)
                    {
                        log($"[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} / INFO] Now Playing: {newValues}");
                        Console.WriteLine($"Now Playing: {newValues}");
                        log($"[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} / INFO] Writing song to txt file");
                        File.WriteAllText("nowPlayingFile.txt", newValues);
                        log($"[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} / INFO] Writed!");
                        values = newValues;
                        goto exit;
                    }
                }
                catch (Exception e)
                {
                    log($"[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} / ERROR] {e}");
                    Console.WriteLine("Program is crashed. Check latest.log file. If you have some questions/found bug, open issue on my github - https://github.com/FanyaOff/Obs-Now-Playing");
                }
            }
        }

        public static void log(string text)
        {
            using (StreamWriter w = File.AppendText(logFile))
            {
                w.WriteLine(text, "\n");
            }
        }

        private static async Task<GlobalSystemMediaTransportControlsSessionManager> GetSystemMediaTransportControlsSessionManager() =>
            await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

        private static async Task<GlobalSystemMediaTransportControlsSessionMediaProperties> GetMediaProperties(GlobalSystemMediaTransportControlsSession session) =>
            await session.TryGetMediaPropertiesAsync();
    }
}
