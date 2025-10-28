using Reqnroll;
using System;
using FluentAssertions;

namespace CucumberXunitTest.StepDefinitions;

[Binding]
public sealed class BookingStepDefinitions
{
    private DateTime _startDate;
    private DateTime _endDate;
    private bool _roomAvailable;
    private bool _bookingCreated;

    [Given(@"at least one room is available for next week")]
    public void GivenAtLeastOneRoomIsAvailableForNextWeek()
    {
        _startDate = DateTime.Today.AddDays(7);
        _endDate = DateTime.Today.AddDays(12);
        _roomAvailable = true;
    }

    [Given(@"no rooms are available next month")]
    public void GivenNoRoomsAreAvailableNextMonth()
    {
        _startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
            .AddMonths(1)
            .AddDays(5);
        _endDate = _startDate.AddDays(5);
        _roomAvailable = false;
    }

    [When(@"I navigate to the Create Booking page")]
    public void WhenINavigateToTheCreateBookingPage()
    {
        // Simulate navigation
    }

    [When(@"I enter the start and end dates for next week")]
    public void WhenIEnterTheStartAndEndDatesForNextWeek()
    {
        // Normally handled via UI, but we reuse the same values
    }

    [When(@"I enter the start and end dates for next month")]
    public void WhenIEnterTheStartAndEndDatesForNextMonth()
    {
        // Normally handled via UI, but we reuse the same values
    }

    [When(@"I submit the booking form")]
    public void WhenISubmitTheBookingForm()
    {
        _bookingCreated = _roomAvailable && _endDate > _startDate;
    }

    [Then(@"the booking should be created successfully")]
    public void ThenTheBookingShouldBeCreatedSuccessfully()
    {
        _bookingCreated.Should().BeTrue("the booking should succeed if a room was available and dates were valid");
    }

    [Then(@"the booking should not be created")]
    public void ThenTheBookingShouldNotBeCreated()
    {
        _bookingCreated.Should().BeFalse("the booking should fail if no rooms were available");
    }
}
