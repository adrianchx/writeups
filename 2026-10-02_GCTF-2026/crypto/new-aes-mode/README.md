# New AES Mode - GCTF 2026

Solved by `adrianchx`

## Introduction

New AES Mode is a cryptography challenge that introduces a homebrew "improvement" on `AES-GCM`. The description pokes fun at the complexity of GCM, and the challenge server hands you a target ciphertext and then acts as an oracle: you can encrypt your own plaintexts and decrypt under any nonce you can produce a valid tag for.

The goal: recover a random 32-byte challenge plaintext and submit it back.

The challenge provides a socket at a random host:port. Once connected, it prints a challenge nonce, ciphertext, and tag, and presents a menu with three options: encrypt, decrypt, or submit the challenge plaintext.

It also includes `main.py`, which contains the full source of the new mode:

```python
import hashlib
import secrets
import signal

from Crypto.Cipher import AES


FLAG = "gctf26{fake_flag}"
KEY = secrets.token_bytes(16)
MAX_LENGTH = 1024


def xor(a, b):
    return bytes(x ^ y for x, y in zip(a, b))


def digest(ciphertext):
    length = len(ciphertext).to_bytes(16, "big")
    return hashlib.sha3_256(ciphertext + length).digest()[:16]


def make_tag(ciphertext, nonce):
    first_counter = AES.new(KEY, AES.MODE_ECB).encrypt(nonce + bytes(4))
    return xor(digest(ciphertext), first_counter)


def crypt(data, nonce):
    cipher = AES.new(KEY, AES.MODE_CTR, nonce=nonce, initial_value=1)
    return cipher.encrypt(data)


def encrypt(plaintext, nonce):
    ciphertext = crypt(plaintext, nonce)
    return ciphertext, make_tag(ciphertext, nonce)


def decrypt(ciphertext, tag, nonce):
    if not secrets.compare_digest(tag, make_tag(ciphertext, nonce)):
        raise ValueError("invalid tag")
    return crypt(ciphertext, nonce)

...
```

## The Solve

Reading the source closely, the "new mode" is actually two independent pieces stacked on top of each other:

1. **Encryption** = AES-CTR with `nonce=nonce, initial_value=1`. A standard stream cipher.
2. **Authentication** = a custom tag computed from the ciphertext and nonce only.

Two serious flaws jump out.

**Flaw 1: the tag is a function of the ciphertext, not the plaintext.** `make_tag(ct, nonce)` takes the **ciphertext** as input. That means if you know any valid `(nonce, ciphertext, tag)` triplet, you can forge a valid tag for **any other ciphertext under the same nonce** — because the tag depends only on data you control plus a value derived from the key and the nonce.

**Flaw 2: nonce reuse gives keystream reuse.** AES-CTR is a stream cipher — the keystream is determined by `(KEY, nonce)` alone. If the server lets us decrypt under a chosen nonce whose tag we can forge, we can extract the keystream for that nonce and XOR it against the challenge ciphertext.

The challenge gives us:

- A random 32-byte **challenge plaintext** (unknown to us).
- A random 12-byte **challenge nonce**.
- The corresponding **challenge ciphertext** and **challenge tag**, both printed to us.

And then a menu with three options:

