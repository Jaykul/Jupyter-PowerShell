# A [Jupyter](https://jupyter.org/) Kernel for [PowerShell](https://github.com/PowerShell/PowerShell)

Create PowerShell notebooks in a web browser, with commands and captured output. Add markdown blocks for documentation!

You can use Jupyter with PowerShell to produce documentation of your troubleshooting, researching, and even your regular processes. You can also use HTML and Javascript with your PowerShell data to create visual reports, do numerical analysis, etc.

## Get it for yourself

The easiest way to try the kernel is using Binder. You can just click here: [![Binder](https://mybinder.org/badge.svg)](https://mybinder.org/v2/gh/jaykul/Jupyter-PowerShell/master)

The next easiest way is using docker (if you have docker installed). You can start a copy like this:

```posh
docker run -it --rm -p 8888:8888 jaykul/powershell-notebook-base
```

You can also install the kernel locally (assuming you have Jupyter or a clone installed) using [chocolatey](http://chocolatey.org/):

```posh
choco install jupyter-powershell --allow-prerelease
```

## Current Status

The PowerShell kernel is based on PowerShell Core, in order to be cross-platform and standalone.

At this point, I'm only handling two messages:

* KernelInfo request
* Execute request

### Features

Since Jupyter is all about interaction and documentation, if you want details about the features, you can read the [Features](https://github.com/Jaykul/Jupyter-PowerShell/blob/master/Features.ipynb) notebook here on github, or by running the binder link above.

## PowerShell Core

In order to get cross-platform support, this kernel is based on [PowerShell Core](https://github.com/PowerShell/PowerShell).

To build it yourself --or to run the "PowerShell (Core)" kernel-- you need [dotnet core 2](https://www.microsoft.com/net/core).  You can build it by running `dotnet restore; dotnet build` from the root. If you want to build it in Visual Studio, you need VS 2017 version 15.3 or higher.

## A Note on the Jupyter library

This kernel is being written in C#, and in the process, I've taken some messaging code from the [iCSharp kernel](https://github.com/zabirauf/icsharp) and made a generic library for .Net with a re-usable core for anyone who needs to create [Jupyter](https://jupyter.org/) kernels in .Net languages -- so feel free to borrow that if you like (it's under the Apache license).
