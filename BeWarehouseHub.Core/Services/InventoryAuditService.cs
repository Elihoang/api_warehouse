using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Core.Services;

public class InventoryAuditService
{
    private readonly IInventoryAuditRepository _auditRepository;
    private readonly IInventoryAuditDetailRepository _auditDetailRepository;
    private readonly IStockRepository _stockRepository;

    public InventoryAuditService(
        IInventoryAuditRepository auditRepository,
        IInventoryAuditDetailRepository auditDetailRepository,
        IStockRepository stockRepository)
    {
        _auditRepository = auditRepository;
        _auditDetailRepository = auditDetailRepository;
        _stockRepository = stockRepository;
    }

    public async Task<IEnumerable<InventoryAudit>> GetAllAsync()
        => await _auditRepository.GetAllAsync();

    public async Task<InventoryAudit?> GetByIdAsync(Guid id)
        => await _auditRepository.GetByIdAsync(id);

    public async Task<InventoryAudit?> GetAuditWithDetailsAsync(Guid auditId)
        => await _auditRepository.GetAuditWithDetailsAsync(auditId);

    public async Task<IEnumerable<InventoryAudit>> GetAuditsByWarehouseIdAsync(Guid warehouseId)
        => await _auditRepository.GetAuditsByWarehouseIdAsync(warehouseId);

    public async Task<IEnumerable<InventoryAudit>> GetAuditsByStatusAsync(string status)
        => await _auditRepository.GetAuditsByStatusAsync(status);

    public async Task<InventoryAudit> CreateAuditAsync(InventoryAudit audit)
    {
        // Kiểm tra audit code đã tồn tại chưa
        var existing = await _auditRepository.GetAuditByCodeAsync(audit.AuditCode);
        if (existing != null)
        {
            throw new InvalidOperationException($"Mã kiểm kê {audit.AuditCode} đã tồn tại.");
        }

        audit.AuditDate = DateTime.SpecifyKind(audit.AuditDate, DateTimeKind.Utc);
        audit.CreatedAt = DateTime.UtcNow;

        await _auditRepository.AddAsync(audit);
        return audit;
    }

    public async Task<InventoryAuditDetail> AddAuditDetailAsync(InventoryAuditDetail detail)
    {
        // Check if detail for this product already exists
        var existingDetails = await _auditDetailRepository.GetDetailsByAuditIdAsync(detail.AuditId);
        var existingDetail = existingDetails.FirstOrDefault(d => d.ProductId == detail.ProductId);

        if (existingDetail != null)
        {
            // Update existing detail
            existingDetail.ActualQuantity = detail.ActualQuantity;
            existingDetail.Variance = detail.ActualQuantity - existingDetail.SystemQuantity;
            existingDetail.Note = detail.Note;
            existingDetail.AuditedAt = DateTime.UtcNow;
            existingDetail.AuditedByUserId = detail.AuditedByUserId;

            await _auditDetailRepository.UpdateAsync(existingDetail);
            return existingDetail;
        }

        // Create new detail if not exists
        detail.Variance = detail.ActualQuantity - detail.SystemQuantity;
        detail.AuditedAt = DateTime.UtcNow;

        await _auditDetailRepository.AddAsync(detail);
        return detail;
    }

    public async Task<IEnumerable<InventoryAuditDetail>> GetAuditDetailsAsync(Guid auditId)
        => await _auditDetailRepository.GetDetailsByAuditIdAsync(auditId);

    public async Task<IEnumerable<InventoryAuditDetail>> GetDetailsWithVarianceAsync(Guid auditId)
        => await _auditDetailRepository.GetDetailsWithVarianceAsync(auditId);

