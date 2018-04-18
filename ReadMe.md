# A [Jupyter](https://jupyter.org/) Kernel for [PowerShell](https://github.com/PowerShell/PowerShell)

This kernel is being written in C#, and in the process, I've taken some messaging code from the [iCSharp kernel](https://github.com/zabirauf/icsharp) and made a generic library for .Net with a re-usable core for anyone who needs to create [Jupyter](https://jupyter.org/) kernels in .Net languages -- so feel free to borrow that if you like (it's under the Apache license).

## Install

I am finally doing a preliminary release: you can download from the releases link, unzip it somewhere, and run the `Install.ps1` script. Note that if you run this on Linux or OS X you should expect to see only "PowerShell (Core)" but on Windows you'll see both -- but only the "PowerShell (Full)" will actually work unless you have PowerShell Core installed in your PATH and working.

## Current Status

At this point, I'm only handling two messages:

* KernelInfo request
* Execute request

The PowerShell kernel is _working_, and returning text output _and errors_ as on the console (see examples below).

## Features

Apart from the built-in Jupyter features, I'm going to add some output enhancements so you can hook into widgets, etc. However, there's none of thata yet, except that:

* If you output HTML, it's rendered. I'm currently detecting this in the most simplistic fashion: by testing if the output starts with "<" and ends with ">". That probably needs work, but it's good enough for now.
* When a command outputs objects, you get the text rendering, but the actual objects are also returned as application/json data.

## PowerShell Core

In order to get cross-platform support, this kernel is based on [PowerShell Core](https://github.com/PowerShell/PowerShell).

To build it yourself --or to run the "PowerShell (Core)" kernel-- you need [dotnet core 2](https://www.microsoft.com/net/core).  You can build it by running `dotnet restore; dotnet build` from the `Source` folder. If you want to build it in Visual Studio, you need VS 2017 version 15.3 or higher.

## Examples

I have [a version of this document with examples](https://github.com/Jaykul/Jupyter-PowerShell/blob/master/ReadMe.ipynb) in it as a Jupyter Notebook, which mostly works, in read-only mode, on github...
