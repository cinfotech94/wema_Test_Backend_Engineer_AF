using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wema_Test_Backend_Engineer_Afeez.Domain.DTO
{
    public class BankResponse
    {
        public string bankName { get; set; }
        public string bankCode { get; set; }
    }

    public class BanksRoot
    {
        public List<BankResponse> result { get; set; }
        public string errorMessage { get; set; }
        public List<string> errorMessages { get; set; }
        public bool hasError { get; set; }
        public DateTime timeGenerated { get; set; }
    }
}
