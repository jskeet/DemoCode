#!/bin/bash

set -e

cd $(readlink -f $(dirname ${BASH_SOURCE}))

echo "Before dotnet run (first)"
dotnet run -- $1
echo "After dotnet run (first)"

echo "Before dotnet run (second)"
dotnet run -- $1
echo "After dotnet run (second)"
