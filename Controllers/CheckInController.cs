using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WorkSpaceApi.DTOS;
using WorkSpaceApi.Services;
using Twilio;
using System.Threading.Tasks;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Voice.V1.DialingPermissions;
using Microsoft.AspNetCore.Authorization;

namespace WorkSpaceApi.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CheckInController : ControllerBase
    {
        private readonly ICheckInService _CheckInService;
        private readonly ISMSService _SMSService;
        public CheckInController(ICheckInService CheckInService,ISMSService sMSService)
        {
            _CheckInService = CheckInService;
            _SMSService = sMSService;

        }
        
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var checkins= await _CheckInService.GetAll();
            return Ok(checkins);

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var checkin = await _CheckInService.GetOrderById(id);
            if(checkin == null) {
                return BadRequest("Invalid Id");
            }
            return Ok(checkin);

        }
        [HttpGet("History/{id}")]
        public async Task<IActionResult> GetHistoryCheckIns(int id)
        {
            
            if (_CheckInService.IsValidUser(id.ToString()))
            {
                var history = await _CheckInService.GetAllForUser(id.ToString());
                return Ok(history);
            }
              return BadRequest("Invalid userId");

        }
        [HttpPut("Edit")]
        public async Task<IActionResult> EditCheckIn([FromBody]EditCheckinDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var res=await _CheckInService.Update(model);
            if (!res.status)
            {
                return BadRequest(res.Message);
            }
            return Ok(res);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult>DeleteById(int id)
        {
            var res=await _CheckInService.DeleteById(id);
            if (!res.IsNullOrEmpty())
            {
                return BadRequest(res.ToString());  
            }
            return Ok();
        }
        [HttpPost("Add")]
        public async Task<IActionResult>AddCheckin(CheckInRequest model)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result=await _CheckInService.AddCheckIn(model);
            if (!result.Status)
            {
                return BadRequest(result);
            }
            return Ok(result);
            
        }
        [HttpPost("AddOrder")]
        public async Task<IActionResult> AddOrder(AddOrderDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _CheckInService.AddOrder(model);
            if (!String.IsNullOrEmpty(result))
            {
                return BadRequest(result);
            }
            return Ok(result);

        }
        [HttpPut("EditOrder")]
        public async Task<IActionResult>EditOrder(EditOrderDto model)
        {
            if(!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            string res=await _CheckInService.UpdateOrder(model);
            if(!String.IsNullOrEmpty(res))
            {
                return BadRequest(res);
            }
            return Ok();
        }
        [HttpPost("DeleteOrder")]
        public async Task<IActionResult> DeleteOrder(DeleteOrderRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _CheckInService.DeleteOrder(model);
            if (!String.IsNullOrEmpty(result))
            {
                return BadRequest(result);
            }
            return Ok(result);

        }

        [HttpGet("CheckOut/{id}")]
        public async Task<IActionResult>CheckOut(int id)
        {
            var result=await _CheckInService.Checkout(id);
            if(!result.status)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
        [HttpPost("Send")]
        public async Task<IActionResult> Send([FromBody]SendSmsDto sms)
        {
            var result = _SMSService.Send(sms.MobilePhone, sms.Body);
            if(!String.IsNullOrEmpty(result.ErrorMessage))
            {
                return BadRequest(result.ErrorMessage);
            }
            return Ok();

        }

    }
}
