using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityBill.Business.DTOs;
using UtilityBill.Business.Services;
using UtilityBill.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using UtilityBill.Data.Repositories;

namespace UtilityBill.Api.Controllers
{
    //[Authorize]
    public class RoomsController : BaseApiController
    {
        private readonly IRoomService _roomService;
        private readonly IUnitOfWork _unitOfWork;
        public RoomsController(IRoomService roomService, IUnitOfWork unitOfWork)
        {
            _roomService = roomService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet] // GET: api/rooms
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return Ok(rooms);
        }

        [HttpGet("{id}")] // GET: api/rooms/{id}
        public async Task<ActionResult<Room>> GetRoom(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound(); // 404 Not Found
            }
            return Ok(room);
        }

        [HttpPost] // POST: api/rooms
        public async Task<ActionResult<Room>> CreateRoom(CreateRoomDto roomDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var newRoom = await _roomService.CreateRoomAsync(roomDto);
            // Trả về response 201 Created cùng với location và đối tượng vừa tạo
            return CreatedAtAction(nameof(GetRoom), new { id = newRoom.Id }, newRoom);
        }

        [HttpPut("{id}")] // PUT: api/rooms/{id}
        public async Task<IActionResult> UpdateRoom(int id, CreateRoomDto roomDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _roomService.UpdateRoomAsync(id, roomDto);
            if (!result)
            {
                return NotFound(); // Không tìm thấy phòng để cập nhật
            }

            return NoContent(); // 204 No Content: Thành công nhưng không có body trả về
        }

        [HttpDelete("{id}")] // DELETE: api/rooms/{id}
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var result = await _roomService.DeleteRoomAsync(id);
            if (!result)
            {
                return NotFound(); // Không tìm thấy phòng để xóa
            }
            return NoContent(); // 204 No Content
        }
        // Thêm 2 action này vào trong class RoomsController

        [HttpPost("{roomId}/assign-tenant")]
        public async Task<IActionResult> AssignTenant(int roomId, AssignTenantDto assignDto)
        {
            var history = await _roomService.AssignTenantAsync(roomId, assignDto);
            if (history == null)
            {
                return BadRequest("Không thể gán phòng. Phòng không tồn tại, đã có người ở, hoặc ID khách thuê không hợp lệ.");
            }
            return Ok(history);
        }

        [HttpPost("{roomId}/unassign-tenant")]
        public async Task<IActionResult> UnassignTenant(int roomId)
        {
            var success = await _roomService.UnassignTenantAsync(roomId);
            if (!success)
            {
                return BadRequest("Không thể hủy gán. Phòng không tồn tại hoặc không có người ở.");
            }
            return NoContent(); // Thành công
        }
        // Thêm action này vào RoomsController.cs

        [HttpGet("{roomId}/history")]
        public async Task<IActionResult> GetRoomHistory(int roomId)
        {
            var historiesFromDb = await _unitOfWork.TenantHistoryRepository.GetHistoriesByRoomIdAsync(roomId);

            // Map từ List<TenantHistory> sang List<TenantHistoryDto>
            var historyDtos = historiesFromDb.Select(h => new TenantHistoryDto
            {
                Id = h.Id,
                RoomId = h.RoomId,
                TenantId = h.TenantId,
                // Kiểm tra null cho Tenant để tránh lỗi
                TenantName = h.Tenant?.FullName ?? "N/A",
                MoveInDate = h.MoveInDate,
                MoveOutDate = h.MoveOutDate
            }).ToList();

            return Ok(historyDtos);
        }
    }
}