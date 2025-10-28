Feature: CreateBookingFailure

    @negative @booking
    Scenario: Fail to create a booking when no rooms are available
        Given no rooms are available from "today + 10" to "today + 15"
        When I navigate to the Create Booking page
        And I enter the start date "today + 10"
        And I enter the end date "today + 15"
        And I submit the booking form
        Then the booking should not be created