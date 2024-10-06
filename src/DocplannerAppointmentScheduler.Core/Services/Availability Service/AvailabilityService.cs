using DocplannerAppointmentScheduler.Core.DTOs;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using AutoMapper;
using DocplannerAppointmentScheduler.Domain;
using System.Diagnostics;


namespace DocplannerAppointmentScheduler.Core.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IMapper _mapper;
        private HttpClient _httpClient;

        public AvailabilityService(IMapper mapper)
        {
            _mapper = mapper;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://draliatest.azurewebsites.net/api/availability/");

            string apiKey = "techuser:secretpassWord";

            byte[] apiKeyBytes = Encoding.UTF8.GetBytes(apiKey);
            string apiKeySecret = Convert.ToBase64String(apiKeyBytes);
            
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", apiKeySecret);
        }
        public async Task<WeeklyAvailabilityDTO> GetWeeklyAvailabilityAsync(int weekNumber, int year)
        {
            try
            {
                DateTime mondayOfSelectedWeek = ISOWeek.ToDateTime(year, weekNumber, DayOfWeek.Monday);
                string mondayFormatted = mondayOfSelectedWeek.ToString("yyyyMMdd");

                var externalServiceResponse = await _httpClient.GetAsync($"https://draliatest.azurewebsites.net/api/availability/GetWeeklyAvailability/{mondayFormatted}");

                if (!externalServiceResponse.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error fetching weekly availability. Status code: {externalServiceResponse.StatusCode}");
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

        public async Task<bool> TakeSlotAsync(AppointmentRequestDTO request)
        {
            try
            {
                request.Slot.Start.ToString("yyyy-MM-ddTHH:mm:ss");
                request.Slot.End.ToString("yyyy-MM-ddTHH:mm:ss");

                var body = JsonConvert.SerializeObject(request);
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var externalServiceResponse = await _httpClient.PostAsync("https://draliatest.azurewebsites.net/api/availability/TakeSlot", content);

                if (!externalServiceResponse.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error fetching weekly availability. Status code: {externalServiceResponse.StatusCode}");
                }
                return true;
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
    }
}
