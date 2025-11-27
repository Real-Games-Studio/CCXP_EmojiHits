"""Add new tracks from musics.csv to music_database.json and copy placeholder media."""
from __future__ import annotations

import argparse
import csv
import json
import shutil
from pathlib import Path
from typing import Iterable


def _clean(value: str) -> str:
    return value.strip().replace("“", '"').replace("”", '"')


def _read_csv(path: Path) -> Iterable[dict[str, str]]:
    with path.open("r", encoding="utf-8", newline="") as csvfile:
        reader = csv.reader(csvfile)
        for row in reader:
            if not row or all(not cell.strip() for cell in row):
                continue
            slug = row[0].strip()
            if not slug:
                continue
            letra = _clean(row[1]) if len(row) > 1 else ""
            nome = _clean(row[2]) if len(row) > 2 and row[2].strip() else slug
            autor = _clean(row[3]) if len(row) > 3 else ""
            yield {"slug": slug, "nome": nome, "autor": autor, "letra": letra}


def _ensure_parent(path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)


def _copy_sample(source: Path, dest: Path) -> bool:
    if dest.exists():
        return False
    shutil.copyfile(source, dest)
    return True


def run(
    csv_path: Path,
    json_path: Path,
    sample_mp3: Path,
    sample_png: Path,
    audio_dir: Path,
    emoji_dir: Path,
) -> None:
    data = {}
    if json_path.exists():
        data = json.loads(json_path.read_text(encoding="utf-8"))
    songs = data.setdefault("musicas", [])
    existing_tracks = {entry.get("arquivoMusica") for entry in songs if entry.get("arquivoMusica")}

    added = 0
    copied_mp3 = 0
    copied_png = 0

    for song in _read_csv(csv_path):
        arquivo_musica = f"{song['slug']}.mp3"
        if arquivo_musica in existing_tracks:
            continue
        entry = {
            "musica": song["nome"],
            "autor": song["autor"],
            "letra": song["letra"],
            "arquivoImagemEmoji": f"Emojis/{song['slug']}.png",
            "arquivoMusica": arquivo_musica,
        }
        songs.append(entry)
        existing_tracks.add(arquivo_musica)
        _ensure_parent(audio_dir / arquivo_musica)
        if _copy_sample(sample_mp3, audio_dir / arquivo_musica):
            copied_mp3 += 1
        _ensure_parent(emoji_dir / f"{song['slug']}.png")
        if _copy_sample(sample_png, emoji_dir / f"{song['slug']}.png"):
            copied_png += 1
        added += 1

    if added:
        json_path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    print("New tracks added:", added)
    print("MP3 files copied:", copied_mp3)
    print("PNG files copied:", copied_png)


def _build_parser() -> argparse.ArgumentParser:
    repo_root = Path(__file__).resolve().parent.parent
    default_csv = repo_root / "Assets" / "StreamingAssets" / "musicas.csv"
    default_json = (
        repo_root / "Assets" / "StreamingAssets" / "Files" / "Data" / "music_database.json"
    )
    files_dir = repo_root / "Assets" / "StreamingAssets" / "Files"

    parser = argparse.ArgumentParser(description="Sync musics.csv into music_database.json")
    parser.add_argument("--csv", type=Path, default=default_csv, help="Path to musics.csv")
    parser.add_argument(
        "--json",
        type=Path,
        default=default_json,
        help="Path to music_database.json",
    )
    parser.add_argument("--mp3", type=Path, default=files_dir / "sampleMP3ToDefault.mp3", help="Sample MP3 to duplicate")
    parser.add_argument("--png", type=Path, default=files_dir / "samplePNGToDefault.png", help="Sample PNG to duplicate")
    parser.add_argument(
        "--audio-dir",
        type=Path,
        default=files_dir / "Audio",
        help="Destination directory for MP3 files",
    )
    parser.add_argument(
        "--emoji-dir",
        type=Path,
        default=files_dir / "Emojis",
        help="Destination directory for emoji PNGs",
    )
    return parser


def main() -> None:
    parser = _build_parser()
    args = parser.parse_args()
    run(args.csv, args.json, args.mp3, args.png, args.audio_dir, args.emoji_dir)


if __name__ == "__main__":
    main()
