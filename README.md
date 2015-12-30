Pipes
=====

Navigation
----------
Know what you're looking for? Try one of the links below:
* [GettingStarted](GettingStarted.md) - A quick guide to help you begin writing code and using pipes.
* [UpcomingFeatures](UpcomingFeatures.md) - What might be added to Pipes in future. (Email michael.bradley@hotmail.co.uk with suggestions)
* [Specification](Specifics.md) - A list of the pipes, inlets and outlets available and what they all do.
* [Technicalities](Technicalities.md) - Various implementation specific details. If you think you've found a bug, it might help to look here.
* [Disclaimer](Disclaimer.md) - I warn you that I am not perfect so things can go wrong...

Below is a quick summary of why I believe Pipes are fun!

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

Code Examples
-------------
### Getting Started
The life of a pipe begins with the **PipeBuilder**. You specify the pipe you would like using a fluent builder style. Here are some examples:

```c#
var basicPipe = PipeBuilder.New.BasicPipe<string>().Build();
var capacityPipe = PipeBuilder.New.CapacityPipe<string>().WithCapacity(100).Build();
var eitherInletPipe = PipeBuilder.New.EitherInletPipe<string>().WithPrioritisingTieBreaker(Priority.Right).Build();
```

Pipes have **Inlets** and **Outlets**. You send messages down inlets, and you receive messages from outlets. For example:

```c#
// Send a message into the pipe to be picked up by a "receiver"
capacityPipe.Inlet.Send("Hello");

// Receive a message from the pipe that was sent by a "sender"
var message = capacityPipe.Outlet.Receive();

message.Should().Be("Hello");
```

You can connect pipes together to achieve unique pipe systems to suit your needs. For example:
```c#
basicPipe.Outlet.ConnectTo(capacityPipe.Inlet);
capacityPipe.Outlet.ConnectTo(eitherInletPipe.LeftInlet);

basicPipe.Inlet.Send("Travelled a long way");
var travelledMessage = eitherInletPipe.Outlet.Receive();
travelledMessage.Should().Be("Travelled a long way");
```

The behaviour of the individual pipes will be explained later. The important bit is that inlets and outlets can be connected together, and when you send / receive messages down the pipe system, it will be quickly resolved over the whole system.

That's essentially all there is to it! Pipes, their inlets and their outlets are all customisable, so it's a little tricky to describe exactly what they do without going into specifics, but normally:
* Any number of threads can try to receive from the same outlet.
* Any number of threads can try to send down the same inlet.
* Most pipes force a sending thread to wait for a receiving thread to be available and vice versa.
* The whole pipe system is thread safe.

<sup>**Note:** When multiple threads interact with the same in/outlet, they are held in a queue to ensure "fairness".</sup>
