#!/bin/bash

set -e

if [[ -z "$1" ]]
then
  echo "Please specify the version number"
  exit 1
fi

export VERSION=$1

sed -i s/Development/$VERSION.0/g DigiMixer.Wpf/Versions.cs

echo "Restoring MSIX project"
rm -rf DigiMixer.Msix/{obj,bin}
msbuild.exe \
  -nologo \
  -p:Configuration=Release \
  -p:Platform=x64 \
  -verbosity:quiet \
  -t:restore \
  DigiMixer.Msix

echo "Building C# code"

# Publish the WPF app
dotnet publish -nologo -clp:NoSummary -v quiet -c Release --self-contained -p:RuntimeIdentifier=win-x64 -p:Platform=x64 DigiMixer.Wpf

sed -i -E "s/^    Version=\".*\" \/>/    Version=\"$VERSION.0\" \/>/g" DigiMixer.Msix/Package.appxmanifest

echo "Building MSIX"
msbuild.exe \
  -nologo \
  -p:Configuration=Release \
  -p:Platform=x64 \
  -verbosity:quiet \
  DigiMixer.Msix

sed -i s/$VERSION.0/Development/g DigiMixer.Wpf/Versions.cs

echo "Copying MSIX into tmp directory"
rm -rf tmp
mkdir tmp
cp DigiMixer.Msix/AppPackages/DigiMixer.Msix_$VERSION.0_x64_Test/DigiMixer.Msix_$VERSION.0_x64.msix \
   tmp/DigiMixer-$VERSION.0.msix
