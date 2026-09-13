using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ClipAngel
{
    /// <summary>
    /// Wraps the Windows 10 built-in OCR engine (Windows.Media.Ocr).
    /// All WinRT types are resolved at runtime via reflection so the assembly
    /// compiles and runs on Windows Vista/7/8 with no Windows SDK reference.
    /// Guard every call with <see cref="IsOcrSupported"/> first.
    /// </summary>
    internal static class OcrHelper
    {
        // Lazily loaded AsTask open-generic method from System.Runtime.WindowsRuntime.
        private static MethodInfo _asTaskOpenGeneric;

        // ── public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns true when the OS is Windows 10 or later.
        /// Environment.OSVersion is NOT used because .NET returns a compatibility
        /// shim value (6.x) when the app manifest lacks a Win10 SupportedOS entry.
        /// </summary>
        public static bool IsOcrSupported()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine
                    .OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key == null) return false;
                    // CurrentMajorVersionNumber (DWORD) exists only on Windows 10+.
                    object major = key.GetValue("CurrentMajorVersionNumber");
                    return major != null && (int)major >= 10;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Recognises text in <paramref name="bitmap"/> using the Windows OCR engine.
        /// Call only when <see cref="IsOcrSupported"/> returns true.
        /// [NoInlining] prevents JIT from touching WinRT paths on old Windows.
        ///
        /// Scaling is performed inside the WIC codec pipeline via BitmapTransform
        /// using the Fant algorithm — the same high-quality scaler Windows uses
        /// internally, and significantly better than any GDI+ pre-scaling.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Task<string> RecognizeTextAsync(Bitmap bitmap)
        {
            // Capture dimensions before handing off to the thread pool.
            int origW = bitmap.Width;
            int origH = bitmap.Height;
            return Task.Run(() => RecognizeSync(bitmap, origW, origH));
        }

        /// <summary>
        /// Recognizes text in bitmap using either local OCR (Windows 10+) or online OCR (OpenAI).
        /// Automatically selects the appropriate OCR method based on settings and availability.
        /// </summary>
        public static async Task<string> RecognizeTextAutoAsync(Bitmap bitmap)
        {
            bool useOnlineOcr = Properties.Settings.Default.UseOnlineOcr;
            string apiKey = Properties.Settings.Default.OnlineOcrApiKey;
            string endpoint = Properties.Settings.Default.OnlineOcrEndpoint;
            string model = Properties.Settings.Default.OnlineOcrModel;

            // Try online OCR first if configured
            if (useOnlineOcr && OnlineOcrHelper.IsOnlineOcrConfigured(apiKey, endpoint))
            {
                try
                {
                    return await OnlineOcrHelper.RecognizeTextAsync(bitmap, apiKey, endpoint, model);
                }
                catch (Exception ex)
                {
                    // Fallback to local OCR if online fails
                    System.Diagnostics.Debug.WriteLine($"Online OCR failed: {ex.Message}");
                }
            }

            // Fallback to local OCR
            if (IsOcrSupported())
            {
                return await RecognizeTextAsync(bitmap);
            }

            throw new InvalidOperationException(Properties.Resources.OcrNotAvailable);
        }

        // ── private implementation ────────────────────────────────────────────────

        private static string RecognizeSync(Bitmap bitmap, int origW, int origH)
        {
            // Load System.Runtime.WindowsRuntime from GAC (Win8+ / .NET 4.5+).
            Assembly srtwr = Assembly.Load(
                "System.Runtime.WindowsRuntime, Version=4.0.0.0, Culture=neutral, " +
                "PublicKeyToken=b77a5c561934e089");

            MethodInfo asRas = GetAsRandomAccessStreamMethod(srtwr);

            // ── 1. Original bitmap → BMP stream → WinRT IRandomAccessStream ──────
            // We pass the ORIGINAL unscaled bitmap. Scaling is done by WIC below.
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Position = 0;
                object rasStream = asRas.Invoke(null, new object[] { ms });

                // ── 2. BitmapDecoder.CreateAsync(stream) ──────────────────────────
                Type bdType = GetWinRtType("Windows.Graphics.Imaging.BitmapDecoder");
                MethodInfo createAsync = GetStaticMethod(bdType, "CreateAsync", 1);
                object decoder = AwaitOp(srtwr, createAsync,
                                         createAsync.Invoke(null, new[] { rasStream }));

                // ── 3. Build BitmapTransform: 2× scale with Fant interpolation ────
                //
                // BitmapInterpolationMode enum values (Windows.Graphics.Imaging):
                //   0 = NearestNeighbor
                //   1 = Linear
                //   2 = Cubic   (bicubic)
                //   3 = Fant    (WIC high-quality Fant filter — best overall)
                //
                // Scaling is performed by the WIC codec at pixel-data level,
                // which is significantly higher quality than GDI+ HighQualityBicubic
                // and is the same internal algorithm used by Windows Photo Viewer,
                // Paint 3D, and other system imaging apps.
                Type transformType = GetWinRtType("Windows.Graphics.Imaging.BitmapTransform");
                object transform = Activator.CreateInstance(transformType);
                transformType.GetProperty("ScaledWidth").SetValue(transform, (uint)(origW * 2));
                transformType.GetProperty("ScaledHeight").SetValue(transform, (uint)(origH * 2));
                PropertyInfo modeProp = transformType.GetProperty("InterpolationMode");
                // Fant = 3
                modeProp.SetValue(transform, Enum.ToObject(modeProp.PropertyType, 3));

                // ── 4. GetSoftwareBitmapAsync(format, alpha, transform, exif, color)
                //
                // BitmapPixelFormat.Bgra8            = 87
                // BitmapAlphaMode.Premultiplied       = 0
                // ExifOrientationMode.Ignore          = 0
                // ColorManagementMode.DoNotColorManage= 0
                MethodInfo getSbAsync5 = GetInstanceMethod(
                    decoder.GetType(), "GetSoftwareBitmapAsync", 5);

                Type pixFmtType  = GetWinRtType("Windows.Graphics.Imaging.BitmapPixelFormat");
                Type alphaMdType = GetWinRtType("Windows.Graphics.Imaging.BitmapAlphaMode");
                Type exifType    = GetWinRtType("Windows.Graphics.Imaging.ExifOrientationMode");
                Type colorType   = GetWinRtType("Windows.Graphics.Imaging.ColorManagementMode");

                object softBitmap = AwaitOp(srtwr, getSbAsync5, getSbAsync5.Invoke(decoder,
                    new[]
                    {
                        Enum.ToObject(pixFmtType,  87), // Bgra8
                        Enum.ToObject(alphaMdType,  0), // Premultiplied
                        transform,
                        Enum.ToObject(exifType,     0), // IgnoreExifOrientation
                        Enum.ToObject(colorType,    0)  // DoNotColorManage
                    }));

                // ── 5. OcrEngine.TryCreateFromUserProfileLanguages() ──────────────
                Type ocrType = GetWinRtType("Windows.Media.Ocr.OcrEngine");
                MethodInfo tryCreate = ocrType.GetMethod(
                    "TryCreateFromUserProfileLanguages",
                    BindingFlags.Public | BindingFlags.Static);
                object engine = tryCreate.Invoke(null, null);
                if (engine == null)
                    throw new InvalidOperationException(Properties.Resources.OcrEngineUnavailable);

                // ── 6. engine.RecognizeAsync(softBitmap) ──────────────────────────
                MethodInfo recognizeAsync = GetInstanceMethod(
                    engine.GetType(), "RecognizeAsync", 1);
                object ocrResult = AwaitOp(srtwr, recognizeAsync,
                                           recognizeAsync.Invoke(engine, new[] { softBitmap }));

                // ── 7. Collect OcrResult.Lines → joined with newlines ─────────────
                // OcrResult.Text concatenates everything with spaces and no line breaks.
                // We iterate Lines manually to preserve the original line structure.
                object lines = ocrResult.GetType()
                                        .GetProperty("Lines")
                                        .GetValue(ocrResult);
                var lineTexts = new System.Collections.Generic.List<string>();
                foreach (object line in (System.Collections.IEnumerable)lines)
                {
                    string lineText = (string)line.GetType()
                                                   .GetProperty("Text")
                                                   .GetValue(line);
                    lineTexts.Add(lineText);
                }
                return string.Join(Environment.NewLine, lineTexts.ToArray());
            }
        }

        // ── reflection helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Awaits a WinRT IAsyncOperation&lt;TResult&gt; via AsTask&lt;TResult&gt;().
        /// TResult is inferred from <paramref name="method"/>.ReturnType so we never
        /// need to inspect the runtime COM object's interfaces.
        /// </summary>
        private static object AwaitOp(Assembly srtwr, MethodInfo method, object asyncOp)
        {
            Type returnType = method.ReturnType;
            Type resultType = returnType.IsGenericType
                ? returnType.GetGenericArguments()[0]
                : typeof(object);

            if (_asTaskOpenGeneric == null)
                _asTaskOpenGeneric = FindAsTaskOpenGeneric(srtwr);

            MethodInfo asTaskClosed = _asTaskOpenGeneric.MakeGenericMethod(resultType);
            object task = asTaskClosed.Invoke(null, new[] { asyncOp });

            object awaiter = task.GetType().GetMethod("GetAwaiter").Invoke(task, null);
            awaiter.GetType().GetMethod("GetResult").Invoke(awaiter, null);

            return task.GetType().GetProperty("Result").GetValue(task);
        }

        private static MethodInfo FindAsTaskOpenGeneric(Assembly srtwr)
        {
            Type extType = srtwr.GetType("System.WindowsRuntimeSystemExtensions");
            if (extType == null)
                throw new InvalidOperationException(
                    "System.WindowsRuntimeSystemExtensions not found.");

            foreach (MethodInfo m in extType.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (m.Name != "AsTask" || !m.IsGenericMethodDefinition) continue;
                ParameterInfo[] ps = m.GetParameters();
                if (ps.Length != 1) continue;
                Type pt = ps[0].ParameterType;
                if (pt.IsGenericType &&
                    pt.GetGenericTypeDefinition().Name.StartsWith("IAsyncOperation"))
                    return m;
            }
            throw new InvalidOperationException(
                "AsTask<TResult>(IAsyncOperation<TResult>) not found.");
        }

        private static MethodInfo GetAsRandomAccessStreamMethod(Assembly srtwr)
        {
            Type extType = srtwr.GetType("System.IO.WindowsRuntimeStreamExtensions");
            if (extType == null)
                throw new InvalidOperationException(
                    "System.IO.WindowsRuntimeStreamExtensions not found.");

            foreach (MethodInfo m in extType.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (m.Name != "AsRandomAccessStream") continue;
                ParameterInfo[] ps = m.GetParameters();
                if (ps.Length == 1 &&
                    typeof(Stream).IsAssignableFrom(ps[0].ParameterType))
                    return m;
            }
            throw new InvalidOperationException("AsRandomAccessStream(Stream) not found.");
        }

        private static Type GetWinRtType(string fullName)
        {
            Type t = Type.GetType(fullName + ", Windows, ContentType=WindowsRuntime");
            if (t == null)
                throw new InvalidOperationException(
                    "WinRT type '" + fullName + "' could not be loaded. Windows 10 required.");
            return t;
        }

        private static MethodInfo GetStaticMethod(Type type, string name, int paramCount)
        {
            foreach (MethodInfo m in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                if (m.Name == name && m.GetParameters().Length == paramCount) return m;
            throw new InvalidOperationException(
                "Static method '" + name + "'/" + paramCount + " params not found on "
                + type.FullName + ".");
        }

        private static MethodInfo GetInstanceMethod(Type type, string name, int paramCount)
        {
            foreach (MethodInfo m in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                if (m.Name == name && m.GetParameters().Length == paramCount) return m;
            throw new InvalidOperationException(
                "Instance method '" + name + "'/" + paramCount + " params not found on "
                + type.FullName + ".");
        }
    }
}
