using WorkSpaceApi.DTOS;
using WorkSpaceApi.Models;

namespace WorkSpaceApi.Services
{
    public interface ICheckInService
    {
        Task<CheckInResponse>AddCheckIn(CheckInRequest model);
        Task<string> AddOrder(AddOrderDto model);
        Task<string> UpdateOrder(EditOrderDto model);
        Task<string> DeleteOrder(DeleteOrderRequest model);
        bool IsValidUser(string userId);
        Task<List<CheckIns>> GetAll();
        Task<List<CheckIns>> GetAllForUser(string id);
        Task<CheckIns> GetOrderById(int id);
        Task<EditCheckInResultDto> Update(EditCheckinDto model);
        Task<CheckOutResultDto> Checkout(int id);
      
        Task<string> DeleteById(int id);
    }
}
