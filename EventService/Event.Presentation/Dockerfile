FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["EventService/Event.Presentation/Event.Presentation.csproj", "EventService/Event.Presentation/"]
COPY ["EventService/Event.Application/Event.Application.csproj", "EventService/Event.Application/"]
COPY ["EventService/Event.Infrastructure/Event.Infrastructure.csproj", "EventService/Event.Infrastructure/"]
COPY ["EventService/Event.Domain/Event.Domain.csproj", "EventService/Event.Domain/"]
RUN dotnet restore "EventService/Event.Presentation/Event.Presentation.csproj"

COPY . .
WORKDIR /src/EventService/Event.Presentation
RUN dotnet publish "Event.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Event.Presentation.dll"]
