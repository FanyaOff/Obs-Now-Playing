using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace NWP_App
{
    internal class Program
    {
        private static string currentSong = string.Empty;
        private const string nowPlayingFile = "nowPlayingFile.txt";

        static async Task Main(string[] args)
        {
            Console.WriteLine("NWP Started - Monitoring for song changes");
            Console.WriteLine("For OBS: add Text source with 'Read from file' option and select nowPlayingFile.txt");

            File.WriteAllText(nowPlayingFile, string.Empty);

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                await MonitorMediaChanges(cts.Token);
            }
            finally
            {
                cts.Dispose();
            }
        }

        private static async Task MonitorMediaChanges(CancellationToken token)
        {
            var sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            sessionManager.CurrentSessionChanged += async (sender, args) => {
                await UpdateNowPlaying(sessionManager);
            };

            await UpdateNowPlaying(sessionManager);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    await UpdateNowPlaying(sessionManager);
                    await Task.Delay(5000, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await Task.Delay(10000, token);
                }
            }
        }

        private static async Task UpdateNowPlaying(GlobalSystemMediaTransportControlsSessionManager sessionManager)
        {
            try
            {
                var session = sessionManager.GetCurrentSession();
                if (session == null)
                {
                    if (currentSong != string.Empty)
                    {
                        currentSong = string.Empty;
                        File.WriteAllText(nowPlayingFile, string.Empty);
                        Console.WriteLine("No active media session detected");
                    }
                    return;
                }

                var mediaProperties = await session.TryGetMediaPropertiesAsync();
                string newSong = string.Empty;

                if (!string.IsNullOrEmpty(mediaProperties.Artist) || !string.IsNullOrEmpty(mediaProperties.Title))
                {
                    newSong = $"{mediaProperties.Artist} - {mediaProperties.Title}".Trim();

                    if (newSong.StartsWith(" - ")) newSong = newSong.Substring(3);
                    if (newSong.EndsWith(" - ")) newSong = newSong.Substring(0, newSong.Length - 3);
                }

                if (currentSong != newSong)
                {
                    currentSong = newSong;
                    File.WriteAllText(nowPlayingFile, newSong);

                    if (!string.IsNullOrEmpty(newSong))
                        Console.WriteLine($"Now Playing: {newSong}");
                }

                session.MediaPropertiesChanged += async (s, e) => {
                    await UpdateNowPlaying(sessionManager);
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating media info: {ex.Message}");
            }
        }
    }
}