1. Encrypt (with a fresh random nonce — not useful, we can't control the nonce).
2. Decrypt (with any nonce, ciphertext, and tag we provide).
3. Submit challenge plaintext.

The decryption option is the oracle we need, and it refuses only one specific combination:

```python
if nonce == challenge_nonce and ciphertext == challenge_ciphertext:
    print("The challenge ciphertext cannot be decrypted directly.")
```

So we can't ask it to decrypt the exact pair we were given. But we can ask it to decrypt **anything else under the same challenge nonce** — including a ciphertext we craft.

### Deriving the tag for our forged ciphertext

The tag function is:

```python
def make_tag(ciphertext, nonce):
    first_counter = AES.new(KEY, AES.MODE_ECB).encrypt(nonce + bytes(4))
    return xor(digest(ciphertext), first_counter)
```

For a fixed nonce, `first_counter` is a **constant** — it's just `AES-ECB(KEY, nonce || 0x00000000)`. We don't know `KEY`, but we can recover `first_counter` from the one known tag we have:

```py
challenge_tag = digest(challenge_ciphertext) XOR first_counter
```

Rearranged:

```py
first_counter = challenge_tag XOR digest(challenge_ciphertext)
```

Both `challenge_tag` and `challenge_ciphertext` were printed to us. `digest()` is a public function — `SHA3-256(ct || len(ct).to_bytes(16, "big"))[:16]`. So we can compute `first_counter` locally with no secrets.

Now **any** ciphertext under the same nonce gets a valid tag via:

```py
tag' = digest(ct') XOR first_counter
```

### Recovering the keystream

We want the keystream used to encrypt the challenge. Pick a **zero ciphertext** of the same length as the challenge:

```python
forged_ct = b"\x00" * len(challenge_ct)
forged_tag = xor(digest(forged_ct), first_counter)
```

Send this to the server's decrypt option along with the challenge nonce. The server verifies the tag — it passes because we just computed it correctly. Then it calls `crypt(forged_ct, challenge_nonce)`:

```py
plaintext = forged_ct XOR keystream
```

But `forged_ct` is all zeros. So:

```py
plaintext = keystream
```

The server prints the keystream to us.

The specific guard in the source (`nonce == challenge_nonce and ciphertext == challenge_ciphertext`) doesn't trigger — we're using the challenge nonce but a different ciphertext. That's the loophole.

### Recovering the plaintext

Now we have the keystream. The challenge plaintext was:

```py
challenge_plaintext = challenge_ciphertext XOR keystream
```

XOR the two, submit it as option 3, get the flag.

The full solve script:

`solve.py`

```python
import hashlib
from pwn import remote

def digest(ct):
    length = len(ct).to_bytes(16, "big")
    return hashlib.sha3_256(ct + length).digest()[:16]

def xor(a, b):
    return bytes(x ^ y for x, y in zip(a, b))

io = remote("play.gctf.ctf.onl", 31904)

io.recvuntil(b"Challenge nonce: ")
challenge_nonce = bytes.fromhex(io.recvline().strip().decode())

io.recvuntil(b"Challenge ciphertext: ")
challenge_ct = bytes.fromhex(io.recvline().strip().decode())

io.recvuntil(b"Challenge tag: ")
challenge_tag = bytes.fromhex(io.recvline().strip().decode())

# Recover first_counter = challenge_tag XOR digest(challenge_ct)
first_counter = xor(challenge_tag, digest(challenge_ct))

# Forge a tag for a zero ciphertext under the same nonce
forged_ct = b"\x00" * len(challenge_ct)
forged_tag = xor(digest(forged_ct), first_counter)

# Ask the server to decrypt it → returns the keystream
io.recvuntil(b"> ")
io.sendline(b"2")
io.recvuntil(b"Nonce (hex): ")
io.sendline(challenge_nonce.hex().encode())
io.recvuntil(b"Ciphertext (hex): ")
io.sendline(forged_ct.hex().encode())
io.recvuntil(b"Tag (hex): ")
io.sendline(forged_tag.hex().encode())

io.recvuntil(b"Plaintext: ")
keystream = bytes.fromhex(io.recvline().strip().decode())

# Recover the challenge plaintext
challenge_pt = xor(challenge_ct, keystream)

# Submit
io.recvuntil(b"> ")
io.sendline(b"3")
io.recvuntil(b"Challenge plaintext (hex): ")
io.sendline(challenge_pt.hex().encode())

print(io.recvline().decode())   # Correct!
print(io.recvline().decode())   # Flag: gctf26{...}
```

Output:

```py
Correct!
Flag: gctf26{9465de87459cb191de3ddca6a35ccd6343b8cd5503754fa1e0255cb75279c274}
```

`gctf26{9465de87459cb191de3ddca6a35ccd6343b8cd5503754fa1e0255cb75279c274}`

>- **AES-CTR is a stream cipher.** Same key + same nonce = same keystream. This is exactly why GCM's spec requires unique nonces for every message — the custom mode ignores that and lets the attacker choose the nonce.
> - **The tag binds to the ciphertext, not the plaintext.** This is the second flaw. Even if the nonce were unique, an attacker who knows one valid `(ct, tag)` pair under a nonce can forge tags for arbitrary ciphertexts under that same nonce. A real MAC should authenticate the key, the nonce, the ciphertext, and any associated data — and it should not be structured in a way that lets you solve for a key-dependent constant from one sample.
> - **`first_counter` is recoverable from a single known tag.** Because `make_tag` XORs a public hash of the ciphertext with a key-derived constant, and the ciphertext is known, the constant falls out immediately: `first_counter = tag XOR digest(ct)`.
> - **The "you can't decrypt the challenge directly" guard is fragile.** It only blocks the exact `(nonce, ciphertext)` pair. Changing the ciphertext to anything else — e.g. zeros — bypasses it while keeping the nonce fixed, which is what we need for the keystream oracle.
> - **CTR's `initial_value=1` doesn't save it.** `first_counter` in `make_tag` uses counter block 0 (`nonce || 0x00000000`), which differs from the CTR block used at `initial_value=1`. But it doesn't matter — the keystream used by `crypt` is still deterministic per nonce, and we recover it anyway because the tag forgery doesn't depend on knowing the counter block's value.
