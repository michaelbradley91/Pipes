Getting Started
=================
[Click to return to Pipes](README.md)
### Creating Pipes
The **PipeBuilder** provides you with a fluent syntax for creating pipes. While you can use the constructors on pipes directly,
this class should simplify the work. Here are some examples:

```c#
var basicPipe = PipeBuilder.New.BasicPipe<string>().Build();
var capacityPipe = PipeBuilder.New.CapacityPipe<string>().WithCapacity(100).Build();
var eitherInletPipe = PipeBuilder.New.EitherInletPipe<string>().WithPrioritisingTieBreaker(Priority.Right).Build();
```

The generic types above specify the type of messages processed by the pipe.

<sup>**Note:**Some pipes might have multiple types if their inlets / outlets expose different types of messages.</sup>

### Using Pipes
Pipes have **Inlets** and **Outlets**. You send messages down inlets, and you receive messages from outlets. For example:

```c#
// Send a message into the pipe to be picked up by a "receiver"
capacityPipe.Inlet.Send("Hello");

// Receive a message from the pipe that was sent by a "sender"
var message = capacityPipe.Outlet.Receive();

message.Should().Be("Hello");
```

### Connecting Pipes
You can connect pipes together to achieve unique pipe systems to suit your needs. For example:
```c#
basicPipe.Outlet.ConnectTo(capacityPipe.Inlet);
capacityPipe.Outlet.ConnectTo(eitherInletPipe.LeftInlet);

basicPipe.Inlet.Send("Travelled a long way");
var travelledMessage = eitherInletPipe.Outlet.Receive();
travelledMessage.Should().Be("Travelled a long way");
```
Inlets can be connected to outlets allowing you to build a "network" of pipes called a **pipe system**.

The end result is that this pipe system exposes some inlets and outlets to the outside world - a bunch of interacting services / threads.

Each service can focus on just sending or receiving messages down the inlets and outlets it was passed, **without worrying about the design of the system as a whole**.

The pipe system should be constructed where it makes sense to know how the application is connected together. For web developers, this resembles the role of the [Composition Root](http://blog.ploeh.dk/2011/07/28/CompositionRoot/).

The behaviour of individual pipes is explained in [Specifics](Specifics.md).

### Summary
That's essentially all there is to it! Pipes, their inlets and their outlets are all customisable, so it's a little tricky to describe exactly what they do without going into specifics, but normally:
* Any number of threads can try to receive from the same outlet.
* Any number of threads can try to send down the same inlet.
* Most pipes force a sending thread to wait for a receiving thread to be available and vice versa.
* The whole pipe system is thread safe.

<sup>**Note:** When multiple threads interact with the same in/outlet, they are held in a queue to ensure "fairness".</sup>
