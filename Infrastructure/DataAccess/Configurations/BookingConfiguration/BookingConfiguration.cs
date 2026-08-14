using Domain.Enums;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Configurations.BookingConfiguration
{
    public class BookingConfiguration : IEntityTypeConfiguration<BookingEntity>
    {
        public void Configure(EntityTypeBuilder<BookingEntity> builder)
        {
            builder.ToTable("bookings");
            builder.HasKey(b => b.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(b => b.EventId).IsRequired();
            builder.Property(b => b.Status).IsRequired().HasConversion<string>().HasMaxLength(20).HasDefaultValue(BookingStatus.Pending);
            builder.Property(b => b.CreatedAt).IsRequired();
            builder.Property(b => b.ProcessedAt).IsRequired(false);
            builder.HasIndex(b => b.EventId);
            builder.HasIndex(b => b.Status);
        }
    }
}
