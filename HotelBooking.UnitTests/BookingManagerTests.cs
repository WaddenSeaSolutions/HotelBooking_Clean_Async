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

        [Theory]
        [InlineData(1, 2)] // Available dates (future)
        [InlineData(2, 3)]
        [InlineData(5, 6)]
        [InlineData(25, 26)] // Available dates (after existing bookings)
        public async Task CreateBooking_RoomAvailable_ReturnsTrue(int startDaysFromToday, int endDaysFromToday)
        {
            // Arrange
            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(startDaysFromToday),
                EndDate = DateTime.Today.AddDays(endDaysFromToday)
            };

            // Act
            bool result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData(3, 4)]
        [InlineData(25, 26)]
        public async Task CreateBooking_RoomAvailable_SetsRoomIdAndIsActive(int startDaysFromToday, int endDaysFromToday)
        {
            // Arrange
            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(startDaysFromToday),
                EndDate = DateTime.Today.AddDays(endDaysFromToday)
            };

            // Act
            await bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(booking.RoomId >= 0);
            Assert.True(booking.IsActive);
        }

        [Theory]
        [InlineData(10, 15)] // Overlaps with existing booking (10-20)
        [InlineData(15, 20)] // Overlaps with existing booking
        [InlineData(12, 18)] // Within existing booking period
        [InlineData(8, 12)]  // Overlaps start of existing booking
        [InlineData(18, 22)] // Overlaps end of existing booking
        public async Task CreateBooking_NoRoomAvailable_ReturnsFalse(int startDaysFromToday, int endDaysFromToday)
        {
            // Arrange
            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(startDaysFromToday),
                EndDate = DateTime.Today.AddDays(endDaysFromToday)
            };

            // Act
            bool result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(15, 16, 0, false)] // No room available, initial values preserved
        [InlineData(12, 18, 999, true)] // No room available, initial values preserved
        public async Task CreateBooking_NoRoomAvailable_DoesNotModifyBookingProperties(
            int startDaysFromToday, 
            int endDaysFromToday, 
            int initialRoomId, 
            bool initialIsActive)
        {
            // Arrange
            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(startDaysFromToday),
                EndDate = DateTime.Today.AddDays(endDaysFromToday),
                RoomId = initialRoomId,
                IsActive = initialIsActive
            };

            // Act
            await bookingManager.CreateBooking(booking);

            // Assert
            Assert.Equal(initialRoomId, booking.RoomId);
            Assert.Equal(initialIsActive, booking.IsActive);
        }

        // Example using MemberData for more complex test data
        public static IEnumerable<object[]> GetBookingTestData()
        {
            yield return new object[] { DateTime.Today.AddDays(1), DateTime.Today.AddDays(2), true };
            yield return new object[] { DateTime.Today.AddDays(3), DateTime.Today.AddDays(4), true };
            yield return new object[] { DateTime.Today.AddDays(25), DateTime.Today.AddDays(26), true };
            yield return new object[] { DateTime.Today.AddDays(15), DateTime.Today.AddDays(16), false };
            yield return new object[] { DateTime.Today.AddDays(10), DateTime.Today.AddDays(20), false };
        }

        [Theory]
        [MemberData(nameof(GetBookingTestData))]
        public async Task CreateBooking_VariousDateRanges_ReturnsExpectedResult(
            DateTime startDate, 
            DateTime endDate, 
            bool expectedResult)
        {
            // Arrange
            var booking = new Booking
            {
                StartDate = startDate,
                EndDate = endDate
            };

            // Act
            bool result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        // Example using ClassData for even more complex scenarios
        public class BookingTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                // Available room scenarios
                yield return new object[] 
                { 
                    new Booking { StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(2) }, 
                    true, 
                    "Single day booking in future" 
                };
                
                yield return new object[] 
                { 
                    new Booking { StartDate = DateTime.Today.AddDays(25), EndDate = DateTime.Today.AddDays(30) }, 
                    true, 
                    "Multi-day booking after existing reservations" 
                };

                // No room available scenarios
                yield return new object[] 
                { 
                    new Booking { StartDate = DateTime.Today.AddDays(15), EndDate = DateTime.Today.AddDays(16) }, 
                    false, 
                    "Booking during occupied period" 
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(BookingTestData))]
        public async Task CreateBooking_ComplexScenarios_ReturnsExpectedResult(
            Booking booking, 
            bool expectedResult, 
            string scenario)
        {
            // Act
            bool result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        // Repository interaction test with data-driven approach
        [Theory]
        [InlineData(1, 2)]
        [InlineData(3, 4)]
        public async Task CreateBooking_RoomAvailable_AddsBookingToRepository(int startDaysFromToday, int endDaysFromToday)
        {
            // Arrange
            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(startDaysFromToday),
                EndDate = DateTime.Today.AddDays(endDaysFromToday)
            };
            var initialBookingCount = (await bookingRepository.GetAllAsync()).Count();

            // Act
            await bookingManager.CreateBooking(booking);

            // Assert
            var finalBookingCount = (await bookingRepository.GetAllAsync()).Count();
            Assert.Equal(initialBookingCount + 1, finalBookingCount);
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