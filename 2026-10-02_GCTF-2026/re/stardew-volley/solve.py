#!/usr/bin/env python3
import hashlib, os
from cryptography.hazmat.primitives.ciphers.aead import AESGCM

# --- Key derivation (SessionMixer.Compose in C#) ---
uid   = "Razlan.StardewVolley"
ctx   = (uid + "\nVolleyball").encode()
rally = bytes.fromhex("14251e6d0c50")
norm  = bytes.fromhex("47434638213f26")
key   = hashlib.sha256(ctx + b"\x00" + rally + b"\x00" + norm).digest()

# --- Load and slice volley.dat (RallyCodec.ReadFrame) ---
here  = os.path.dirname(os.path.abspath(__file__))
data  = open(os.path.join(here, "StardewVolley\\assets\\volley.dat"), "rb").read()
assert data[:4] == b"SVL1", "not an SVL1 frame"

nonce = data[4:16]
tag   = data[16:32]
ct    = data[32:]

# --- AES-GCM decrypt with AAD = "SVL1" ---
pt = AESGCM(key).decrypt(nonce, ct + tag, b"SVL1")
print(pt.decode())