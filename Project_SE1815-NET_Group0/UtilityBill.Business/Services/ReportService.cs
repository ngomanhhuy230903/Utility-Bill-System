using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityBill.Data.Repositories;

namespace UtilityBill.Business.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMeterReadingRepository _meterReadingRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IUnitOfWork unitOfWork, ILogger<ReportService> logger)
        {
            _unitOfWork = unitOfWork;
            _meterReadingRepository = unitOfWork.MeterReadingRepository;
            _paymentRepository = unitOfWork.PaymentRepository;
            _logger = logger;
        }

        public async Task<IList<ConsumptionReportDto>> GetConsumptionReport(DateTime from, DateTime to, string groupBy)
        {
            return await _meterReadingRepository.GetConsumptionReport(from, to, groupBy);
        }

        public async Task<IList<RevenueReportDto>> GetRevenueReport(DateTime from, DateTime to, string groupBy)
        {
            return await _paymentRepository.GetRevenueReport(from, to, groupBy);
        }
    }
}
