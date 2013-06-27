Everything I know about API design, I learned from the Spice Girls

This was my [Pecha Kucha](http://en.wikipedia.org/wiki/PechaKucha "Pecha Kucha") talk at [CodeMash](http://codemash.org) in 2013. These notes are basically just a transcript of the [YouTube video](http://www.youtube.com/watch?v=9KOMMpn-r2M) recorded by the fabulous [Cori Drew](http://truncatedcodr.wordpress.com/). Occasionally I've edited the notes to include what I was *trying* to say rather than what I ended up *actually* saying...

**Slide 1: Introduction**
Firstly, an apology for last year. Last year my Pecha Kucha session was on coding in the style of Glee, and it didn't have a lot of content. I was going for cheap laughs. Tonight is deadly serious. I feel passionately about API design. These are serious points - all taken from the Spice Girls song Wannabe.

**Slide 2: Tell me what you want**
The obvious first slide - this was what the whole session was derived from. Listen to requirements. Requirements gathering is obviously incredibly important. If you're putting together an API, it's got to have a purpose. It's got to be providing value to people.

**Slide 3: (An ear)**
It's no use providing some complicated but elegant thing if you're not listening to what people are telling you they actually need. So listen to your users. It's very hard for new, open source projects. If you think you've got a great idea, how can you find out what people really need from it? But really you've got to try to find that balance, and listen to users as early and as often as you possibly can.

**Slide 4: What you really, really want**  
And don't just *half*-listen to them, and think "Do you know what? I think I know what you want now. I've got these two concepts that users think are the same, so I'm just going to put them into the same type, maybe use inheritance." You know, all this code reuse is great stuff.

**Slide 5: Class diagram for ZigazigAh**
Code reuse is an antipattern sometimes. So here we have... we want a ZigazigAh. We don't have two different Zig type of things. All we should have is a ZigazigAh. But instead, someone who's far to fond of object orientation thinks that they'll create an AbstractZig class.

**Slide 6: I wanna (ha) x4**
Handling failures. So clearly this is the Spice Girls trying to execute the "wanna" operation four times, and failing each time with a "ha" exception. You need to think about this stuff.

**Slide 7: Wanna / TryWanna / wannt() throws HaException**    
The success path is important, but you really need to know *when* you're going to fail, *why* you're going to fail. If it's your user's fault, let them know. There are loads of different ways to approach this, but it's impossible if you don't take it into account to start with. You've got to think about this stuff, and document it. It's all about making it clear to the user.

**Slide 8: If you want my future forget my past**
Okay, so sometimes we all make mistakes. And APIs that have gone before will probably have made mistakes. Coding conventions on the platform you're using will have made mistakes. So what are you going to do? You can follow the existing convention and be comfortable for your users, but provide a somewhat-broken experience, or you can break the mould.

**Slide 9: `object[] x = new string[20];` - really?**  
And as you can see, platforms have the same problem. So the .NET designers said, "Hey, we want to be compatible with Java. We'll make all the same mistakes they've already made." Sometimes it's the right decision. Sometimes the comfort zone of your users is more important than elegance. But sometimes you just want to break those ties.

**Slide 10: Now don't go wasting my precious time**
Now, performance. It clearly matters, and you will have no ideas which of your users it's going to matter to, or what they're doing. At least unless you've got some fancy telemetry that's going to check it all out in dogfood like the Roslyn team.

**Slide 11: Stopwatch and memory chip**
But it will matter, and you're not going to know how, so you've got to benchmark. I'm not saying you should micro-optimize everything. I'm saying you should work out which things you want to be fast, make it clear to your users which things *aren't* fast, and time everything. Occasionally you can even indulge in some micro-optimization. We all love to do it, even though we know it's evil. Just occasionally, it's fine.

**Slide 12: If you wanna be my lover, you gotta get with my friends**
So last year we had "Bridge over Troubled Water" representing interoperability - whereas the Spice Girls say you've got to get with their friends. You've got to be compatible with the various platforms your users are already using. So you need to think - and will depend on the API.

**Slide 13: Interoperability diagram**
If you've got some sort of object model, how am I going to save that to a database? How am I going to represent it in JSON? How am I going to represent it in XML? Are there other interfaces I should be implementing so that people can interoperate better? Are there even other conversions that would make more sense? So think about how people are going to use your API in the context of other things.

**Slide 14: Make it last forever, friendship never ends**
Now, versioning. Backward compatibility and support. These are three of my favourite things, and I'm sure you love them too. Okay, it's horrible. You will have made some mistakes. How can you change your API without breaking everyone? Everyone will have a different strategy for this, but you need to be clear about what you're going to do beforehand.

**Slide 15: v1.0, 1.0.1, 1.1 etc**
I would urge you not to go down the `java.util.Date` route, of saying "We're never actually going to take away anything. We can mark it deprecated, we'll never take it away." No - work out when you can have clean breaks to rectify the mistakes of the past. I would highly recommend going to [semver.org](http://semver.org). Learn about semantic versioning, apply it, on you go.

**Slide 16: I won't be hasty - I'll give you a try. If you really bug me then I'll say goodbye**
So, it's easy to get users to try something once. But you've got to make sure it's love at first sight. So after your user has downloaded something, what do they do? If you've just given them an API reference, they're lost - they're at sea.

**Slide 17: Man heart woman**
You've got to give them tutorials, guides - get them into your head. Make them understand *why* your API is designed the way it is. And then give them support on [Stack Overflow](http://stackoverflow.com), mailing lists, etc. Make this a *glorious* experience for them. And remember, they're coming from nothing: all they've got is what you will provide them.

**Slide 18: So here's the story from A to Z...**
So early on I talked about listening to requirements, and in Wannabe we hear loads of different requirements from loads of different girls, apparently. I don't understand all of these requirements - I hope that the Spice Girls do. Sometimes you will get contradictory requirements... remember it's still *your* API.

**Slide 19: "Must do X." "Must not do X."**
You get to decide what will happen. Don't be afraid to occasionally say no. But ideally, find out why someone wants a particular feature they've asked for. What's their bigger picture? How can you get your API involved in a way that you're comfortable with that also meets their needs? 

**Slide 20: Slam your body down and wind it all around...**
And I've got no idea about this. It may be an HTML thing, and I clearly have a lot still to learn from the Spice Girls. So maybe if you understand this, you can tell me.