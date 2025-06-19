using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;

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

        public async Task CreateRoomAsync(Room room)
        {
            // Đây là một ví dụ về business logic:
            // Khi tạo mới một phòng, trạng thái phải là "Vacant" (còn trống).
            if (string.IsNullOrEmpty(room.Status))
            {
                room.Status = "Vacant";
            }

            await _unitOfWork.RoomRepository.AddAsync(room);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateRoomAsync(Room room)
        {
            _unitOfWork.RoomRepository.Update(room);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteRoomAsync(int id)
        {
            var room = await _unitOfWork.RoomRepository.GetByIdAsync(id);
            if (room != null)
            {
                _unitOfWork.RoomRepository.Delete(room);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}