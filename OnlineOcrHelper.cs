using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ClipAngel
{
    /// <summary>
    /// Online OCR helper using direct HTTP calls to OpenAI API
    /// Supports OpenAI Vision API (GPT-4 Vision, GPT-4o) and OpenRouter
    /// </summary>
    internal static class OnlineOcrHelper
    {
        private static readonly HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

        // ── public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Checks if online OCR is configured and available
        /// </summary>
        public static bool IsOnlineOcrConfigured(string apiKey, string endpoint)
        {
            return !string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(endpoint);
        }

        /// <summary>
        /// Recognizes text in bitmap using OpenAI Vision API via HTTP with retry mechanism
        /// </summary>
        public static async Task<string> RecognizeTextAsync(Bitmap bitmap, string apiKey, string endpoint, string modelName = "gpt-4o")
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            if (!IsOnlineOcrConfigured(apiKey, endpoint))
                throw new InvalidOperationException(ClipAngel.Properties.Resources.OnlineOcrNotConfigured);

            // Get API key from environment variable if available (best practice)
            string effectiveApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ??
                                    Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ??
                                    apiKey;

            // Use default model if not specified
            if (string.IsNullOrWhiteSpace(modelName))
                modelName = "gpt-4o";

            // Retry mechanism for transient errors
            int maxRetries = 3;
            Exception lastException = null;
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    return await RecognizeTextInternalAsync(bitmap, effectiveApiKey, endpoint, modelName);
                }
                catch (Exception ex) when (attempt < maxRetries - 1)
                {
                    lastException = ex;
                    System.Diagnostics.Debug.WriteLine($"OCR attempt {attempt + 1} failed: {ex.Message}");
                    // Wait before retry with exponential backoff
                    await Task.Delay(TimeSpan.FromSeconds(2 * (attempt + 1)));
                }
            }

            throw new InvalidOperationException(ClipAngel.Properties.Resources.OnlineOcrRetriesFailed, lastException);
        }

        // ── private helpers ──────────────────────────────────────────────────────

        private static async Task<string> RecognizeTextInternalAsync(Bitmap bitmap, string apiKey, string endpoint, string modelName)
        {
            // Convert bitmap to base64
            string base64Image = ConvertBitmapToBase64(bitmap);

            // Prepare API endpoint
            string apiUrl = endpoint.TrimEnd('/');
            if (!apiUrl.EndsWith("/chat/completions"))
                apiUrl += "/chat/completions";

            // Create request body
            var requestBody = new
            {
                model = modelName,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "text",
                                text = "Please extract all text from this image. Return only the recognized text without any additional commentary or formatting."
                            },
                            new
                            {
                                type = "image_url",
                                image_url = new
                                {
                                    url = $"data:image/png;base64,{base64Image}"
                                }
                            }
                        }
                    }
                },
                max_tokens = 4096,
                temperature = 0
            };

            // Serialize to JSON
            string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);

            // Create request
            using (var request = new HttpRequestMessage(HttpMethod.Post, apiUrl))
            {
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                // Send request
                using (var response = await httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();

                    // Parse response
                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(responseContent);

                    // Safe JSON parsing with null checks
                    if (jsonResponse?.choices == null)
                        throw new InvalidOperationException("Invalid API response: no choices");

                    var choices = jsonResponse.choices;
                    if (!((System.Collections.IEnumerable)choices).GetEnumerator().MoveNext())
                        throw new InvalidOperationException("Invalid API response: empty choices array");

                    if (choices[0]?.message?.content == null)
                        throw new InvalidOperationException("Invalid API response: no content");

                    string result = choices[0].message.content.ToString();
                    return result ?? string.Empty;
                }
            }
        }

        private static string ConvertBitmapToBase64(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                byte[] imageBytes = ms.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }
    }
}
