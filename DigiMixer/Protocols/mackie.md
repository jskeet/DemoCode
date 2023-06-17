# Mackie (DL16S, DL32S, DL32R) protocol

This protocol is entirely undocumented as far as I can see. This document
is the result of observation and experimentation. All the terminology is
therefore my own, and may not be ideal.

## Basic protocol

All communication occurs on a single client-initiated TCP connection. The
default port that the mixer listens on is 50001.

Both the client and the mixer can send *messages*.

All numeric values are represented in big-endian order (e.g. a 16-bit value is
represented by the most significant 8 bits followed by the least significant
8 bits). String representations are covered later in this document.

## Message structure

The structure of each message is the same:

Header: 6 bytes
Header checksum: 2 bytes
Body: Some multiple of 4 bytes
Body checksum: 4 bytes

The 6 bytes of the header are:

0: Always 0xab
1: Sequence number
2-3: Number of 4-byte chunks in the body 
4: Message type
5: Message subtype (currently called "command" in the code)

The header checksum is a 16-bit value derived by subtracting the sum of all
header bytes from 0xffff.

If the number of body chunks (specified in header bytes 2-3) is 0, both the body
*and* the body checksum are absent, so the total message size is 8 bytes.

Otherwise, the body "chunks" follow the header checksum, and the final 4 bytes
of the message are the body checksum. This is a 32-bit value derived by
subtracting the sum of all body bytes from 0xffffffff.

## Message types and sequence numbers

There are four known types of message:

- Request (0)
- Response (1)
- Error (5)
- Broadcast (8)

Each request message is expected to provoke a response message with the same
sequence number (to identify which message it's a response to).

Error messages are used as responses to invalid requests (e.g. making an info
request with an invalid type of info).

Broadcast messages are used for regular meter reporting; these do not need a
response.

Each side (client and mixer) keeps a sequence number starting at 1, incrementing
in the obvious way (with no gaps) and wrapping back to 1 after reaching 255.
(A sequence number of 0 is never used.) This sequence number is used for request
and broadcast messages; response and error messages use the sequence number
of the message they're responding to. Broadcast and request messages use the same
sequence (so a request with sequence number 3 might be followed by a broadcast
with a sequence number 4, for example - there aren't two independent sequences).

## Message subtypes

Each message has a subtype, indicating its purpose. The known subtypes are listed
below, with more information about each one in its own section afterwards.

- 0x01: Used as the very first request message, and sent every 2.5 seconds
  by the client to keep the connection alive.
- 0x03: Sent once from each side early on, to establish basic client and
  mixer information.
- 0x04: Sent once from client to mixer, this requests detailed version information.
- 0x06: Indicates the size of message the requester is willing to receive.
- 0x0e: Requests various kinds of general information from the peer.
- 0x13: Used to both report channel information (e.g. fader position, muting etc),
        or indicate a change to that information (e.g. "please mute this channel").
        Also used to report meter information.
- 0x15: Client request for the mixer to report meter information periodically
- 0x16: Client request indicating which meter values should be reported
- 0x18: Used to report channel names.

### Subtype 0x01: keep-alive

### Subtype 0x03: handshake

### Subtype 0x04: version

### Subtype 0x06: message size control

### Subtype 0x0e: info

### Subtype 0x13: channel values

### Subtype 0x15: meter layout

### Subtype 0x16: meter control

### Subtype 0x18: channel names
