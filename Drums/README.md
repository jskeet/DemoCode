This directory contains the code for my Roland V-Drum Explorer.

This is not a MIDI sequencer or anything similar; it's purely for
fetching and manipulating information from Roland V-Drum kits.

Currently I can only test this with my TD-17 drum module. The design
is intended to extend to other drum kits that have the same basic
functionality (responding to the same MIDI SysEx messages) even
though the data represented can vary significantly. Attempting to
generalize from a single example is never straightforward, and it's
entirely possible that significant work will be required if this
code ever supports more drum kits. Still, it's interesting to try.
In theory it would be possible to write the equivalent code any
other kit that I can get the technical information for, but the
chances of it actually being correct without a real implementation
to test against are pretty slim.

I'm very grateful to Roland for providing the [TD-17 MIDI
implementation](https://www.roland.com/global/support/by_product/td-17/owners_manuals/b28f606f-fa2e-4cb3-b4ec-1d25ce06a918/) document which has been absolutely vital in
writing this code.
