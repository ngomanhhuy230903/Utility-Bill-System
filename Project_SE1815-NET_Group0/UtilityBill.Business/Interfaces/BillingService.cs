using Microsoft.Extensions.Logging;
using UtilityBill.Business.Interfaces;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;

namespace UtilityBill.Business.Services
{
    public class BillingService : IBillingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBillingConfigService _configService;
        private readonly ILogger<BillingService> _logger;

        public BillingService(IUnitOfWork unitOfWork, IBillingConfigService configService, ILogger<BillingService> logger)
        {
            _unitOfWork = unitOfWork;
            _configService = configService;
            _logger = logger;
        }

        public async Task GenerateInvoicesForPreviousMonthAsync()
        {
            _logger.LogInformation("Bắt đầu Job tạo hóa đơn hàng tháng...");

            // 1. Xác định kỳ cần tính hóa đơn (tháng trước)
            var today = DateTime.UtcNow;
            var previousMonthDate = today.AddMonths(-1);
            int invoiceYear = previousMonthDate.Year;
            int invoiceMonth = previousMonthDate.Month;

            // 2. Lấy tất cả các phòng đang có người ở
            var occupiedRooms = (await _unitOfWork.RoomRepository.GetAllAsync())
                                .Where(r => r.Status == "Occupied");

            foreach (var room in occupiedRooms)
            {
                _logger.LogInformation("Đang xử lý phòng: {RoomNumber}", room.RoomNumber);

                // 3. Kiểm tra xem hóa đơn đã tồn tại chưa
                if (await _unitOfWork.InvoiceRepository.CheckIfInvoiceExistsAsync(room.Id, invoiceYear, invoiceMonth))
                {
                    _logger.LogWarning("Hóa đơn cho phòng {RoomNumber} kỳ {Month}/{Year} đã tồn tại. Bỏ qua.", room.RoomNumber, invoiceMonth, invoiceYear);
                    continue;
                }

                // 4. Lấy chỉ số điện/nước của kỳ này và kỳ trước
                // Kỳ này là đầu tháng hiện tại (ví dụ: 1/7)
                // Kỳ trước là đầu tháng trước (ví dụ: 1/6)
                var currentReading = await _unitOfWork.MeterReadingRepository.GetByRoomAndMonthAsync(room.Id, today.Year, today.Month);
                var previousReading = await _unitOfWork.MeterReadingRepository.GetByRoomAndMonthAsync(room.Id, invoiceYear, invoiceMonth);

                if (currentReading == null || previousReading == null)
                {
                    _logger.LogError("Không tìm thấy đủ chỉ số điện/nước cho phòng {RoomNumber} để tính hóa đơn kỳ {Month}/{Year}", room.RoomNumber, invoiceMonth, invoiceYear);
                    continue;
                }

                // 5. Tính toán tiêu thụ và tiền
                decimal electricUsed = currentReading.ElectricReading - previousReading.ElectricReading;
                decimal waterUsed = currentReading.WaterReading - previousReading.WaterReading;

                decimal roomPrice = room.Price;
                decimal electricPrice = _configService.GetElectricPrice();
                decimal waterPrice = _configService.GetWaterPrice();

                decimal electricAmount = electricUsed * electricPrice;
                decimal waterAmount = waterUsed * waterPrice;
                decimal totalAmount = roomPrice + electricAmount + waterAmount;

                // 6. Tạo đối tượng Invoice và InvoiceDetail
                var invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    RoomId = room.Id,
                    InvoicePeriodMonth = invoiceMonth,
                    InvoicePeriodYear = invoiceYear,
                    CreatedAt = DateTime.UtcNow,
                    DueDate = new DateTime(today.Year, today.Month, 15), // Hạn chót là ngày 15
                    TotalAmount = totalAmount,
                    Status = "Pending"
                };

                invoice.InvoiceDetails.Add(new InvoiceDetail { Description = $"Tiền phòng tháng {invoiceMonth}/{invoiceYear}", Quantity = 1, UnitPrice = roomPrice, Amount = roomPrice });
                invoice.InvoiceDetails.Add(new InvoiceDetail { Description = $"Tiền điện ({electricUsed:N0} kWh)", Quantity = electricUsed, UnitPrice = electricPrice, Amount = electricAmount });
                invoice.InvoiceDetails.Add(new InvoiceDetail { Description = $"Tiền nước ({waterUsed:N0} m³)", Quantity = waterUsed, UnitPrice = waterPrice, Amount = waterAmount });

                await _unitOfWork.InvoiceRepository.AddAsync(invoice);
                _logger.LogInformation("Đã tạo hóa đơn {InvoiceId} cho phòng {RoomNumber} với tổng tiền {TotalAmount}", invoice.Id, room.RoomNumber, totalAmount);
            }

            // 7. Lưu tất cả các hóa đơn vừa tạo vào DB
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Job tạo hóa đơn hàng tháng hoàn tất.");
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId)
        {
            _logger.LogInformation("Getting invoice by ID: {InvoiceId}", invoiceId);
            
            var invoice = await _unitOfWork.InvoiceRepository.GetInvoiceWithDetailsAsync(invoiceId);
            
            if (invoice == null)
            {
                _logger.LogWarning("Invoice not found with ID: {InvoiceId}", invoiceId);
                return null;
            }
            
            _logger.LogInformation("Successfully retrieved invoice {InvoiceId} for room {RoomNumber}", invoice.Id, invoice.Room?.RoomNumber);
            return invoice;
        }
    }
}