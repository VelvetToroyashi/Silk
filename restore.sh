#!/bin/sh
for i in ./src/*; do
    if [ -d "$i" ]; then
        echo "Restoring $i"
        dotnet restore "$i"/*.csproj 1> /dev/null
    fi
done

echo "Compiling for $TARGET_ARCH. This may take a while."

dotnet publish ./src/Silk/Silk.csproj --no-restore -c Release -r linux-musl-$TARGET_ARCH -o out 1> /dev/null