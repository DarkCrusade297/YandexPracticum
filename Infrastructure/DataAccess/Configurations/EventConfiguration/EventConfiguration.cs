using EventManagerSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagerSystem.DataAccess.Configurations.EventConfiguration
{
    public class EventConfiguration : IEntityTypeConfiguration<EventModel>
    {

        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<EventModel> builder)
        {
            builder.ToTable("events");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.Title).IsRequired().HasMaxLength(150);
            builder.Property(e => e.Description).HasMaxLength(1000);
            builder.Property(e => e.StartAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(e => e.EndAt).IsRequired();
            builder.Property(e => e.TotalSeats).IsRequired();
            builder.Property(e => e.AvailableSeats).IsRequired();
            builder.HasMany(e => e.bookingModels).WithOne(b =>  b.Event).HasForeignKey(b => b.EventId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
