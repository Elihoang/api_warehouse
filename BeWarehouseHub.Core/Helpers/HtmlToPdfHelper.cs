using SelectPdf;

namespace BeWarehouseHub.Core.Helpers;

/// <summary>
/// Helper để convert HTML sang PDF
/// Sử dụng SelectPdf (free library)
/// </summary>
public static class HtmlToPdfHelper
{
    /// <summary>
    /// Convert HTML string to PDF bytes
    /// </summary>
    public static byte[] ConvertHtmlToPdf(string htmlContent)
    {
        // Tạo converter với cấu hình tối ưu cho tiếng Việt
        var converter = new HtmlToPdf();
        
        // Cấu hình page
        converter.Options.PdfPageSize = PdfPageSize.A4;
        converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
        converter.Options.MarginTop = 10;
        converter.Options.MarginBottom = 10;
        converter.Options.MarginLeft = 10;
        converter.Options.MarginRight = 10;
        
        // Cấu hình font để hiển thị tiếng Việt đúng
        converter.Options.EmbedFonts = true;
        converter.Options.DisplayHeader = false;
        converter.Options.DisplayFooter = false;
        
        // Convert HTML to PDF
        var pdfDocument = converter.ConvertHtmlString(htmlContent);
        
        // Convert to byte array
        byte[] pdfBytes = pdfDocument.Save();
        
        // Close document
        pdfDocument.Close();
        
        return pdfBytes;
    }

    /// <summary>
    /// Convert HTML string to PDF bytes với custom options
    /// </summary>
    public static byte[] ConvertHtmlToPdf(string htmlContent, Action<HtmlToPdfOptions> configureOptions)
    {
        var converter = new HtmlToPdf();
        
        // Apply default config
        converter.Options.PdfPageSize = PdfPageSize.A4;
        converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
        converter.Options.EmbedFonts = true;
        
        // Apply custom config
        configureOptions?.Invoke(converter.Options);
        
        var pdfDocument = converter.ConvertHtmlString(htmlContent);
        byte[] pdfBytes = pdfDocument.Save();
        pdfDocument.Close();
        
        return pdfBytes;
    }
}
