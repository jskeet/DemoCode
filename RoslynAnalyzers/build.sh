#!/bin/sh

dotnet build -c Release RoslynAnalyzers.slnx
dotnet pack -c Release -o $PWD JonSkeet.RoslynAnalyzers/JonSkeet.RoslynAnalyzers.Package
