using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using BeWarehouseHub.Share.DTOs.Import;

public static class PdfImportHelper
{
    public static byte[] GenerateImportReceiptPdf(ImportReceiptDto receipt)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Times New Roman"));

                // ================= HEADER =================
                page.Header().Column(col =>
                {
                    col.Item()
                        .AlignCenter()
                        .Text("PHIẾU NHẬP KHO")
                        .FontSize(22)
                        .Bold()
                        .FontColor(Colors.Green.Darken2);

                    col.Item()
                        .AlignCenter()
                        .Text($"Số chứng từ: {receipt.ImportId.ToString()[..8].ToUpper()}");

                    col.Item()
                        .PaddingVertical(5)
                        .Row(row =>
                        {
                            row.RelativeItem().Text($"Ngày nhập: {receipt.ImportDate:dd/MM/yyyy HH:mm}");
                            row.RelativeItem().AlignRight().Text($"Kho nhập: {receipt.WarehouseName}");
                        });

                    col.Item().Text($"Người thực hiện: {receipt.UserName}");

                    col.Item()
                        .PaddingVertical(5)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Darken2);
                });

                // ================= CONTENT =================
                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item()
                        .Border(1)
                        .BorderColor(Colors.Grey.Lighten1)
                        .Background(Colors.Grey.Lighten4)
                        .Padding(15)
                        .CornerRadius(4)
                        .Column(box =>
                        {
                            box.Item().Table(table =>
                            {
                                // ---- COLUMNS ----
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40);   // STT
                                    columns.RelativeColumn(4);    // Tên SP
                                    columns.RelativeColumn(2);    // ĐVT
                                    columns.RelativeColumn(2);    // SL
                                    columns.RelativeColumn(3);    // Đơn giá
                                    columns.RelativeColumn(3);    // Thành tiền
                                });

                                // ---- HEADER ----
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("STT").Bold();
                                    header.Cell().Element(CellStyle).Text("Tên sản phẩm").Bold();
                                    header.Cell().Element(CellStyle).Text("ĐVT").AlignCenter().Bold();
                                    header.Cell().Element(CellStyle).Text("SL").AlignCenter().Bold();
                                    header.Cell().Element(CellStyle).Text("Đơn giá").AlignRight().Bold();
                                    header.Cell().Element(CellStyle).Text("Thành tiền").AlignRight().Bold();

                                    header.Cell()
                                        .ColumnSpan(6)
                                        .PaddingVertical(4)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Black);
                                });

                                // ---- ROWS ----
                                foreach (var (item, index) in receipt.Details.Select((x, i) => (x, i)))
                                {
                                    var amount = item.Quantity * item.Price;

                                    table.Cell().Element(CellStyle).Text((index + 1).ToString());
                                    table.Cell().Element(CellStyle).Text(item.ProductName);
                                    table.Cell().Element(CellStyle).AlignCenter().Text(item.Unit);
                                    table.Cell().Element(CellStyle).AlignCenter().Text(item.Quantity.ToString("N0"));
                                    table.Cell().Element(CellStyle).AlignRight().Text(item.Price.ToString("N0"));
                                    table.Cell().Element(CellStyle).AlignRight().Text(amount.ToString("N0"));
                                }

                                // ---- TOTAL ROW ----
                                table.Cell()
                                    .ColumnSpan(3)
                                    .Element(CellStyle)
                                    .AlignRight()
                                    .Text("Tổng cộng:")
                                    .Bold();

                                table.Cell()
                                    .Element(CellStyle)
                                    .AlignCenter()
                                    .Text(receipt.Details.Sum(x => x.Quantity).ToString("N0"))
                                    .Bold();

                                table.Cell()
                                    .Element(CellStyle)
                                    .AlignRight();

                                table.Cell()
                                    .Element(CellStyle)
                                    .AlignRight()
                                    .Text(receipt.Details.Sum(x => x.Quantity * x.Price).ToString("N0"))
                                    .Bold();
                            });
                        });
                });

                // ================= FOOTER =================
                page.Footer()
                    .AlignRight()
                    .Text($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);
            });
        }).GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .PaddingVertical(4)
            .PaddingHorizontal(3);
    }
}
