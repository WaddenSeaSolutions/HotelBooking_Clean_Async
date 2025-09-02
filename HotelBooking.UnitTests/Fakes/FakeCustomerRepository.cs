using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.Core;

namespace HotelBooking.UnitTests.Fakes
{
    public class FakeCustomerRepository : IRepository<Customer>
    {
        // This field is exposed so that a unit test can validate that the
        // Add method was invoked.
        public bool addWasCalled = false;
        public Task AddAsync(Customer entity)
        {
            addWasCalled = true;
            return Task.CompletedTask;
        }

        // This field is exposed so that a unit test can validate that the
        // Edit method was invoked.
        public bool editWasCalled = false;
        public Task EditAsync(Customer entity)
        {
            editWasCalled = true;
            return Task.CompletedTask;
        }

        public Task<Customer> GetAsync(int id)
        {
            Task<Customer> customerTask = Task.Factory.StartNew(() => new Customer { Id = 1, Name = "John Doe", Email = "john@example.com" });
            return customerTask;
        }

        public Task<IEnumerable<Customer>> GetAllAsync()
        {
            IEnumerable<Customer> customers = new List<Customer>
            {
                new Customer { Id = 1, Name = "John Doe", Email = "john@example.com" },
                new Customer { Id = 2, Name = "Jane Smith", Email = "jane@example.com" },
            };
           
            Task<IEnumerable<Customer>> customersTask = Task.Factory.StartNew(() => customers);
            return customersTask;
        }

        // This field is exposed so that a unit test can validate that the
        // Remove method was invoked.
        public bool removeWasCalled = false;
        public Task RemoveAsync(int id)
        {
            removeWasCalled = true;
            return Task.CompletedTask;
        }
    }
}