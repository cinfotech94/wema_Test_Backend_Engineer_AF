using Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
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
                type = "ountbound",
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
                    string httpRespnse = await GenericService.SendGetRequestAsync(point);
                    if (httpRespnse == null)
                    {
                        logEntryRequest.action = "CBank Not Found";
                        logEntryRequest.details = $"the http response from {point} is null";
                        logEntryRequest.status = LogStatus.Error;
                        response = new List<BankResponse>();
                        return response;
                    }
                    BanksRoot banksRoot = await GenericService.Deserialize<BanksRoot>(httpRespnse);
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
        public async Task<string> OnboardNewCustomer(CustomerRequest customerRequest, string controller)
        {
            var logEntryRequest = new LogEntryRequest()
            {
                id = Guid.NewGuid(),
                dateTime = DateTime.Now,
                type = "inbound",
                method = "OnboardCustomer",
                controller = controller,
            };
            var response = "";
            try
            {
                string verifyExistence = await _cacheService.GetAsync<string>(customerRequest.phoneNumber + "OTP");
                if (verifyExistence != null)
                {
                    if (customerRequest.OTP.Equals(verifyExistence))
                    {
                        bool verifyLGA = await ConfirmLGA(customerRequest.residentialState, customerRequest.LGA);
                        if(verifyLGA)
                        {
                            var customer = new Customer()
                            {
                                phoneNumber = customerRequest.phoneNumber,
                                email = customerRequest.email,
                                password = customerRequest.password,
                                residentialState = customerRequest.residentialState,
                                LGA = customerRequest.LGA,
                            };
                            int repoResponse = await _CustomerRepo.AddAsync<Customer>(customer);
                            if (repoResponse == 0)
                            {
                                logEntryRequest.action = "account onboarding is not succesful";
                                logEntryRequest.details = $"return {customerRequest.phoneNumber} and {customerRequest.email} has not been use to create account succesfully";
                                logEntryRequest.status = LogStatus.Error;
                                response = "account onboarding is not succesful";
                            }
                            else
                            {
                                logEntryRequest.action = "account onboarding is succesful";
                                logEntryRequest.details = $"return {customerRequest.phoneNumber} and {customerRequest.email} has been use to create account succesfully";
                                logEntryRequest.status = LogStatus.Success;
                                response = "onboarding of account is succesful";
                            }
                        }
                        else
                        {
                            logEntryRequest.action = "Check your state and local government very well";
                            logEntryRequest.details = $"return {customerRequest.residentialState} and {customerRequest.LGA} his wronged";
                            logEntryRequest.status = LogStatus.Error;
                            response = "Check your state and local government very well";
                        }
                       
                    }
                    else
                    {
                        logEntryRequest.action = "CHeck yor Otp or phone number well one of it is not correct";
                        logEntryRequest.details = $"return {customerRequest.phoneNumber} and {customerRequest.email} wrong OTP or Phone NUmber is used";
                        logEntryRequest.status = LogStatus.Danger;
                        response = "CHeck your Otp or phone number well one of it is not correct";
                    }
                }
                else
                {
                    logEntryRequest.action = "Either you have not request for password or it has passed 5 miniutes";
                    logEntryRequest.details = $"return {customerRequest.phoneNumber} and {customerRequest.email} have not request for otp and is trying to create account";
                    logEntryRequest.status = LogStatus.Warning;
                    response = "Either you have not request for password or it has passed 5 miniutes";
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
    
        public async Task<string> SendOtp(string number, string controler)
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
        private async Task< bool> ConfirmLGA(string state, string LGA)
        {
            string json = @"{
          ""States"": [
            {
              ""State"": ""Abia"",
              ""LGAs"": [
                ""Aba North"",
                ""Aba South"",
                ""Arochukwu"",
                ""Bende"",
                ""Ikwuano"",
                ""Isiala-Ngwa North"",
                ""Isiala-Ngwa South"",
                ""Isuikwato"",
                ""Obi Nwa"",
                ""Ohafia"",
                ""Osisioma"",
                ""Ngwa"",
                ""Ugwunagbo"",
                ""Ukwa East"",
                ""Ukwa West"",
                ""Umuahia North"",
                ""Umuahia South"",
                ""Umu-Neochi""
              ]
            },
            {
              ""State"": ""Adamawa"",
              ""LGAs"": [
                ""Demsa"",
                ""Fufore"",
                ""Ganaye"",
                ""Gireri"",
                ""Gombi"",
                ""Guyuk"",
                ""Hong"",
                ""Jada"",
                ""Lamurde"",
                ""Madagali"",
                ""Maiha"",
                ""Mayo-Belwa"",
                ""Michika"",
                ""Mubi North"",
                ""Mubi South"",
                ""Numan"",
                ""Shelleng"",
                ""Song"",
                ""Toungo"",
                ""Yola North"",
                ""Yola South""
              ]
            },
            {
              ""State"": ""Anambra"",
              ""LGAs"": [
                ""Aguata"",
                ""Anambra East"",
                ""Anambra West"",
                ""Anaocha"",
                ""Awka North"",
                ""Awka South"",
                ""Ayamelum"",
                ""Dunukofia"",
                ""Ekwusigo"",
                ""Idemili North"",
                ""Idemili south"",
                ""Ihiala"",
                ""Njikoka"",
                ""Nnewi North"",
                ""Nnewi South"",
                ""Ogbaru"",
                ""Onitsha North"",
                ""Onitsha South"",
                ""Orumba North"",
                ""Orumba South"",
                ""Oyi""
              ]
            },
            {
              ""State"": ""Akwa Ibom"",
              ""LGAs"": [
                ""Abak"",
                ""Eastern Obolo"",
                ""Eket"",
                ""Esit Eket"",
                ""Essien Udim"",
                ""Etim Ekpo"",
                ""Etinan"",
                ""Ibeno"",
                ""Ibesikpo Asutan"",
                ""Ibiono Ibom"",
                ""Ika"",
                ""Ikono"",
                ""Ikot Abasi"",
                ""Ikot Ekpene"",
                ""Ini"",
                ""Itu"",
                ""Mbo"",
                ""Mkpat Enin"",
                ""Nsit Atai"",
                ""Nsit Ibom"",
                ""Nsit Ubium"",
                ""Obot Akara"",
                ""Okobo"",
                ""Onna"",
                ""Oron"",
                ""Oruk Anam"",
                ""Udung Uko"",
                ""Ukanafun"",
                ""Uruan"",
                ""Urue-Offong/Oruko "",
                ""Uyo""
              ]
            },
            {
              ""State"": ""Bauchi"",
              ""LGAs"": [
                ""Alkaleri"",
                ""Bauchi"",
                ""Bogoro"",
                ""Damban"",
                ""Darazo"",
                ""Dass"",
                ""Ganjuwa"",
                ""Giade"",
                ""Itas/Gadau"",
                ""Jama'are"",
                ""Katagum"",
                ""Kirfi"",
                ""Misau"",
                ""Ningi"",
                ""Shira"",
                ""Tafawa-Balewa"",
                ""Toro"",
                ""Warji"",
                ""Zaki""
              ]
            },
            {
              ""State"": ""Bayelsa"",
              ""LGAs"": [
                ""Brass"",
                ""Ekeremor"",
                ""Kolokuma/Opokuma"",
                ""Nembe"",
                ""Ogbia"",
                ""Sagbama"",
                ""Southern Jaw"",
                ""Yenegoa""
              ]
            },
            {
              ""State"": ""Benue"",
              ""LGAs"": [
                ""Ado"",
                ""Agatu"",
                ""Apa"",
                ""Buruku"",
                ""Gboko"",
                ""Guma"",
                ""Gwer East"",
                ""Gwer West"",
                ""Katsina-Ala"",
                ""Konshisha"",
                ""Kwande"",
                ""Logo"",
                ""Makurdi"",
                ""Obi"",
                ""Ogbadibo"",
                ""Oju"",
                ""Okpokwu"",
                ""Ohimini"",
                ""Oturkpo"",
                ""Tarka"",
                ""Ukum"",
                ""Ushongo"",
                ""Vandeikya""
              ]
            },
            {
              ""State"": ""Borno"",
              ""LGAs"": [
                ""Abadam"",
                ""Askira/Uba"",
                ""Bama"",
                ""Bayo"",
                ""Biu"",
                ""Chibok"",
                ""Damboa"",
                ""Dikwa"",
                ""Gubio"",
                ""Guzamala"",
                ""Gwoza"",
                ""Hawul"",
                ""Jere"",
                ""Kaga"",
                ""Kala/Balge"",
                ""Konduga"",
                ""Kukawa"",
                ""Kwaya Kusar"",
                ""Mafa"",
                ""Magumeri"",
                ""Maiduguri"",
                ""Marte"",
                ""Mobbar"",
                ""Monguno"",
                ""Ngala"",
                ""Nganzai"",
                ""Shani""
              ]
            },
            {
              ""State"": ""Cross River"",
              ""LGAs"": [
                ""Akpabuyo"",
                ""Odukpani"",
                ""Akamkpa"",
                ""Biase"",
                ""Abi"",
                ""Ikom"",
                ""Yarkur"",
                ""Odubra"",
                ""Boki"",
                ""Ogoja"",
                ""Yala"",
                ""Obanliku"",
                ""Obudu"",
                ""Calabar South"",
                ""Etung"",
                ""Bekwara"",
                ""Bakassi"",
                ""Calabar Municipality""
              ]
            },
            {
              ""State"": ""Delta"",
              ""LGAs"": [
                ""Oshimili"",
                ""Aniocha"",
                ""Aniocha South"",
                ""Ika South"",
                ""Ika North-East"",
                ""Ndokwa West"",
                ""Ndokwa East"",
                ""Isoko south"",
                ""Isoko North"",
                ""Bomadi"",
                ""Burutu"",
                ""Ughelli South"",
                ""Ughelli North"",
                ""Ethiope West"",
                ""Ethiope East"",
                ""Sapele"",
                ""Okpe"",
                ""Warri North"",
                ""Warri South"",
                ""Uvwie"",
                ""Udu"",
                ""Warri Central"",
                ""Ukwani"",
                ""Oshimili North"",
                ""Patani""
              ]
            },
            {
              ""State"": ""Ebonyi"",
              ""LGAs"": [
                ""Edda"",
                ""Afikpo"",
                ""Onicha"",
                ""Ohaozara"",
                ""Abakaliki"",
                ""Ishielu"",
                ""lkwo"",
                ""Ezza"",
                ""Ezza South"",
                ""Ohaukwu"",
                ""Ebonyi"",
                ""Ivo""
              ]
            },
            {
              ""State"": ""Enugu"",
              ""LGAs"": [
                ""Enugu South"",
                ""Igbo-Eze South"",
                ""Enugu North"",
                ""Nkanu"",
                ""Udi Agwu"",
                ""Oji-River"",
                ""Ezeagu"",
                ""IgboEze North"",
                ""Isi-Uzo"",
                ""Nsukka"",
                ""Igbo-Ekiti"",
                ""Uzo-Uwani"",
                ""Enugu Eas"",
                ""Aninri"",
                ""Nkanu East"",
                ""Udenu""
              ]
            },
            {
              ""State"": ""Edo"",
              ""LGAs"": [
                ""Esan North-East"",
                ""Esan Central"",
                ""Esan West"",
                ""Egor"",
                ""Ukpoba"",
                ""Central"",
                ""Etsako Central"",
                ""Igueben"",
                ""Oredo"",
                ""Ovia SouthWest"",
                ""Ovia South-East"",
                ""Orhionwon"",
                ""Uhunmwonde"",
                ""Etsako East"",
                ""Esan South-East""
              ]
            },
            {
              ""State"": ""Ekiti"",
              ""LGAs"": [
                ""Ado"",
                ""Ekiti-East"",
                ""Ekiti-West"",
                ""Emure/Ise/Orun"",
                ""Ekiti South-West"",
                ""Ikere"",
                ""Irepodun"",
                ""Ijero"",
                ""Ido/Osi"",
                ""Oye"",
                ""Ikole"",
                ""Moba"",
                ""Gbonyin"",
                ""Efon"",
                ""Ise/Orun"",
                ""Ilejemeje""
              ]
            },
            {
              ""State"": ""Enugu"",
              ""LGAs"": [
                ""Aninri"",
                ""Enugu East"",
                ""Ezeagu"",
                ""Igbo Etiti"",
                ""Igbo Eze North"",
                ""Igbo Eze South"",
                ""Isi Uzo"",
                ""Nkanu East"",
                ""Nkanu West"",
                ""Nsukka"",
                ""Oji River"",
                ""Udenu"",
                ""Udi""
              ]
            },
            {
              ""State"": ""Federal Capital Territory"",
              ""LGAs"": [
                ""Abaji"",
                ""Abuja Municipal"",
                ""Bwari"",
                ""Gwagwalada"",
                ""Kuje"",
                ""Kwali""
              ]
            },
            {
              ""State"": ""Gombe"",
              ""LGAs"": [
                ""Akko"",
                ""Balanga"",
                ""Billiri"",
                ""Dukku"",
                ""Kaltungo"",
                ""Kwami"",
                ""Shomgom"",
                ""Funakaye"",
                ""Gombe"",
                ""Nafada"",
                ""Yamaltu/Deba""
              ]
            },
            {
              ""State"": ""Imo"",
              ""LGAs"": [
                ""Aboh-Mbaise"",
                ""Ahiazu-Mbaise"",
                ""Ehime-Mbano"",
                ""Ezinihitte"",
                ""Ideato North"",
                ""Ideato South"",
                ""Ihitte/Uboma"",
                ""Ikeduru"",
                ""Isiala Mbano"",
                ""Isu"",
                ""Mbaitoli"",
                ""Ngor-Okpala"",
                ""Njaba"",
                ""Nkwerre"",
                ""Obowo"",
                ""Oguta"",
                ""Ohaji/Egbema"",
                ""Okigwe"",
                ""Orlu"",
                ""Orsu"",
                ""Oru East"",
                ""Oru West"",
                ""Owerri Municipal"",
                ""Owerri North"",
                ""Owerri West""
              ]
            },
            {
              ""State"": ""Jigawa"",
              ""LGAs"": [
                ""Auyo"",
                ""Babura"",
                ""Biriniwa"",
                ""Birnin Kudu"",
                ""Buji"",
                ""Dutse"",
                ""Gagarawa"",
                ""Garki"",
                ""Gumel"",
                ""Guri"",
                ""Gwaram"",
                ""Gwiwa"",
                ""Hadejia"",
                ""Jahun"",
                ""Kafin Hausa"",
                ""Kazaure"",
                ""Kiri Kasama"",
                ""Kiyawa"",
                ""Maigatari"",
                ""Malam Madori"",
                ""Miga"",
                ""Ringim"",
                ""Roni"",
                ""Sule-Tankarkar"",
                ""Taura"",
                ""Yankwashi""
              ]
            },
            {
              ""State"": ""Kano"",
              ""LGAs"": [
                ""Ajingi"",
                ""Albasu"",
                ""Bagwai"",
                ""Bebeji"",
                ""Bichi"",
                ""Bunkure"",
                ""Dala"",
                ""Dambatta"",
                ""Dawakin Kudu"",
                ""Dawakin Tofa"",
                ""Doguwa"",
                ""Fagge"",
                ""Gabasawa"",
                ""Garko"",
                ""Garun Mallam"",
                ""Gaya"",
                ""Gezawa"",
                ""Gwale"",
                ""Gwarzo"",
                ""Kabo"",
                ""Kano"",
                ""Karaye"",
                ""Kibiya"",
                ""Kiru"",
                ""Kumbotso"",
                ""Kunchi"",
                ""Kura"",
                ""Madobi"",
                ""Makoda"",
                ""Minjibir"",
                ""Nasarawa"",
                ""Rano"",
                ""Rimin Gado"",
                ""Rogo"",
                ""Shanono"",
                ""Sumaila"",
                ""Takai"",
                ""Tarauni"",
                ""Tofa"",
                ""Tsanyawa"",
                ""Tudun Wada"",
                ""Ungogo"",
                ""Warawa"",
                ""Wudil""
              ]
            },
            {
              ""State"": ""Kogi"",
              ""LGAs"": [
                ""Adavi"",
                ""Ajaokuta"",
                ""Ankpa"",
                ""Bassa"",
                ""Dekina"",
                ""Ibaji"",
                ""Idah"",
                ""Igalamela-Odolu"",
                ""Ijumu"",
                ""Kabba/Bunu"",
                ""Kogi"",
                ""Lokoja"",
                ""Mopa-Muro"",
                ""Ofu"",
                ""Ogori/Magongo"",
                ""Okehi"",
                ""Okene"",
                ""Olamaboro"",
                ""Omala"",
                ""Yagba East"",
                ""Yagba West""
              ]
            },
            {
              ""State"": ""Kwara"",
              ""LGAs"": [
                ""Asa"",
                ""Baruten"",
                ""Edu"",
                ""Ekiti"",
                ""Ifelodun"",
                ""Ilorin East"",
                ""Ilorin West"",
                ""Irepodun"",
                ""Isin"",
                ""Kaiama"",
                ""Moro"",
                ""Offa"",
                ""Oke-Ero"",
                ""Oyun"",
                ""Pategi""
              ]
            },
            {
              ""State"": ""Lagos"",
              ""LGAs"": [
                ""Agege"",
                ""Ajeromi-Ifelodun"",
                ""Alimosho"",
                ""Amuwo-Odofin"",
                ""Apapa"",
                ""Badagry"",
                ""Epe"",
                ""Eti-Osa"",
                ""Ibeju-Lekki"",
                ""Ifako-Ijaiye"",
                ""Ikeja"",
                ""Ikorodu"",
                ""Kosofe"",
                ""Lagos Island"",
                ""Lagos Mainland"",
                ""Mushin"",
                ""Ojo"",
                ""Oshodi-Isolo"",
                ""Shomolu"",
                ""Surulere""
              ]
            },
            {
              ""State"": ""Nasarawa"",
              ""LGAs"": [
                ""Akwanga"",
                ""Awe"",
                ""Doma"",
                ""Karu"",
                ""Keana"",
                ""Keffi"",
                ""Lafia"",
                ""Nasarawa"",
                ""Nasarawa Egon"",
                ""Obi"",
                ""Toto"",
                ""Wamba""
              ]
            },
            {
              ""State"": ""Niger"",
              ""LGAs"": [
                ""Agaie"",
                ""Agwara"",
                ""Bida"",
                ""Borgu"",
                ""Bosso"",
                ""Chanchaga"",
                ""Edati"",
                ""Gbako"",
                ""Gurara"",
                ""Katcha"",
                ""Kontagora"",
                ""Lapai"",
                ""Lavun"",
                ""Magama"",
                ""Mariga"",
                ""Mashegu"",
                ""Mokwa"",
                ""Munya"",
                ""Paikoro"",
                ""Rafi"",
                ""Rijau"",
                ""Shiroro"",
                ""Suleja"",
                ""Tafa"",
                ""Wushishi""
              ]
            },
            {
              ""State"": ""Ogun"",
              ""LGAs"": [
                ""Abeokuta North"",
                ""Abeokuta South"",
                ""Ado-Odo/Ota"",
                ""Ewekoro"",
                ""Ifo"",
                ""Ijebu East"",
                ""Ijebu North"",
                ""Ijebu North East"",
                ""Ijebu Ode"",
                ""Ikenne"",
                ""Imeko Afon"",
                ""Ipokia"",
                ""Obafemi Owode"",
                ""Odogbolu"",
                ""Odeda"",
                ""Ogun Waterside"",
                ""Remo North"",
                ""Shagamu""
              ]
            },
            {
              ""State"": ""Ondo"",
              ""LGAs"": [
                ""Akoko North-East"",
                ""Akoko North-West"",
                ""Akoko South-West"",
                ""Akoko South-East"",
                ""Akure North"",
                ""Akure South"",
                ""Ese Odo"",
                ""Idanre"",
                ""Ifedore"",
                ""Ilaje"",
                ""Ile Oluji/Okeigbo"",
                ""Irele"",
                ""Odigbo"",
                ""Okitipupa"",
                ""Ondo East"",
                ""Ondo West"",
                ""Ose"",
                ""Owo""
              ]
            },
            {
              ""State"": ""Osun"",
              ""LGAs"": [
                ""Aiyedaade"",
                ""Aiyedire"",
                ""Atakunmosa East"",
                ""Atakunmosa West"",
                ""Boluwaduro"",
                ""Boripe"",
                ""Ede North"",
                ""Ede South"",
                ""Egbedore"",
                ""Ejigbo"",
                ""Ife Central"",
                ""Ife East"",
                ""Ife North"",
                ""Ife South"",
                ""Ifedayo"",
                ""Ifelodun"",
                ""Ila"",
                ""Ilesha East"",
                ""Ilesha West"",
                ""Irepodun"",
                ""Irewole"",
                ""Isokan"",
                ""Iwo"",
                ""Obokun"",
                ""Odo-Otin"",
                ""Ola Oluwa"",
                ""Olorunda"",
                ""Oriade"",
                ""Orolu"",
                ""Osogbo""
              ]
            },
            {
              ""State"": ""Oyo"",
              ""LGAs"": [
                ""Afijio"",
                ""Akinyele"",
                ""Atiba"",
                ""Atisbo"",
                ""Egbeda"",
                ""Ibadan North"",
                ""Ibadan North-East"",
                ""Ibadan North-West"",
                ""Ibadan South-East"",
                ""Ibadan South-West"",
                ""Ibarapa Central"",
                ""Ibarapa East"",
                ""Ibarapa North"",
                ""Ido"",
                ""Irepo"",
                ""Iseyin"",
                ""Itesiwaju"",
                ""Iwajowa"",
                ""Kajola"",
                ""Lagelu"",
                ""Ogbomosho North"",
                ""Ogbomosho South"",
                ""Ogo Oluwa"",
                ""Olorunsogo"",
                ""Oluyole"",
                ""Ona Ara"",
                ""Orelope"",
                ""Ori Ire"",
                ""Oyo"",
                ""Oyo East"",
                ""Saki East"",
                ""Saki West"",
                ""Surulere""
              ]
            },
            {
              ""State"": ""Plateau"",
              ""LGAs"": [
                ""Barkin Ladi"",
                ""Bassa"",
                ""Bokkos"",
                ""Jos East"",
                ""Jos North"",
                ""Jos South"",
                ""Kanam"",
                ""Kanke"",
                ""Langtang North"",
                ""Langtang South"",
                ""Mangu"",
                ""Mikang"",
                ""Pankshin"",
                ""Qua'an Pan"",
                ""Riyom"",
                ""Shendam"",
                ""Wase""
              ]
            },
            {
              ""State"": ""Rivers"",
              ""LGAs"": [
                ""Abua/Odual"",
                ""Ahoada East"",
                ""Ahoada West"",
                ""Akuku Toru"",
                ""Andoni"",
                ""Asari-Toru"",
                ""Bonny"",
                ""Degema"",
                ""Eleme"",
                ""Emuoha"",
                ""Etche"",
                ""Gokana"",
                ""Ikwerre"",
                ""Khana"",
                ""Obio/Akpor"",
                ""Ogba/Egbema/Ndoni"",
                ""Ogu/Bolo"",
                ""Okrika"",
                ""Omuma"",
                ""Opobo/Nkoro"",
                ""Oyigbo"",
                ""Port Harcourt"",
                ""Tai""
              ]
            },
            {
              ""State"": ""Sokoto"",
              ""LGAs"": [
                ""Binji"",
                ""Bodinga"",
                ""Dange Shuni"",
                ""Gada"",
                ""Goronyo"",
                ""Gudu"",
                ""Gwadabawa"",
                ""Illela"",
                ""Isa"",
                ""Kebbe"",
                ""Kware"",
                ""Rabah"",
                ""Sabon Birni"",
                ""Shagari"",
                ""Silame"",
                ""Sokoto North"",
                ""Sokoto South"",
                ""Tambuwal"",
                ""Tangaza"",
                ""Tureta"",
                ""Wamako"",
                ""Wurno"",
                ""Yabo""
              ]
            },
            {
              ""State"": ""Taraba"",
              ""LGAs"": [
                ""Ardo Kola"",
                ""Bali"",
                ""Donga"",
                ""Gashaka"",
                ""Gassol"",
                ""Ibi"",
                ""Jalingo"",
                ""Karim Lamido"",
                ""Kumi"",
                ""Lau"",
                ""Sardauna"",
                ""Takum"",
                ""Ussa"",
                ""Wukari"",
                ""Yorro"",
                ""Zing""
              ]
            },
            {
              ""State"": ""Yobe"",
              ""LGAs"": [
                ""Bade"",
                ""Bursari"",
                ""Damaturu"",
                ""Fika"",
                ""Fune"",
                ""Geidam"",
                ""Gujba"",
                ""Gulani"",
                ""Jakusko"",
                ""Karasuwa"",
                ""Machina"",
                ""Nangere"",
                ""Nguru"",
                ""Potiskum"",
                ""Tarmuwa"",
                ""Yunusari"",
                ""Yusufari""
              ]
            },
            {
              ""State"": ""Zamfara"",
              ""LGAs"": [
                ""Anka"",
                ""Bakura"",
                ""Birnin Magaji/Kiyaw"",
                ""Bukkuyum"",
                ""Bungudu"",
                ""Gummi"",
                ""Gusau"",
                ""Kaura Namoda"",
                ""Maradun"",
                ""Maru"",
                ""Shinkafi"",
                ""Talata Mafara"",
                ""Chafe"",
                ""Zurmi""
              ]
            }
          ]
        }";
            bool response= false;
            StateRoot stateRoot =await  GenericService.Deserialize<StateRoot>(json);
            // Perform the check
            if (stateRoot != null)
            {
                var stateData = stateRoot.States.FirstOrDefault(s => s.State.Equals(state, StringComparison.OrdinalIgnoreCase));
                if (stateData != null)
                {
                    return stateData.LGAs.Contains(LGA, StringComparer.OrdinalIgnoreCase);
                }
            }

            return response;
        }

    }
}
