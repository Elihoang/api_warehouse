using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using BeWarehouseHub.Share.DTOs.Export;
using NPOI.SS.Util;

namespace BeWarehouseHub.Core.Helpers.Excel
{
    public static class ExcelExportHelper
    {
        public static byte[] ExportReceiptToExcel(ExportReceiptDto receipt)
        {
            using var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Phiếu xuất kho");

            var bold = workbook.CreateFont(); bold.IsBold = true; bold.FontHeightInPoints = 11;
            var titleFont = workbook.CreateFont(); titleFont.IsBold = true; titleFont.FontHeightInPoints = 18;

            var center = workbook.CreateCellStyle(); center.Alignment = HorizontalAlignment.Center;
            var boldCenter = workbook.CreateCellStyle(); boldCenter.CloneStyleFrom(center); boldCenter.SetFont(bold);

            var headerStyle = workbook.CreateCellStyle();
            headerStyle.FillForegroundColor = IndexedColors.LightOrange.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            headerStyle.SetFont(bold);
            headerStyle.Alignment = HorizontalAlignment.Center;

            int rowIdx = 0;

            // Tiêu đề
            var titleRow = sheet.CreateRow(rowIdx++);
            titleRow.HeightInPoints = 40;
            var titleCell = titleRow.CreateCell(0);
            titleCell.SetCellValue("PHIẾU XUẤT KHO");
            var ts = workbook.CreateCellStyle();
            ts.SetFont(titleFont);
            ts.Alignment = HorizontalAlignment.Center;
            ts.VerticalAlignment = VerticalAlignment.Center;
            titleCell.CellStyle = ts;
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

            rowIdx += 2;

            // Thông tin chung
            var info = sheet.CreateRow(rowIdx++);
            info.CreateCell(5).SetCellValue("Số phiếu:");
            info.CreateCell(6).SetCellValue(receipt.ExportId.ToString("N").ToUpper()[..8]);

            info = sheet.CreateRow(rowIdx++);
            info.CreateCell(0).SetCellValue("Ngày xuất:");
            info.CreateCell(1).SetCellValue(receipt.ExportDate.ToString("dd/MM/yyyy HH:mm"));
            info.CreateCell(5).SetCellValue("Kho:");
            info.CreateCell(6).SetCellValue(receipt.WarehouseName ?? "");

            info = sheet.CreateRow(rowIdx++);
            info.CreateCell(0).SetCellValue("Người xuất:");
            info.CreateCell(1).SetCellValue(receipt.UserName ?? "");

            rowIdx += 2;

            // Header bảng
            var header = sheet.CreateRow(rowIdx++);
            string[] cols = { "STT", "Mã SP", "Tên sản phẩm", "ĐVT", "SL", "Đơn giá", "Thành tiền" };
            for (int i = 0; i < cols.Length; i++)
            {
                var c = header.CreateCell(i);
                c.SetCellValue(cols[i]);
                c.CellStyle = headerStyle;
            }

            // Dòng dữ liệu
            int stt = 1;
            decimal total = 0;
            foreach (var d in receipt.Details)
            {
                var row = sheet.CreateRow(rowIdx++);
                row.CreateCell(0).SetCellValue(stt++);
                row.CreateCell(1).SetCellValue(d.ProductId.ToString("N")[..8]);
                row.CreateCell(2).SetCellValue(d.ProductName ?? "");
                row.CreateCell(3).SetCellValue(d.Unit ?? "");
                row.CreateCell(4).SetCellValue(d.Quantity);
                row.CreateCell(5).SetCellValue((double)d.Price);
                row.CreateCell(6).SetCellValue((double)(d.Quantity * d.Price));
                total += d.Quantity * d.Price;
            }

            // Tổng cộng (đã fix lỗi null)
            var totalRow = sheet.CreateRow(rowIdx++);
            totalRow.CreateCell(4).SetCellValue("TỔNG CỘNG:");
            totalRow.CreateCell(6).SetCellValue((double)total);
            for (int i = 4; i <= 6; i++)
            {
                var cell = totalRow.GetCell(i, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                cell.CellStyle = boldCenter;
            }

            rowIdx += 3;
            
            using var ms = new MemoryStream();
            workbook.Write(ms);
            return ms.ToArray();
        }
    }
}