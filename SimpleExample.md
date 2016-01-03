Simple Example
==============
[Click to return to Pipes](README.md)
### First Pipe System
A **capacity** pipe allows us to send a message down the pipe without waiting for a receiver - an asynchronous write. The capacity pipe will send these messages on later as soon as receivers arrive.

An **either outlet** pipe allows us to send a message so long as their is a receiver on either of the pipe's outlets. If there isn't one yet, we'll be forced to wait for a receiver to arrive.

What if we wanted to be able to send a message asynchronously to the either outlet pipe? These messages should be passed to receivers arriving on the outlets of the either outlet pipe, but we don't want to wait for them. We can do this as follows:

```c#
var capacityPipe = PipeBuilder.New.CapacityPipe<string>().WithCapacity(int.MaxValue).Build();
var eitherOutletPipe = PipeBuilder.New.EitherOutletPipe<string>().Build();

capacityPipe.Outlet.ConnectTo(eitherOutletPipe.Inlet);

capacityPipe.Inlet.Send("Hi");
// The "Hi" message now sits in the capacity pipe, waiting for a receiver to appear.

// A receiver arrives on the left outlet
var message = eitherOutletPipe.LeftOutlet.Receive();
// As the either outlet pipe is connected to the capacity pipe, this acts as a receiver
// for the capacity pipe as well. Hence, we receive the message in the capacity pipe!

message.Should().Be("Hi");
```

The capacity pipe and either outlet pipe form a **pipe system**. This system has one inlet - the inlet of the capacity pipe - and two outlets - the outlets of the either outlet pipe. Note that the system does not expose the inlet of the either outlet pipe or the outlet of the capacity pipe because they are connected.

### Creating Your Own Pipe

If we believe we'll need to reuse the pipe system above, we can actually turn it into a pipe! We can then create this pipe elsewhere as needed and share its behaviour.

Firstly, we'll create an interface for our pipe to decide what it will look like.

```c#
public interface IAsynchronousEitherOutletPipe<TMessage> : IPipe
{
    ISimpleInlet<TMessage> Inlet { get; }
    ISimpleOutlet<TMessage> LeftOutlet { get; }
    ISimpleOutlet<TMessage> RightOutlet { get; }
}
```

Some notes:
* The interface **IPipe** must be extended for this to be recognised as a pipe in a pipe system.
* The **Simple Inlet/Outlets** interfaces allow threads to use these to send / receive messages, while also allowing the inlets / outlets to be connected to other pipes.

Our implementation is then:
```c#
public class AsynchronousEitherOutletPipe<TMessage> : CompositePipe, IAsynchronousEitherOutletPipe<TMessage>
{
    public ISimpleInlet<TMessage> Inlet { get; }
    public ISimpleOutlet<TMessage> LeftOutlet { get; }
    public ISimpleOutlet<TMessage> RightOutlet { get; }

    public AsynchronousEitherOutletPipe(
        ISimpleInlet<TMessage> inlet, 
        ISimpleOutlet<TMessage> leftOutlet,
        ISimpleOutlet<TMessage> rightOutlet)
        : base(new[] {inlet}, new[] {leftOutlet, rightOutlet})
    {
        Inlet = inlet;
        LeftOutlet = leftOutlet;
        RightOutlet = rightOutlet;

        // Create the internal pipe system.
        var eitherOutletPipe = PipeBuilder.New.EitherOutletPipe<TMessage>().Build();
        var capacityPipe = PipeBuilder.New.CapacityPipe<TMessage>().WithCapacity(int.MaxValue).Build();

        capacityPipe.Outlet.ConnectTo(eitherOutletPipe.Inlet);
        
        // Wire the internal pipe system to "this" pipe.
        CreateAndConnectAdapter(capacityPipe.Inlet, Inlet);
        CreateAndConnectAdapter(eitherOutletPipe.LeftOutlet, LeftOutlet);
        CreateAndConnectAdapter(eitherOutletPipe.RightOutlet, RightOutlet);
    }
}
```
Some notes:
* Extend the **CompositePipe** abstract class when building a pipe which is just a combination of existing pipes.
    * It handles the less interesting *plumbing* for you. (sorry :) )
