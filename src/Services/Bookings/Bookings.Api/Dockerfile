FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Shared/Messaging.Contracts/Messaging.Contracts.csproj", "src/Shared/Messaging.Contracts/"]

COPY ["src/Services/Bookings/Bookings.Domain/Bookings.Domain.csproj", "src/Services/Bookings/Bookings.Domain/"]
COPY ["src/Services/Bookings/Bookings.Application/Bookings.Application.csproj", "src/Services/Bookings/Bookings.Application/"]
COPY ["src/Services/Bookings/Bookings.Infrastructure/Bookings.Infrastructure.csproj", "src/Services/Bookings/Bookings.Infrastructure/"]
COPY ["src/Services/Bookings/Bookings.Api/Bookings.Api.csproj", "src/Services/Bookings/Bookings.Api/"]

RUN dotnet restore "src/Services/Bookings/Bookings.Api/Bookings.Api.csproj"

COPY . .

RUN dotnet publish "src/Services/Bookings/Bookings.Api/Bookings.Api.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "Bookings.Api.dll"]