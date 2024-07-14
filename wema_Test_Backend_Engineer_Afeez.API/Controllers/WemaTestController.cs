using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wema_Test_Backend_Engineer_Afeez.Domain.DTO;
using wema_Test_Backend_Engineer_Afeez.Domain.Enums;
using wema_Test_Backend_Engineer_Afeez.Services.Interface;

namespace wema_Test_Backend_Engineer_Afeez.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WemaTestController : ControllerBase

    {
        private readonly IWemaTestMain _wematestMain;

        public WemaTestController(IWemaTestMain wematestMain)
        {
            _wematestMain = wematestMain;
        }


        [HttpPost(Name = "sendOtp")]
        public async Task<IActionResult> sendOtp(string phoneNo)
        {
            string otpResponses = await _wematestMain.SendOtp(phoneNo, "WemaTestController");
            return Ok(otpResponses);
        }

        [HttpPost(Name = "OnboardCustomer")]
        public async Task<IActionResult> OnboardCustomer(CustomerRequest customerRequest)
        {
            string onboardResponses = await _wematestMain.OnboardNewCustomer(customerRequest, "WemaTestController");
            return Ok(onboardResponses);
        }
        [HttpGet(Name = "GettAllOnboardCustomer")]
        public async Task<IActionResult> GettAllOnboardCustomer()
        {
            IEnumerable<CustomerResponse> customerResponses = await _wematestMain.GetOnboardCustomer("WemaTestController");
            return Ok(customerResponses);
        }
        [HttpGet(Name = "GettAllExistingBank")]
        public async Task<IActionResult> GettAllExistingBank()
        {
            IEnumerable<BankResponse> customerResponses = await _wematestMain.GetexistingBank("WemaTestController");
            return Ok(customerResponses);
        }
        [HttpGet(Name = "GettAllExistingBank")]
        public async Task<IActionResult> GettAlllog(LogStatus logSTatus)
        {
            IEnumerable<LogEntryResponse> logResponses = await _wematestMain.GetLogs(logSTatus,"WemaTestController");
            return Ok(logResponses);
        }
    }
}
