# Utility projects for WPF (and other MVVM apps)

This directory contains utility projects which I (Jon Skeet) have
found useful in multiple apps. I make no claim that they're
*generally* useful, and they're certainly not curated with the same
level of detail I'd apply in a more formal setting.

You're welcome to fetch the code here and build it; it's unlikely
that I'll publish it on NuGet any time soon.

Licensed under the Apache 2.0 licence, as is everything in this repo.

### JonSkeet.CoreAppUtil

This targets vanilla .NET 8, with "simple" dependencies for Noda
Time, JSON and logging support. It provides ViewModel functionality,
as well as code I commonly use for loading and saving JSON
configuration files.

This code can be used from console apps, WPF apps and MAUI.

### JonSkeet.WpfUtil

This is WPF-specific, building on JonSkeet.CoreAppUtil for
WPF-specific functionality.

### JonSkeet.WpfLogging

Provides in-memory logging and a WPF view of the in-memory log.
