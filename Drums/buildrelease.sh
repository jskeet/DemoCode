#!/bin/bash

set -e

if [[ $PFX_PATH == "" || $PFX_PASSWORD == "" ]]
then
  echo "Please set PFX_PATH and PFX_PASSWORD"
  exit 1
fi

# Build executables
dotnet build -c Release -v quiet VDrumExplorer.Wpf
# Sign executable in-place
signtool sign -a -fd SHA256 -f $PFX_PATH -p $PFX_PASSWORD -t http://timestamp.comodoca.com/authenticode VDrumExplorer.Wpf/bin/Release/net472/VDrumExplorer.Wpf.exe

version=$(grep \<Version\> VDrumExplorer.Wpf/VDrumExplorer.Wpf.csproj | sed s/\<[^\>]*\>//g | sed 's/ //g')

rm -rf tmp
mkdir tmp
cd tmp

# .NET Core build has been removed due to NAudio requirements

# Desktop
release_dir=VDrumExplorer-$version
mkdir $release_dir
cp ../VDrumExplorer.Wpf/bin/Release/net472/* $release_dir
cp ../td17.vdrum $release_dir
cp ../LICENSE* $release_dir
cp ../README.md $release_dir
zip -rq VDrumExplorer-$version.zip $release_dir

# Setup
msbuild.exe -property:Configuration=Release -verbosity:quiet ../VDrumExplorer.Setup
cp ../VDrumExplorer.Setup/bin/Release/en-us/VDrumExplorer-Setup.msi .
# Sign the installer as well
signtool sign -a -fd SHA256 -f $PFX_PATH -p $PFX_PASSWORD -t http://timestamp.comodoca.com/authenticode VDrumExplorer-Setup.msi

cd ..
