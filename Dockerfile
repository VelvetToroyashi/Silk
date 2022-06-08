# Build it
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build

WORKDIR /Silk
COPY . ./
RUN dotnet restore 

RUN dotnet publish ./src/Silk/Silk.csproj -c Release -o out 

# Run it
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine

# Install cultures (same approach as Alpine SDK image)
RUN apk add --no-cache icu-libs

# Disable the invariant mode (set in base image)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Update OpenSSL for the bot to properly work (Discord sucks)
RUN apk upgrade --update-cache --available && \
    apk add openssl && \
    rm -rf /var/cache/apk/*

WORKDIR /Silk
COPY --from=build /Silk/out .

RUN chmod +x ./Silk

CMD ["./Silk"]
