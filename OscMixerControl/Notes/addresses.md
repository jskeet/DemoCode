# OSC addresses for the XR16

- ch/01/config/name - name of an input
- ch/01/mix/on - 0 or 1, muted or unmuted
- ch/01/mix/fader - level for main mix
- ch/01/mix/01/level - level for bus 01; mixes 7-10 are fx

- bus/1/config/name - name of a bus (note 1 not 01)
- 

Meter 0:
- First parameter: input (0-15?)
- Second parameter: ?



- /xremote sends feedback, /xremotenfb doesn't (i.e. when you set a value, does it get sent back to the client?)

- /info: "V0.04", "XR16-0F...", "XR16", "1.17"
- /xinfo: "192.168.1.170", "V0.04", "XR16-0F...", "XR16", "1.17"



On startup, X-Air-Edit sends:

- /status
- /batchsubscribe meters/0 /meters/0 0 0 0
- /batchsubscribe meters/3 /meters/3 0 0 0
- /batchsubscribe meters/7 /meters/7 0 0 0
- /unsubscribe meters2
- /unsubscribe meters5
(The below requests happen twice each
- /-prefs/name
- /-prefs/lan
- /-prefs/lan/addr
- /-prefs/lan/mask
- /-prefs/lan/gateway
(Various other prefs)
/-snap/01
...
/-snap/64
/-snap
/xremotenfb
/-stat/solosw
/-stat/rta
(Various other stats)
/config/chlink
(Various other configs)
/dca/1/config  
/dca/2/config
/dca/3/config
/dca/4/config
/dca/1
/dca/2
/dca/3
/dca/4

/ch/01/config
Then *lots* of stuff for /ch/[01]-[16]
/rtn/aux/config
/rtn/aux/preamp
/rtn/aux/eq
(Various other rtn values)
/bus/1/config
/bus/1/dyn
/bus/1/dyn/filter
(Various other bus values for bus 1-6)
/fxsend/1/config
/fxsend/1/mix
/fxsend/1/grp
(Ditto for fxsend 2-4)
/lr/config
(Various other lr values)
/routing/main/01
(Various other routing values)
/headamp/01
...
/headamp/24
/fx/1
/fx/1/par
/fx/2
/fx/2/par
/fx/3
/fx/3/par
/fx/4
/fx/4/par
/-action/setclock "20201 24074609"
/batchsubscribe meters/1 /meters/1 0 0 0
/batchsubscribe meters/6 /meters/6 0 0 0
/-action/setclock "20201 24074609"
/config/chlink
(Various other configs, again...)
/dca/1/config (why?)


Occasional renewal of meters 0, 3, 7, 1, 6

Steady state: occasionally fetches all values (why?), renews meters


XR16 meters, all tested with parameters 0, 0, time_factor:

0: 8 values, e.g. b8c8 b8c8 0000 0000 ab62 ab62 b8c8 b8c8. Values 0,1,6,7 always seem the same; 2,3 are always 0000; 4,5 are the same but different to 0,1,6,7
Next step: try panning to see if that changes the "equalness"
Unclear whether parameters are relevant.
  0   (8)   pre l, pre r, gate gr, comp gr, post l, post r, gate key, comp key; (single channel data)

1: 40 values, including a bunch of 8000 values; rest in the range a000-c000
  1   (40)  16x ch pre, aux pre l, aux pre r, fx1 pre l, fx1 pre r, ... fx4 prel, fx4 pre r, 
            bus1 pre, ..., bus6 pre, fxsend1 pre, ..., fxsend4 pre, main post l, main post r, mon l, mon r

2: 36 values, range 8000-df00
  2   (36)  16x preamp in, 2x aux in, 18x usb in

3: 56 values, mostly 8000 or 0000, except values 0 and 12 which vary
  3   (56)  4x (fx in l, fx in r, 10x fx state mtr, fx out l, fx out r)

4: 100 values, almost all 8000, except for last 7 values 
  4   (100) 100x rta level

5: 44 values, mostly 8000, but could be bus outputs? For example, value 1 goes to 8000 when bus 2 fader level is at -infinity
Values 6 and 7 look like master output L/R - they observe panning
Values 43 and 44 look like solo output
Values 0-5 look like bus outputs, but buses 5 and 6 (values 4 and 5) don't get updated...
Why doesn't X-Air use this?
  5   (44)  6x aux out, main l out, main r out, 16x ultranet out, 18x usb out, phones l, phones r


6: 39 values, always 0000
  6   (39)  16x ch gate gr, 16x ch comp gr, 6x bus comp gr, main comp gr

7: 16 values, always 0000?
  7   (16)  16x ch automix gr

8: 4 values, always 8000
  8   (4)   4x dca (ch post)

9: 4 values, first two always equal, but varying; last two always 8000
  9   (4)   recorder in l, recorder in r, recorder out l, recorder out r;


When input 2 is set to "left only"... meter 0, final value pair varies...
