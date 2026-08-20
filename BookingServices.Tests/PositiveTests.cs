using Application.Common.Interfaces;
using Application.DTO.Bookings;
using Application.DTO.Events;
using Application.Repositories.Booking;
using Application.Services;
using Application.Services.BookingService;
using Application.Services.EventService;
using Application.Services.PasswordService;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using Infrastructure.DataAccess;
using Infrastructure.Repositories.Booking;
using Infrastructure.Repositories.Event;
using Infrastructure.Repositories.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingServices.Tests
{
    public class PositiveTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly AppDbContext _context;

        private readonly Guid _testUserId;

        public PositiveTests()
        {
            var dbName = Guid.NewGuid().ToString();

            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddSingleton<IPasswordService, PasswordService>();

            _serviceProvider = services.BuildServiceProvider();

            _scope = _serviceProvider.CreateScope();

            _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
            _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
            _userRepository = _scope.ServiceProvider.GetRequiredService<IUserRepository>();
            _passwordService = _scope.ServiceProvider.GetRequiredService<IPasswordService>();
            _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

            _testUserId = CreateTestUserAsync().GetAwaiter().GetResult();
        }

        private async Task<Guid> CreateTestUserAsync()
        {
            var passwordHash = _passwordService.Hash("Test-Password-123");
            var user = new UserModel(Guid.NewGuid(), "test-user", passwordHash, UserRoles.User);

            var created = await _userRepository.CreateUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return created.Id;
        }

        private async Task<Guid> CreateAdditionalUserAsync(string login)
        {
            var passwordHash = _passwordService.Hash("Test-Password-123");
            var user = new UserModel(Guid.NewGuid(), login, passwordHash, UserRoles.User);

            var created = await _userRepository.CreateUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return created.Id;
        }

        [Fact]
        public async Task CreateBookingAsync_ValidDto_ReturnsBookingInfo()
        {
            var createEventDto = CreateDefaultEventDto(totalSeats: 10);
            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            var booking = await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);

            Assert.NotNull(booking);
            Assert.Equal(createdEvent.Id, booking.EventId);
            Assert.Equal(BookingStatus.Pending, booking.Status);
        }

        [Fact]
        public async Task CreateBookingAsync_ValidDto_AvailableSeatsMinus()
        {
            var createEventDto = CreateDefaultEventDto(totalSeats: 10);
            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            var booking = await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);
            var eventDto = await _eventService.GetEventAsync(createdEvent.Id);

            Assert.NotNull(eventDto);
            Assert.NotNull(booking);
            Assert.Equal(createdEvent.Id, booking.EventId);
            Assert.Equal(BookingStatus.Pending, booking.Status);
            Assert.Equal(9, eventDto.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_ValidDto_BookingsAfterLimit()
        {
            var createEventDto = CreateDefaultEventDto(totalSeats: 1);
            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            var booking = await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);

            Assert.NotNull(booking);
            Assert.Equal(createdEvent.Id, booking.EventId);
            Assert.Equal(BookingStatus.Pending, booking.Status);

            await Assert.ThrowsAsync<NoAvailableSeatsException>(() =>
                _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId));
        }

        [Fact]
        public async Task CreateBookingAsync_ValidDto_BookingsBeforeLimit()
        {
            var createEventDto = CreateDefaultEventDto(totalSeats: 3);
            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            var booking1 = await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);
            var booking2 = await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);
            var booking3 = await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);

            var eventDto = await _eventService.GetEventAsync(createdEvent.Id);

            Assert.NotNull(eventDto);
            Assert.NotNull(booking1);
            Assert.NotNull(booking2);
            Assert.NotNull(booking3);

            Assert.Equal(createdEvent.Id, booking1.EventId);
            Assert.Equal(createdEvent.Id, booking2.EventId);
            Assert.Equal(createdEvent.Id, booking3.EventId);

            Assert.Equal(BookingStatus.Pending, booking1.Status);
            Assert.Equal(BookingStatus.Pending, booking2.Status);
            Assert.Equal(BookingStatus.Pending, booking3.Status);

            Assert.Equal(0, eventDto.AvailableSeats);

            Assert.NotEqual(booking1.Id, booking2.Id);
            Assert.NotEqual(booking1.Id, booking3.Id);
            Assert.NotEqual(booking2.Id, booking3.Id);
        }

        [Fact]
        public async Task CreateBookingAsync_CreateFewBookingForOneEvent_ReturnsBookingInfo()
        {
            var createEventDto = CreateDefaultEventDto(totalSeats: 10);
            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            var booking1 = await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);
            var booking2 = await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);

            Assert.NotNull(booking1);
            Assert.NotNull(booking2);

            Assert.Equal(createdEvent.Id, booking1.EventId);
            Assert.Equal(createdEvent.Id, booking2.EventId);

            Assert.Equal(BookingStatus.Pending, booking1.Status);
            Assert.Equal(BookingStatus.Pending, booking2.Status);

            Assert.NotEqual(booking1.Id, booking2.Id);
        }

        [Fact]
        public async Task GetBookingAsync_ValidDto_ReturnsBooking()
        {
            var createEventDto = CreateDefaultEventDto(totalSeats: 10);
            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            var booking = await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);

            var createdBooking = await _bookingService.GetBookingByIdAsync(booking.Id, _testUserId, UserRoles.User);

            Assert.NotNull(createdBooking);
            Assert.Equal(booking.Id, createdBooking.Id);
            Assert.Equal(BookingStatus.Pending, createdBooking.Status);
            Assert.Null(createdBooking.ProcessedAt);
        }

        [Fact]
        public async Task GetBookingAsync_ValidDto_ReturnsConfirmedStatus()
        {
            var createEventDto = CreateDefaultEventDto(totalSeats: 10);
            var createdEvent = await _eventService.CreateEventAsync(createEventDto);
            var bookingCreatedDto = await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);

            await _bookingService.UpdateBookingAsync(bookingCreatedDto.Id);

            var createdBooking = await _bookingService.GetBookingByIdAsync(bookingCreatedDto.Id, _testUserId, UserRoles.User);

            Assert.NotNull(createdBooking);
            Assert.NotNull(bookingCreatedDto);
            Assert.Equal(bookingCreatedDto.Id, createdBooking.Id);
            Assert.Equal(BookingStatus.Confirmed, createdBooking.Status);
            Assert.NotNull(createdBooking.ProcessedAt);
        }

        [Fact]
        public async Task GetBookingAsync_ValidDto_ReturnsRejectedStatus()
        {
            var createEventDto = CreateDefaultEventDto(totalSeats: 1);
            var createdEvent = await _eventService.CreateEventAsync(createEventDto);
            var bookingCreatedDto = await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);
            var bookingDto = await _bookingService.GetBookingByIdAsync(bookingCreatedDto.Id, _testUserId, UserRoles.User);

            await _bookingService.RejectBookingAsync(bookingDto.Id);

            var rejectedBooking = await _bookingService.GetBookingByIdAsync(bookingDto.Id, _testUserId, UserRoles.User);

            var secondBooking = await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);

            Assert.NotNull(rejectedBooking);
            Assert.NotNull(bookingCreatedDto);
            Assert.NotNull(bookingDto);
            Assert.Equal(bookingDto.Id, rejectedBooking.Id);
            Assert.Equal(BookingStatus.Rejected, rejectedBooking.Status);
            Assert.NotNull(rejectedBooking.ProcessedAt);

            Assert.NotNull(secondBooking);
            Assert.Equal(createdEvent.Id, secondBooking.EventId);
            Assert.Equal(BookingStatus.Pending, secondBooking.Status);
        }

        [Fact]
        public async Task CreateBookingAsync_TenConcurrentRequests_ReturnsTenUniqueBookings()
        {
            const int concurrentRequests = 10;

            var createEventDto = CreateDefaultEventDto(totalSeats: 10);
            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            var tasks = Enumerable.Range(0, concurrentRequests)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                    return await bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);
                }))
                .ToArray();

            var bookings = await Task.WhenAll(tasks);

            using var checkScope = _serviceProvider.CreateScope();
            var eventService = checkScope.ServiceProvider.GetRequiredService<IEventService>();
            var updatedEvent = await eventService.GetEventAsync(createdEvent.Id);

            Assert.NotNull(updatedEvent);
            Assert.Equal(10, bookings.Length);

            Assert.All(bookings, Assert.NotNull);
            Assert.All(bookings, b => Assert.Equal(createdEvent.Id, b!.EventId));
            Assert.All(bookings, b => Assert.Equal(BookingStatus.Pending, b!.Status));

            var uniqueIds = bookings.Select(b => b!.Id).Distinct().ToList();

            Assert.Equal(10, uniqueIds.Count);
            Assert.Equal(0, updatedEvent.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_TwentyConcurrentRequestsFiveSeats_ReturnsExactlyFiveSuccessAndFifteenExceptions()
        {
            const int concurrentRequests = 20;

            var createEventDto = CreateDefaultEventDto(totalSeats: 5);
            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            var tasks = Enumerable.Range(0, concurrentRequests)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                    return await SafeCreateBookingAsync(bookingService, createdEvent.Id, _testUserId);
                }))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            var succeeded = results.Where(r => r.Success).ToList();
            var failed = results.Where(r => !r.Success).ToList();

            using var checkScope = _serviceProvider.CreateScope();
            var context = checkScope.ServiceProvider.GetRequiredService<AppDbContext>();

            var updatedEvent = await context.Events
                .AsNoTracking()
                .FirstAsync(e => e.Id == createdEvent.Id);

            Assert.Equal(5, succeeded.Count);
            Assert.Equal(15, failed.Count);

            Assert.All(succeeded, r => Assert.NotNull(r.Booking));
            Assert.All(succeeded, r => Assert.Equal(createdEvent.Id, r.Booking!.EventId));
            Assert.All(succeeded, r => Assert.Equal(BookingStatus.Pending, r.Booking!.Status));

            Assert.All(failed, r => Assert.IsType<NoAvailableSeatsException>(r.Error));

            var uniqueIds = succeeded.Select(r => r.Booking!.Id).Distinct().ToList();

            Assert.Equal(5, uniqueIds.Count);
            Assert.Equal(0, updatedEvent.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_EventAlreadyPassed_ReturnsEventAlreadyPassedException()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "Past Event",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(10),
                EndAt = DateTime.UtcNow.AddDays(11),
                TotalSeats = 10
            };

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);
            var eventId = createdEvent.Id;

            var eventEntity = await _context.Events.SingleAsync(e => e.Id == eventId);
            eventEntity.StartAt = DateTime.UtcNow.AddDays(-10);
            eventEntity.EndAt = DateTime.UtcNow.AddDays(-9);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<EventAlreadyPassedException>(() =>
                _bookingService.CreateBookingAsync(eventId, _testUserId));

            Assert.Empty(await _context.Bookings.ToListAsync());
        }

        [Fact]
        public async Task CreateBookingAsync_UserReachedActiveBookingsLimit_ThrowsBookingLimitExceededException()
        {
            // Arrange
            var createEventDto = CreateDefaultEventDto(totalSeats: BookingLimitExceededException.MaxActiveBookingsPerUser + 5);
            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            // Act
            for (var i = 0; i < BookingLimitExceededException.MaxActiveBookingsPerUser; i++)
            {
                var booking = await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);
                Assert.NotNull(booking);
                Assert.Equal(BookingStatus.Pending, booking.Status);
            }

            // Assert
            await Assert.ThrowsAsync<BookingLimitExceededException>(() =>
                _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId));

            var bookingsCount = await _context.Bookings.CountAsync(b => b.UserId == _testUserId);
            Assert.Equal(BookingLimitExceededException.MaxActiveBookingsPerUser, bookingsCount);
        }

        [Fact]
        public async Task CreateBookingAsync_TwoUsersIndependentLimits_SecondUserNotAffectedByFirstUsersLimit()
        {
            // Arrange
            var secondUserId = await CreateAdditionalUserAsync("second-test-user");

            var createEventDto = CreateDefaultEventDto(totalSeats: BookingLimitExceededException.MaxActiveBookingsPerUser * 2 + 5);
            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            // Act — первый пользователь доходит до своего лимита
            for (var i = 0; i < BookingLimitExceededException.MaxActiveBookingsPerUser; i++)
            {
                await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);
            }

            await Assert.ThrowsAsync<BookingLimitExceededException>(() =>
                _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId));

            // Act
            var secondUserBooking = await _bookingService.CreateBookingAsync(createdEvent.Id, secondUserId);

            // Assert
            Assert.NotNull(secondUserBooking);
            Assert.Equal(BookingStatus.Pending, secondUserBooking.Status);
            Assert.Equal(secondUserId, secondUserBooking.UserId);

            var firstUserBookingsCount = await _context.Bookings.CountAsync(b => b.UserId == _testUserId);
            var secondUserBookingsCount = await _context.Bookings.CountAsync(b => b.UserId == secondUserId);

            Assert.Equal(BookingLimitExceededException.MaxActiveBookingsPerUser, firstUserBookingsCount);
            Assert.Equal(1, secondUserBookingsCount);
        }

        [Fact]
        public async Task CancelBookingAsync_OtherUserTriesToCancel_ThrowsForbiddenOperationException()
        {
            var createEventDto = CreateDefaultEventDto(totalSeats: 10);
            var createdEvent = await _eventService.CreateEventAsync(createEventDto);
            var booking = await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);

            var strangerId = await CreateAdditionalUserAsync("stranger-user");

            await Assert.ThrowsAsync<ForbiddenOperationException>(() =>
                _bookingService.CancelBookingAsync(booking.Id, strangerId, UserRoles.User));

            var stillActive = await _bookingService.GetBookingByIdAsync(booking.Id, _testUserId, UserRoles.User);
            Assert.Equal(BookingStatus.Pending, stillActive.Status);
            Assert.Null(stillActive.ProcessedAt);

            var eventAfter = await _eventService.GetEventAsync(createdEvent.Id);
            Assert.Equal(9, eventAfter.AvailableSeats);
        }

        [Fact]
        public async Task CancelBookingAsync_OwnerCancelsOwnBooking_ReturnsCancelledStatusAndReleasesSeat()
        {
            var createEventDto = CreateDefaultEventDto(totalSeats: 1);
            var createdEvent = await _eventService.CreateEventAsync(createEventDto);
            var booking = await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);

            await _bookingService.CancelBookingAsync(booking.Id, _testUserId, UserRoles.User);

            var cancelled = await _bookingService.GetBookingByIdAsync(booking.Id, _testUserId, UserRoles.User);
            Assert.Equal(BookingStatus.Cancelled, cancelled.Status);
            Assert.NotNull(cancelled.ProcessedAt);

            var eventAfter = await _eventService.GetEventAsync(createdEvent.Id);
            Assert.Equal(1, eventAfter.AvailableSeats);
        }

        [Fact]
        public async Task CancelBookingAsync_AdminCancelsAnotherUsersBooking_ReturnsCancelledStatus()
        {
            var adminId = await CreateAdditionalUserAsync("admin-user", UserRoles.Admin);

            var createEventDto = CreateDefaultEventDto(totalSeats: 10);
            var createdEvent = await _eventService.CreateEventAsync(createEventDto);
            var booking = await _bookingService.CreateBookingAsync(createdEvent.Id, _testUserId);

            await _bookingService.CancelBookingAsync(booking.Id, adminId, UserRoles.Admin);

            var cancelled = await _bookingService.GetBookingByIdAsync(booking.Id, _testUserId, UserRoles.User);
            Assert.Equal(BookingStatus.Cancelled, cancelled.Status);
            Assert.NotNull(cancelled.ProcessedAt);
        }

        private static CreateEventDto CreateDefaultEventDto(int totalSeats)
        {
            return new CreateEventDto
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(30),
                EndAt = DateTime.UtcNow.AddDays(31),
                TotalSeats = totalSeats
            };
        }

        private static async Task<(bool Success, CreatedBookingDto? Booking, Exception? Error)> SafeCreateBookingAsync(
            IBookingService bookingService,
            Guid eventId,
            Guid userId)
        {
            try
            {
                var booking = await bookingService.CreateBookingAsync(eventId, userId);
                return (true, booking, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex);
            }
        }

        public void Dispose()
        {
            _scope.Dispose();
            _serviceProvider.Dispose();
        }

        private async Task<Guid> CreateAdditionalUserAsync(string login, UserRoles role = UserRoles.User)
        {
            var passwordHash = _passwordService.Hash("Test-Password-123");
            var user = new UserModel(Guid.NewGuid(), login, passwordHash, role);

            var created = await _userRepository.CreateUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return created.Id;
        }
    }
}
