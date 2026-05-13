# Kynetic Klicker

External 13 CPS clicker utility built for Kynetic-owned Minecraft servers.

## Features

- Max 13 CPS
- Hold LMB to click
- Release LMB to stop instantly
- Toggle keybind support
- Exit keybind support
- Keybind memory system
- CPS memory system
- Minecraft-only clicking mode
- Minimize to tray
- Custom Kynetic branding
- Dark themed UI
- Standalone self-contained EXE publishing

## Requirements

- Windows 10/11
- .NET 10 SDK

Download the SDK from Microsoft:
https://dotnet.microsoft.com/

Check your installed SDKs:

```bat
dotnet --list-sdks
```

You should see a version beginning with `10.`.

## Building

Run this from the repo root:

```bat
scripts\build-release.bat
```

Final EXE output:

```text
dist\KyneticKlicker.exe
```

The EXE is built as:

- standalone
- self-contained
- single-file

## Running From Source

```bat
scripts\run-dev.bat
```

## Settings Storage

Settings are stored locally in:

```text
%AppData%\KyneticKlicker\settings.json
```

Stored values:

- Toggle keybind
- Exit keybind
- CPS
- Minecraft-only mode

## Usage

1. Launch the program.
2. Press the toggle keybind to enable the clicker.
3. Hold Left Mouse Button to click.
4. Release Left Mouse Button to stop.
5. Use the CPS slider to select 1-13 CPS.

## License

This project is proprietary software owned by KyneticStudios LLC.

See `LICENSE.txt` for full terms.

## Ownership

Copyright (c) 2026 KyneticStudios LLC  
All Rights Reserved.
