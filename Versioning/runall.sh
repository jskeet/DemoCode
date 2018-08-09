#!/bin/sh

set -e

rm -rf output
for i in examples/*.txt; do
  echo $i
  dotnet run -p ExtractCode -- $i output
  ./runcode.sh output/$(echo $i | sed s/examples//g | sed s/.txt//g)
done
