using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Media.Imaging;
using Avalonia.Input;
using System.Text.RegularExpressions;
using System.IO;
using System;
using System.Threading.Tasks;
using Avalonia.Input.Platform;
using Avalonia.Media;

namespace TransactionOCR;

public partial class MainWindow : Window
{
    private string? _imagePath;
    public MainWindow()
    {
        InitializeComponent();
        SetupDragDrop();

        if (OperatingSystem.IsLinux())
        {
            System.Runtime.InteropServices.NativeLibrary.Load("/usr/lib/libleptonica-1.82.0.so");
            System.Runtime.InteropServices.NativeLibrary.Load("/usr/lib/libtesseract-5.so");
        }
    }

    public string GetTessdataPath()
    {
        if (OperatingSystem.IsWindows())
        {
            return "./tessdata";
        }
        else
        {
            return "/usr/share/tessdata";
        }
    }

    private void SetupDragDrop()
    {
        DragDrop.SetAllowDrop(DropZone, true);
        DropZone.AddHandler(DragDrop.DropEvent, OnFileDrop);
    }

    private void OnFileDrop(object? sender, DragEventArgs e)
    {
        
        var files = e.DataTransfer.TryGetFiles();
        if (files == null) return;

        foreach (var file in files)
        {
            var path = file.Path.LocalPath;

            if (IsImageFile(path))
            {
                LoadImage(path);
            }
            break;
        }
    }

    private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Transaction Image",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Images")
                {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" }
                }
            }
        });

        if (files.Count == 0) return;

        var path = files[0].Path.LocalPath;
        LoadImage(path);
    }

    private void LoadImage(string path)
    {
        _imagePath = path;

        var bitmap = new Bitmap(path);
        PreviewImage.Source = bitmap;

        PreviewImage.IsVisible = true;
        ImagePlaceholder.IsVisible = false;

        ClearButton.IsVisible = true;

        OcrOutputBox.Text = "Running OCR...";
        AmountResult.Text = "--";
        AmountResult.Foreground = Avalonia.Media.Brushes.Gray;

        RunOcr();
    }

    private void RunOcr()
    {
        if (_imagePath == null) return;

        try
        {
            var ocrText = TesseractApi.OcrImage(_imagePath);
            OcrOutputBox.Text = ocrText;
            FilterData(ocrText);
        }
        catch (Exception ex)
        {
            var error = ex;
            while (error.InnerException != null)
                error = error.InnerException;

            OcrOutputBox.Text = $"Error: {error.Message}\n\nFull: {ex}";
        }
    }

    private void FilterData(string text)
    {
        var amountMatch = Regex.Match(text, @"\d{1,3}(?:,\d{3})*\.\d{2}|\d+\.\d{2}");
        var transactionId = Regex.Match(text, "BLA.+");
        var transactionDate = Regex.Match(text, "Transaction date.+");

        if (amountMatch.Success)
        {
            AmountResult.Text = amountMatch.Value;

            AmountResult.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0F6E56"));
        }
        else
        {
            AmountResult.Text = "Not Found";
            
            AmountResult.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A32D2D"));
        }

        if (transactionId.Success)
        {
            TransactionID.Text = transactionId.Value;
            TransactionID.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0f6e56"));
        }
        else
        {
            TransactionID.Text = "Not Found";
            TransactionID.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A32D2D"));
        }

        if (transactionDate.Success)
        {
            TransactionDate.Text = transactionDate.Value.Replace("Transaction date ", "");
            TransactionDate.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0f6e56"));
        }
        else
        {
            TransactionDate.Text = "Not Found";
            TransactionDate.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A32D2D"));
        }
    }

    private async Task CopyToClipboard(string? text)
    {
        if (string.IsNullOrEmpty(text) || text == "—" || text == "Not Found") return;

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
            await clipboard.SetTextAsync(text);
    }
        
    private async void CopyAmount(object? sender, RoutedEventArgs e)
    {
        await CopyToClipboard(AmountResult.Text);
    }

    private async void CopyTransactionID(object? sender, RoutedEventArgs e)
    {
        await CopyToClipboard(TransactionID.Text);
    }

    private async void CopyTransactionDate(object? sender, RoutedEventArgs e)
    {
        await CopyToClipboard(TransactionDate.Text);
    }

    private void ClearButton_Click(object? sender, RoutedEventArgs e)
    {
        _imagePath = null;

        OcrOutputBox.Text = string.Empty;
        AmountResult.Text = "—";
        AmountResult.Foreground = Avalonia.Media.Brushes.Gray;
        TransactionID.Text = "—";
        TransactionID.Foreground = Avalonia.Media.Brushes.Gray;
        TransactionDate.Text = "—";
        TransactionDate.Foreground = Avalonia.Media.Brushes.Gray;

        PreviewImage.Source = null;
        PreviewImage.IsVisible = false;
        ImagePlaceholder.IsVisible = true;

        ClearButton.IsVisible = false;
    }

    private bool IsImageFile(string path)
    {
        var ext = Path.GetExtension(path).ToLower();
        return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp";
    }
}