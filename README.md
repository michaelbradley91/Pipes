Pipes
=====
Disclaimer
----------
This package is pretty new so it might have bugs in it. It is thoroughly tested (when this was written - 226 tests!) but as this project concerns parallel programming, it is hard to test enough!

If you find a bug, please email michael.bradley@hotmail.co.uk, preferably with the simplest code you can manage to reproduce the problem.

Features That Might Be Coming
-----------------------------

* Generalised versions of the pipes (n inlets / m outlets).
* Repeater pipe that repeats a message n times.
* Filter pipe to automatically "swallow" certain messages.

Please email michael.bradley@hotmail.co.uk if you'd like something specific. Thanks!

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

This project is written in C# and is available as a Nuget Package with ID Parallel.Pipes, so it's super easy to get started! Read more below to learn how to use them.

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

Specifics
---------

### Inlet and Outlet Behaviour
This package comes with one default inlet and one default outlet (called "simple" inlet and outlet), and it's expected you'll typically use these without thinking about them. They expose the following:

#### Inlets:
```c#
// Try to send a message, and wait indefinitely for the pipe system to accept the message
// (the thread can still be interrupted safely)
basicPipe.Inlet.Send("Message");

// Throw a TimeoutException if the message can't be sent within 10 seconds.
basicPipe.Inlet.Send("Waiting a While", TimeSpan.FromSeconds(10));

// Throw an InvalidOperationException if the message can't be sent "right now"
basicPipe.Inlet.SendImmediately("Impatient Message");

basicPipe.Inlet.ConnectTo(anotherPipe.Outlet);
basicPipe.Inlet.Disconnect();
```

#### Outlets:
```c#
// Try to receive a message, and wait indefinitely for the pipe system to provide one.
// (the thread can still be safely interrupted)
basicPipe.Outlet.Receive();

// Throw a TimeoutException if a message can't be received within 10 seconds.
basicPipe.Outlet.Receive(TimeSpan.FromSeconds(10));

// Throw an InvalidOperationException if a message can't be received "right now"
basicPipe.Outlet.ReceiveImmediately();

basicPipe.Outlet.ConnectTo(anotherPipe.Inlet);
basicPipe.Outlet.Disconnect();
```

#### More Stuff about Inlets and Outlets:
When you connect an inlet to an outlet, any possible sends and receives in the resulting pipe system are processed **immediately**. Normally, you will create your pipe system and then never change it, but you can exploit this behaviour. (Disconnecting and connecting is also thread safe).

For example, you can implement pass the parcel by connecting and disconnecting capacity pipes!

An inlet can only be connected to one outlet and vice versa. You cannot attempt to send down a connected inlet, or receive from a connected outlet. This will throw an exception.

You cannot connect an inlet or outlet if a thread is waiting on such an inlet / outlet. Doing so will result in an exception.

Finally, inlets and outlets are strongly typed like their pipes. This means that an inlet can only be connected to an outlet expecting messages of the same type.

<sup>**Note:** If you are concerned this implies a pipe system is restricted to a single type - don't worry. Pipes can still have inlets and outlets of different types associated to them. An example in the package is the "TransformPipe" that accepts "x" and returns "f(x)" where f's result type might be different to x's type.

### Pipes

This is a **very** brief explanation of the different pipes available in the package.

#### Basic Pipe
* One inlet
* One outlet
* No parameters

If you try to receive a message from the outlet, you'll be forced to wait for a sender on the inlet.
If you try to send a message down the inlet, you'll be forced to wait for a receiver of your message on the outlet.

#### Capacity Pipe
* One inlet
* One outlet
* Has a "capacity"
* Exposes the messages stored in the pipe (readonly)

Messages can be stored in this pipe up to its capacity. A capacity pipe will send its messages on to receivers (or other pipes connected to its outlet) as quickly as it can.

When you send down this pipe:
* If the pipe is full and no receivers exist, you'll be forced to wait.
* If the pipe has spare capacity your message will be stored in the pipe and sent on to a receiver when possible. (Your thread will not be blocked)

When you receive from this pipe:
* If the pipe has stored messages you will receive the oldest message.
* Otherwise if the pipe is empty, you will have to wait for a sender.

#### Either Inlet Pipe
* Two inlets
* One outlet
* Includes a tie breaker

If you try to receive a message from this pipe, you will receive a message from either the left or the right inlet - whichever arrives first. If it is possible to receive both from the left and right inlets, then the tie breaker will decide which side to use.

You can implement any tie breaker you like. The package provides three: randomised, prioritised, and alternating.

#### Either Outlet Pipe
* One inlet
* Two outlets
* Includes a tie breaker

Much like the either inlet pipe, if you try to send a message, it will go either the left or right sender - whichever arrives first. If they are both available, the tie breaker decides who the message will go to.

#### Splitting Pipe
* One inlet
* Two outlets
* No other parameters

This is the really important one. A splitting pipe will copy the message sent to its inlet to both outlets. Specifically, if you try to send on this pipe, you will be made to wait until a receiver arrives on the left outlet, **and** receiver arrives on the right outlet. Once the receivers are there, they both receive a copy of the message ("at the same time").

Note that this pipe acts in a similar way to a transistor, and provides you with a lot of power.

#### Source Pipe
* Zero inlets
* One outlet
* Accepts a "Message Producer"

A source pipe produces messages using a "Producer" which is simply a function. The producer could always produce the same message, or produce messages in as sequence. This function however should not rely on other pipes to determine its values. Essentially - keep the function simple.

#### Sink Pipe
* One inlet
* Zero outlets
* No other parameters

A sink pipe consumes messages and throws them away. This might not seem terribly useful at first, but by connecting it to a splitting pipe you effectively turn a splitting pipe into a basic pipe, if that gives you some ideas. It will always be willing to receive messages.

#### Transform Pipe
* One inlet
* One outlet
* A "map" function

A transform pipe converts the message arriving on its inlet with the "map" function, and sends the result to its outlet. So x -> Transform Pipe -> f(x). As with the source pipe, this function should be simple. In particular, it should not utilise other pipes.

### Restriction of Pipe Systems to Trees

**This is a technical section. Feel free to skip as you'll probably never need to know this.**

If you start building complex pipe systems, you'll probably find its restriction that your system forms a tree a bit unreasonable. Obviously, your pipe system is unlikely to make sense if you include a cycle, but it is possible your pipe system still forms a DAG (directed acyclic graph).

The reason for this is the splitting pipe. The splitting pipe needs to know that the receivers on its left outlet and right outlet will not disappear. Consider the following:
* A sender arrives.
* The splitting pipe sees that there is a receiver on its left outlet and a receiver on its right outlet.
* The splitting pipe forwards the message to the left outlet.
* The splitting pipe then tries to forward the message to the right outlet, but the right outlet no longer has a receiver!!

This is deeply confusing for the poor splitting pipe. The pipe system is considered "locked" during the bullets above, so a receiver should not have been able to leave. However, if the left and right receivers are really the "same" receiver (the same thread), the splitting pipe won't know what to do. This happens if you connect the two outlets of a splitting pipe to the two inlets of an either inlet pipe for example.

After attempting a few workarounds, it turned out this is actually quite a pain to solve. Therefore, the restriction that the pipe system forms a tree was put in place.

After considering a number of common use-cases, it seems a tree is sufficient for almost any situation. However, if you desperately need to form a DAG the pipe system will **probably** still work so long as the splitting pipe's receivers are known to be different threads. However, I'd recommend you avoid hacking around with this unless you're extremely confident you understand pipes.


