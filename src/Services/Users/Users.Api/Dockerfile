FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Services/Users/Users.Domain/Users.Domain.csproj", "src/Services/Users/Users.Domain/"]
COPY ["src/Services/Users/Users.Application/Users.Application.csproj", "src/Services/Users/Users.Application/"]
COPY ["src/Services/Users/Users.Infrastructure/Users.Infrastructure.csproj", "src/Services/Users/Users.Infrastructure/"]
COPY ["src/Services/Users/Users.Api/Users.Api.csproj", "src/Services/Users/Users.Api/"]

RUN dotnet restore "src/Services/Users/Users.Api/Users.Api.csproj"

COPY . .

RUN dotnet publish "src/Services/Users/Users.Api/Users.Api.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "Users.Api.dll"]