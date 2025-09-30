Feature: CreateBooking
In order to reserve rooms for customers
As a hotel staff member
I want to be able to create bookings for specific dates

    @positive @booking
    Scenario: Successfully create a booking with available rooms
        Given at least one room is available from "2025-10-15" to "2025-10-20"
        When I navigate to the Create Booking page
        And I enter the start date "2025-10-15"
        And I enter the end date "2025-10-20"
        And I submit the booking form
        Then the booking should be created successfully