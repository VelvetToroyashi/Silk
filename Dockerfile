# Build it
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /silk
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Run it
FROM mcr.microsoft.com/dotnet/core/runtime:3.1

WORKDIR /silk
RUN mkdir -p /silk/SilkBot/ServerConfigs
COPY --from=build /silk/out .

RUN chmod +x ./Silk!

CMD ["./Silk!"]