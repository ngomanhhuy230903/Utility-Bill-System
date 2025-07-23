using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UtilityBill.Business.DTOs;
using UtilityBill.Business.Interfaces;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;

namespace UtilityBill.Business.Services
{
    public class MeterReadingService : IMeterReadingService
    {
        private readonly IMeterReadingRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public MeterReadingService(
           IMeterReadingRepository repo,
           IUnitOfWork uow,
           IMapper mapper)
        {
            _repo = repo;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MeterReadingReadDto>> GetMeterReadingsAsync(int? month = null, int? year = null)
        {
            var entities = await _repo.GetAllAsync();
            if (month.HasValue)
                entities = entities.Where(e => e.ReadingMonth == month.Value);
            if (year.HasValue)
                entities = entities.Where(e => e.ReadingYear == year.Value);
            return _mapper.Map<IEnumerable<MeterReadingReadDto>>(entities);
        }

        public async Task<MeterReadingReadDto?> GetMeterReadingByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity is null
                ? null
                : _mapper.Map<MeterReadingReadDto>(entity);
        }

        public async Task<MeterReadingReadDto> CreateMeterReadingAsync(MeterReadingCreateDto dto)
        {
            // 1. Ensure room exists
            var roomExists = await _uow.RoomRepository.GetByIdAsync(dto.RoomId);
            if (roomExists is null)
                throw new KeyNotFoundException($"Room with ID {dto.RoomId} not found.");

            // 2. Prevent duplicate reading for same room/month/year
            var already = await _repo.GetByRoomAndMonthAsync(dto.RoomId, dto.ReadingYear, dto.ReadingMonth);
            if (already is not null)
                throw new InvalidOperationException(
                    $"A reading for Room {dto.RoomId}, {dto.ReadingMonth}/{dto.ReadingYear} already exists.");

            // 3. Map, add, and save
            var entity = _mapper.Map<MeterReading>(dto);
            await _repo.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return _mapper.Map<MeterReadingReadDto>(entity);
        }

        public async Task<bool> UpdateMeterReadingAsync(
            int roomId,
            int readingYear,
            int readingMonth,
            MeterReadingUpdateDto dto)
        {
            // 1. Look up the existing reading by composite key
            var entity = await _repo.GetByRoomAndMonthAsync(roomId, readingYear, readingMonth);
            if (entity is null)
                return false;

            // 2. Update allowed fields
            entity.ElectricReading = dto.ElectricReading;
            entity.WaterReading = dto.WaterReading;
            entity.ReadingDate = dto.ReadingDate;
            entity.RecordedByUserId = dto.RecordedByUserId;

            // 3. Persist changes
            _repo.Update(entity);
            var changes = await _uow.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> DeleteMeterReadingAsync(
            int roomId,
            int readingYear,
            int readingMonth)
        {
            // 1. Find the entity
            var entity = await _repo.GetByRoomAndMonthAsync(roomId, readingYear, readingMonth);
            if (entity == null) return false;

            // 2. Delete
            _repo.Delete(entity);
            var changes = await _uow.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<(int successCount, int failCount)> BulkCreateMeterReadingsAsync(IList<MeterReadingUploadDto> dtos)
        {
            int success = 0, fail = 0;

            foreach (var upload in dtos)
            {
                // 1. Resolve RoomNumber → RoomId
                var room = await _uow.RoomRepository.GetByRoomNumberAsync(upload.RoomNumber);
                if (room == null)
                {
                    fail++;
                    continue;
                }

                // 2. Map to CreateDto
                var createDto = new MeterReadingCreateDto
                {
                    RoomId = room.Id,
                    ReadingMonth = upload.ReadingDate.Month,
                    ReadingYear = upload.ReadingDate.Year,
                    ElectricReading = upload.ElectricReading,
                    WaterReading = upload.WaterReading,
                    ReadingDate = upload.ReadingDate,
                    RecordedByUserId = "admin-user-guid"
                };

                try
                {
                    await CreateMeterReadingAsync(createDto);
                    success++;
                }
                catch
                {
                    // e.g. unique constraint violation
                    fail++;
                }
            }

            return (success, fail);
        }

        public async Task<MeterReadingReadDto?> GetByRoomAndPeriodAsync(int roomId, int year, int month)
        {
            var entity = await _repo.GetByRoomAndMonthAsync(roomId, year, month);
            return entity is null
                ? null
                : _mapper.Map<MeterReadingReadDto>(entity);
        }

        public async Task<IList<ConsumptionReportDto>> GetConsumptionReportAsync(DateTime from, DateTime to, string groupBy)
        {
            return await _repo.GetConsumptionReport(from, to, groupBy);
        }
    }
}
