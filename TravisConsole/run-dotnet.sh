#!/bin/bash

set -e

cd $(readlink -f $(dirname ${BASH_SOURCE}))

echo "Before dotnet run (first)"
dotnet run -- $1
echo "After dotnet run (first)"

echo "Before dotnet run (second)"
dotnet run -- $1
echo "After dotnet run (second)"

echo "Before dotnet run --no-build"
dotnet run --no-build -- $1
echo "After dotnet run --no-build "

echo "Before dotnet dll"
dotnet bin/Debug/netcoreapp3.1/TravisConsole.dll $1
echo "After dotnet dll"
