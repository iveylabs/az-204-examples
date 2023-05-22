FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build-env
RUN apt-get update -yq 
RUN apt-get install curl gnupg python2 -yq 
RUN curl -sL https://deb.nodesource.com/setup_6.x | bash -
RUN apt-get install -y nodejs npm
WORKDIR /app
RUN mkdir PartsUnlimitedWebsite && mkdir PartsUnlimited.Models

# Copy csproj and restore as distinct layers
COPY ./PartsUnlimitedWebsite/PartsUnlimitedWebsite.csproj ./PartsUnlimitedWebsite/
COPY ./PartsUnlimited.Models/PartsUnlimited.Models.csproj ./PartsUnlimited.Models/
WORKDIR /app/PartsUnlimitedWebsite
RUN dotnet restore

WORKDIR /app
# Copy everything else and build
COPY . .
WORKDIR /app/PartsUnlimitedWebsite
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY --from=build-env /app/PartsUnlimitedWebsite/out .
ENTRYPOINT ["dotnet", "PartsUnlimitedWebsite.dll"]