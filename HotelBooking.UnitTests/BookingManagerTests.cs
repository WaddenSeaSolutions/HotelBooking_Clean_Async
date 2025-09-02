using System;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;
        IRepository<Booking> bookingRepository;

        public BookingManagerTests(){
            DateTime start = DateTime.Today.AddDays(10);
            DateTime end = DateTime.Today.AddDays(20);
            bookingRepository = new FakeBookingRepository(start, end);
            IRepository<Room> roomRepository = new FakeRoomRepository();
            bookingManager = new BookingManager(bookingRepository, roomRepository);
        }

        [Fact]
        public async Task FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException()
        {
            // Arrange
            DateTime date = DateTime.Today;

            // Act
            Task result() => bookingManager.FindAvailableRoom(date, date);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);
            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_ReturnsAvailableRoom()
        {
            // This test was added to satisfy the following test design
            // principle: "Tests should have strong assertions".

            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);

            var bookingForReturnedRoomId = (await bookingRepository.GetAllAsync()).
                Where(b => b.RoomId == roomId
                           && b.StartDate <= date
                           && b.EndDate >= date
                           && b.IsActive);
            
            // Assert
            Assert.Empty(bookingForReturnedRoomId);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_StartDateLaterThanEndDate_ThrowsArgumentException()
        {
            DateTime startDate = DateTime.Today.AddDays(2);
            DateTime endDate = DateTime.Today.AddDays(1);

            Task result() => bookingManager.GetFullyOccupiedDates(startDate, endDate);

            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_NoRoomsExist_ReturnsEmptyList()
        {
            DateTime startDate = DateTime.Today;
            DateTime endDate = DateTime.Today.AddDays(5);

            var result = await bookingManager.GetFullyOccupiedDates(startDate, endDate);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_NoBookingsExist_ReturnsEmptyList()
        {
            DateTime startDate = DateTime.Today;
            DateTime endDate = DateTime.Today.AddDays(5);

            var result = await bookingManager.GetFullyOccupiedDates(startDate, endDate);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_MultipleBookingsOverlap_AllDatesFullyOccupied()
        {
            DateTime startDate = DateTime.Today;
            DateTime endDate = DateTime.Today.AddDays(2);

            var result = await bookingManager.GetFullyOccupiedDates(startDate, endDate);

            Assert.Equal(new List<DateTime> { startDate, startDate.AddDays(1), startDate.AddDays(2) }, result);
        }



    }
}
