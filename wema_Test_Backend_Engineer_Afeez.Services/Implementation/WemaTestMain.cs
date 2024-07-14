using Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wema_Test_Backend_Engineer_Afeez.Data.Repository;
using wema_Test_Backend_Engineer_Afeez.Domain.DTO;
using wema_Test_Backend_Engineer_Afeez.Domain.Enums;
using wema_Test_Backend_Engineer_Afeez.Domain.Model;
using wema_Test_Backend_Engineer_Afeez.Services.Interface;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace wema_Test_Backend_Engineer_Afeez.Services.Implementation
{
    public class WemaTestMain : IWemaTestMain
    {
        private readonly ILogService _logService;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;
        private readonly IRepo _CustomerRepo;
        public WemaTestMain(ILogService logService,ICacheService cacheService, IConfiguration configuration, IRepo CustomerRepo) 
        {
            _logService= logService;
            _cacheService= cacheService;
            _configuration = configuration;
            _CustomerRepo= CustomerRepo;
        }
        public async Task<List<BankResponse>> GetexistingBank(string controler)
        {
            var logEntryRequest = new LogEntryRequest()
            {
                id = Guid.NewGuid(),
                dateTime = DateTime.Now,
                type = "inbound",
                method = "GetexistingBank",
                controller = controler,
            };
            var response = new List<BankResponse>();
            try
            {
                List<BankResponse> bankResponses = await _cacheService.GetAsync<List<BankResponse>>("ListBanks");
                if (bankResponses != null && bankResponses.Count > 0)
                {
                    logEntryRequest.action = "Customers Found";
                    logEntryRequest.details = $"return {bankResponses.Count()} number of banks from cache";
                    logEntryRequest.status = LogStatus.Success;
                    response = bankResponses;
                }
                else
                {
                    string point = "https://wema-alatdev-apimgt.azure-api.net/alat-test/api/Shared/GetAllBanks";
                    string httpRespnse = await GenericService.SendRequestAsync(point, HttpMethod.Get);
                    if (httpRespnse == null)
                    {
                        logEntryRequest.action = "CBank Not Found";
                        logEntryRequest.details = $"the http response from {point} is null";
                        logEntryRequest.status = LogStatus.Error;
                        response = new List<BankResponse>();
                    }
                    BanksRoot banksRoot = GenericService.Deserialize<BanksRoot>(httpRespnse);
                    logEntryRequest.action = "Banks found";
                    logEntryRequest.details = $"return {banksRoot.result.Count()} number of banks";
                    logEntryRequest.status = LogStatus.Success;
                    response = banksRoot.result;
                    TimeSpan timeSpan = new TimeSpan(48, 0, 0);
                    await _cacheService.AddOrUpdateAsync<List<BankResponse>>("ListBanks", response, timeSpan);
                }
            }
            catch (Exception ex)
            {
                logEntryRequest.action = ex.Source;
                logEntryRequest.details = ex.Message + ex.InnerException;
                logEntryRequest.status = LogStatus.Exception;
            }
            _logService.AddLog(logEntryRequest);
            return response;
        }

        public async Task<IEnumerable<CustomerResponse>> GetOnboardCustomer(string controler)
        {
            var logEntryRequest = new LogEntryRequest()
            {
                id = Guid.NewGuid(),
                dateTime = DateTime.Now,
                type = "inbound",
                method = "OnboardCustomer",
                controller = controler,
            };
            var response = new List<CustomerResponse>();
            try
            {
                List<CustomerResponse> customerResponses = await _cacheService.GetAsync<List<CustomerResponse>>("ListOnboardCustomers");
                if (customerResponses != null && customerResponses.Count > 0)
                {
                    logEntryRequest.action = "Customers Found";
                    logEntryRequest.details = $"return {customerResponses.Count()} number of customer from cache";
                    logEntryRequest.status = LogStatus.Success;
                    response=customerResponses;
                }
                else
                {
                    IQueryable<Customer> customers = await _CustomerRepo.GetAsync<Customer>();
                    if (customers.Count() > 0)
                    {
                        logEntryRequest.action = "Customers Found";
                        logEntryRequest.details = $"return {customers.Count()} number of customer";
                        logEntryRequest.status = LogStatus.Success;
                        response = customers.Select(c => new CustomerResponse
                        {
                            phoneNumber = c.phoneNumber,
                            email = c.email,
                            residentialState = c.residentialState,
                            LGA = c.LGA
                        }).ToList();
                        TimeSpan timeSpan = new TimeSpan(48, 0, 0);
                        await _cacheService.AddOrUpdateAsync<List<CustomerResponse>>("ListOnboardCustomers", response, timeSpan);
                    }
                    else
                    {
                        logEntryRequest.action = "Customers Found";
                        logEntryRequest.details = $"return {customers.Count()} number of customer";
                        logEntryRequest.status = LogStatus.Success;
                        response = new List<CustomerResponse>();
                    }
                }
                
            }
            catch (Exception ex)
            {
                logEntryRequest.action = ex.Source;
                logEntryRequest.details = ex.Message + ex.InnerException;
                logEntryRequest.status = LogStatus.Exception;
            }
            
            _logService.AddLog(logEntryRequest);
            return response;
        }
        public async Task<IEnumerable<LogEntryResponse>> GetLogs( LogStatus type, string controler)
        {
            var logEntryRequest = new LogEntryRequest()
            {
                id = Guid.NewGuid(),
                dateTime = DateTime.Now,
                type = "inbound",
                method = "GetexistingBank",
                controller = controler,
            };
            var response = new List<LogEntryResponse>();
            try
            {
                IEnumerable<LogEntryRequest> getLogs = await _logService.GetLogs(type);
                response= getLogs.Select(c => new LogEntryResponse
                {
                    dateTime= c.dateTime,
                    method = c.method,
                    controller = c.controller,
                    action = c.action,
                    type = c.type,
                    details = c.details,
                    status = c.status
                }).ToList();
            }
            catch(Exception ex)
            {
                logEntryRequest.action = ex.Source;
                logEntryRequest.details = ex.Message + ex.InnerException;
                logEntryRequest.status = LogStatus.Exception;
            }
            _logService.AddLog(logEntryRequest);
            return response;
        }
    
        public async Task<string> SendOtp(string otp,string number, string controler)
        {
            var logEntryRequest = new LogEntryRequest()
            {
                id = Guid.NewGuid(),
                dateTime = DateTime.Now,
                type = "inbound",
                method = "SendOtp",
                controller = controler,
            };
            var response = "";
            try
            {
                string verifyExistence =await _cacheService.GetAsync<string>(number + "OTP");
                if (verifyExistence != null) 
                {
                    logEntryRequest.action = "OTP";
                    logEntryRequest.details = "Double Request";
                    logEntryRequest.status = LogStatus.Danger;
                    response = "You have request for this OTP please goto onboarding or wait for 5 minutes and resent OTP";
                }
                else
                {
                    bool verifyPhoneNumber = GenericService.VerifyPhoneNumber(number);
                    if (verifyPhoneNumber)
                    {
                        TimeSpan fiveMinutes = new TimeSpan(0, 5, 0);
                        string OTP = GenericService.GenerateOTP(6);
                        await _cacheService.AddOrUpdateAsync<string>(number + "OTP", OTP, fiveMinutes);
                        logEntryRequest.action = "OTP";
                        logEntryRequest.details = $"This {number}, request for OTP AT {logEntryRequest.dateTime} and is succes";
                        logEntryRequest.status = LogStatus.Success;
                        response = $"Your OTP is {OTP}, please use it within 5 minutes";
                    }
                    else
                    {
                        logEntryRequest.action = "VerifyPhoneNumber";
                        logEntryRequest.details = "The phone number is not correct";
                        logEntryRequest.status = LogStatus.Warning;
                        response = "please confirm the number";
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                logEntryRequest.action = ex.Source;
                logEntryRequest.details = ex.Message + ex.InnerException;
                logEntryRequest.status = LogStatus.Exception;
            }
            _logService.AddLog(logEntryRequest);
            return response;
        }
        private bool ConfirmLGA(string state, string LGA)
        {
            var response = false;
            var statesSection = _configuration.GetSection("States");
            StateRoot states = statesSection.Get<StateRoot>(); 
            var checkState = states.States.FirstOrDefault(s => s.State == state);
            if (checkState != null)
            {
                if (checkState.LGAs.Contains(LGA))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
