using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wema_Test_Backend_Engineer_Afeez.Domain.DTO;
using wema_Test_Backend_Engineer_Afeez.Domain.Enums;

namespace wema_Test_Backend_Engineer_Afeez.Services.Interface
{
    public interface ILogService
    {
        void AddLog(LogEntryRequest logEntry);
        Task<IEnumerable<LogEntryRequest>> GetLogs(LogStatus status);
    }
}
