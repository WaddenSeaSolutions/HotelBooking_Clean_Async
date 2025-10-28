using Reqnroll;
using System;
using FluentAssertions;
using System.Text.RegularExpressions;

namespace CucumberXunitTest.StepDefinitions;

[Binding]
public sealed class BookingStepDefinitions
{
    private DateTime _startDate;
    private DateTime _endDate;
    private bool _roomAvailable;
    private bool _bookingCreated;

    // Utility: parse relative or absolute date expressions
    private DateTime ParseDateExpression(string expression)
    {
        expression = expression.Trim().ToLower();

        if (expression.StartsWith("today"))
        {
            var match = Regex.Match(expression, @"today\s*([\+\-])\s*(\d+)");
            if (match.Success)
            {
                int offset = int.Parse(match.Groups[2].Value);
                return match.Groups[1].Value == "+" 
                    ? DateTime.Today.AddDays(offset) 
                    : DateTime.Today.AddDays(-offset);
            }
            return DateTime.Today;
        }

        // fallback to direct date parsing
        return DateTime.Parse(expression);
    }

    [Given(@"at least one room is available from ""(.*)"" to ""(.*)""")]
    public void GivenAtLeastOneRoomIsAvailableFromTo(string startDate, string endDate)
    {
        _startDate = ParseDateExpression(startDate);
        _endDate = ParseDateExpression(endDate);
        _roomAvailable = true;
    }

    [Given(@"no rooms are available from ""(.*)"" to ""(.*)""")]
    public void GivenNoRoomsAreAvailableFromTo(string startDate, string endDate)
    {
        _startDate = ParseDateExpression(startDate);
        _endDate = ParseDateExpression(endDate);
        _roomAvailable = false;
    }

    [When(@"I navigate to the Create Booking page")]
    public void WhenINavigateToTheCreateBookingPage()
    {
        // Simulate navigation
    }

    [When(@"I enter the start date ""(.*)""")]
    public void WhenIEnterTheStartDate(string startDate)
    {
        _startDate = ParseDateExpression(startDate);
    }

    [When(@"I enter the end date ""(.*)""")]
    public void WhenIEnterTheEndDate(string endDate)
    {
        _endDate = ParseDateExpression(endDate);
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
