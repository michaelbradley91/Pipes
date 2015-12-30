Simple Example
==============
[Click to return to Pipes](README.md)
### First Pipe System
A **capacity** pipe allows us to send a message down the pipe without waiting for a receiver - an asynchronouse write. The capacity pipe will send these messages on later as soon as receivers arrive.

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

If we believe we'll need to reuse the pipe system above, we can actually turn it into a pipe! We can then create this pipe elsewhere as needed and share its behaviour. It can be implemented as follows:

```c#
public interface IAsynchronousEitherOutletPipe<TMessage> : 
    IEitherOutletPipe<IPrioritisingTieBreaker, TMessage> { }

public class AsynchronousEitherOutletPipe<TMessage> : 
    EitherOutletPipe<IPrioritisingTieBreaker, TMessage>, IAsynchronousEitherOutletPipe<TMessage>
{
    public AsynchronousEitherOutletPipe(
        ISimpleInlet<TMessage> inlet, 
        ISimpleOutlet<TMessage> leftOutlet, 
        ISimpleOutlet<TMessage> rightOutlet) 
        : base(inlet, leftOutlet, rightOutlet, new PrioritisingTieBreaker(Priority.Left))
    {
    }

    public static IAsynchronousEitherOutletPipe<TMessage> Create()
    {
        var eitherOutletPipe = PipeBuilder.New.EitherOutletPipe<TMessage>().Build();
        var capacityPipe = PipeBuilder.New.CapacityPipe<TMessage>().WithCapacity(int.MaxValue).Build();

        eitherOutletPipe.Inlet.ConnectTo(capacityPipe.Outlet);

        return new AsynchronousEitherOutletPipe<TMessage>(
            capacityPipe.Inlet, 
            eitherOutletPipe.LeftOutlet, 
            eitherOutletPipe.RightOutlet);
    } 
}
```

<sup>**Note:** We'll see a more difficult and complete example later.</sup>

The important thing to note is that the "Create" method connects the pipes together, so it actually does most of the work. In fact, the class above would just be an either outlet pipe with a prioritising tie breaker if it were not for the create method!

The code from the previous section now becomes:

```c#
var asynchronousEitherOutletPipe = AsynchronousEitherOutletPipe<string>.Create();

asynchronousEitherOutletPipe.Inlet.Send("Hi");

var message = asynchronousEitherOutletPipe.LeftOutlet.Receive();

message.Should().Be("Hi");
```

Other code can now create an asynchronous either outlet pipe without duplicating the logic of connecting the pipes together!

We also have an added bonus that the connected inlet and outlet are hidden from the outside world - they cannot be disconnected easily by a different process.

### Extending the Pipe Builder

Before we created our new pipe above, all pipes were created using the Pipe Builder. It would be great if we could add our pipe to this fluent build syntax. As it just uses regular non-static c# classes, we can!

```c#
public static class PipeExtensions
{
    public static IAsynchronousEitherOutletPipeBuilder<TMessage> AsynchronousEitherOutletPipe<TMessage>(this IPipeBuilder pipeBuilder)
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
        var eitherOutletPipe = PipeBuilder.New.EitherOutletPipe<TMessage>().Build();
        var capacityPipe = PipeBuilder.New.CapacityPipe<TMessage>().WithCapacity(int.MaxValue).Build();

        eitherOutletPipe.Inlet.ConnectTo(capacityPipe.Outlet);

        return new AsynchronousEitherOutletPipe<TMessage>(
            capacityPipe.Inlet,
            eitherOutletPipe.LeftOutlet,
            eitherOutletPipe.RightOutlet);
    }
}
```

Finally, our code for the asynchronous pipe can become:
```c#
var asynchronousEitherOutletPipe = PipeBuilder.New.AsynchronousEitherOutletPipe<string>().Build();

asynchronousEitherOutletPipe.Inlet.Send("Hi");

var message = asynchronousEitherOutletPipe.LeftOutlet.Receive();

message.Should().Be("Hi");
```
By extending the pipe builder, everyone else can easily find our pipe. We can also extend our asynchronous either outlet pipe builder to provide greater flexibility as with any builder. We might one day write:

```c#
var asynchronousEitherOutletPipe = 
    PipeBuilder.New.AsynchronousEitherOutletPipe<string>().WithPrioritisingTieBreaker().Build();
```

That's it for the simple example. If you understood all of that, you can do most of what you'll ever need to.
