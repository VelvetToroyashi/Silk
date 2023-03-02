# Build it
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG TARGETPLATFORM

WORKDIR /Silk
# Copy the main source project files
COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done

# Copy the test project files
COPY tests/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p tests/${file%.*}/ && mv $file tests/${file%.*}/; done

# Copy the plugins, too.
COPY plugins/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p plugins/${file%.*}/ && mv $file plugins/${file%.*}/; done

COPY ../Directory.Build.targets ./
COPY ../Silk.sln .

RUN dotnet restore -p Silk

RUN if [ "$TARGETPLATFORM" = "linux/arm64" ] ; then DOTNET_TARGET=linux-musl-arm64 ; else DOTNET_TARGET=linux-musl-x64 ; fi \
    && echo $DOTNET_TARGET > /tmp/rid

WORKDIR /Silk
COPY . ./

RUN echo "Compling for $(cat /tmp/rid)"
RUN dotnet publish ./src/Silk/Silk.csproj --no-restore -c Release -o out -r $(cat /tmp/rid) --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true

# Run it
FROM --platform=$TARGETARCH mcr.microsoft.com/dotnet/aspnet:7.0-alpine

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