    /// <summary>
    /// Hoàn thành kiểm kê và cập nhật tồn kho theo kết quả kiểm kê
    /// </summary>
    public async Task<InventoryAudit> CompleteAuditAsync(Guid auditId, bool updateStock = true)
    {
        var audit = await _auditRepository.GetAuditWithDetailsAsync(auditId);
        if (audit == null)
        {
            throw new InvalidOperationException("Không tìm thấy phiếu kiểm kê.");
        }

        if (audit.Status == "Completed")
        {
            throw new InvalidOperationException("Phiếu kiểm kê đã được hoàn thành.");
        }

        audit.Status = "Completed";
        audit.CompletedDate = DateTime.UtcNow;

        // Nếu updateStock = true, cập nhật tồn kho theo số lượng thực tế
        if (updateStock)
        {
            foreach (var detail in audit.InventoryAuditDetails)
            {
                if (detail.Variance != 0)
                {
                    var stock = await _stockRepository.GetByProductAndWarehouseAsync(
                        detail.ProductId, 
                        audit.WarehouseId);

                    if (stock != null)
                    {
                        stock.Quantity = detail.ActualQuantity;
                        await _stockRepository.UpdateAsync(stock);
                    }
                }
            }
        }

        await _auditRepository.UpdateAsync(audit);
        return audit;
    }

    /// <summary>
    /// Hủy kiểm kê
    /// </summary>
    public async Task CancelAuditAsync(Guid auditId)
    {
        var audit = await _auditRepository.GetByIdAsync(auditId);
        if (audit == null)
        {
            throw new InvalidOperationException("Không tìm thấy phiếu kiểm kê.");
        }

        if (audit.Status == "Completed")
        {
            throw new InvalidOperationException("Không thể hủy phiếu kiểm kê đã hoàn thành.");
        }

        audit.Status = "Cancelled";
        await _auditRepository.UpdateAsync(audit);
    }

    /// <summary>
    /// Tạo audit details tự động từ tồn kho hiện tại
    /// </summary>
    public async Task<List<InventoryAuditDetail>> GenerateAuditDetailsFromStockAsync(Guid auditId, Guid warehouseId)
    {
        var stocks = await _stockRepository.GetByWarehouseIdAsync(warehouseId);
        var details = new List<InventoryAuditDetail>();

        foreach (var stock in stocks)
        {
            var detail = new InventoryAuditDetail
            {
                AuditDetailId = Guid.NewGuid(),
                AuditId = auditId,
                InventoryAudit = null!, // Will be loaded by EF Core
                ProductId = stock.ProductId,
                Product = null!, // Will be loaded by EF Core
                SystemQuantity = stock.Quantity,
                ActualQuantity = 0, // Sẽ được nhập sau khi kiểm kê thực tế
                Variance = 0,
                AuditedAt = DateTime.UtcNow
            };

            await _auditDetailRepository.AddAsync(detail);
            details.Add(detail);
        }

        return details;
    }

