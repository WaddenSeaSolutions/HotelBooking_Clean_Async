Feature: CreateBooking
In order to reserve rooms for customers
As a hotel staff member
I want to be able to create bookings for specific dates

    @positive @booking
    Scenario: Successfully create a booking for next week
        Given at least one room is available for next week
        When I navigate to the Create Booking page
        And I enter the start and end dates for next week
        And I submit the booking form
        Then the booking should be created successfully
