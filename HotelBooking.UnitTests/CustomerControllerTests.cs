using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.WebApi.Controllers;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests
{
    public class CustomersControllerTests
    {
        private CustomersController controller;
        private Mock<IRepository<Customer>> fakeCustomerRepository;

        public CustomersControllerTests()
        {
            fakeCustomerRepository = new Mock<IRepository<Customer>>();
            controller = new CustomersController(fakeCustomerRepository.Object);
        }

        // Test data for different customer scenarios
        public static IEnumerable<object[]> GetCustomerTestData()
        {
            // Empty list
            yield return new object[] 
            { 
                new List<Customer>(), 
                0, 
                "Empty customer list" 
            };

            // Single customer
            yield return new object[] 
            { 
                new List<Customer> 
                { 
                    new Customer { Id = 1, Name = "John Doe", Email = "john@example.com" } 
                }, 
                1, 
                "Single customer" 
            };

            // Multiple customers
            yield return new object[] 
            { 
                new List<Customer> 
                { 
                    new Customer { Id = 1, Name = "John Doe", Email = "john@example.com" },
                    new Customer { Id = 2, Name = "Jane Smith", Email = "jane@example.com" },
                    new Customer { Id = 3, Name = "Bob Johnson", Email = "bob@example.com" }
                }, 
                3, 
                "Multiple customers" 
            };

            // Large customer list
            yield return new object[] 
            { 
                Enumerable.Range(1, 10).Select(i => new Customer 
                { 
                    Id = i, 
                    Name = $"Customer {i}", 
                    Email = $"customer{i}@example.com" 
                }).ToList(), 
                10, 
                "Large customer list" 
            };
        }

        [Theory]
        [MemberData(nameof(GetCustomerTestData))]
        public async Task Get_VariousCustomerCounts_ReturnsCorrectCount(
            List<Customer> customers, 
            int expectedCount, 
            string scenario)
        {
            // Arrange
            mockRepository.Setup(repo => repo.GetAllAsync())
                         .ReturnsAsync(customers);

            // Act
            var result = await controller.Get();

            // Assert
            Assert.Equal(expectedCount, result.Count());
        }

        [Theory]
        [MemberData(nameof(GetCustomerTestData))]
        public async Task Get_VariousCustomerLists_ReturnsExactCustomers(
            List<Customer> customers, 
            int expectedCount, 
            string scenario)
        {
            // Arrange
            mockRepository.Setup(repo => repo.GetAllAsync())
                         .ReturnsAsync(customers);

            // Act
            var result = await controller.Get();
            var resultList = result.ToList();

            // Assert
            Assert.Equal(expectedCount, resultList.Count);
            for (int i = 0; i < expectedCount; i++)
            {
                Assert.Equal(customers[i].Id, resultList[i].Id);
                Assert.Equal(customers[i].Name, resultList[i].Name);
                Assert.Equal(customers[i].Email, resultList[i].Email);
            }
        }

        // Simple InlineData test for repository interaction verification
        [Theory]
        [InlineData(0)] // Empty
        [InlineData(1)] // Single
        [InlineData(5)] // Multiple
        [InlineData(10)] // Many
        public async Task Get_DifferentCustomerCounts_CallsRepositoryExactlyOnce(int customerCount)
        {
            // Arrange
            var customers = Enumerable.Range(1, customerCount)
                .Select(i => new Customer { Id = i, Name = $"Customer {i}", Email = $"customer{i}@test.com" })
                .ToList();
            
            mockRepository.Setup(repo => repo.GetAllAsync())
                         .ReturnsAsync(customers);

            // Act
            await controller.Get();

            // Assert - This demonstrates mocking framework verification
            mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        // Test with specific customer data scenarios using InlineData
        [Theory]
        [InlineData(1, "Test User", "test@example.com")]
        [InlineData(999, "Admin User", "admin@hotel.com")]
        [InlineData(42, "Special Customer", "special@vip.com")]
        public async Task Get_SingleSpecificCustomer_ReturnsCorrectCustomerDetails(
            int customerId,
            string customerName,
            string customerEmail)
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { Id = customerId, Name = customerName, Email = customerEmail }
            };
            
            mockRepository.Setup(repo => repo.GetAllAsync())
                         .ReturnsAsync(customers);

            // Act
            var result = await controller.Get();
            var firstCustomer = result.First();

            // Assert
            Assert.Equal(customerId, firstCustomer.Id);
            Assert.Equal(customerName, firstCustomer.Name);
            Assert.Equal(customerEmail, firstCustomer.Email);
        }

        // Complex data scenarios using ClassData
        public class CustomerScenarioData : System.Collections.IEnumerable
        {
            public System.Collections.IEnumerator GetEnumerator()
            {
                // VIP customers scenario
                yield return new object[]
                {
                    new List<Customer>
                    {
                        new Customer { Id = 1, Name = "VIP Customer 1", Email = "vip1@luxury.com" },
                        new Customer { Id = 2, Name = "VIP Customer 2", Email = "vip2@luxury.com" }
                    },
                    "VIP customers scenario"
                };

                // Regular customers scenario
                yield return new object[]
                {
                    new List<Customer>
                    {
                        new Customer { Id = 10, Name = "Regular Customer", Email = "regular@normal.com" }
                    },
                    "Regular customer scenario"
                };
            }
        }

        [Theory]
        [ClassData(typeof(CustomerScenarioData))]
        public async Task Get_ComplexCustomerScenarios_ReturnsExpectedData(
            List<Customer> customers,
            string scenarioDescription)
        {
            // Arrange
            mockRepository.Setup(repo => repo.GetAllAsync())
                         .ReturnsAsync(customers);

            // Act
            var result = await controller.Get();
            var resultList = result.ToList();

            // Assert
            Assert.Equal(customers.Count, resultList.Count);
            Assert.True(customers.All(c => resultList.Any(r => 
                r.Id == c.Id && r.Name == c.Name && r.Email == c.Email)));
        }
    }
}