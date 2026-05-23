#!/bin/sh
# UjangKeduAwoo.sh - Jalankan bot UjangKeduAwoo
# Mode development: selalu bersihkan, rebuild, dan jalankan.
# Mode release (dikomentari): jalankan tanpa rebuild.

# Development mode: selalu rebuild
rm -rf bin obj
dotnet build
dotnet run --no-build

# Uncomment di bawah untuk release mode (jalankan tanpa rebuild)
# if [ -d "bin" ]; then
#   dotnet run --no-build
# else
#   dotnet build
#   dotnet run --no-build
# fi
