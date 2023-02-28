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
} else {
  echo "Detected 'arm64v8' architecture, setting 'arm64'..."
  export BUILD_ARCH=arm64;
}  fi