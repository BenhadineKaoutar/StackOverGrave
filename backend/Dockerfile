# Use .NET 9 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Use .NET 9 runtime for running
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Set environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production

# Render will set PORT at runtime, we'll bind to it in the app
ENTRYPOINT ["sh", "-c", "dotnet StackOverGrave.Api.dll --urls http://0.0.0.0:${PORT:-5000}"]
