# Base image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

# Copy .csproj file and restore as distinct layers
COPY PartsUnlimited.PaymentsService.csproj .
RUN dotnet restore --use-current-runtime

# Copy everything else and publish the app
COPY . .
RUN dotnet publish -c Release -o /app --use-current-runtime --self-contained false --no-restore

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS run
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "PartsUnlimited.PaymentsService.dll"]