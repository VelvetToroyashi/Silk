#!/bin/sh
for i in ./src/*; do
    if [ -d "$i" ]; then
        echo "Restoring $i"
        dotnet restore "$i"/*.csproj 1> /dev/null
    fi
done

if [ "$TARGETARCH" = "arm64" ]; then 
{ 
  echo "Compiling for ARM"; 
  dotnet publish ./src/Silk/Silk.csproj --no-restore -c Release -r linux-musl-arm64 -o out; 
} 
else 
{ 
  echo "Compiling for $TARGETARCH"; 
  dotnet publish ./src/Silk/Silk.csproj --no-restore -c Release -r linux-musl-x64 -o out; 
}  
fi