# Behringer Wing protocol

Patrick-Gilles Maillot has published a huge amount of documentation on the Wing protocol,
as part of his site at https://sites.google.com/site/patrickmaillot/wing

The Wing-specific PDF is available at https://drive.google.com/file/d/1-iptgd2Uxw4qPEbmegG2Sqccf8AbRRfk/view

*This* document is basically my notes on interpreting *that* document, as there are aspects I've found unclear
or at least had to read through a few times. It purely focuses on the binary protocol, ignoring OSC.

## Escaping

Patrick's document states:

> When communicating with WING, the escape byte 0xdf should be handled carefully, as shown in the two
> routines shown below for sending and receiving data.

There's then some C code, and examples:

> (Receiving) Example: The sequence D702DFDEAF0E02 will in fact represent D702DFAF0E02
>
> (Sending) Examples: With current tx channel being 3, sending d702dfaf0e02 to channel 1 will
> transfer dfd0d702dfaf0e02, sending d702dfdf0e02 to channel 2 will transfer dfd1d702dfdf0e02,
> and sending d702dfd10e02 to channel 1 again will transfer dfd0d702dfded10e02

The C code for receiving is harder to understand than the sending code, which is effectively:

- If the "current channel" is not the destination of the message, send 0xDF followed by (0xD0 + channel ID).
- Start off with an `esc` flag of false
- For each data byte `db` we've been asked to send:
  - If `db` is 0xDF, set `esc` to true and send 0xDF
  - For any other value
	- If `esc` is true *and* or `db` is in the range `[0xD0, 0xDE]`, send 0xDE and then `db`
	- Otherwise, just send `db`
	- In either case, set `esc` to false
- At the end, if `esc` is true, send 0xDE

In other words, 0xDF is escaped to 0xDF 0xDE, but only when the subsequent byte is in the range `[0xD0, 0xDE]`
or we've reached the end of the message.

The receiving code handles a single byte, and takes exactly one action out of:

- Changing the current channel
- Setting the "escape" flag
- Reporting a data byte

# Channels

It's unclear (at the moment) whether interleaving is intended to occur within a message, or whether
receiving 0xDF followed by a value in the range `[0xD0, 0xDD]` always means "end of message, switch channel".

