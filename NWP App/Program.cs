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
        static void Main(string[] args)
        {
            Console.WriteLine("Starting check thread.");
            Console.WriteLine("If you want add this to obs, add new source Text, check the box named by Read from file and select created file");
            updateFile().GetAwaiter().GetResult();
        }

        public async static Task updateFile()
        {
            while (true)
            {
                exit:
                var gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
                var mediaProperties = await GetMediaProperties(gsmtcsm.GetCurrentSession());
                string newValues = $"{mediaProperties.Artist} - {mediaProperties.Title}";
                if (values != newValues)
                {
                    Console.WriteLine($"Now Playing: {newValues}");
                    File.WriteAllText("nowPlayingFile.txt", newValues);
                    values = newValues;
                    goto exit;
                }
            }
        }
        private static async Task<GlobalSystemMediaTransportControlsSessionManager> GetSystemMediaTransportControlsSessionManager() =>
            await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

        private static async Task<GlobalSystemMediaTransportControlsSessionMediaProperties> GetMediaProperties(GlobalSystemMediaTransportControlsSession session) =>
            await session.TryGetMediaPropertiesAsync();
    }
}
