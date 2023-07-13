using ImageDownloaderApp.Factories;
using ImageDownloaderApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageDownloaderApp
{
    class Program
    {
        private static readonly HttpClientFactory clientFactory = new HttpClientFactory();
        private static SemaphoreSlim semaphore;

        private static string imageSource;

        private static int downloadedCount;
        private static int totalCount;
        private static int parallelism;
        private static string savePath;

        private static readonly object lockObject = new object();

        static async Task Main(string[] args)
        {
            Init();
            
            using var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cts.Cancel();

                Console.WriteLine("\nDownload canceled. Cleaning up...");
                CleanUp(savePath);
            };

            Console.WriteLine("\n");
            Console.WriteLine($"Downloading {totalCount} images ({parallelism} parallel downloads at most)\n");

            List<Task> downloadTasks = await CreateDownloadTasks(cts.Token);
            await Task.WhenAll(downloadTasks);

            Console.WriteLine("\n");
            Console.WriteLine("Download completed");
            Console.ReadLine();
        }

        static void Init()
        {
            var input = System.Text.Json.JsonSerializer.Deserialize<Input>(File.ReadAllText("Input.json"));

            totalCount = input.Count;
            parallelism = input.Parallelism;
            savePath = input.SavePath;
            imageSource = input.ImageSource;

            semaphore = new SemaphoreSlim(parallelism);

            CreateDirectory();
        }

        static void CreateDirectory()
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
        }

        static async Task<List<Task>> CreateDownloadTasks(CancellationToken cancellationToken)
        {
            List<Task> downloadTasks = new List<Task>();
            for (int i = 1; i <= totalCount; i++)
            {
                await semaphore.WaitAsync();
                downloadTasks.Add(DownloadImageAsync(i, cancellationToken));
            }

            return downloadTasks;
        }

        static async Task DownloadImageAsync(int imageNumber, CancellationToken cancellationToken)
        {
            try
            {
                using var client = clientFactory.CreateClient();
                var response = await client.GetByteArrayAsync(imageSource, cancellationToken);

                lock (lockObject)
                {
                    downloadedCount++;

                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write($"Progress: {downloadedCount}/{totalCount}");
                }

                string imagePath = Path.Combine(savePath, $"{imageNumber}.png");
                await File.WriteAllBytesAsync(imagePath, response);
            }
            finally
            {
                semaphore.Release();
            }
        }

        static void CleanUp(string savePath)
        {
            if (Directory.Exists(savePath))
            {
                Directory.Delete(savePath, true);
            }

            Environment.Exit(0);
        }
    }
}
