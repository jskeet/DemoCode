#!/bin/bash

set -e

if [[ $PFX_PATH == "" || $PFX_PASSWORD == "" ]]
then
  echo "Please set PFX_PATH and PFX_PASSWORD"
  exit 1
fi

# Build executables
dotnet build -c Release -v quiet VDrumExplorer.Wpf
# Sign executables in-place
signtool sign -a -fd SHA256 -f $PFX_PATH -p $PFX_PASSWORD -t http://timestamp.comodoca.com/authenticode VDrumExplorer.Wpf/bin/Release/netcoreapp3.0/VDrumExplorer.Wpf.exe
signtool sign -a -fd SHA256 -f $PFX_PATH -p $PFX_PASSWORD -t http://timestamp.comodoca.com/authenticode VDrumExplorer.Wpf/bin/Release/net472/VDrumExplorer.Wpf.exe

version=$(grep \<Version\> VDrumExplorer.Wpf/VDrumExplorer.Wpf.csproj | sed s/\<[^\>]*\>//g | sed 's/ //g')

rm -rf tmp
mkdir tmp
cd tmp

# .NET Core
release_dir=VDrumExplorer-DotNetCore-$version
mkdir $release_dir
cp ../VDrumExplorer.Wpf/bin/Release/netcoreapp3.0/* $release_dir
cp ../td17.vdrum $release_dir
cp ../LICENSE* $release_dir
cp ../README.md $release_dir
zip -rq VDrumExplorer-DotNetCore-$version.zip $release_dir

# Desktop
release_dir=VDrumExplorer-Desktop-$version
mkdir $release_dir
cp ../VDrumExplorer.Wpf/bin/Release/net472/* $release_dir
cp ../td17.vdrum $release_dir
cp ../LICENSE* $release_dir
cp ../README.md $release_dir
zip -rq VDrumExplorer-Desktop-$version.zip $release_dir

# Setup
msbuild.exe -property:Configuration=Release -verbosity:quiet ../VDrumExplorer.Setup
cp ../VDrumExplorer.Setup/bin/Release/en-us/VDrumExplorer-Setup.msi .
# Sign the installer as well
signtool sign -a -fd SHA256 -f $PFX_PATH -p $PFX_PASSWORD -t http://timestamp.comodoca.com/authenticode VDrumExplorer-Setup.msi

cd ..
