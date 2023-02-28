# Build it
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build

# https://github.com/moby/moby/issues/34129 for explaination of this
ARG TARGETARCH

WORKDIR /Silk
COPY . ./

# Really a restore script, oops
RUN chmod +x ./restore.sh && ./restore.sh 

# Run it
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:7.0-alpine

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
