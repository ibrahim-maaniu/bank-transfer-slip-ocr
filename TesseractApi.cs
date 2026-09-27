using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TransactionOCR;

internal static class TesseractApi
{
    public static string OcrImage(string imagePath)
    {
        if (OperatingSystem.IsWindows())
            return WindowsTesseract.OcrImage(imagePath, "./tessdata");
        else
            return LinuxTesseract.OcrImage(imagePath, "/usr/share/tessdata");
    }
}

internal static class LinuxTesseract
{
    static LinuxTesseract()
    {
        NativeLibrary.Load(
            Path.Combine(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "x64"),
                "tesseract55.dll"
            )
        );
        NativeLibrary.Load(
            Path.Combine(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "x64"),
                "leptonica-1.85.0.dll"
            )
        );
    }

    [DllImport("tesseract55.dll")]
    private static extern IntPtr TessBaseAPICreate();

    [DllImport("tesseract55.dll")]
    private static extern int TessBaseAPIInit3(IntPtr handle, string datapath, string language);

    [DllImport("tesseract55.dll")]
    private static extern void TessBaseAPISetImage2(IntPtr handle, IntPtr pix);

    [DllImport("tesseract55.dll")]
    private static extern IntPtr TessBaseAPIGetUTF8Text(IntPtr handle);

    [DllImport("tesseract55.dll")]
    private static extern void TessDeleteText(IntPtr text);

    [DllImport("tesseract55.dll")]
    private static extern void TessBaseAPIEnd(IntPtr handle);

    [DllImport("tesseract55.dll")]
    private static extern void TessBaseAPIDelete(IntPtr handle);

    [DllImport("leptonica-1.85.0.dll")]
    private static extern IntPtr pixRead(string filename);

    [DllImport("leptonica-1.85.0.dll")]
    private static extern void pixDestroy(ref IntPtr pix);

    public static string OcrImage(string imagePath, string tessDataPath)
    {
        var api = TessBaseAPICreate();
        if (api == IntPtr.Zero)
            throw new Exception("Failed to create Tesseract API.");

        try
        {
            if (TessBaseAPIInit3(api, tessDataPath, "eng") != 0)
                throw new Exception("Failed to initialize Tesseract.");

            var pix = pixRead(imagePath);
            if (pix == IntPtr.Zero)
                throw new Exception($"Failed to load image: {imagePath}");

            try
            {
                TessBaseAPISetImage2(api, pix);
                var textPtr = TessBaseAPIGetUTF8Text(api);
                if (textPtr == IntPtr.Zero)
                    throw new Exception("OCR returned no text.");

                try
                {
                    return Marshal.PtrToStringAnsi(textPtr) ?? string.Empty;
                }
                finally
                {
                    TessDeleteText(textPtr);
                }
            }
            finally
            {
                pixDestroy(ref pix);
            }
        }
        finally
        {
            TessBaseAPIEnd(api);
            TessBaseAPIDelete(api);
        }
    }
}

internal static class WindowsTesseract
{
    static WindowsTesseract()
    {
        NativeLibrary.Load(
            Path.Combine(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "x64"),
                "leptonica-1.85.0.dll"
            )
        );
        NativeLibrary.Load(
            Path.Combine(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "x64"),
                "tesseract55.dll"
            )
        );
    }
    [DllImport("tesseract55.dll")]
    private static extern IntPtr TessBaseAPICreate();

    [DllImport("tesseract55.dll")]
    private static extern int TessBaseAPIInit3(IntPtr handle, string datapath, string language);

    [DllImport("tesseract55.dll")]
    private static extern void TessBaseAPISetImage2(IntPtr handle, IntPtr pix);

    [DllImport("tesseract55.dll")]
    private static extern IntPtr TessBaseAPIGetUTF8Text(IntPtr handle);

    [DllImport("tesseract55.dll")]
    private static extern void TessDeleteText(IntPtr text);

    [DllImport("tesseract55.dll")]
    private static extern void TessBaseAPIEnd(IntPtr handle);

    [DllImport("tesseract55.dll")]
    private static extern void TessBaseAPIDelete(IntPtr handle);

    [DllImport("libleptonica-6.dll")]
    private static extern IntPtr pixRead(string filename);

    [DllImport("libleptonica-6.dll")]
    private static extern void pixDestroy(ref IntPtr pix);

    public static string OcrImage(string imagePath, string tessDataPath)
    {
        var api = TessBaseAPICreate();
        if (api == IntPtr.Zero)
            throw new Exception("Failed to create Tesseract API.");

        try
        {
            if (TessBaseAPIInit3(api, tessDataPath, "eng") != 0)
                throw new Exception("Failed to initialize Tesseract.");

            var pix = pixRead(imagePath);
            if (pix == IntPtr.Zero)
                throw new Exception($"Failed to load image: {imagePath}");

            try
            {
                TessBaseAPISetImage2(api, pix);
                var textPtr = TessBaseAPIGetUTF8Text(api);
                if (textPtr == IntPtr.Zero)
                    throw new Exception("OCR returned no text.");

                try
                {
                    return Marshal.PtrToStringAnsi(textPtr) ?? string.Empty;
                }
                finally
                {
                    TessDeleteText(textPtr);
                }
            }
            finally
            {
                pixDestroy(ref pix);
            }
        }
        finally
        {
            TessBaseAPIEnd(api);
            TessBaseAPIDelete(api);
        }
    }
}