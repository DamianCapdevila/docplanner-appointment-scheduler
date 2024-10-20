using DocplannerAppointmentScheduler.Core.Services;
using DocplannerAppointmentScheduler.Core.Mappers;
using DocplannerAppointmentScheduler.Api.Mappers;
using DocplannerAppointmentScheduler.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add automappers.
builder.Services.AddAutoMapper(typeof(CoreAndDomainMapper));
builder.Services.AddAutoMapper(typeof(ApiAndCoreMapper));

// Add an http client factory to services, it is used to instantiate an http client for external availability api calls
builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddScoped<ISchedulerService, SchedulerService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.UseExceptionHandler();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
