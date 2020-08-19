# Build it
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /silk
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Run it
FROM mcr.microsoft.com/dotnet/core/runtime:3.1

# Gotta fix an issue where it complains about not finding a path
WORKDIR /silk
COPY --from=build /silk/out .

RUN chmod +x ./Silk!

CMD ["./Silk!"]