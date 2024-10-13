# Docplanner Appointment Scheduler

## Value

Using this Appointment Scheduler, patients can book appointments with a doctor.

## Requirements

- Patients are able to see available slots in a given week.
- Patients are able to select an available slot, fill their details and book an appointment.

## Assumptions

- Doctors interact with the availability service, we have no control over it.
- Facility is fixed by the availability service, we have no control over it. We assume that at a previous stage the patient selected the facility.
- There will be always at the most 1 facility inside the external availability service response. 
- In reality, external availability service should store busy slots from us when we make an appointment. It doesn´t, but we assume it does.

- Patients interact with the appointment scheduler service, we have control over it.
- Patients can book 1 slot at a time.
- We assume that the patients already have created a user in the appointment scheduler, so we don´t handle authentication here. 
- In a further development of the appointment scheduler service, we could enable patients to make changes to their appointments, appointments will be an object in our domain.
- We assume we don´t store the appointments in a database, that could be done in a further step of development.

- ANY OTHER ASSUMPTIONS CAN BE DERIVED FROM TESTS.


## How to run the Appointment Scheduler

### Preconditions
Before running the application, ensure that the following preconditions are met:

1. **Environment Variables Setup**:
   The application depends on two environment variables for authentication with the external availability service:
   - `AvailabilityServiceUser`: The username for the availability service.
   - `AvailabilityServicePassword`: The password for the availability service.

   Ensure that these environment variables are properly configured in your system or development environment. You can set them up using the following commands (depending on your OS):

   - **For Windows (Powershell)**:
     ```powershell
     $env:AvailabilityServiceUser = "your_username"
     $env:AvailabilityServicePassword = "your_password"
     ```

   - **For macOS/Linux**:
     ```bash
     export AvailabilityServiceUser="your_username"
     export AvailabilityServicePassword="your_password"
     ```
   
   - These environment variables are mandatory for the service to function. If they are not set, the API will return a `400 Bad Request` response with an error message indicating which environment variable is missing. 

        For example, the response might look like this:

        ```json
        {
            "message": "Configuration error: AvailabilityServiceUser is missing. Please ensure all required environment variables are set."
        }
        ```


2. **Required Software**:
   - [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0): Ensure you have the correct .NET SDK installed.

3. **External Availability Service**: Make sure the external availability service is running or accessible.

### Running the application

1. **Clone the Repository**  
   Clone the repository to your local machine using the following command:  
   ```bash
   git clone <repository-url
   ```
2. **Restore Dependencies**
   Navigate to the repository directory using:
   ```bash
   cd <repository-directory>  
   ```
   Navigate to the project directory using:
   ```bash
   cd <repository-directory>/DocplannerAppointmentScheduler.Api  
   ```
   Restore dependencies using:
   ```bash
   dotnet restore  
   ```
3. **Start the Application**
   Start the application using:
   ```bash
   cd dotnet run  
   ```
4. **Access the API**
   Once the application is running, you can access the API Swagger UI at: http://localhost:5234/swagger
 

### Troubleshooting
  
- If the availability service is down, you will receive a `503 Service Unavailable` error when attempting to schedule or retrieve appointments. Ensure that the external service is available before proceeding.


### Preconditions

### Troubleshooting


## Technical Overview

 ### API Layer Overview

 ### Core Layer Overview

 ### Domain Overview
