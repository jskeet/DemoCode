#!/bin/bash

set -e

cd $1

csc -nologo -langVersion:latest -target:library -out:Library.dll Before.cs
csc -nologo -langVersion:latest -target:exe -out:Client.exe -r:Library.dll Client.cs

# It's okay if this fails, e.g. due to an expected exception.
set +e
./Client.exe > before-result.txt 2>&1
set -e

csc -nologo -target:library -out:Library.dll -langVersion:latest After.cs

# It's okay if this fails
set +e
./Client.exe > after-result-1.txt 2>&1 

# It's okay if compilation fails; only try to run if it succeeds
csc -nologo -langVersion:latest -target:exe -out:Client.exe -r:Library.dll Client.cs \
  > after-result-2.txt 2>&1 \
  && ./Client.exe > after-result-2.txt 2>&1

# Always succeed even if the previous command failed
exit 0
