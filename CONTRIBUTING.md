# Contributing to NVLite

Thanks for your interest in contributing! Here's how you can help.

## Reporting Bugs

Open an [issue](https://github.com/baldknobber/nv-lite/issues) with:
- What you expected vs. what happened
- Your GPU model and driver version
- Steps to reproduce

## Submitting Community Profiles

Community profiles let users share optimized NVIDIA driver settings for specific games.

### How to submit a profile

1. **Export your profile** from NVLite (Profiles page → Export to JSON)
2. **Fork this repository**
3. **Add your JSON file** to the `community-profiles/` directory
   - File name: `game-name.json` (lowercase, hyphens, e.g. `cyberpunk-2077.json`)
   - Must follow the [profile schema](community-profiles/profile-schema.json)
4. **Update** `community-profiles/profiles-index.json` to include your profile entry
5. **Open a Pull Request** with:
   - Game name and what the profile optimizes (performance, quality, VR, etc.)
   - GPU(s) you tested on
   - Driver version tested

### Profile requirements

- Must pass JSON schema validation (automated check runs on your PR)
- File size under 50 KB
- Must include: `profileName`, `gameName`, `settings`, `contributor`
- Should include: `gpuSeries`, `driverVersionTested`, `description`, `tags`
- No duplicate profiles for the same game (update the existing one instead)
- Settings must be valid NVAPI DRS setting IDs with valid DWORD values

### What gets rejected

- Profiles with no real settings changes (just defaults)
- Troll/spam submissions
- Profiles that could damage hardware (extreme overclock values aren't stored in DRS, but we check anyway)

## Code Contributions

1. Fork and clone the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Make your changes
4. Run tests: `dotnet test`
5. Open a Pull Request

### Code style

- Follow existing patterns in the codebase
- Use `CommunityToolkit.Mvvm` attributes for ViewModels
- Keep UI logic in ViewModels, not code-behind

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
