## iPowerShell - a [Jupyter](https://jupyter.org/) Kernel for PowerShell

I've finally got a working PowerShell Jupyter kernel! 

In the process, I've taken some messaging code from [zabirauf's iCSharp kernel](https://github.com/zabirauf/icsharp) and made a generic "Jupyter .NET" library to provide a re-usable core for creating Jupyter kernels in .Net languages -- so feel free to borrow that if you like (it's under the Apache license).

## First Working Version

At this point, I'm only handling two messages from Jupyter:

* KernelInfo request
* Execute request

The PowerShell kernel is _working_ at this point, but I'm always returning text, json, and HTML output -- and Jupyter notebooks are smart enough to display the HTML output.  The problem is that PowerShell's `ConvertTo-Html` isn't smart enough to only show the default columns, and do reasonable things with numbers and strings -- so I have a bit of work to do yet.

In any case, it's working, as you can [see here](ReadMe.ipynb).
