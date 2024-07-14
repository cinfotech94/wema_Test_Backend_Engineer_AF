using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wema_Test_Backend_Engineer_Afeez.Domain.DTO;
using wema_Test_Backend_Engineer_Afeez.Domain.Enums;
using wema_Test_Backend_Engineer_Afeez.Services.Interface;

namespace wema_Test_Backend_Engineer_Afeez.Services.Implementation
{
    public class LogService : ILogService
    {
        private readonly List<LogEntryRequest> _logs = new List<LogEntryRequest>();

        public void AddLog(LogEntryRequest logEntry)
        {
            logEntry.id = Guid.NewGuid();
            logEntry.dateTime = DateTime.UtcNow;
            _logs.Add(logEntry);

            LogToSerilog(logEntry);
        }

        public async Task<IEnumerable<LogEntryRequest>> GetLogs(LogStatus status)
        {
            return _logs.Where(log => log.status == status);
        }

        private void LogToSerilog(LogEntryRequest logEntry)
        {
            switch (logEntry.status)
            {
                case LogStatus.Danger:
                case LogStatus.Error:
                case LogStatus.Exception:
                    Serilog.Log.Error("{@LogEntry}", logEntry);
                    break;
                case LogStatus.Warning:
                    Serilog.Log.Warning("{@LogEntry}", logEntry);
                    break;
                case LogStatus.Success:
                    Serilog.Log.Information("{@LogEntry}", logEntry);
                    break;
                default:
                    Serilog.Log.Information("{@LogEntry}", logEntry);
                    break;
            }
        }
    }
}
