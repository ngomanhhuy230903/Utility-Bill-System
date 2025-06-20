using UtilityBill.Business.DTOs;
using UtilityBill.Business.Services;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UtilityBill.Business.Services
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Room>> GetAllRoomsAsync()
        {
            return await _unitOfWork.RoomRepository.GetAllAsync();
        }

        public async Task<Room?> GetRoomByIdAsync(int id)
        {
            return await _unitOfWork.RoomRepository.GetByIdAsync(id);
        }

        public async Task<Room> CreateRoomAsync(CreateRoomDto roomDto)
        {
            var room = new Room
            {
                RoomNumber = roomDto.RoomNumber,
                Block = roomDto.Block,
                Floor = roomDto.Floor,
                Area = roomDto.Area,
                Price = roomDto.Price,
                Status = "Vacant",
                CreatedAt = DateTime.UtcNow,
                QRCodeData = $"QR-ROOM-{roomDto.RoomNumber}-{Guid.NewGuid()}"
            };

            await _unitOfWork.RoomRepository.AddAsync(room);
            await _unitOfWork.SaveChangesAsync();

            return room;
        }

        public async Task<bool> UpdateRoomAsync(int roomId, CreateRoomDto roomDto)
        {
            var existingRoom = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);

            if (existingRoom == null)
            {
                return false; // Trả về false nếu không tìm thấy phòng
            }

            // Cập nhật các thuộc tính từ DTO
            existingRoom.RoomNumber = roomDto.RoomNumber;
            existingRoom.Block = roomDto.Block;
            existingRoom.Floor = roomDto.Floor;
            existingRoom.Area = roomDto.Area;
            existingRoom.Price = roomDto.Price;

            _unitOfWork.RoomRepository.Update(existingRoom);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRoomAsync(int id)
        {
            var room = await _unitOfWork.RoomRepository.GetByIdAsync(id);
            if (room == null)
            {
                return false; // Trả về false nếu không tìm thấy phòng
            }

            _unitOfWork.RoomRepository.Delete(room);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}