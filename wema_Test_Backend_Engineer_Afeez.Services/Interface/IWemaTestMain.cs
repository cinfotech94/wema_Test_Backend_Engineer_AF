using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wema_Test_Backend_Engineer_Afeez.Domain.DTO;
using wema_Test_Backend_Engineer_Afeez.Domain.Enums;

namespace wema_Test_Backend_Engineer_Afeez.Services.Interface
{
    public interface IWemaTestMain
    {
        Task<string> SendOtp(string otp, string number, string controler);
        Task<IEnumerable<CustomerResponse>> GetOnboardCustomer(string controler);
        //CustomerResponse GetOnboardCustomer(int id, string controler);
        Task<List<BankResponse>> GetexistingBank(string controler);
        Task<IEnumerable<LogEntryResponse>> GetLogs(LogStatus type, string controler);
    }
}
