using DocplannerAppointmentScheduler.Core.DTOs;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using AutoMapper;
using DocplannerAppointmentScheduler.Domain;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using DocplannerAppointmentScheduler.Core.Exceptions;
using DocplannerAppointmentScheduler.Core.Results;


namespace DocplannerAppointmentScheduler.Core.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IMapper _mapper;
        private IHttpClientFactory _httpClientFactory;

        public AvailabilityService(IMapper mapper, IHttpClientFactory httpClientFactory)
        {
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateExternalAvailabilityServiceHttpClient()
        {
            string apiKeySecret = CalculateApiKeySecret();

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://draliatest.azurewebsites.net/api/availability/");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", apiKeySecret);

            return httpClient;
        }

        private static string CalculateApiKeySecret()
        {
            string apiUser = Environment.GetEnvironmentVariable("AvailabilityServiceUser");
            string apiPassword = Environment.GetEnvironmentVariable("AvailabilityServicePassword");

            if (string.IsNullOrEmpty(apiUser))
                throw new MissingEnvironmentVariableException("AvailabilityServiceUser environment variable is missing.");
            if (string.IsNullOrEmpty(apiPassword))
                throw new MissingEnvironmentVariableException("AvailabilityServicePassword environment variable is missing.");


            string apiKey = apiUser + ":" + apiPassword;

            byte[] apiKeyBytes = Encoding.UTF8.GetBytes(apiKey);
            string apiKeySecret = Convert.ToBase64String(apiKeyBytes);
            return apiKeySecret;
        }

        public async Task<Result<WeeklyAvailabilityDTO>> GetWeeklyAvailabilityAsync(int weekNumber, int year)
        {
            try
            {
                DateTime mondayOfSelectedWeek = ISOWeek.ToDateTime(year, weekNumber, DayOfWeek.Monday);
                string mondayFormatted = mondayOfSelectedWeek.ToString("yyyyMMdd");

                var httpClient = CreateExternalAvailabilityServiceHttpClient();

                var OccupancyResponse = await httpClient.GetAsync($"GetWeeklyAvailability/{mondayFormatted}");
                var filteredOccupancyResponse = FilterExternalServiceResponse(OccupancyResponse);
                
                if (filteredOccupancyResponse.StatusCode != HttpStatusCode.OK)
                {
                    return await Task.FromResult(Result<WeeklyAvailabilityDTO>.
                                     Failure(new Error(Message: "An error occurred in the external availability service while processing the request.",
                                                       Code: $"{filteredOccupancyResponse.StatusCode}")));
                }

                var weeklyAvailability = await DetermineWeeklyAvailability(filteredOccupancyResponse, mondayOfSelectedWeek);

                return await Task.FromResult(Result<WeeklyAvailabilityDTO>.Success(weeklyAvailability));
            }
            catch (MissingEnvironmentVariableException ex)
            {
                return await Task.FromResult(Result<WeeklyAvailabilityDTO>.
                                     Failure(new Error(Message: $"Configuration error: {ex.Message}. Please ensure all required environment variables are set.",
                                                       Code: $"{HttpStatusCode.Unauthorized}")));
            }
            catch (Exception)
            {
                return await Task.FromResult(Result<WeeklyAvailabilityDTO>.
                                     Failure(new Error(Message: "An error occurred in the external availability service while processing the request.",
                                                       Code: $"{HttpStatusCode.InternalServerError}")));
            }
        }

        private async Task<WeeklyAvailabilityDTO> DetermineWeeklyAvailability(HttpResponseMessage occupancyResponse, DateTime startOfTheWeek)
        {

            try
            {
                FacilityOccupancyDTO? facilityOccupancy = await DeserializeFacilityOccupancy(occupancyResponse);
                return CalculateWeeklyAvailability(facilityOccupancy, startOfTheWeek);
            }
            catch (Exception)
            { 
                throw;
            }
        }

        private static async Task<FacilityOccupancyDTO?> DeserializeFacilityOccupancy(HttpResponseMessage externalServiceResponse)
        {
            var responseContent = await externalServiceResponse.Content.ReadAsStringAsync();
            var facilityOccupancyDTO = JsonConvert.DeserializeObject<FacilityOccupancyDTO>(responseContent);
            return facilityOccupancyDTO;
        }

        private WeeklyAvailabilityDTO CalculateWeeklyAvailability(FacilityOccupancyDTO? facilityOccupancy, DateTime startOfTheWeek)
        {
            try
            {
                FacilityOccupancy occupancy = _mapper.Map<FacilityOccupancy>(facilityOccupancy);
                
                var weeklyAvailability = new WeeklyAvailability(occupancy).GetAvailability(startOfTheWeek);

                var weeklyAvailabilityDto = _mapper.Map<WeeklyAvailabilityDTO>(weeklyAvailability);
                return weeklyAvailabilityDto;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Result<bool>> TakeSlotAsync(AppointmentRequestDto request)
        {
            try
            {
                request.Start.ToString("yyyy-MM-ddTHH:mm:ss");
                request.End.ToString("yyyy-MM-ddTHH:mm:ss");

                var body = JsonConvert.SerializeObject(request, Formatting.Indented);
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var httpClient = CreateExternalAvailabilityServiceHttpClient();

                var response = await httpClient.PostAsync("TakeSlot", content);
                var filteredResponse = FilterExternalServiceResponse(response);

                if (filteredResponse.StatusCode != HttpStatusCode.OK)
                {
                    return await Task.FromResult(Result<bool>.
                                     Failure(new Error(Message: "An error occurred in the external availability service while processing the request.",
                                                       Code: $"{filteredResponse.StatusCode}")));
                }
                return await Task.FromResult(Result<bool>.Success(true));
            }
            catch (MissingEnvironmentVariableException ex)
            {
                return await Task.FromResult(Result<bool>.
                                     Failure(new Error(Message: $"Configuration error: {ex.Message}. Please ensure all required environment variables are set.",
                                                       Code: $"{HttpStatusCode.Unauthorized}")));
            }
            catch (Exception)
            {
                return await Task.FromResult(Result<bool>.
                                     Failure(new Error(Message: "An error occurred in the external availability service while processing the request.",
                                                       Code: $"{HttpStatusCode.InternalServerError}")));
            }
        }

        private static HttpResponseMessage FilterExternalServiceResponse(HttpResponseMessage externalServiceResponse)
        {
            if (externalServiceResponse.IsSuccessStatusCode)
            {
                return externalServiceResponse;
            }
            else if (externalServiceResponse.StatusCode == HttpStatusCode.BadRequest)
            {
                return externalServiceResponse;
            }

            else if (externalServiceResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("Authorization is not sufficient to utilize external availability service. Please check credentials.", Encoding.UTF8, "application/json")
                };
            }

            else if (externalServiceResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return externalServiceResponse;
            }

            return new HttpResponseMessage(externalServiceResponse.StatusCode)
            {
                Content = new StringContent("An error occurred while using external availability service. Please try again later.", Encoding.UTF8, "application/json")
            };
        }
    }
}
