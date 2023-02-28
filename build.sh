#!/bin/sh
for i in ./src/*; do
    if [ -d "$i" ]; then
        echo "Restoring $i"
        dotnet restore "$i"/*.csproj 1> /dev/null
    fi
done

echo "Setting architecture variable for dotnet..."
if [ "$ARCH" = "amd64" ]; then {
  echo "Detected 'amd64' architecture, setting 'x64'..."
  export BUILD_ARCH=x64
} elif [ "$1" = "--musl"]; then 
{
  echo "Detected arm64v8 (with MUSL), setting 'musl-arm64..."
  export BUILD_ARCH=musl-arm64    
} else {
  echo "Detected 'arm64v8' architecture, setting 'arm64'..."
  export BUILD_ARCH=arm64;
}  fi

echo "Architecture set to $BUILD_ARCH"

echo "Building. This may take a while..."
dotnet publish ./src/Silk/Silk.csproj -c Release -o out --no-restore -r linux-$BUILD_ARCH