# SimHaven

SimHaven is an independent community server for *The Sims Online*, built on the open-source FreeSO project.

This repository contains the FreeSO-derived SimHaven game client, server, patcher, administration tools, and related development utilities. It contains source code only. It does **not** contain SimHaven's live configuration, player data, databases, website, Discord bot, deployment files, or release packages.

The SimHaven launcher is maintained separately in [SimHaven/SimHaven-Launcher](https://github.com/SimHaven/SimHaven-Launcher).

## Built on FreeSO

SimHaven is built upon [FreeSO](https://github.com/riperiperi/FreeSO), the open-source reimplementation of *The Sims Online* created by Rhys ([riperiperi](https://github.com/riperiperi)) with contributions from the wider FreeSO community.

FreeSO represents years of reverse engineering, engine development, preservation work, and community effort. SimHaven would not exist without that foundation. We are deeply grateful to Rhys ([riperiperi](https://github.com/riperiperi)) and every FreeSO contributor for making their work available under the Mozilla Public License 2.0.

SimHaven maintains and extends FreeSO for its own community. It is an independent project and is not operated, sponsored, or endorsed by the original FreeSO developers or Electronic Arts.

The original FreeSO Git history and contributor attribution are preserved in this repository.

## About the technology

FreeSO is a ground-up reimplementation of *The Sims Online* using MonoGame. Its technology includes hardware rendering, dynamic lighting, high-resolution output, expanded building support, networking, server infrastructure, content tools, and numerous quality-of-life improvements.

SimHaven builds on that technology with changes for its own game service and community.

### 3D mode

The FreeSO engine includes a 3D mode that reconstructs object geometry from the depth information stored with the original sprites. It also generates walls and floors at runtime and provides an alternate camera.

The mode can be enabled with the `-3d` launch parameter. This feature originated in FreeSO.

### Volcanic

Volcanic is FreeSO's live object-development and debugging environment. It can inspect, modify, and save game objects alongside a running SimAntics virtual machine, including editing object scripts and resources.

### Simitone and The Sims 1

Parts of FreeSO's content system, HIT virtual machine, and SimAntics virtual machine also support data from *The Sims*. That work formed a foundation for [Simitone](https://github.com/riperiperi/Simitone), Rhys's separate reimplementation of *The Sims* engine.

## Original game files

This source repository does not distribute Electronic Arts' original game data. Original objects, avatars, user-interface resources, audio, and other game assets are obtained separately during installation.

*The Sims*, *The Sims Online*, and related names and marks belong to Electronic Arts Inc. SimHaven is an independent fan project and is not affiliated with or endorsed by Electronic Arts.

## Repository guide

Most development takes place under `TSOClient`. The repository includes:

- Game clients and shared simulation code
- City, lot, user, and API server components
- Database access and schema tooling
- Client patching and update utilities
- Administration and content-development tools
- FreeSO documentation under `Documentation`

Useful starting points include:

- [Initial setup](Documentation/Initial%20Setup.md)
- [Building FreeSO](Documentation/Building%20FreeSO.md)
- [Database setup](Documentation/Database%20Setup.md)
- [Server configuration](Documentation/Server%20Configuration.md)
- [Server operation guidelines](Documentation/Server%20Operation%20Guidelines.md)
- [Updates](Documentation/Updates.md)

Public examples use placeholder values. Never commit live credentials, private configuration, player information, databases, logs, or deployment material.

## Contributing and security

Bug reports and carefully reviewed contributions are welcome. Please do not include credentials, private user information, copyrighted game data, or live deployment files in an issue or pull request.

External contributions are treated as untrusted until their complete contents have been reviewed and tested away from the live SimHaven service.

Do not publish exploitable security details or private user information in a public issue. Contact the SimHaven maintainers privately before disclosure.

## License

FreeSO and SimHaven modifications in this repository are distributed under the [Mozilla Public License 2.0](LICENSE.md). Existing FreeSO copyright, license, and contributor notices remain applicable.

Individual third-party components and tools may carry their own copyright and license terms. Electronic Arts game data is not covered by the MPL and is not included in this source repository.
