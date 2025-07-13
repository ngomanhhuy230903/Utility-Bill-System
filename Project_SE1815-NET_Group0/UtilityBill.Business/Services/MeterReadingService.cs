using System;
using System.Collections.Generic;

using System.Text;
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
            var entity = _mapper.Map<MeterReading>(dto);
            await _repo.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return _mapper.Map<MeterReadingReadDto>(entity);
        }

        public async Task<bool> UpdateMeterReadingAsync(int id, MeterReadingCreateDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity is null) return false;

            _mapper.Map(dto, entity);
            _repo.Update(entity);
            var count = await _uow.SaveChangesAsync();
            return count > 0;
        }

        public async Task<bool> DeleteMeterReadingAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity is null) return false;

            _repo.Delete(entity);
            var count = await _uow.SaveChangesAsync();
            return count > 0;
        }
    }

}
