using Reqnroll;
using System;
using FluentAssertions;

namespace CucumberTests.StepDefinitions;

[Binding]
public sealed class BookingStepDefinitions
{
    private DateTime _startDate;
    private DateTime _endDate;
    private bool _roomAvailable;
    private bool _bookingCreated;

    [Given(@"at least one room is available from ""(.*)"" to ""(.*)""")]
    public void GivenAtLeastOneRoomIsAvailableFromTo(string startDate, string endDate)
    {
        _startDate = DateTime.Parse(startDate);
        _endDate = DateTime.Parse(endDate);

        // simulate that at least one room is available
        _roomAvailable = true;
    }

    [When(@"I navigate to the Create Booking page")]
    public void WhenINavigateToTheCreateBookingPage()
    {
        // Here you would simulate navigation in your app
        // (e.g. opening a page or calling an API endpoint)
    }

    [When(@"I enter the start date ""(.*)""")]
    public void WhenIEnterTheStartDate(string startDate)
    {
        _startDate = DateTime.Parse(startDate);
    }

    [When(@"I enter the end date ""(.*)""")]
    public void WhenIEnterTheEndDate(string endDate)
    {
        _endDate = DateTime.Parse(endDate);
    }

    [When(@"I submit the booking form")]
    public void WhenISubmitTheBookingForm()
    {
        if (_roomAvailable && _endDate > _startDate)
        {
            _bookingCreated = true;
        }
        else
        {
            _bookingCreated = false;
        }
    }

    [Then(@"the booking should be created successfully")]
    public void ThenTheBookingShouldBeCreatedSuccessfully()
    {
        _bookingCreated.Should().BeTrue("the booking should succeed if a room was available and dates were valid");
    }
}