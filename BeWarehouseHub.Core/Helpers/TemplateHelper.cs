using System.Globalization;
using System.Text;

namespace BeWarehouseHub.Core.Helpers;

public static class TemplateHelper
{
    /// <summary>
    /// Đọc template HTML từ file
    /// </summary>
    public static async Task<string> ReadTemplateAsync(string templatePath)
    {
        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Không tìm thấy template: {templatePath}");
        
        return await File.ReadAllTextAsync(templatePath, Encoding.UTF8);
    }

    /// <summary>
    /// Thay thế placeholder {{key}} bằng value
    /// </summary>
    public static string ReplacePlaceholder(string template, string key, string value)
    {
        return template.Replace($"{{{{{key}}}}}", value ?? "");
    }

    /// <summary>
    /// Thay thế nhiều placeholders
    /// </summary>
    public static string ReplacePlaceholders(string template, Dictionary<string, string> replacements)
    {
        foreach (var kvp in replacements)
        {
            template = ReplacePlaceholder(template, kvp.Key, kvp.Value);
        }
        return template;
    }

    /// <summary>
    /// Format số tiền theo định dạng Việt Nam (1.000.000)
    /// </summary>
    public static string FormatCurrency(decimal amount)
    {
        return amount.ToString("#,##0", new CultureInfo("vi-VN"));
    }

    /// <summary>
    /// Convert số thành chữ (Tiếng Việt)
    /// </summary>
    public static string NumberToWords(decimal number)
    {
        if (number == 0) return "Không đồng";

        string[] ones = { "", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
        string[] tens = { "", "mười", "hai mươi", "ba mươi", "bốn mươi", "năm mươi", "sáu mươi", "bảy mươi", "tám mươi", "chín mươi" };
        string[] scales = { "", "nghìn", "triệu", "tỷ" };

        if (number < 0) return "Số âm không hợp lệ";

        long intPart = (long)number;
        var result = new StringBuilder();

        if (intPart == 0)
        {
            return "Không đồng";
        }

        int scaleIndex = 0;
        while (intPart > 0)
        {
            int threeDigits = (int)(intPart % 1000);
            if (threeDigits > 0)
            {
                string chunk = ConvertThreeDigits(threeDigits, ones, tens);
                if (scaleIndex > 0)
                    chunk += " " + scales[scaleIndex];
                
                if (result.Length > 0)
                    result.Insert(0, chunk + " ");
                else
                    result.Insert(0, chunk);
            }
            intPart /= 1000;
            scaleIndex++;
        }

        // Capitalize first letter
        if (result.Length > 0)
        {
            result[0] = char.ToUpper(result[0]);
        }

        return result.ToString().Trim() + " đồng chẵn";
    }

    private static string ConvertThreeDigits(int number, string[] ones, string[] tens)
    {
        var result = new StringBuilder();

        int hundreds = number / 100;
        int remainder = number % 100;
        int tensDigit = remainder / 10;
        int onesDigit = remainder % 10;

        // Hàng trăm
        if (hundreds > 0)
        {
            result.Append(ones[hundreds] + " trăm");
            if (remainder > 0)
                result.Append(" ");
        }

        // Hàng chục
        if (tensDigit > 1)
        {
            result.Append(tens[tensDigit]);
            if (onesDigit > 0)
            {
                result.Append(" ");
                if (onesDigit == 1)
                    result.Append("mốt");
                else if (onesDigit == 5 && tensDigit > 0)
                    result.Append("lăm");
                else
                    result.Append(ones[onesDigit]);
            }
        }
        else if (tensDigit == 1)
        {
            result.Append("mười");
            if (onesDigit > 0)
            {
                result.Append(" ");
                if (onesDigit == 5)
                    result.Append("lăm");
                else
                    result.Append(ones[onesDigit]);
            }
        }
        else if (onesDigit > 0)
        {
            if (hundreds > 0)
                result.Append("lẻ ");
            result.Append(ones[onesDigit]);
        }

        return result.ToString();
    }

    /// <summary>
    /// Tạo HTML rows cho bảng sản phẩm
    /// </summary>
    public static string GenerateProductRows<T>(
        IEnumerable<T> items,
        Func<T, int, string> rowGenerator)
    {
        var sb = new StringBuilder();
        int index = 1;
        foreach (var item in items)
        {
            sb.AppendLine(rowGenerator(item, index));
            index++;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Generate short ID from Guid (8 ký tự đầu)
    /// </summary>
    public static string GetShortId(Guid id)
    {
        return id.ToString("N")[..8].ToUpper();
    }
}
