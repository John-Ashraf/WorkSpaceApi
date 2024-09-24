using Microsoft.AspNetCore.Mvc;
using WorkSpaceApi.DTOS;
using WorkSpaceApi.Models;

namespace WorkSpaceApi.Services
{
    public interface IAuth
    {
        Task<AuthModel> RegisterAsync(RegisterationModel model);
        Task<AuthModel> LoginAsync(LoginRequestModel model);
        Task<string> AddRole(AddRoleRequestModel model);
        Task<AuthModel> RefreshToken(string token);
        Task<bool> RevokeTokenAsync(string token);
    }
}
