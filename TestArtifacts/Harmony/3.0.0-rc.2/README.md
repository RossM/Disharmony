# Harmony 3 compatibility dependency

`net10.0/0Harmony.dll` is the merged .NET 10 assembly from the official
[Harmony 3.0.0-rc.2 release](https://github.com/pardeike/Harmony/releases/tag/v3.0.0-rc.2).
It is used for the Harmony 3 compatibility tests and the RimWorld 1.7 mod build.

- Source commit: `1fea0264584637a8a530c413263dfadb249f8de7`
- Download: `Harmony-Fat.3.0.0.0-rc.2.zip` from that release
- Archive SHA-256: `38A8568BA2BA03D3CC60537404717CB2A43D3CD5A60F4DA30BE216CCE3BC36EB`
- DLL SHA-256: `94DD929A06F720869F6A718A64BC6713A37DBE153C2F4C16C031B2587D7EE694`

The adjacent `LICENSE` contains Harmony's MIT license. The mod's bundled
third-party notices are in `Disharmony.RimWorld/About/HarmonyNotices.txt`.

Run the .NET 10 compatibility tests with:

```powershell
.\Disharmony.Tests\Run-DisharmonyTests.ps1 -Runtime Dotnet
```
