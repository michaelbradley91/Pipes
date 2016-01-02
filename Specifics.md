Specifics
=========
[Click to return to Pipes](README.md)
Inlet and Outlet Behaviour
--------------------------
This package comes with with two types of inlets and outlets:

### Simple Inlets and Outlets:

These are the inlets and outlets you'll use most often. Inlets let your thread send messages down them, while outlets let your thread wait to receive a message from them.

The Simple Inlet exposes:
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

The Simple Outlet exposes:
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

### Adapter Inlets and Outlets:

These are exactly like simple inlets and outlets, but they do not allow threads to send or receive messages with them. They can be connected to other pipes however.

These are used when building composite pipes - pipes that are really made up of many "internal" pipes and do not want to directly expose certain inlets and outlets. However, their use is entirely optional.

### More Stuff about Inlets and Outlets:
When you connect an inlet to an outlet, any possible sends and receives in the resulting pipe system are processed **immediately**. Normally, you will create your pipe system and then never change it, but you can exploit this behaviour. (Disconnecting and connecting is also thread safe).

For example, you can implement pass the parcel by connecting and disconnecting capacity pipes!

An inlet can only be connected to one outlet and vice versa. You cannot attempt to send down a connected inlet, or receive from a connected outlet. This will throw an exception.

You cannot connect an inlet or outlet if a thread is waiting on such an inlet / outlet. Doing so will result in an exception.

Finally, inlets and outlets are strongly typed like their pipes. This means that an inlet can only be connected to an outlet expecting messages of the same type.

<sup>**Note:** If you are concerned this implies a pipe system is restricted to a single type - don't worry. Pipes can still have inlets and outlets of different types associated to them. An example in the package is the "TransformPipe" that accepts "x" and returns "f(x)" where f's result type might be different to x's type.

Pipe Behaviour
--------------
This is a **very** brief explanation of the different pipes available in the package.

### Basic Pipe
* One inlet
* One outlet
* No parameters

If you try to receive a message from the outlet, you'll be forced to wait for a sender on the inlet.
If you try to send a message down the inlet, you'll be forced to wait for a receiver of your message on the outlet.

### Capacity Pipe
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

### Either Inlet Pipe
* Two inlets
* One outlet
* Includes a tie breaker

If you try to receive a message from this pipe, you will receive a message from either the left or the right inlet - whichever arrives first. If it is possible to receive both from the left and right inlets, then the tie breaker will decide which side to use.

You can implement any tie breaker you like. The package provides three: randomised, prioritised, and alternating.

### Either Outlet Pipe
* One inlet
* Two outlets
* Includes a tie breaker

Much like the either inlet pipe, if you try to send a message, it will go either the left or right sender - whichever arrives first. If they are both available, the tie breaker decides who the message will go to.

### Splitting Pipe
* One inlet
* Two outlets
* No other parameters

This is the really important one. A splitting pipe will copy the message sent to its inlet to both outlets. Specifically, if you try to send on this pipe, you will be made to wait until a receiver arrives on the left outlet, **and** receiver arrives on the right outlet. Once the receivers are there, they both receive a copy of the message ("at the same time").

Note that this pipe acts in a similar way to a transistor, and provides you with a lot of power.

### Source Pipe
* Zero inlets
* One outlet
* Accepts a "Message Producer"

A source pipe produces messages using a "Producer" which is simply a function. The producer could always produce the same message, or produce messages in as sequence. This function however should not rely on other pipes to determine its values. Essentially - keep the function simple.

### Sink Pipe
* One inlet
* Zero outlets
* No other parameters

A sink pipe consumes messages and throws them away. This might not seem terribly useful at first, but by connecting it to a splitting pipe you effectively turn a splitting pipe into a basic pipe, if that gives you some ideas. It will always be willing to receive messages.

### Transform Pipe
* One inlet
* One outlet
* A "map" function

A transform pipe converts the message arriving on its inlet with the "map" function, and sends the result to its outlet. So x -> Transform Pipe -> f(x). As with the source pipe, this function should be simple. In particular, it should not utilise other pipes.

### Valved Pipe
* One inlet
* One outlet
* One valve

A valved pipe allows you to send or receive a message at the same time through its valve. The syntax is:
```c#
var result = valvedPipe.Valve.ReceiveOrSend(messageToSend);
if (result.MessageReceived)
{
    var receivedMessage = result.GetReceivedMessage();
    // I received a message. Therefore, "messageToSend" was not sent.
}
else
{
    // I sent a message. Therefore, I sent "messageToSend".
}
```
The valve listens to the pipe's inlet for messages to receive, and it tries to send your message down its outlet.
Note that by sticking pipes to the inlet / outlet of this pipe, you can try to receive or send from many pipes at once!
