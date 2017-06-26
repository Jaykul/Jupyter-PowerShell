# A [Jupyter](https://jupyter.org/) Kernel for [PowerShell](https://github.com/PowerShell/PowerShell)

This kernel is being written in C#, and in the process, I've taken some messaging code from the [iCSharp kernel](https://github.com/zabirauf/icsharp) and made a generic library for .Net with a re-usable core for anyone who needs to create [Jupyter](https://jupyter.org/) kernels in .Net languages -- so feel free to borrow that if you like (it's under the Apache license).

## Current Status

At this point, I'm only handling two messages:

* KernelInfo request
* Execute request

The PowerShell kernel is therefore _working_, and returning simple text output as on the console (see examples below).

### NO ERROR OUTPUT

I am doing something wrong in the way I'm sending error output back, so there's currently [no output when there are errors](https://github.com/Jaykul/Jupyter-PowerShell/issues/3). That's my number one priority to fix, if anyone knows what I'm doing wrong -- I'll get to it tonight, otherwise.

## Features

Apart from the built-in Jupyter features, I'm going to add some output enhancements so you can hook into widgets, etc. However, there's none of thata yet, except that:

* If you output HTML, it's rendered. I'm currently detecting this in the most simplistic fashion: by testing if the output starts with "<" and ends with ">". That probably needs work, but it's good enough for now.
* When a command outputs objects, you get the text rendering, but the actual objects are also returned as application/json data.

## PowerShell Core

In order to get cross-platform support, this kernel is using [PowerShell Core](https://github.com/PowerShell/PowerShell), which means you'll want to have PowerShell 6 Beta 3 installed to try it out. I'm hoping to provide a Full Framework (aka Windows PowerShell) version too, once I start doing releases.

To use it or build it, you need [dotnet core 2 preview](https://www.microsoft.com/net/core/preview), and if you want to contribute, and want to build it in Visual Studio, you need [VS 2017 Preview version 15.3](https://www.visualstudio.com/vs/preview/).