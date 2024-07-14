using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wema_Test_Backend_Engineer_Afeez.Domain.DTO
{
    public class CustomerResponse
    {
        [Required]
        public string phoneNumber { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public string residentialState { get; set; }
        [Required]
        public string LGA { get; set; }
    }
}
