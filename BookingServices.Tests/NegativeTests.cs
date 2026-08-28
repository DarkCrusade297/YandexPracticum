// using Application.Common.Interfaces;
// using Application.DTO.Events;
// using Application.Repositories.Booking;
// using Application.Services;
// using Application.Services.BookingService;
// using Application.Services.EventService;
// using Application.Services.PasswordService;
// using Domain.Enums;
// using Domain.Exceptions;
// using Domain.Models;
// using Infrastructure.DataAccess;
// using Infrastructure.Repositories.Booking;
// using Infrastructure.Repositories.Event;
// using Infrastructure.Repositories.User;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace BookingServices.Tests
// {
//     public class NegativeTests : IDisposable
//     {
//         private readonly ServiceProvider _serviceProvider;
//         private readonly IServiceScope _scope;
//
//         private readonly IEventService _eventService;
//         private readonly IBookingService _bookingService;
//         private readonly IUserRepository _userRepository;
//         private readonly IPasswordService _passwordService;
//         private readonly AppDbContext _context;
//
//         private readonly Guid _testUserId;
//         public NegativeTests()
//         {
//             var dbName = Guid.NewGuid().ToString();
//
//             var services = new ServiceCollection();
//
//             services.AddDbContext<AppDbContext>(options =>
//                 options.UseInMemoryDatabase(dbName));
//
//             services.AddScoped<IUserRepository, UserRepository>();
//             services.AddScoped<IEventRepository, EventRepository>();
//             services.AddScoped<IBookingRepository, BookingRepository>();
//
//             services.AddScoped<IEventService, EventService>();
//             services.AddScoped<IBookingService, BookingService>();
//             services.AddScoped<IPasswordService, PasswordService>();
//
//             _serviceProvider = services.BuildServiceProvider();
//
//             _scope = _serviceProvider.CreateScope();
//
//             _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
//             _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
//             _userRepository = _scope.ServiceProvider.GetRequiredService<IUserRepository>();
//             _passwordService = _scope.ServiceProvider.GetRequiredService<IPasswordService>();
//             _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
//
//             _testUserId = CreateTestUserAsync().GetAwaiter().GetResult();
//         }
//
//         private async Task<Guid> CreateTestUserAsync()
//         {
//             var passwordHash = _passwordService.Hash("Test-Password-123");
//             var user = new UserModel(Guid.NewGuid(), "test-user", passwordHash, UserRoles.User);
//
//             var created = await _userRepository.CreateUserAsync(user);
//             await _userRepository.SaveChangesAsync();
//
//             return created.Id;
//         }
//
//         [Fact]
//         public async Task CreateBookingAsync_NotValidEventId_ReturnsNotFoundException()
//         {
//             // Arrange
//             var nonExistingEventId = Guid.NewGuid();
//
//             // Act & Assert
//             await Assert.ThrowsAsync<NotFoundException>(() =>
//                 _bookingService.CreateBookingAsync(nonExistingEventId, _testUserId));
//
//             Assert.Empty(await _context.Bookings.ToListAsync());
//         }
//
//         [Fact]
//         public async Task GetBookingAsync_NotValidBookingId_ReturnsNotFoundException()
//         {
//             // Arrange
//             var nonExistingBookingId = Guid.NewGuid();
//
//             // Act & Assert
//             await Assert.ThrowsAsync<NotFoundException>(() =>
//                 _bookingService.GetBookingByIdAsync(nonExistingBookingId, _testUserId, UserRoles.User));
//
//             Assert.Empty(await _context.Bookings.ToListAsync());
//         }
//
//         [Fact]
//         public async Task CreateBookingAsync_CreateBookingAfterEventWasDeleted_ReturnsNotFoundException()
//         {
//             // Arrange
//             var createEventDto = new CreateEventDto
//             {
//                 Title = "Title",
//                 Description = "Description",
//                 StartAt = DateTime.UtcNow.AddDays(30),
//                 EndAt = DateTime.UtcNow.AddDays(31),
//                 TotalSeats = 10
//             };
//
//             var createdEvent = await _eventService.CreateEventAsync(createEventDto);
//             var eventId = createdEvent.Id;
//
//             Assert.NotNull(createdEvent);
//             Assert.Equal("Title", createdEvent.Title);
//
//             // Act
//             await _eventService.DeleteEventAsync(eventId);
//
//             // Assert
//             await Assert.ThrowsAsync<NotFoundException>(() =>
//                 _eventService.GetEventAsync(eventId));
//
//             await Assert.ThrowsAsync<NotFoundException>(() =>
//                 _bookingService.CreateBookingAsync(eventId, _testUserId));
//
//             Assert.Empty(await _context.Bookings.ToListAsync());
//         }
//
//         public void Dispose()
//         {
//             _scope.Dispose();
//             _serviceProvider.Dispose();
//         }
//     }
// }
