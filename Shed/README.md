# Building the Shed Voice Control System

This probably isn't useful for anyone other than me (Jon Skeet), but
if you *do* want to build and run this code:

- Build the Shed.Controllers and Shed.CommandLine projects with
  dotnet cli
- Pack the Shed.Controllers project with `dotnet pack`
- Copy the package into a local NuGet source
- Open the solution in Visual Studio, and the Shed.Uwp project should
  then be able to build, by fetching the package you've just created.
  Project-to-project references from csproj to xproj files don't work
  in VS. (Eventually it'll all be VS, of course.)
