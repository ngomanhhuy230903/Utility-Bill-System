using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using UtilityBill.Business.DTOs;
using UtilityBill.Data.Models;

namespace UtilityBill.Business.MappingProfile
{
    internal class MeterReadingProfile : Profile
    {
        public MeterReadingProfile()
        {
            CreateMap<MeterReading, MeterReadingReadDto>();
            CreateMap<MeterReadingCreateDto, MeterReading>();
        }
    }
}
