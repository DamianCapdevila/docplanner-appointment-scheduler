# Docplanner Appointment Scheduler

## Assumptions

- Doctors interact with the availability service, we have no control over it.
- Facility is fixed by the availability service, we have no control over it.
- Availability service provides slots that are fixed in duration.
- There are several slots in a day, no slot can overlap from one day to the other.
- Patients interact with the appointment scheduler service, we have control over it.
- In a further development of the appointment scheduler service, we could enable patients to make changes to their appointments, appointments will be an object in our domain.


## Requirements

- Patients are able to see available slots in a given week.
- Patients are able to select an available slot, fill their details and book an appointment.

## Constraints

- Availability service expects always a Monday to be passed as a date -> We could solve this programmatically, don´t complicate things for the patient.

 ## Domain Overview

 - The domain is centered around two main entities:
	
	1. **Slot**:
		- Represents a time period that is utilized to schedule an appointment. 
		- The details about it (start and end date and time) are retrieved from the availability service. 
		- Stores information regarding the start and end date and time.
	
	2. **Appointment**:
		- Represents a booked time slot.
		- Contains details of the booked Slot and patient information (Comments, Full Name and Contact Information).
	 