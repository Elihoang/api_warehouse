using BeWarehouseHub.Share.DTOs.Export;
using BeWarehouseHub.Share.DTOs.Import;

public class ImportExportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public List<string>? Errors { get; set; }

    // Cho xuất kho
    public List<CreateExportDetailDto>? TempDetails { get; set; }

    // Cho nhập kho (mới thêm)
    public List<CreateImportDetailDto>? TempImportDetails { get; set; }

    // Kết quả sau khi tạo phiếu
    public Guid? ExportId { get; set; }
    public Guid? ImportId { get; set; }
    public int TotalItems { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
}