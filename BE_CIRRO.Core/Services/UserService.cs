using BE_CIRRO.Domain.IRepositories;
using BE_CIRRO.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Core.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
        => await _userRepository.GetAllAsync();

    public async Task<User?> GetUserByIdAsync(Guid id)
        => await _userRepository.GetByIdAsync(id);

    public async Task<User?> GetUserByUsernameAsync(string username)
        => await _userRepository.GetByUsernameAsync(username);

    public async Task<User> CreateUserAsync(User user)
    {
        user.UserId = Guid.NewGuid();
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        await _userRepository.AddAsync(user);
        return user;
    }

    public async Task<User?> UpdateUserAsync(Guid id, User updatedUser)
    {
        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null) return null;

        existingUser.Username = updatedUser.Username;
        existingUser.Password = updatedUser.Password;
        existingUser.Email = updatedUser.Email;
        existingUser.Role = updatedUser.Role;

        await _userRepository.UpdateAsync(existingUser);
        return existingUser;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        await _userRepository.DeleteAsync(user);
        return true;
    }
}
