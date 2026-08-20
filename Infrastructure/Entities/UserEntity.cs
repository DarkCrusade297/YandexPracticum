using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public UserRoles Role { get; set; }

        public ICollection<BookingEntity> Bookings { get; set; } = new List<BookingEntity>();
    }
}
