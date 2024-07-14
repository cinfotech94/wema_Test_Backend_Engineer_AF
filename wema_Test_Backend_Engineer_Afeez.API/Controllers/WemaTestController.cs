using Microsoft.AspNetCore.Mvc;
using wema_Test_Backend_Engineer_Afeez.Domain.DTO;
using wema_Test_Backend_Engineer_Afeez.Domain.Enums;
using wema_Test_Backend_Engineer_Afeez.Services.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace wema_Test_Backend_Engineer_Afeez.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WemaTestController : ControllerBase
    {
        private readonly IWemaTestMain _wemaTestMain;

        public WemaTestController(IWemaTestMain wemaTestMain)
        {
            _wemaTestMain = wemaTestMain;
        }

        [HttpPost("sendOtp")]
        public async Task<IActionResult> SendOtp(string phoneNo)
        {
            string otpResponse = await _wemaTestMain.SendOtp(phoneNo, nameof(WemaTestController));
            return Ok(otpResponse);
        }

        [HttpPost("onboardCustomer")]
        public async Task<IActionResult> OnboardCustomer([FromBody] CustomerRequest customerRequest)
        {
            string onboardResponse = await _wemaTestMain.OnboardNewCustomer(customerRequest, nameof(WemaTestController));
            return Ok(onboardResponse);
        }

        [HttpGet("getAllOnboardCustomers")]
        public async Task<IActionResult> GetAllOnboardCustomers()
        {
            IEnumerable<CustomerResponse> customerResponses = await _wemaTestMain.GetOnboardCustomer(nameof(WemaTestController));
            return Ok(customerResponses);
        }

        [HttpGet("getAllExistingBanks")]
        public async Task<IActionResult> GetAllExistingBanks()
        {
            IEnumerable<BankResponse> bankResponses = await _wemaTestMain.GetexistingBank(nameof(WemaTestController));
            return Ok(bankResponses);
        }

        [HttpGet("getAllLogs")]
        public async Task<IActionResult> GetAllLogs(LogStatus logStatus)
        {
            IEnumerable<LogEntryResponse> logResponses = await _wemaTestMain.GetLogs(logStatus, nameof(WemaTestController));
            return Ok(logResponses);
        }
    }
}
