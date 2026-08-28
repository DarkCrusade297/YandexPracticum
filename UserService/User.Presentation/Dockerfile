FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["UserService/User.Presentation/User.Presentation.csproj", "UserService/User.Presentation/"]
COPY ["UserService/User.Application/User.Application.csproj", "UserService/User.Application/"]
COPY ["UserService/User.Infrastructure/User.Infrastructure.csproj", "UserService/User.Infrastructure/"]
COPY ["UserService/User.Domain/User.Domain.csproj", "UserService/User.Domain/"]
RUN dotnet restore "UserService/User.Presentation/User.Presentation.csproj"

COPY . .
WORKDIR /src/UserService/User.Presentation
RUN dotnet publish "User.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "User.Presentation.dll"]

