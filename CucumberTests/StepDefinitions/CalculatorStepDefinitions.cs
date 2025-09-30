using Reqnroll  ;

namespace CucumberTests.StepDefinitions;

[Binding]
public sealed class CalculatorStepDefinitions
{
    // For additional details on Reqnroll step definitions see https://go.reqnroll.net/doc-stepdef
    int firstNumber, secondNumber, sum;
    Calculator calculator = new Calculator();

    [Given("the first number is {int}")]
    public void GivenTheFirstNumberIs(int number)
    {
        firstNumber = number;
    }

    [Given("the second number is {int}")]
    public void GivenTheSecondNumberIs(int number)
    {
        secondNumber = number;
    }

    [When("the two numbers are added")]
    public void WhenTheTwoNumbersAreAdded()
    {
        sum = calculator.Add(firstNumber, secondNumber);
    }

    [Then("the result should be {int}")]
    public void ThenTheResultShouldBe(int result)
    {
        Assert.Equal(expectedSum, sum);
    }
}