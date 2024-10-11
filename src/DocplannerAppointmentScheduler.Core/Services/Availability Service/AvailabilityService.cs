using DocplannerAppointmentScheduler.Core.DTOs;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using AutoMapper;
using DocplannerAppointmentScheduler.Domain;
using System.Diagnostics;
using System.Net.Http;


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

        public async Task<WeeklyAvailabilityDTO> GetWeeklyAvailabilityAsync(int weekNumber, int year)
        {
            try
            {
                DateTime mondayOfSelectedWeek = ISOWeek.ToDateTime(year, weekNumber, DayOfWeek.Monday);
                string mondayFormatted = mondayOfSelectedWeek.ToString("yyyyMMdd");

                var httpClient = CreateExternalAvailabilityServiceHttpClient();

                var externalServiceResponse = await httpClient.GetAsync($"https://draliatest.azurewebsites.net/api/availability/GetWeeklyAvailability/{mondayFormatted}");

                if (!externalServiceResponse.IsSuccessStatusCode)
                {
                    return new WeeklyAvailabilityDTO();
                }

                var weeklyAvailability = await DetermineWeeklyAvailability(externalServiceResponse);
                return weeklyAvailability;
            }
            catch (HttpRequestException ex)
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
                //Ensure not null of required fields
                request.Start.ToString("yyyy-MM-ddTHH:mm:ss");
                request.End.ToString("yyyy-MM-ddTHH:mm:ss");

                var body = JsonConvert.SerializeObject(request, Formatting.Indented);
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var httpClient = CreateExternalAvailabilityServiceHttpClient();

                var externalServiceResponse = await httpClient.PostAsync("https://draliatest.azurewebsites.net/api/availability/TakeSlot", content);

                return externalServiceResponse;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An unexpected error ocurred: {ex.Message} + Details:" + Environment.NewLine + ex.StackTrace);
                throw;
            }
        }
    }
}
