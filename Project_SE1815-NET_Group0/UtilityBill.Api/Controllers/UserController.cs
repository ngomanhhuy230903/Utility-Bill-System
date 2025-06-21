using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityBill.Data.Repositories;

namespace UtilityBill.Api.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public UsersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet] // GET: api/users
        public async Task<IActionResult> GetUsers()
        {
            var users = await _unitOfWork.UserRepository.GetAllAsync();
            // Trong thực tế, bạn nên trả về một UserDto thay vì User entity
            return Ok(users);
        }
    }
}