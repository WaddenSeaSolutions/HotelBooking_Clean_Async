Feature: CreateBookingFailure

    @negative @booking
    Scenario: Fail to create a booking when no rooms are available next month
        Given no rooms are available next month
        When I navigate to the Create Booking page
        And I enter the start and end dates for next month
        And I submit the booking form
        Then the booking should not be created