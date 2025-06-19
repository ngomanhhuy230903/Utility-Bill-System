using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityBill.Business.Services;
using UtilityBill.Data.Models;

namespace UtilityBill.Api.Controllers
{
    // [Authorize] // Bỏ comment dòng này để bắt buộc phải đăng nhập mới dùng được API này
    public class RoomsController : BaseApiController
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet] // GET: api/rooms
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return Ok(rooms);
        }

        [HttpGet("{id}")] // GET: api/rooms/101
        public async Task<ActionResult<Room>> GetRoom(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return Ok(room);
        }

        [HttpPost] // POST: api/rooms
        public async Task<ActionResult<Room>> CreateRoom(Room room)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _roomService.CreateRoomAsync(room);
            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
        }

        [HttpPut("{id}")] // PUT: api/rooms/101
        public async Task<IActionResult> UpdateRoom(int id, Room room)
        {
            if (id != room.Id)
            {
                return BadRequest();
            }
            await _roomService.UpdateRoomAsync(room);
            return NoContent(); // Trả về status 204 No Content khi thành công
        }

        [HttpDelete("{id}")] // DELETE: api/rooms/101
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            await _roomService.DeleteRoomAsync(id);
            return NoContent();
        }
    }
}