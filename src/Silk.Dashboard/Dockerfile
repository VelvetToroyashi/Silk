# Build it
FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build

WORKDIR /dashboard
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Run it
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine

WORKDIR /dashboard
COPY --from=build /dashboard/out .

ENTRYPOINT ["dotnet", "Silk.Dashboard.dll"]