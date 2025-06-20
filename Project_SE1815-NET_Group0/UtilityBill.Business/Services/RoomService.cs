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
        public async Task<TenantHistoryDto?> AssignTenantAsync(int roomId, AssignTenantDto assignDto)
        {
            // 1. Kiểm tra các điều kiện (giữ nguyên như cũ)
            var room = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);
            if (room == null) return null;
            if (room.Status != "Vacant") return null;

            var tenant = await _unitOfWork.UserRepository.GetUserByIdAsync(assignDto.TenantId);
            if (tenant == null) return null;

            // 2. Tạo bản ghi lịch sử mới (giữ nguyên như cũ)
            var history = new TenantHistory
            {
                RoomId = roomId,
                TenantId = assignDto.TenantId,
                MoveInDate = DateOnly.FromDateTime(assignDto.MoveInDate),
                MoveOutDate = null
            };
            await _unitOfWork.TenantHistoryRepository.AddAsync(history);

            // 3. Cập nhật trạng thái phòng (giữ nguyên như cũ)
            room.Status = "Occupied";
            _unitOfWork.RoomRepository.Update(room);

            // 4. Lưu tất cả thay đổi vào DB (giữ nguyên như cũ)
            await _unitOfWork.SaveChangesAsync();

            // 5. TẠO VÀ TRẢ VỀ DTO, KHÔNG TRẢ VỀ ENTITY
            // Đây là bước sửa lỗi
            return new TenantHistoryDto
            {
                Id = history.Id,
                RoomId = room.Id,
                RoomNumber = room.RoomNumber,
                TenantId = tenant.Id,
                TenantName = tenant.FullName,
                MoveInDate = history.MoveInDate,
                MoveOutDate = history.MoveOutDate
            };
        }

        public async Task<bool> UnassignTenantAsync(int roomId)
        {
            // 1. Kiểm tra phòng
            var room = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);
            if (room == null || room.Status != "Occupied")
            {
                return false; // Phòng không tồn tại hoặc không có người ở
            }

            // 2. Tìm lịch sử thuê hiện tại của phòng
            var currentHistory = await _unitOfWork.TenantHistoryRepository.GetCurrentHistoryByRoomIdAsync(roomId);
            if (currentHistory == null)
            {
                return false; // Không tìm thấy ai đang ở phòng này (dữ liệu không nhất quán)
            }

            // 3. Ghi nhận ngày chuyển đi
            currentHistory.MoveOutDate = DateOnly.FromDateTime(DateTime.UtcNow);
            _unitOfWork.TenantHistoryRepository.Update(currentHistory);

            // 4. Cập nhật trạng thái phòng thành trống
            room.Status = "Vacant";
            _unitOfWork.RoomRepository.Update(room);

            // 5. Lưu thay đổi
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}