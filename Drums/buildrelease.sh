#!/bin/bash

set -e

if [[ $PFX_PATH == "" || $PFX_PASSWORD == "" ]]
then
  echo "Please set PFX_PATH and PFX_PASSWORD"
  exit 1
fi

dotnet build -c Release -v quiet VDrumExplorer.Wpf
version=$(grep \<Version\> VDrumExplorer.Wpf/VDrumExplorer.Wpf.csproj | sed s/\<[^\>]*\>//g | sed 's/ //g')

rm -rf tmp
mkdir tmp
cd tmp

# .NET Core
release_dir=VDrumExplorer-DotNetCore-$version
mkdir $release_dir
cp ../VDrumExplorer.Wpf/bin/Release/netcoreapp3.0/* $release_dir
signtool sign -a -fd SHA256 -f $PFX_PATH -p $PFX_PASSWORD -t http://timestamp.comodoca.com/authenticode $release_dir/VDrumExplorer.Wpf.exe
cp ../td17.vdrum $release_dir
cp ../LICENSE* $release_dir
cp ../README.md $release_dir
zip -rq VDrumExplorer-DotNetCore-$version.zip $release_dir

# Desktop
release_dir=VDrumExplorer-Desktop-$version
mkdir $release_dir
cp ../VDrumExplorer.Wpf/bin/Release/net472/* $release_dir
signtool sign -a -fd SHA256 -f $PFX_PATH -p $PFX_PASSWORD -t http://timestamp.comodoca.com/authenticode $release_dir/VDrumExplorer.Wpf.exe
cp ../td17.vdrum $release_dir
cp ../LICENSE* $release_dir
cp ../README.md $release_dir
zip -rq VDrumExplorer-Desktop-$version.zip $release_dir

cd ..
