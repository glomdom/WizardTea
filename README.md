# WizardTea

NIF types, with a deserializer. Primarily written for usage within [Revive101](https://github.com/Revive101).

**DISCLAIMER:** I have no intention of supporting Bethesda or Bethesda Havok types.

## Developing

Testing your additions is easy.

```
$ # do your changes and stuff, make sure you are in the repository root.
$ cd WizardTea/
$ dotnet run --project ../WizardTea.Generator
$ # generated files will be located in the Generated/ directory
```