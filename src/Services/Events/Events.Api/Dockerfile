FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Services/Events/Events.Domain/Events.Domain.csproj", "src/Services/Events/Events.Domain/"]
COPY ["src/Services/Events/Events.Application/Events.Application.csproj", "src/Services/Events/Events.Application/"]
COPY ["src/Services/Events/Events.Infrastructure/Events.Infrastructure.csproj", "src/Services/Events/Events.Infrastructure/"]
COPY ["src/Services/Events/Events.Api/Events.Api.csproj", "src/Services/Events/Events.Api/"]

RUN dotnet restore "src/Services/Events/Events.Api/Events.Api.csproj"

COPY . .

RUN dotnet publish "src/Services/Events/Events.Api/Events.Api.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "Events.Api.dll"]