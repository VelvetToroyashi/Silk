# Build it
ARG ARCH=amd64
FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine3.17 AS build

RUN bash -c if [ "$ARCH" = "arm64" ]; then export DOTNET_BUILD_ARCH=linux-arm64; else export DOTNET_BUILD_ARCH=linux-x64; fi

WORKDIR /Silk
COPY . ./

RUN sh ./restore.sh && dotnet publish ./src/Silk/Silk.csproj -c Release -o out --no-restore -r $DOTNET_BUILD_ARCH

# Run it
ARG ARCH=amd64
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine3.17

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
