using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace wema_Test_Backend_Engineer_Afeez.Domain.Model
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string residentialState { get; set; }
        public string LGA {  get; set; }
    }
}
//C:\test\latest\wema_Test_Backend_Engineer_Afeez.API\wema_Test_Backend_Engineer_Afeez.API>dotnet ef database update --project ../wema_Test_Backend_Engineer_Afeez.Data --startup-project .