using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityBill.Business.DTOs;
using UtilityBill.Data.Repositories;

namespace UtilityBill.Api.Controllers
{
    //[Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public UsersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet] // GET: api/users
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _unitOfWork.UserRepository.GetAllAsync();

            // Chuyển đổi từ List<User> (Entity) sang List<UserDto>
            // Đây là bước quan trọng nhất để sửa lỗi font
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                FullName = u.FullName,
                Email = u.Email
            }).ToList();

            return Ok(userDtos);
        }
    }
}