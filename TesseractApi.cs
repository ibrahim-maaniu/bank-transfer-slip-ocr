using System;
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
        NativeLibrary.Load("/usr/lib/libleptonica.so.6");
        NativeLibrary.Load("/usr/lib/libtesseract.so.5");
    }

    [DllImport("libtesseract.so.5")]
    private static extern IntPtr TessBaseAPICreate();

    [DllImport("libtesseract.so.5")]
    private static extern int TessBaseAPIInit3(IntPtr handle, string datapath, string language);

    [DllImport("libtesseract.so.5")]
    private static extern void TessBaseAPISetImage2(IntPtr handle, IntPtr pix);

    [DllImport("libtesseract.so.5")]
    private static extern IntPtr TessBaseAPIGetUTF8Text(IntPtr handle);

    [DllImport("libtesseract.so.5")]
    private static extern void TessDeleteText(IntPtr text);

    [DllImport("libtesseract.so.5")]
    private static extern void TessBaseAPIEnd(IntPtr handle);

    [DllImport("libtesseract.so.5")]
    private static extern void TessBaseAPIDelete(IntPtr handle);

    [DllImport("libleptonica.so.6")]
    private static extern IntPtr pixRead(string filename);

    [DllImport("libleptonica.so.6")]
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
    [DllImport("libtesseract-5.dll")]
    private static extern IntPtr TessBaseAPICreate();

    [DllImport("libtesseract-5.dll")]
    private static extern int TessBaseAPIInit3(IntPtr handle, string datapath, string language);

    [DllImport("libtesseract-5.dll")]
    private static extern void TessBaseAPISetImage2(IntPtr handle, IntPtr pix);

    [DllImport("libtesseract-5.dll")]
    private static extern IntPtr TessBaseAPIGetUTF8Text(IntPtr handle);

    [DllImport("libtesseract-5.dll")]
    private static extern void TessDeleteText(IntPtr text);

    [DllImport("libtesseract-5.dll")]
    private static extern void TessBaseAPIEnd(IntPtr handle);

    [DllImport("libtesseract-5.dll")]
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