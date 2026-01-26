using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SentimentAnalyzer.Onnx.Multilingual
{
    /// <summary>
    /// Handles downloading and caching of ONNX models from GitHub Releases.
    /// </summary>
    /// <remarks>
    /// IMPORTANT: The DistilBERT model is hosted on GitHub Releases rather than HuggingFace
    /// because the official HuggingFace ONNX model (onnx/model.onnx) has a known bug where
    /// it was incorrectly exported and produces inverted predictions.
    /// 
    /// The model hosted on GitHub Releases has been correctly exported using Python's optimum library:
    /// <code>
    /// from optimum.onnxruntime import ORTModelForSequenceClassification
    /// model = ORTModelForSequenceClassification.from_pretrained(
    ///     'lxyuan/distilbert-base-multilingual-cased-sentiments-student', export=True)
    /// model.save_pretrained('path/to/save')
    /// </code>
    /// 
    /// If you wish to export the model yourself, use the above Python code.
    /// </remarks>
    public static class ModelDownloader
    {
        // Model hosted on GitHub Releases - correctly exported version
        // Update this URL after uploading to: https://github.com/arafattehsin/SentimentAnalyzer/releases
        private const string DefaultModelUrl = "https://github.com/arafattehsin/SentimentAnalyzer/releases/download/v3.0.0/distilbert-multilingual-sentiment-model.onnx";
        private const string DefaultVocabUrl = "https://huggingface.co/lxyuan/distilbert-base-multilingual-cased-sentiments-student/resolve/main/vocab.txt";

        private static readonly string CacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SentimentAnalyzer",
            "models",
            "distilbert-multilingual-sentiment");

        private static readonly object _downloadLock = new object();

        /// <summary>
        /// Creates an HttpClient configured for downloading from HuggingFace.
        /// </summary>
        private static HttpClient CreateHttpClient()
        {
            // Use default HttpClient - .NET MAUI automatically uses platform-native handlers
            // On Android, this uses AndroidMessageHandler which handles SSL properly
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(30)
            };

            // Set headers that HuggingFace expects
            client.DefaultRequestHeaders.UserAgent.ParseAdd("SentimentAnalyzer/1.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("*/*");

            return client;
        }

        /// <summary>
        /// Gets the path to the cached model, downloading if necessary.
        /// </summary>
        /// <param name="progress">Optional progress callback (0.0 to 1.0).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Path to the ONNX model file.</returns>
        public static async Task<string> EnsureModelAsync(
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var modelPath = Path.Combine(CacheDirectory, "model.onnx");
            var vocabPath = Path.Combine(CacheDirectory, "vocab.txt");

            // If both files exist, return immediately
            if (File.Exists(modelPath) && File.Exists(vocabPath))
            {
                return modelPath;
            }

            // Download if not cached
            Directory.CreateDirectory(CacheDirectory);

            using var httpClient = CreateHttpClient();

            if (!File.Exists(vocabPath))
            {
                progress?.Report(0.0);
                await DownloadFileAsync(httpClient, DefaultVocabUrl, vocabPath, cancellationToken);
                progress?.Report(0.05);
            }

            if (!File.Exists(modelPath))
            {
                await DownloadFileWithProgressAsync(httpClient, DefaultModelUrl, modelPath,
                    p => progress?.Report(0.05 + p * 0.95), cancellationToken);
            }

            progress?.Report(1.0);
            return modelPath;
        }

        /// <summary>
        /// Gets the path to the cached model synchronously, downloading if necessary.
        /// </summary>
        /// <returns>Path to the ONNX model file.</returns>
        public static string EnsureModel()
        {
            return EnsureModelAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets the path to the vocabulary file.
        /// </summary>
        public static string GetVocabPath()
        {
            var vocabPath = Path.Combine(CacheDirectory, "vocab.txt");
            if (!File.Exists(vocabPath))
            {
                EnsureModel();
            }
            return vocabPath;
        }

        /// <summary>
        /// Checks if the model is already cached locally.
        /// </summary>
        public static bool IsModelCached()
        {
            var modelPath = Path.Combine(CacheDirectory, "model.onnx");
            var vocabPath = Path.Combine(CacheDirectory, "vocab.txt");
            return File.Exists(modelPath) && File.Exists(vocabPath);
        }

        /// <summary>
        /// Clears the cached model files.
        /// </summary>
        public static void ClearCache()
        {
            if (Directory.Exists(CacheDirectory))
            {
                Directory.Delete(CacheDirectory, recursive: true);
            }
        }

        /// <summary>
        /// Gets the cache directory path.
        /// </summary>
        public static string GetCacheDirectory() => CacheDirectory;

        private static async Task DownloadFileAsync(
            HttpClient client,
            string url,
            string destinationPath,
            CancellationToken cancellationToken)
        {
            using var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = File.Create(destinationPath);
            await stream.CopyToAsync(fileStream);
        }

        private static async Task DownloadFileWithProgressAsync(
            HttpClient client,
            string url,
            string destinationPath,
            Action<double> progressCallback,
            CancellationToken cancellationToken)
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            var downloadedBytes = 0L;

            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = File.Create(destinationPath);

            var buffer = new byte[81920];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                downloadedBytes += bytesRead;

                if (totalBytes > 0)
                {
                    progressCallback((double)downloadedBytes / totalBytes);
                }
            }
        }
    }
}
