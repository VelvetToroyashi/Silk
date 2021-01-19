# Build it
FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build

WORKDIR /silk
COPY . ./
RUN dotnet restore

RUN dotnet publish ./src/Silk.Core/Silk.Core.csproj -c Release -o out 

# Run it
FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine

# Update OpenSSL for the bot to properly work (Discord sucks)
RUN apk upgrade --update-cache --available && \
    apk add openssl && \
    rm -rf /var/cache/apk/*

WORKDIR /silk
COPY --from=build /silk/out .

RUN chmod +x ./Silk.Core

CMD ["./Silk.Core"]
