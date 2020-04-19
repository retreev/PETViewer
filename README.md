# PETViewer

## Build

Building requires the .NET Core 3.1 SDK to be installed.

Build for the platform of your choice, e.g.:
```
dotnet publish -r win-x64 -c Release
```

## Configuration

If a file `init_models` exists in the executable directory the application will automatically load the PET model file specified on the first line.
