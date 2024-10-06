using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.Core.Mapping;
using DocplannerAppointmentScheduler.Api.Mapping;

var builder = WebApplication.CreateBuilder(args);

// Add automappers.
builder.Services.AddAutoMapper(typeof(CoreAndDomainMapper));
builder.Services.AddAutoMapper(typeof(ApiAndCoreMapper));

// Add services to the container.
builder.Services.AddScoped<ISchedulerService, SchedulerService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
