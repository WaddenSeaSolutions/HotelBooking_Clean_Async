using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Core;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private readonly Mock<IRepository<Booking>> _mockBookingRepository;
        private readonly Mock<IRepository<Room>> _mockRoomRepository;
        private readonly IBookingManager _bookingManager;

        public BookingManagerTests()
        {
            _mockBookingRepository = new Mock<IRepository<Booking>>();
            _mockRoomRepository = new Mock<IRepository<Room>>();
            _bookingManager = new BookingManager(_mockBookingRepository.Object, _mockRoomRepository.Object);
        }

        [Fact]
        public async Task FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException()
        {
            // Arrange
            DateTime date = DateTime.Today;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _bookingManager.FindAvailableRoom(date, date));
        }

        [Theory]
        [InlineData(1)] // Tomorrow
        [InlineData(5)] // 5 days from now
        [InlineData(30)] // 30 days from now
        public async Task FindAvailableRoom_RoomAvailable_ReturnsValidRoomId(int daysFromToday)
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(daysFromToday);
            var availableRooms = new List<Room> 
            { 
                new Room { Id = 1, Description = "Room 1" },
                new Room { Id = 2, Description = "Room 2" }
            };
            var existingBookings = new List<Booking>(); // No existing bookings

            _mockRoomRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(availableRooms);
            _mockBookingRepository.Setup(b => b.GetAllAsync())
                .ReturnsAsync(existingBookings);

            // Act
            int roomId = await _bookingManager.FindAvailableRoom(date, date);

            // Assert
            Assert.True(roomId > 0, "Should return a valid room ID");
            _mockRoomRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockBookingRepository.Verify(b => b.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task FindAvailableRoom_NoRoomsAvailable_ReturnsMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            var rooms = new List<Room> 
            { 
                new Room { Id = 1, Description = "Room 1" }
            };
            var bookings = new List<Booking> 
            { 
                new Booking 
                { 
                    Id = 1, 
                    RoomId = 1, 
                    StartDate = date, 
                    EndDate = date, 
                    IsActive = true 
                }
            };

            _mockRoomRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
            _mockBookingRepository.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);

            // Act
            int roomId = await _bookingManager.FindAvailableRoom(date, date);

            // Assert
            Assert.Equal(-1, roomId);
        }

        [Theory]
        [InlineData(1, 2, true)]   // Available dates
        [InlineData(5, 6, true)]   // Available dates
        [InlineData(1, 1, true)]   // Single day available
        public async Task CreateBooking_RoomAvailable_ReturnsExpectedResult(
            int startDaysFromToday, 
            int endDaysFromToday, 
            bool expectedResult)
        {
            // Arrange
            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(startDaysFromToday),
                EndDate = DateTime.Today.AddDays(endDaysFromToday)
            };

            var availableRooms = new List<Room> 
            { 
                new Room { Id = 1, Description = "Available Room" }
            };
            var existingBookings = new List<Booking>(); // No conflicts

            _mockRoomRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(availableRooms);
            _mockBookingRepository.Setup(b => b.GetAllAsync()).ReturnsAsync(existingBookings);
            _mockBookingRepository.Setup(b => b.AddAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            // Act
            bool result = await _bookingManager.CreateBooking(booking);

            // Assert
            Assert.Equal(expectedResult, result);
            if (expectedResult)
            {
                Assert.True(booking.RoomId > 0, "Room ID should be assigned");
                Assert.True(booking.IsActive, "Booking should be active");
                _mockBookingRepository.Verify(b => b.AddAsync(booking), Times.Once);
            }
        }

        [Theory]
        [InlineData(10, 15, false)] // Overlapping dates
        [InlineData(12, 18, false)] // Within existing booking
        public async Task CreateBooking_NoRoomAvailable_ReturnsFalse(
            int startDaysFromToday, 
            int endDaysFromToday, 
            bool expectedResult)
        {
            // Arrange
            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(startDaysFromToday),
                EndDate = DateTime.Today.AddDays(endDaysFromToday)
            };

            var rooms = new List<Room> 
            { 
                new Room { Id = 1, Description = "Room 1" }
            };
            
            // Existing booking that conflicts
            var existingBookings = new List<Booking> 
            { 
                new Booking 
                { 
                    Id = 1, 
                    RoomId = 1, 
                    StartDate = DateTime.Today.AddDays(10), 
                    EndDate = DateTime.Today.AddDays(20), 
                    IsActive = true 
                }
            };

            _mockRoomRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
            _mockBookingRepository.Setup(b => b.GetAllAsync()).ReturnsAsync(existingBookings);

            // Act
            bool result = await _bookingManager.CreateBooking(booking);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockBookingRepository.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Never, 
                "Should not add booking when no room is available");
        }

        [Fact]
        public async Task CreateBooking_NoRoomAvailable_DoesNotModifyBookingProperties()
        {
            // Arrange
            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(15),
                EndDate = DateTime.Today.AddDays(16),
                RoomId = 999, // Initial value
                IsActive = true // Initial value
            };

            var rooms = new List<Room> { new Room { Id = 1 } };
            var conflictingBookings = new List<Booking> 
            { 
                new Booking 
                { 
                    RoomId = 1, 
                    StartDate = DateTime.Today.AddDays(10), 
                    EndDate = DateTime.Today.AddDays(20), 
                    IsActive = true 
                }
            };

            _mockRoomRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
            _mockBookingRepository.Setup(b => b.GetAllAsync()).ReturnsAsync(conflictingBookings);

            // Act
            bool result = await _bookingManager.CreateBooking(booking);

            // Assert
            Assert.False(result);
            Assert.Equal(999, booking.RoomId); // Should preserve initial value
            Assert.True(booking.IsActive); // Should preserve initial value
        }

        // Data-driven test using MemberData
        public static IEnumerable<object[]> GetBookingScenarios()
        {
            yield return new object[] { 1, 2, true, "Available future dates" };
            yield return new object[] { 25, 26, true, "Available dates after conflicts" };
            yield return new object[] { 15, 16, false, "Conflicting dates" };
        }

        [Theory]
        [MemberData(nameof(GetBookingScenarios))]
        public async Task CreateBooking_VariousScenarios_ReturnsExpectedResult(
            int startDaysFromToday, 
            int endDaysFromToday, 
            bool expectedResult,
            string scenario)
        {
            // Arrange
            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(startDaysFromToday),
                EndDate = DateTime.Today.AddDays(endDaysFromToday)
            };

            // Setup mocks based on scenario
            var rooms = new List<Room> { new Room { Id = 1 } };
            var existingBookings = expectedResult ? new List<Booking>() : new List<Booking> 
            { 
                new Booking 
                { 
                    RoomId = 1, 
                    StartDate = DateTime.Today.AddDays(10), 
                    EndDate = DateTime.Today.AddDays(20), 
                    IsActive = true 
                }
            };

            _mockRoomRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
            _mockBookingRepository.Setup(b => b.GetAllAsync()).ReturnsAsync(existingBookings);
            _mockBookingRepository.Setup(b => b.AddAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            // Act
            bool result = await _bookingManager.CreateBooking(booking);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_StartDateLaterThanEndDate_ThrowsArgumentException()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(2);
            DateTime endDate = DateTime.Today.AddDays(1);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _bookingManager.GetFullyOccupiedDates(startDate, endDate));
        }

        [Fact]
        public async Task GetFullyOccupiedDates_NoRoomsExist_ReturnsEmptyList()
        {
            // Arrange
            DateTime startDate = DateTime.Today;
            DateTime endDate = DateTime.Today.AddDays(5);

            _mockRoomRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Room>());

            // Act
            var result = await _bookingManager.GetFullyOccupiedDates(startDate, endDate);

            // Assert
            Assert.Empty(result);
            _mockRoomRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_NoBookingsExist_ReturnsEmptyList()
        {
            // Arrange
            DateTime startDate = DateTime.Today;
            DateTime endDate = DateTime.Today.AddDays(5);

            _mockRoomRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Room> { new Room { Id = 1 } });
            _mockBookingRepository.Setup(b => b.GetAllAsync())
                .ReturnsAsync(new List<Booking>());

            // Act
            var result = await _bookingManager.GetFullyOccupiedDates(startDate, endDate);

            // Assert
            Assert.Empty(result);
            _mockBookingRepository.Verify(b => b.GetAllAsync(), Times.Once);
        }
    }
}