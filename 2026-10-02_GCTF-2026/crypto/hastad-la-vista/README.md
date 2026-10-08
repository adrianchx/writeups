# Håstad la Vista - GCTF 2026

Solved by `adrianchx`

## Introduction

Håstad la Vista is a cryptography challenge based on **Håstad's Broadcast Attack**, an RSA exploit that abuses small public exponents when the same message is encrypted under multiple moduli.

Files included with the challenge are `output.txt` (the challenge data).

## The Solve

Looking at the title of the challenge: **Håstad la Vista** is a pun on **Håstad's Broadcast Attack**.

Opening `output.txt`, we get:

```
e = 11

n1 = 91944051209684455246236290525921234472856654245210087573728654602312318120122902445432366284740079513057792415389438740126717907321485973578276932933344836457867791390698260266309588920865418794484643275779375254309675600615266520250120639225579624027654885117888424489911029660069679126735245263995260134099
c1 = 62335062864211450094350551475865332834429132410722223171847728975059694510157632395937949595125271133769525543735120910482511393909780468152627325085237588132281848393169919633143141734605321912910067259434301704462554457580201291898901900683370488619195436913737770925196925036133945805951220543667567366908

n2 = 131160498014591409381150591589774793513080190747480785639154661025278899791660711862085425946201315966610997203908823267357108369523329101383089159227409971609507505405725747795278550729057859332511649086348678410344528680892025751939207468452899239511884907589886156440515107993753754001816084366248187205237
c2 = 117073441607056776069168334632539225861584610502539730872914802451739202255599793027744475902853716930550313967721365816110499237767334666462341610535446192285004967572211974387934560073658556968459969356305761123712061974241386455804530457042814698412761026586032972149785475007891047570970243298280836596488

...
```

Two things jump out immediately:

1. **`e = 11`** — a very small public exponent.
2. **11 different moduli** — the same count as `e`.

As we know from the title hint, this is a **Håstad broadcast attack** setup: the same plaintext `m` is encrypted with the same exponent under multiple moduli.

Each ciphertext is `cᵢ = m^11 mod nᵢ`. Because `e` and the number of ciphertexts match, we can use the **Chinese Remainder Theorem (CRT)** to reconstruct `m^11` exactly (no modular wrap-around), then take the integer 11th root to get `m`.

The attack has three conditions, all satisfied here:

1. Same plaintext `m` across all encryptions.
2. Same small exponent `e` (here `e = 11`).
3. **At least `e` different moduli** — we have exactly 11.

The reason this works is that **CRT lets us combine the congruences into one value**:

```text
X ≡ c₁ (mod n₁)
X ≡ c₂ (mod n₂)
...
X ≡ c₁₁ (mod n₁₁)
```

`X` is the unique value less than `n₁·n₂·…·n₁₁` satisfying all of those. If the moduli are pairwise coprime and `m^11 < n₁·n₂·…·n₁₁` (which is true for any realistic flag with 11 × 512-bit moduli), then **`X = m^11` exactly**, with no modular reduction applied. So we just take the integer 11th root.

> More information about [Hastad’s Broadcast Attack](https://docs.xanhacks.xyz/crypto/rsa/08-hastad-broadcast-attack/) here.

---

The solve script with the help of Deepseek:

`unhasta.py`

```py
from sympy.ntheory.modular import crt
from gmpy2 import iroot
from Crypto.Util.number import long_to_bytes
from math import gcd

e = 11

ns = [ ... 11 moduli from output.txt ... ]
cs = [ ... 11 ciphertexts from output.txt ... ]

# Sanity: pairwise coprime?
for i in range(len(ns)):
    for j in range(i+1, len(ns)):
        assert gcd(ns[i], ns[j]) == 1, f"n{i+1}, n{j+1} not coprime"

# CRT: combine all congruences into a single value (== m^e exactly)
M, _ = crt(ns, cs)

# Integer 11th root — because M == m^11 exactly, not m^11 mod N
m, exact = iroot(M, e)
assert exact, "not a perfect 11th root"

print(long_to_bytes(int(m)).decode())
```

`gctf26{h4st4ds_br04dc4st_c4n_b3_d34dly}`
