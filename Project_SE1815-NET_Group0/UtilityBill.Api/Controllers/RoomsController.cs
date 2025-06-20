using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityBill.Business.DTOs;
using UtilityBill.Business.Services;
using UtilityBill.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UtilityBill.Api.Controllers
{
    //[Authorize]
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
    }
}