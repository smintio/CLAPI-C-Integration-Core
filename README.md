# CLAPI-C-Integration-Core
The Smint.io Content Licensing Consumer Integration Core package provides a common codebase for integration to DAM, Web2Print, WCM or other systems, written in .NET Core

**Implemented features**

- Acquiring access and refresh token from Smint.io
- Synchronization of all required Smint.io generic metadata
- Synchronization of all required content and license metadata
- Support for compound assets (aka „multipart“ assets)
- Handling of updates to license purchase transactions that have already been synchronized before
- Live synchronization whenever an asset is being purchased on Smint.io
- Regular synchronization
- Exponential backoff API consumption pattern

**Interesting topics**

*Example*

Please check out our sample implementation at [CLAPI-C-Picturepark-Integration](https://github.com/smintio/CLAPI-C-Picturepark-Integration) to learn more on how to use this package.

*Study the Smint.io Integration Guide*

Please also study the Smint.io Integration Guide which has been provided to you when you signed up as a Smint.io Solution Partner.

**That's it!**

If there is any issues do not hesitate to drop us an email to [support@smint.io](mailto:support@smint.io) and we'll be happy to help!

**Contributors**

- Linda Gratzer, Dataformers GmbH
- Reinhard Holzner, Smint.io Smarter Interfaces GmbH

© 2019 Smint.io Smarter Interfaces GmbH

Licensed under the MIT License