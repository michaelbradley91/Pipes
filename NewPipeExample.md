New Pipe Example
==============
[Click to return to Pipes](README.md)

This assumes a little that you have read the [Composite Pipe Example](SimpleExample.md).
In particular, this example will not cover extending the fluent build syntax.

### Quick Intro
Creating pipes from existing pipes is fairly easy, but now we would like to implement a pipe which cannot be built from existing pipes.

We would like to create a **SwitchOutletPipe** with the following behaviour:
* The switch outlet pipe initially will only send a message down its left outlet.
* Every time a message is sent, it will switch outlets, so that the next message must be sent through the opposite outlet.

For example, we'd like to be able to write this test:
```c#

```