    public async Task<string> GenerateHtmlAsync(Guid auditId, string webRootPath)
    {
        var audit = await _auditRepository.GetAuditWithDetailsAsync(auditId);
        if (audit == null)
            throw new KeyNotFoundException("Không tìm thấy phiếu kiểm kê");

        var templatePath = Path.Combine(webRootPath, "template", "phieu_kiem_ke.html");
        if (!File.Exists(templatePath))
            throw new FileNotFoundException("Không tìm thấy mẫu phiếu in");

        string templateContent = await File.ReadAllTextAsync(templatePath);

        // Thông tin công ty (Hardcode hoặc lấy từ cấu hình)
        var companyName = "CÔNG TY TNHH BE WAREHOUSE";
        var taxCode = "0101234567";
        var address = "Tầng 1, Tòa nhà ABC, 123 Đường XYZ, Quận 1, TP.HCM";
        var phone = "028 1234 5678";
        var email = "info@bewarehouse.com";

        // Thay thế thông tin chung
        templateContent = templateContent.Replace("{{CompanyName}}", companyName)
                                         .Replace("{{TaxCode}}", taxCode)
                                         .Replace("{{CompanyAddress}}", address)
                                         .Replace("{{PhoneNumber}}", phone)
                                         .Replace("{{Email}}", email)
                                         .Replace("{{InventoryNumber}}", audit.AuditCode)
                                         .Replace("{{Year}}", audit.AuditDate.Year.ToString())
                                         .Replace("{{Month}}", audit.AuditDate.Month.ToString("D2"))
                                         .Replace("{{Day}}", audit.AuditDate.Day.ToString("D2"))
                                         .Replace("{{Hour}}", audit.AuditDate.Hour.ToString("D2"))
                                         .Replace("{{Minute}}", audit.AuditDate.Minute.ToString("D2"));

        // Tạo các dòng chi tiết
        var rowsHtml = new System.Text.StringBuilder();
        int stt = 1;

        // Tính tổng cộng
        decimal totalSystemAmount = 0;
        decimal totalActualAmount = 0;
        decimal totalVarianceExcessAmount = 0;
        decimal totalVarianceShortageAmount = 0;

        foreach (var detail in audit.InventoryAuditDetails)
        {
            var price = detail.Product?.Price ?? 0;
            var systemAmount = detail.SystemQuantity * price;
            var actualAmount = detail.ActualQuantity * price;

            var variance = detail.Variance;
            var varianceAmount = Math.Abs(variance) * price;

            string excessQty = variance > 0 ? variance.ToString("N0") : "";
            string excessAmt = variance > 0 ? varianceAmount.ToString("N0") : "";

            string shortageQty = variance < 0 ? Math.Abs(variance).ToString("N0") : "";
            string shortageAmt = variance < 0 ? varianceAmount.ToString("N0") : "";

            totalSystemAmount += systemAmount;
            totalActualAmount += actualAmount;
            if (variance > 0) totalVarianceExcessAmount += varianceAmount;
            if (variance < 0) totalVarianceShortageAmount += varianceAmount;

            rowsHtml.Append("<tr>");
            rowsHtml.Append($"<td class='text-center'>{stt++}</td>");
            rowsHtml.Append($"<td class='text-left'>{detail.Product?.ProductName}</td>");
            rowsHtml.Append($"<td class='text-center'>{detail.Product?.ProductId.ToString().Substring(0, 8).ToUpper()}</td>"); // Lấy 8 ký tự đầu của GUID làm mã
            rowsHtml.Append($"<td class='text-center'>{detail.Product?.Unit}</td>");

            // Sổ kế toán
            rowsHtml.Append($"<td class='text-right'>{detail.SystemQuantity:N0}</td>");
            rowsHtml.Append($"<td class='text-right'>{systemAmount:N0}</td>");

            // Kiểm kê
            rowsHtml.Append($"<td class='text-right'>{detail.ActualQuantity:N0}</td>");
            rowsHtml.Append($"<td class='text-right'>{actualAmount:N0}</td>");

            // Chênh lệch
            rowsHtml.Append($"<td class='text-right'>{excessQty}</td>");
            rowsHtml.Append($"<td class='text-right'>{excessAmt}</td>");
            rowsHtml.Append($"<td class='text-right'>{shortageQty}</td>");
            rowsHtml.Append($"<td class='text-right'>{shortageAmt}</td>");

            rowsHtml.Append("</tr>");
        }

        // Dòng tổng cộng
        rowsHtml.Append("<tr style='font-weight:bold; background-color:#f0f0f0;'>");
        rowsHtml.Append("<td colspan='4' class='text-center'>Cộng</td>");
        rowsHtml.Append($"<td class='text-right'></td>");
        rowsHtml.Append($"<td class='text-right'>{totalSystemAmount:N0}</td>");
        rowsHtml.Append($"<td class='text-right'></td>");
        rowsHtml.Append($"<td class='text-right'>{totalActualAmount:N0}</td>");
        rowsHtml.Append($"<td class='text-right'></td>");
        rowsHtml.Append($"<td class='text-right'>{totalVarianceExcessAmount:N0}</td>");
        rowsHtml.Append($"<td class='text-right'></td>");
        rowsHtml.Append($"<td class='text-right'>{totalVarianceShortageAmount:N0}</td>");
        rowsHtml.Append("</tr>");

        templateContent = templateContent.Replace("{{InventoryRows}}", rowsHtml.ToString());

        return templateContent;
    }
}
