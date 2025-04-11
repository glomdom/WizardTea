# WizardTea

NIF types, with a deserializer. Primarily written for usage within [Revive101](https://github.com/Revive101).

**DISCLAIMER:** I have no intention of supporting Bethesda or anything outside the built-in types.

## Developing

Testing your additions is easy.

```
$ # do your changes and stuff, make sure you are in the repository root.
$ cd WizardTea/
$ dotnet run --project ../WizardTea.Generator
$ # generated files will be located in the Generated/ directory
```

## Todo

Simple roadmap. Nothing is in order. May be outdated.

- [ ] versioned fields (will not be implemented unless it breaks everything)
- [ ] deserializing
  - [x] bitflags
  - [x] bitfields
  - [x] struct fields
  - [x] niobject fields
  - [ ] `defaultT` field is not parsed
  - [ ] `#ARG#` conditions
  - [ ] generic conditions
  - [ ] deserialize methods
