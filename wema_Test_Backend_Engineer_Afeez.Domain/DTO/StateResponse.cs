using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wema_Test_Backend_Engineer_Afeez.Domain.DTO
{
    public class StateRoot
    {
        public List<StateLGA> States { get; set; }
    }

    public class StateLGA
    {
        public string State { get; set; }
        public List<string> LGAs { get; set; }
    }


}
