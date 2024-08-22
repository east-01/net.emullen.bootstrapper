# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.2] - 08-21-2024

Some scripts had errors and example scenes were missing, added them.

### Fixed

- TimeConsumer uses namespace EMullen.Bootstrapper now
- Example Bootstrap/Gameplay scenes provided

### Removed

- Uneccessary TimeConsumer message

## [1.0.1] - 08-21-2024

Package doesn't import properly, fixing.

### Fixed

- Root-folder .meta files since unity needs them to create your package
- All .meta files are necessary, adding them back

## [1.0.0] - 08-21-2024

Initial commit/version. Package overview is in README.md

### Added

- Bootstrapper: Goes in a scene and follows a set BootstrapSequence or initiates one if one is not 
  set.
- BootstrapSequenceManager: Orchestrates the BootstrapSequence, is a Singleton structure that is
  NOT a MonoBehaviour. Recieves updates from Bootstrapper MonoBehaviours.
- BootstrapSequence: A set order of scene build indexes for bootstrap scenes, and a list of target
  scenes to load once the BootstrapSequence has finished. It also contains some settings.
- Custom Bootstrapper editor and BootstrapSequence property drawer that better illustrates how the 
  BootstrapSequence works.
- Example Bootstrap and GameplayScenes with accompanied scripts to run them in 
  Runtime/SampleScripts