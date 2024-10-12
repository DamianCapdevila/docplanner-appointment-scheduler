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

            if (string.IsNullOrEmpty(apiUser)) throw new InvalidOperationException("AvailabilityServiceUser not found in environment variables.");
            if (string.IsNullOrEmpty(apiPassword)) throw new InvalidOperationException("AvailabilityServicePassword not found in environment variables.");

            string apiKey = apiUser + ":" + apiPassword;

            byte[] apiKeyBytes = Encoding.UTF8.GetBytes(apiKey);
            string apiKeySecret = Convert.ToBase64String(apiKeyBytes);
            return apiKeySecret;
        }

        public async Task<HttpResponseMessage> GetWeeklyAvailabilityAsync(int weekNumber, int year)
        {
            try
            {
                DateTime mondayOfSelectedWeek = ISOWeek.ToDateTime(year, weekNumber, DayOfWeek.Monday);
                string mondayFormatted = mondayOfSelectedWeek.ToString("yyyyMMdd");

                var httpClient = CreateExternalAvailabilityServiceHttpClient();

                var response = await httpClient.GetAsync($"GetWeeklyAvailability/{mondayFormatted}");

                var processedResponse = ProcessExternalServiceResponse(response);
                if (processedResponse.StatusCode != HttpStatusCode.OK)
                {
                    return processedResponse;
                }

                var weeklyAvailability = await DetermineWeeklyAvailability(processedResponse);
                var weeklyAvailabilityJson = JsonConvert.SerializeObject(weeklyAvailability,Formatting.Indented);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(weeklyAvailabilityJson, Encoding.UTF8, "application/json")
                };
            }
            catch (Exception)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("An error occurred in the external availability service while processing the request.")
                };
            }
        }

        private async Task<WeeklyAvailabilityDTO> DetermineWeeklyAvailability(HttpResponseMessage externalServiceResponse)
        {

            try
            {
                var responseContent = await externalServiceResponse.Content.ReadAsStringAsync();
                var facilityOccupancy = JsonConvert.DeserializeObject<FacilityOccupancyDTO>(responseContent);
                return CalculateWeeklyAvailability(facilityOccupancy);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine(ex.Message + "Details:" + Environment.NewLine + ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An unexpected error ocurred: {ex.Message} + Details:" + Environment.NewLine + ex.StackTrace);
                throw;
            }
        }

        private WeeklyAvailabilityDTO CalculateWeeklyAvailability(FacilityOccupancyDTO? facilityOccupancy)
        {
            if (facilityOccupancy == null)
            {
                throw new ArgumentNullException(nameof(facilityOccupancy));
            }

            try
            {
                FacilityOccupancy occupancy = _mapper.Map<FacilityOccupancy>(facilityOccupancy);
                var weeklyAvailability = new WeeklyAvailability(occupancy);


                var weeklyAvailabilityDto = _mapper.Map<WeeklyAvailabilityDTO>(weeklyAvailability);
                return weeklyAvailabilityDto;
            }
            catch (AutoMapperMappingException ex)
            {
                Debug.WriteLine(ex.Message + "Details:" + Environment.NewLine + ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An unexpected error ocurred: {ex.Message} + Details:" + Environment.NewLine + ex.StackTrace);
                throw;
            }
        }

        public async Task<HttpResponseMessage> TakeSlotAsync(AppointmentRequestDTO request)
        {
            try
            {
                request.Start.ToString("yyyy-MM-ddTHH:mm:ss");
                request.End.ToString("yyyy-MM-ddTHH:mm:ss");

                var body = JsonConvert.SerializeObject(request, Formatting.Indented);
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var httpClient = CreateExternalAvailabilityServiceHttpClient();

                var response = await httpClient.PostAsync("TakeSlot", content);
                return ProcessExternalServiceResponse(response);
            }
            catch (Exception)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("An error occurred in the external availability service while processing the request.")
                };
            }
        }

        private static HttpResponseMessage ProcessExternalServiceResponse(HttpResponseMessage externalServiceResponse)
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
