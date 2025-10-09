using BE_CIRRO.Core.Services;
using BE_CIRRO.Domain.Models;
using BE_CIRRO.Shared.Common;
using BE_CIRRO.Shared.DTOs;
using BE_CIRRO.Shared.DTOs.User;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BE_CIRRO.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IMapper _mapper;

    public UserController(UserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    // GET: api/User
    [HttpGet]
    [SwaggerOperation("Lấy danh sách tất cả User")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
        return Ok(ApiResponseFactory.Success(userDtos, "Lấy danh sách User thành công"));
    }

    //GET: api/User/{id}
    [HttpGet("{id}")]
    [SwaggerOperation("Lấy thông tin một User theo ID")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponseFactory.NotFound<UserDto>());

        var userDto = _mapper.Map<UserDto>(user);
        return Ok(ApiResponseFactory.Success<UserDto>(userDto));
    }

    //POST: api/User
    [HttpPost]
    [SwaggerOperation("Thêm một User")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = _mapper.Map<User>(dto);
        var created = await _userService.CreateUserAsync(user);

        var userDto = _mapper.Map<UserDto>(created);
        return StatusCode(StatusCodes.Status201Created, ApiResponseFactory.Created<UserDto>(userDto));
    }

    //PATCH: api/Khoa/{id}
    [HttpPatch("{id}")]
    [SwaggerOperation("Cập nhật thông tin một User")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var updatedEntity = _mapper.Map<User>(dto);
        var result = await _userService.UpdateUserAsync(id, updatedEntity);

        if (result == null)
            return NotFound(new { message = "User not found" });

        var userDto = _mapper.Map<UserDto>(result);
        return StatusCode(StatusCodes.Status200OK, ApiResponseFactory.Updated<UserDto>(userDto));
    }

    //DELETE: api/Khoa/{id}
    [HttpDelete("{id}")]
    [SwaggerOperation("Xóa một Khoa")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
                return StatusCode(StatusCodes.Status404NotFound, ApiResponseFactory.NotFound<UserDto>($"Không tìm thấy User với mã {id}", new { MaKhoa = id }));
            await _userService.DeleteUserAsync(id);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponseFactory.ServerError("Đã xảy ra lỗi khi xóa Khoa."));
        }

        return StatusCode(StatusCodes.Status200OK, ApiResponseFactory.Success($"Đã xóa User với mã {id} thành công."));
    }
}
