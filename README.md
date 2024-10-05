# Docplanner Appointment Scheduler

## Value

Using this Appointment Scheduler, patients can book appointments with a doctor.

## Requirements

- Patients are able to see available slots in a given week.
- Patients are able to select an available slot, fill their details and book an appointment.

## Assumptions

- Doctors interact with the availability service, we have no control over it.
- Facility is fixed by the availability service, we have no control over it. We assume that at a prior stage the patient selected the facility, and the availability service provides the free slots for that particular facility.
- Availability service provides slots that are fixed in duration.
- There are several slots in a day, no slot can overlap from one day to the other.
- Patients can book 1 slot at a time.
- First patient to book a given slot wins... this could be handled better, but maybe in a further step.
- Patients interact with the appointment scheduler service, we have control over it.
- We assume that the patients already have created a user in the appointment scheduler, so we don´t handle authentication here. 
- In a further development of the appointment scheduler service, we could enable patients to make changes to their appointments, appointments will be an object in our domain.
- We assume we don´t store the appointments in a database, that could be done in a further step of development.

## Constraints

- Availability service expects always a Monday to be passed as a date -> We could solve this programmatically, don´t complicate things for the patient.

 ## Domain Overview

 - The domain is centered around three main entities:
	
	1. **Slot**:
		- Represents a time period that is utilized to schedule an appointment. 
		- The details about it (start and end date and time, FacilityId) are retrieved from the availability service. 
		- Stores information regarding the start and end date and time.
	
	2. **Patient**:
		- Represents the patient who books the appointment.
		- Contains patient details like name, second name, email, and phone number.
	
	3. **Appointment**:
		- Represents a booked time slot.
		- Contains details of the booked Slot and patient information.
	 