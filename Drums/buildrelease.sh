#!/bin/bash

set -e

dotnet build -c Release VDrumExplorer.Wpf
version=$(grep \<Version\> VDrumExplorer.Wpf/VDrumExplorer.Wpf.csproj | sed s/\<[^\>]*\>//g | sed 's/ //g')

rm -rf tmp
mkdir tmp
cd tmp
mkdir VDrumExplorer-$version
cp ../VDrumExplorer.Wpf/bin/Release/net472/* VDrumExplorer-$version
cp ../td17.vdrum VDrumExplorer-$version
zip -r VDrumExplorer-$version.zip VDrumExplorer-$version
cd ..
