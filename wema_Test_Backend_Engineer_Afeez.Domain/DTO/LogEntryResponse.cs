using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wema_Test_Backend_Engineer_Afeez.Domain.Enums;

namespace wema_Test_Backend_Engineer_Afeez.Domain.DTO
{
    public class LogEntryResponse
    {
        public DateTime dateTime { get; set; }
        public string method { get; set; }
        public string controller { get; set; }
        public string action { get; set; }
        public string type { get; set; }
        public string details { get; set; }
        public LogStatus status { get; set; }
    }
}