* Accept the publically visible inlets and outlets in your constructor.
    * These inlets / outlets will be considered when checking the "pipe graph" to see if it forms a tree.
    * They are required by the base class, which will automatically connect them to this pipe.
    * This also allows people to potentially use their own implementations of in/outlets with this pipe!
* Use **CreateAndConnectAdapter** to associate "internal" in/outlets with your public in/outlets.

#### Use of Adapters

Adapters require some more explanation. The short story is your internal pipe system's in/outlets need to be associated to the corresponding public ones. If you're happy just to follow the example, skip to the next section.

While you could expose the inlets and outlets of your internal pipe system to the outside, this is considered bad practice as then the outside world can see how you actually work - something they shouldn't care about. To make matters worse, it appears that the in/outlets are somehow associated to two pipes at once - the "pipe" you created above and their pipe in your pipe system. While this rarely matters in practice, it's easy to void.

**CreateAndConnectAdapter** will ensure that any requests made against a public inlet will be forwarded to the internal inlet, and vice versa. This leaves the two inlets disconnected, so the internal pipe system will be hidden and any interactions with it are managed by your pipe.

If you're interested in how this actually works, see the [CompositePipe](Pipes/Pipes/Models/Pipes/CompositePipe.cs).

<sup>**Note:** **CreateAndConnectAdapter** provides an added bonus of associating the internal inlet with your pipe. This ensures interactions with your pipe system are managed atomically. This is too technical for this section, but atomicity is managed through [Sticky Shared Resources](https://github.com/michaelbradley91/StickySharedResources). In other words, the method "connects the pipe's resource to the internal in/outlet's resource". </sup>

### Creating a Pipe Builder

Before we created our new pipe above, all pipes were created using the Pipe Builder. It would be great if we could add our pipe to this fluent build syntax. As it just uses regular non-static c# classes, we can!

```c#
public static class PipeExtensions
{
    public static IAsynchronousEitherOutletPipeBuilder<TMessage> AsynchronousEitherOutletPipe<TMessage>(
        this IPipeBuilder pipeBuilder)
    {
        return new AsynchronousEitherOutletPipeBuilder<TMessage>();
    }
}

public interface IAsynchronousEitherOutletPipeBuilder<TMessage>
{
    IAsynchronousEitherOutletPipe<TMessage> Build();
}

public class AsynchronousEitherOutletPipeBuilder<TMessage> : IAsynchronousEitherOutletPipeBuilder<TMessage>
{
    public IAsynchronousEitherOutletPipe<TMessage> Build()
    {
        var promisedPipe = new Promised<IPipe>();

        var inlet = new SimpleInlet<TMessage>(promisedPipe);
        var leftOutlet = new SimpleOutlet<TMessage>(promisedPipe);
        var rightOutlet = new SimpleOutlet<TMessage>(promisedPipe);

        return promisedPipe.Fulfill(new AsynchronousEitherOutletPipe<TMessage>(inlet, leftOutlet, rightOutlet));
    }
}
```
Some notes:
* **IPipeBuilder** is the root class of the builder syntax.
* Inlets and Outlets require a **Promised Pipe**.
    * Communication between pipes and thier in/outlets is bidirectional.
    * Therefore, we provide an object which will hold a pipe before the in/outlets actually require it.

Finally, our code for the asynchronous pipe can become:
```c#
var asynchronousEitherOutletPipe = PipeBuilder.New.AsynchronousEitherOutletPipe<string>().Build();

asynchronousEitherOutletPipe.Inlet.Send("Hi");

var message = asynchronousEitherOutletPipe.LeftOutlet.Receive();

message.Should().Be("Hi");
```
By extending the pipe builder, everyone else can easily find our pipe. This  reduces code duplication, and we can extend our asynchronous either outlet pipe builder to provide greater flexibility. We might one day write:

```c#
var asynchronousEitherOutletPipe = 
    PipeBuilder.New.AsynchronousEitherOutletPipe<string>().WithPrioritisingTieBreaker().Build();
```

That's it for the simple example. If you understood all of that, you can do most of what you'll ever need to.
