# Music Database Utility

`update_music_database.py` keeps the in-game music catalog in sync with `Assets/StreamingAssets/musicas.csv`. It also clones the placeholder audio (`sampleMP3ToDefault.mp3`) and emoji (`samplePNGToDefault.png`) into the `Files/Audio` and `Files/Emojis` folders when a new slug is encountered.

## Usage

1. Open a terminal in the repo root (`CCXP_EmojiHits`).
2. Run the script with the configured Python interpreter:

```powershell
C:/Users/AndrePundek/AppData/Local/Programs/Python/Python313/python.exe tools/update_music_database.py
```

3. The script will:
   - append any new entries from `musicas.csv` into `Files/Data/music_database.json`,
   - copy the sample MP3 into `Files/Audio/<slug>.mp3`,
   - copy the sample PNG into `Files/Emojis/<slug>.png`.

Optional arguments let you override each path if your layout changes.
