# .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY DiyanetNamazVakti.Api/DiyanetNamazVakti.Api.csproj DiyanetNamazVakti.Api/
RUN dotnet restore DiyanetNamazVakti.Api/DiyanetNamazVakti.Api.csproj

# Copy everything else and build
COPY DiyanetNamazVakti.Api/ DiyanetNamazVakti.Api/
WORKDIR /src/DiyanetNamazVakti.Api
RUN dotnet build -c Release -o /app/build

# Publish
FROM build AS publish
WORKDIR /src/DiyanetNamazVakti.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Copy published app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "DiyanetNamazVakti.Api.dll"]
