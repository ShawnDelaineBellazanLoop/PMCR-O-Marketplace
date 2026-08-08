#!/usr/bin/env python3
"""Generate a fresh trail UUID for a new PMCR-O cycle.

Usage:
    python3 new_trail_id.py

Prints a single uuid4 string to stdout. Never hand-type a trail UUID and
never reuse one across trails -- orchestrator calls this fresh for every
new .pmcro/trails/<domain>/<uuid>/ directory it creates.
"""
import uuid

if __name__ == "__main__":
    print(uuid.uuid4())
