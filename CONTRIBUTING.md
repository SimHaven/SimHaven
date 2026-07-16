# Contributing to SimHaven

Thank you for helping improve SimHaven's FreeSO-derived client and server.

## Before opening an issue

Use GitHub issues for reproducible source-code bugs and focused feature requests. Account support, player disputes, and questions about playing SimHaven should use SimHaven's normal community support channels instead.

Do not post security vulnerabilities, credentials, private player information, or sensitive server details in a public issue. Follow [SECURITY.md](SECURITY.md) for private reporting.

## Reporting a bug

Open an issue in [SimHaven/SimHaven](https://github.com/SimHaven/SimHaven/issues) and use the bug-report template.

Identify the affected component, such as the game client, patcher, city server, lot server, user API, admin panel, or development tools. Include the version, reproduction steps, exact error, expected result, and testing environment.

Review and redact logs before attaching them. Remove usernames, personal paths, tokens, account information, private URLs, and player data.

## Requesting a feature

Open an issue in [SimHaven/SimHaven](https://github.com/SimHaven/SimHaven/issues) and use the feature-request template. Explain the problem, proposed behavior, alternatives, affected components, and whether the change requires a database, configuration, dependency, or update-system modification.

## Contributing code

1. Fork this repository.
2. Create a clearly named branch, such as `fix/patcher-cleanup` or `feature/admin-events`.
3. Make and test the smallest focused change that solves the problem.
4. Open a pull request targeting `main`.
5. Complete the pull-request checklist and describe how you tested the change.

A pull request is reviewed before any contributed code, installer, script, workflow, or dependency hook is run. Testing must take place away from SimHaven's live deployment.

Maintainers may request changes or decline work that conflicts with SimHaven's direction, security requirements, licensing obligations, or operational needs. Merging source code does not automatically deploy it to SimHaven.

## Do not submit

Never include:

- Credentials, tokens, keys, passwords, or live configuration
- Player, account, database, analytics, or support data
- The live server's `nfs` runtime state, lots, inventories, or persistent objects
- Unreviewed logs, crash reports, database dumps, or backups
- Electronic Arts game files
- SimHaven's private deployment, CDN, website, or release packages
- SimHaven's licensed Discord bot or any of its files
- Compiled binaries or installers unless a maintainer explicitly requests them
- New dependencies or GitHub workflow changes without explaining their purpose, source, and license

## Documentation and testing

Relevant setup and build information is under [Documentation](Documentation). Public configuration examples must use obvious placeholders and must never be copied from a live deployment.

Describe the tests you performed and any client, server, database, or update compatibility risks in your pull request.

## License

By contributing, you agree that your contribution may be distributed under the repository's [Mozilla Public License 2.0](LICENSE.md). FreeSO and third-party copyright and license notices must be preserved.
