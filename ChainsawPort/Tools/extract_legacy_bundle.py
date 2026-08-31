#!/usr/bin/env python3
"""Extract editable art/audio from the preserved Android chainsaw AssetBundle."""

from pathlib import Path
import argparse
import re

import UnityPy


def safe_name(value: str) -> str:
    value = re.sub(r"[^A-Za-z0-9._-]+", "_", value or "unnamed")
    return value.strip("._") or "unnamed"


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("bundle", type=Path)
    parser.add_argument(
        "--project",
        type=Path,
        default=Path(__file__).resolve().parents[1],
        help="ChainsawPort Unity project root",
    )
    args = parser.parse_args()

    source = args.project / "Assets" / "ChainsawPort" / "Source"
    model_dir = source / "Models"
    texture_dir = source / "Textures"
    audio_dir = source / "Audio"
    for directory in (model_dir, texture_dir, audio_dir):
        directory.mkdir(parents=True, exist_ok=True)

    environment = UnityPy.load(str(args.bundle))
    exported = []

    for obj in environment.objects:
        if obj.type.name == "Mesh":
            mesh = obj.read()
            destination = model_dir / f"{safe_name(mesh.m_Name)}.obj"
            destination.write_text(mesh.export(), encoding="utf-8")
            exported.append(destination)
        elif obj.type.name == "Texture2D":
            texture = obj.read()
            destination = texture_dir / f"{safe_name(texture.m_Name)}.png"
            texture.image.save(destination)
            exported.append(destination)
        elif obj.type.name == "AudioClip":
            clip = obj.read()
            for sample_name, sample_data in clip.samples.items():
                destination = audio_dir / safe_name(sample_name)
                destination.write_bytes(sample_data)
                exported.append(destination)

    manifest = source / "EXTRACTION.txt"
    manifest.write_text(
        "Legacy Chainsaw AssetBundle extraction\n"
        f"Source={args.bundle.name}\n"
        "SourceUnity=2021.3.5f1\n"
        "SourcePlatform=Android\n"
        "Purpose=Editable inputs for Patch 6 / Unity 2021.3.16f1 rebuild\n\n"
        + "\n".join(str(path.relative_to(args.project)) for path in exported)
        + "\n",
        encoding="utf-8",
    )
    print(f"Exported {len(exported)} assets to {source}")


if __name__ == "__main__":
    main()
