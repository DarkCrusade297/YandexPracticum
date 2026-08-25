using Event.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Event.Infrastructure.DataAccess;

public sealed class ProcessedBookingConfiguration : IEntityTypeConfiguration<ProcessedBookingEntity>
{
    public void Configure(EntityTypeBuilder<ProcessedBookingEntity> builder)
    {
        builder.ToTable("processed_bookings");
        builder.HasKey(entity => entity.BookingId);
        builder.Property(entity => entity.BookingId).ValueGeneratedNever();
        builder.Property(entity => entity.EventId).IsRequired();
        builder.Property(entity => entity.ProcessedAt).IsRequired();
    }
}
