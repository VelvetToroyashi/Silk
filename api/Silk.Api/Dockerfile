FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /api
COPY ["Silk.Api/Silk.Api.csproj", "Silk.Api/"]
RUN dotnet restore "api/Silk.Api/Silk.Api.csproj"
COPY . .
WORKDIR "/api/Silk.Api"
RUN dotnet build "Silk.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Silk.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Silk.Api.dll"]
