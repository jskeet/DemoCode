# Sample code for working with the X-Touch Mini

The [Behringer X-Touch
Mini](https://www.behringer.com/product.html?modelCode=P0B3M) is a
compact control surface with 8 knobs, 16 general-purpose buttons, a
fader, and 2 layer buttons.

I'm interested in using it to control a Behringer
[XR-16](https://www.behringer.com/product.html?modelCode=P0BI7) or
[XR-18](https://www.behringer.com/product.html?modelCode=P0BI8)
digital mixer, using [code already described in an earlier blog
post](https://codeblog.jonskeet.uk/2021/01/27/osc-mixer-control-in-c/)... and it turns out, that's pretty simple to do.

The code here is divided into three projects:

- XTouchMini.Model: the core controller code
- XTouchMini.Console: a simple test tool using the X-Touch Mini in
  "Standard mode"
- XTouchMini.MixerControl: a prototype of the full XR-16/18 mixer
  control, using the X-Touch Mini in "Mackie Control mode" for
  more fine-grained control over the display

All this code should be regarded as *very* much prototype code,
put together in a few hours, but hopefully of interest anyway.
