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

        
    }
}