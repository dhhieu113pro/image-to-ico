#!/usr/bin/env python3
from pathlib import Path
import subprocess
import sys


def run(args: list[str], cwd: Path) -> None:
    print(f"\n> {' '.join(args)}", flush=True)
    subprocess.run(args, cwd=cwd, check=True)


def main() -> int:
    repo = Path(__file__).resolve().parent.parent
    try:
        run([
            "dotnet", "test", "ImageToIco.slnx",
            "--configuration", "Release",
            "/p:CollectCoverage=true",
            "/p:Threshold=100",
            "/p:ThresholdType=line%2cbranch%2cmethod",
            "/p:ThresholdStat=total",
            "/p:CoverletOutputFormat=json"
        ], repo)
    except subprocess.CalledProcessError as exc:
        return exc.returncode
    print("\n100% line, branch, and method coverage gate passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
