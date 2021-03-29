# Build it
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build

WORKDIR /Silk
COPY . ./
RUN dotnet restore

RUN dotnet publish ./src/Silk.Core/Silk.Core.csproj -c Release -o out 

# Run it
FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine

# Update OpenSSL for the bot to properly work (Discord sucks)
RUN apk upgrade --update-cache --available && \
    apk add openssl && \
    rm -rf /var/cache/apk/*

WORKDIR /Silk
COPY --from=build /Silk/out .

RUN chmod +x ./Silk.Core

CMD ["./Silk.Core"]
