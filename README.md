Pipes
=====

Navigation
----------
Know what you're looking for? Try one of the links below:
* [Getting Started](GettingStarted.md) - A quick guide to help you begin writing code and using pipes.
* [Upcoming Features](UpcomingFeatures.md) - What might be added to Pipes in future. (Email michael.bradley@hotmail.co.uk with suggestions)
* [Specification](Specifics.md) - A list of the pipes, inlets and outlets available and what they all do.
* [Technicalities](Technicalities.md) - Various implementation specific details. If you think you've found a bug, it might help to look here.
* [Disclaimer](Disclaimer.md) - I warn you that I am not perfect so things can go wrong...

#### Examples:
* [Custom Composite Pipe](SimpleExample.md) - an example of creating a new pipe from existing pipes.
* [Custom New Pipe](NewPipeExample.md) - an example of creating a pipe that cannot be created from existing pipes.

Pipes are, in my opinion, fun and easy to use. Their focus is on extensibility and ease of use, and could even be an educational tool. However, they are pretty fast too and should make your code much more maintainable!

Summary
-------

**Pipes** is a concurrency abstraction designed to simplify communication and synchronisation between threads.

Concurrency is about completing multiple tasks at the same time. As more and more devices have multiple cores to exploit, parallel programming is set to become the norm! However, the tools we have now often feel unintuitive, produce convoluted systems and are hard to reason about.

Pipes aims to simplify this. Processes communicate and synchronise with each other by sending messages through (thread-safe) pipes. To illustrate:

**Thread 1:**
```c#
//... do some work
var message = pipe.Outlet.Receive(); //blocks until thread 2 is read to send the message
//... do more work
```

**Thread 2:**
```c#
//... do some work
pipe.Inlet.Send(message); //blocks until thread 1 is ready to receive the message
//... do more work
```

The real power of pipes can be better seen when pipes have multiple inlets / outlets. You can:
* Create your own pipes easily. (For example, see if you can understand how the [capacity pipe](https://github.com/michaelbradley91/Pipes/blob/master/Pipes/Pipes/Models/Pipes/CapacityPipe.cs) was implemented.)
* Create your own inlets / outlets.
* Connect multiple pipes together to form a pipe system!

This project is written in C# and is available as a Nuget Package with ID Parallel.Pipes, so it's easy to get started!

Pipes was originally inspired by CSO developed by Bernard Sufrin at Oxford University. You can learn more about CSO [here](http://www.cs.ox.ac.uk/people/bernard.sufrin/CSO/cso-doc-scala2.11.4/index.html#ox.CSO$). However, Pipes' goals have deviated from CSO a little - there is a much stronger focus on making it extensible - so you have the power to do whatever you need to without worrying about thread management at all!
