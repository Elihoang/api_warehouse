// ExcelImportHelper.cs

using BeWarehouseHub.Share.DTOs.Import;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

public static class ExcelImportHelper
{
    public static byte[] ExportReceiptToExcel(ImportReceiptDto receipt)
    {
        using var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("Phiếu nhập kho");

        var bold = workbook.CreateFont(); bold.IsBold = true;
        var titleFont = workbook.CreateFont(); titleFont.IsBold = true; titleFont.FontHeightInPoints = 18;

        var center = workbook.CreateCellStyle(); center.Alignment = HorizontalAlignment.Center;
        var boldCenter = workbook.CreateCellStyle(); boldCenter.CloneStyleFrom(center); boldCenter.SetFont(bold);

        var headerStyle = workbook.CreateCellStyle();
        headerStyle.FillForegroundColor = IndexedColors.LightGreen.Index;
        headerStyle.FillPattern = FillPattern.SolidForeground;
        headerStyle.SetFont(bold);
        headerStyle.Alignment = HorizontalAlignment.Center;

        int rowIdx = 0;

        // Tiêu đề
        var titleRow = sheet.CreateRow(rowIdx++);
        titleRow.HeightInPoints = 40;
        var titleCell = titleRow.CreateCell(0);
        titleCell.SetCellValue("PHIẾU NHẬP KHO");
        var ts = workbook.CreateCellStyle();
        ts.SetFont(titleFont); ts.Alignment = HorizontalAlignment.Center; ts.VerticalAlignment = VerticalAlignment.Center;
        titleCell.CellStyle = ts;
        sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

        rowIdx += 2;

        // Thông tin
        var info = sheet.CreateRow(rowIdx++);
        info.CreateCell(5).SetCellValue("Số phiếu:");
        info.CreateCell(6).SetCellValue(receipt.ImportId.ToString("N").ToUpper()[..8]);

        info = sheet.CreateRow(rowIdx++);
        info.CreateCell(0).SetCellValue("Ngày nhập:");
        info.CreateCell(1).SetCellValue(receipt.ImportDate.ToString("dd/MM/yyyy HH:mm"));
        info.CreateCell(5).SetCellValue("Kho:");
        info.CreateCell(6).SetCellValue(receipt.WarehouseName);

        info = sheet.CreateRow(rowIdx++);
        info.CreateCell(0).SetCellValue("Người nhập:");
        info.CreateCell(1).SetCellValue(receipt.UserName);

        rowIdx += 2;

        // Header bảng
        var header = sheet.CreateRow(rowIdx++);
        string[] cols = { "STT", "Mã SP", "Tên sản phẩm", "ĐVT", "SL", "Đơn giá", "Thành tiền" };
        sheet.SetColumnWidth(6, 20 * 256);
        for (int i = 0; i < cols.Length; i++)
        {
            var c = header.CreateCell(i);
            c.SetCellValue(cols[i]);
            c.CellStyle = headerStyle;
        }

        int stt = 1;
        decimal total = 0;
        foreach (var d in receipt.Details)
        {
            var row = sheet.CreateRow(rowIdx++);
            row.CreateCell(0).SetCellValue(stt++);
            row.CreateCell(1).SetCellValue(d.ProductId.ToString("N")[..8]);
            row.CreateCell(2).SetCellValue(d.ProductName);
            row.CreateCell(3).SetCellValue(d.Unit);
            row.CreateCell(4).SetCellValue(d.Quantity);
            row.CreateCell(5).SetCellValue((double)d.Price);
            row.CreateCell(6).SetCellValue((double)(d.Quantity * d.Price));
            total += d.Quantity * d.Price;
        }

        var totalRow = sheet.CreateRow(rowIdx++);
        totalRow.CreateCell(4).SetCellValue("TỔNG CỘNG:");
        totalRow.CreateCell(6).SetCellValue((double)total);
        for (int i = 4; i <= 6; i++)
            totalRow.GetCell(i, MissingCellPolicy.CREATE_NULL_AS_BLANK).CellStyle = boldCenter;

        using var ms = new MemoryStream();
        workbook.Write(ms);
        return ms.ToArray();
    }
}