# Sequence Gap Scanner

A lightweight Windows utility for video editors and media professionals. Quickly scan any folder to find missing files in numbered sequences — camera cards, render outputs, batch exports, dailies, and more.

## What it does

When you're working with large numbered file sequences (e.g. `A001C001_250101_R1VX.mov`, `frame_0001.exr`, `IMG_4823.JPG`), a single missing file can cause problems downstream — broken timelines, failed renders, incomplete deliveries. Sequence Gap Scanner spots those gaps instantly.

- Scans a folder and groups files by sequence
- Highlights missing files in red
- Shows exact sequence numbers and expected filenames for every gap
- Supports any file type — filter by extension or scan everything
- Export results to a text report

## Download

Go to the [Releases](https://github.com/DrywaterDevCo/SequenceGapScanner/releases) page and download the latest version:

| File | Description |
|------|-------------|
| `SequenceGapScanner-vX.X.X-Setup.exe` | Installer — adds a Start Menu shortcut and uninstaller |
| `SequenceGapScanner-vX.X.X-win-x64.zip` | Standalone — unzip and run, no install needed |

No .NET installation required. Windows 10 or later.

## How to use

1. Click **Browse** and select a folder
2. Optionally enter file extensions to filter (e.g. `.mov, .mp4`) — leave blank to scan all files
3. Click **Scan** (or press F5)
4. Click any group header to expand it and see the files
5. Missing files are highlighted in red with their expected filename
6. Click **Export** to save a full report as a text file

**Tips:**
- Double-click any file row to reveal it in Explorer
- Right-click a row to copy the file path or expected filename
- Prefix an extension with `-` to exclude it (e.g. `-.xml`)

## License

MIT — free to use, modify, and distribute.

---

Built by [Drywater Dev Co](https://github.com/DrywaterDevCo)
