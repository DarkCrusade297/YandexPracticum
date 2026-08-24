FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BookingService/Booking.Presentation/Booking.Presentation.csproj", "BookingService/Booking.Presentation/"]
COPY ["BookingService/Booking.Application/Booking.Application.csproj", "BookingService/Booking.Application/"]
COPY ["BookingService/Booking.Infrastructure/Booking.Infrastructure.csproj", "BookingService/Booking.Infrastructure/"]
COPY ["BookingService/Booking.Domain/Booking.Domain.csproj", "BookingService/Booking.Domain/"]
RUN dotnet restore "BookingService/Booking.Presentation/Booking.Presentation.csproj"

COPY . .
WORKDIR /src/BookingService/Booking.Presentation
RUN dotnet publish "Booking.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Booking.Presentation.dll"]

