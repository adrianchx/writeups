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
