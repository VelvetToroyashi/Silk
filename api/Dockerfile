FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /Silk

COPY . .

COPY ["./Silk.Api/Silk.Api.csproj", "Silk.Api/"]
RUN dotnet restore "Silk.Api/Silk.Api.csproj"
WORKDIR "./Silk.Api"
RUN dotnet build "Silk.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Silk.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Silk.Api.dll"]