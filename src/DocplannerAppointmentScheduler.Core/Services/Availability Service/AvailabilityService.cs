using DocplannerAppointmentScheduler.Core.DTOs;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using AutoMapper;
using DocplannerAppointmentScheduler.Domain;


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
            DateTime mondayOfSelectedWeek = ISOWeek.ToDateTime(year, weekNumber, DayOfWeek.Monday);
            string mondayFormatted = mondayOfSelectedWeek.ToString("yyyyMMdd");

            var externalServiceResponse = await _httpClient.GetAsync($"https://draliatest.azurewebsites.net/api/availability/GetWeeklyAvailability/{mondayFormatted}");

            if(!externalServiceResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error fetching weekly availability. Status code: {externalServiceResponse.StatusCode}");
            }
            
            var weeklyAvailability = await DetermineWeeklyAvailability(externalServiceResponse);

           
            return weeklyAvailability;
        }

        private async Task<WeeklyAvailabilityDTO> DetermineWeeklyAvailability(HttpResponseMessage externalServiceResponse)
        {

            var responseContent = await externalServiceResponse.Content.ReadAsStringAsync();
            var facilityOccupancy = JsonConvert.DeserializeObject<FacilityOccupancyDTO>(responseContent);
            return await CalculateWeeklyAvailability(facilityOccupancy);
        }

        private async Task<WeeklyAvailabilityDTO> CalculateWeeklyAvailability(FacilityOccupancyDTO? facilityOccupancy)
        {
            if (facilityOccupancy == null)
            {
                throw new ArgumentNullException(nameof(facilityOccupancy));
            }
            
            FacilityOccupancy occupancy = _mapper.Map<FacilityOccupancy>(facilityOccupancy);

            //Calculations of weekly availability are done inside the domain
            var weeklyAvailability = new WeeklyAvailability(occupancy);

            //Map weekly availability to its DTO to then pass it to the API
            var weeklyAvailabilityDto =  _mapper.Map<WeeklyAvailabilityDTO>(weeklyAvailability);

            //Return the DTO.
            return weeklyAvailabilityDto;
        }

        public Task<bool> TakeSlotAsync(AppointmentRequestDTO request)
        {
            throw new NotImplementedException();
        }
    }
}
