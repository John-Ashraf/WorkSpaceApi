using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WorkSpaceApi.Data;
using WorkSpaceApi.DTOS;
using WorkSpaceApi.Helpers;
using WorkSpaceApi.Models;

namespace WorkSpaceApi.Services
{
    public class CheckInService : ICheckInService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;
        private readonly ApplicationDbContext _context;
        public CheckInService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<JWT> jwt, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _jwt = jwt.Value;
            _context = context;
        }
        public bool IsValidUser(string id)
        {
            var user=_userManager.FindByIdAsync(id);
            if(user==null) { return  false; }return true;
        }
        public async Task<CheckInResponse> AddCheckIn(CheckInRequest model)
        {
            var user = await _userManager.FindByIdAsync(model.userId); https://open.spotify.com/user/31itkh7gjboruedngz5u7uqteloe/collection
            if (user == null)
            {
                return new CheckInResponse {
                    Message = "Invalid UserId"
                };
            }
            if (model.HourPrice <= 0)
            {
                return new CheckInResponse
                {
                    Message = "Hour Price Should Be greater than zero"
                };
            }
            var checkIn = _context.CheckIns.SingleOrDefault(ch => ch.Appuser == user && ch.checkout == null);
            if (checkIn != null)
            {
                return new CheckInResponse
                {
                    Message = "No CheckIn valid now !",
                    startDate = checkIn.checkin,
                    hourcost = checkIn.HourPrice,

                };
            }
            CheckIns newcheckin = new CheckIns {
                checkin = DateTime.UtcNow,
                Name = model.Name + ' ' + (Guid.NewGuid().ToString()),
                HourPrice = model.HourPrice,
                Appuser = user,
                AppuserId = user.Id,
            };
            _context.CheckIns.Add(newcheckin);
            _context.SaveChanges();
            return new CheckInResponse { Message = "Done", startDate = newcheckin.checkin, Status = true, hourcost = newcheckin.HourPrice };
        }

        public async Task<string> AddOrder(AddOrderDto model)
        {
            if (model.Itemprice <= 0)
            {
                return "Invalid Price";
            }
            var checkin = await _context.CheckIns.SingleOrDefaultAsync(ch => ch.Id == model.checkinId);
            if (checkin == null)
            {
                return "Invalid CheckinId ";
            }
            var order = new Order { ItemName = model.ItemName, Itemprice = model.Itemprice, checkinId = checkin.Id };
            var res = await _context.Order.AddAsync(order);
            checkin.Orders.Add(order);
            await _context.SaveChangesAsync();
            return String.Empty;

        }

        public async Task<string> UpdateOrder(EditOrderDto model)
        {
            var order=await _context.Order.SingleOrDefaultAsync(O=>O.OrderId==model.OrderId);
            if(order == null)
            {
                return "Invalid OrderId";
            }
            if(model.ItemPrice <= 0)
            {
                return "Price must be greater than zero";
            }
            var isvalidCheckin=_context.CheckIns.Any(Ch=>Ch.Id==model.CheckInId);
            if (!isvalidCheckin)
            {
                return "Invalid CheckInId";
            }
            order.checkinId = model.CheckInId;
            order.ItemName = model.ItemName;
            order.Itemprice = model.ItemPrice;
            _context.SaveChanges();
            return String.Empty; 

        }
        public async Task<string> DeleteOrder(DeleteOrderRequest model)
        {
            var order = _context.Order.FirstOrDefault(O => O.checkinId == model.CheckInId && O.OrderId == model.OrderId);
            if (order == null)
            {
                return "Invalid CheckinId or OrderId ";
            }
            _context.Order.Remove(order);
            _context.SaveChanges();
            return String.Empty;
        }

        public async Task<List<CheckIns>> GetAll()
        {
            var checkins = await _context.CheckIns.Where(ch => ch.checkout == null).ToListAsync();
            return checkins;
        }
        public async Task<List<CheckIns>> GetAllForUser(string id)
        {
           
            var checkins=_context.CheckIns.Where(ch=>ch.AppuserId==id).ToList();
            return checkins;

        }
        public async Task<EditCheckInResultDto> Update(EditCheckinDto model)
        {
            if (model.HourPrice <= 0)
            {
                return new EditCheckInResultDto()
                {
                    Message = "Invalid Hours price"
                };  
            }
            var checkin = _context.CheckIns.Include(ch => ch.Orders).SingleOrDefault(ch => ch.Id == model.checkinId);
            if (checkin == null)
            {
                return new EditCheckInResultDto()
                {
                    Message = "Invalid CheckIn Id"
                };
            
            };
            var user = await _userManager.FindByIdAsync(model.userId);
            if (checkin.AppuserId != model.userId)
            {
                if(user == null)
                {
                    return new EditCheckInResultDto()
                    {
                        Message = "Invalid userId"
                    };
                }
                
                var checkintmp =await _context.CheckIns.SingleOrDefaultAsync(ch => ch.AppuserId == user.Id && ch.checkout == null);
                if (checkintmp != null)
                {
                    return new EditCheckInResultDto()
                    {
                        Message = $"this user has a checkin active right now {checkintmp.Name}"
                    };
                }
            }
            checkin.HourPrice=model.HourPrice;
            checkin.Name = model.Name + ' ' + (Guid.NewGuid().ToString());
            checkin.AppuserId=model.userId;
            _context.SaveChanges();
            return new EditCheckInResultDto()
            {
                Message = "Done!",
                status=true,
                HourPrice=model.HourPrice,
                Name=checkin.Name,
                UserId=model.userId
            };


        }

        public async Task<CheckIns>GetOrderById(int id)
        {
            var checkin=_context.CheckIns.SingleOrDefault(ch=>ch.Id==id);
            return checkin;
        }

        public async Task<string> DeleteById(int id)
        {
            var checkin=await _context.CheckIns.SingleOrDefaultAsync(ch=>ch.Id==id);
            if(checkin == null)
            {
                return "Invalid Id";
            }
            _context.CheckIns.Remove(checkin);
            _context.SaveChanges();
            return String.Empty ;

        }

        public async Task<CheckOutResultDto> Checkout(int id)
        {
            CheckOutResultDto result=new CheckOutResultDto();
            var checkin=await _context.CheckIns.Include(ch=>ch.Orders).Include(ch=>ch.Appuser).SingleOrDefaultAsync(ch=>ch.Id==id);
            if (checkin == null)
            {
                result.Message = "Invalid CheckIn id";
                return result;
            }
            if (checkin.checkout != null)
            {
                result.Message = "this is a old checkin";
                return result;
            }
            checkin.checkout = DateTime.UtcNow;
            var dffinHours = (checkin.checkout - checkin.checkin).Value.TotalHours;
            
            double sum = dffinHours*checkin.HourPrice;
            foreach (var order in checkin.Orders)
            {
                sum += order.Itemprice;
            }
            result.status = true;
            result.Message = "Done!";
            result.Amount=sum;
            result.CheckinID=checkin.Id;
            result.Username = checkin.Appuser.FirstName+' '+ checkin.Appuser.LastName;
            _context.SaveChanges();
            return result;
          
        }

    }
}
