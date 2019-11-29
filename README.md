Smint.io CLAPI-C .NET Core integration core library
===================================================

The Smint.io Content Licensing Consumer Integration Core package provides a
common codebase for integration to
[digital asset management (DAM)](https://en.wikipedia.org/wiki/Digital_asset_management),
Web2Print, WCM or other systems, written in .NET Core.


Current version is: 1.0.0


Requirements
------------

### *Smint.io Content Licensing Consumer API library* - add credentials to load the library

This library depends on the *Smint.io Content Licensing Consumer API library ("CLAPI-C")*, that will handle
all connection to the RESTful Smint.io API. Access to the *CLAPI-C* library is
restricted. Get in contact with [Smint.io](https://www.smint.io) and request
access. You will need to sign an NDA first.

You will need an account with Microsoft Visual Studio cloud offerings, as
the CLAPI-C library is hosted there.


Implemented features
--------------------

- Acquiring access and refresh token from Smint.io
- Synchronization of all required Smint.io generic metadata
- Synchronization of all required content and license metadata
- Support for compound assets (aka „multipart“ assets)
- Handling of updates to license purchase transactions that have already been synchronized before
- Live synchronization whenever an asset is being purchased on Smint.io
- Regular synchronization
- Exponential backoff API consumption pattern



Requirements
------------

1.  [.NET Core 2.2](https://dotnet.microsoft.com/download/dotnet-core/2.2)

2.  [NuGet package manager](https://docs.microsoft.com/en-us/nuget/install-nuget-client-tools)

3.  [Azure Artifacts Credential Provider](https://github.com/microsoft/artifacts-credprovider#azure-artifacts-credential-provider)
    in order to authorize dotnet build system with Azure development
    platform. On first run, use `dotnet build --interactive` to enable
    interactive authorization with Azure OAuth system.

4.  add the feed to the [Smint.io SmintIo.CLAPI.Consumer.Client library](https://smintio.visualstudio.com/CLAPIC-API-Clients)
    to NuGet package manager. It will enable the build system to download
    the necessary Smint.io SDK.

    ```
    nuget sources Add -Name "(Smint.io) CLAPI-C-Clients" -Source "https://smintio.pkgs.visualstudio.com/_packaging/CLAPIC-API-Clients/nuget/v3/index.json"
    ```

    On first build, use `dotnet build --interactive` to enable
    interactive authorization with Azure OAuth system.
    *However, on OS X, nuget command line does not activate OAuth
    authorization. So you need to use any IDE to do this task. Afterwards
    command line tools `dotnet` works seemlessly as it will find the session
    token created with the IDE.

5.  [DocFX documentation builder](https://dotnet.github.io/docfx/) and
    ensure it can be found in the command path. Verify proper install with
    the command

    ```
    docfx --version
    ```


Topics of interest
------------------

### Example

Please check out our sample implementation at
[CLAPI-C-Picturepark-Integration](https://github.com/smintio/CLAPI-C-Picturepark-Integration)
to learn more on how to use this package.



### Study the Smint.io Integration Guide

Please also study the Smint.io Integration Guide which has been provided to you when you signed up as a Smint.io Solution Partner.


### That's it!

If there is any issues do not hesitate to drop us an email to [support@smint.io](mailto:support@smint.io) and we'll be happy to help!

Contributors
------------

- Linda Gratzer, Dataformers GmbH
- Reinhard Holzner, Smint.io GmbH

© 2019 [Smint.io GmbH](https://www.smint.io)

Licensed under the [MIT License](https://opensource.org/licenses/MIT)
