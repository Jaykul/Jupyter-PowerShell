## iPowerShell - a Jupyter Kernel for PowerShell

I'm working on a PowerShell Jupyter kernel, but so far what I've done is take some messaging code from [zabirauf's iCSharp kernel](https://github.com/zabirauf/icsharp) and make a generic "Jupyter .NET" library to provide the re-usable for creating Jupyter kernels in .Net languages.

### It's _not ready yet!_

I've just gotten the bare minimum running, this is the first check-in.

* KernelInfo message handling works
* Execute is an echo host (doesn't run code, just echo's back)

I figured I'd share this first commit publically because it's going to be a good place to start if you want to write your own kernel in .net for something else.
For what it's worth, though, the FSharp team has an awesome [iFSharp kernel](https://github.com/fsprojects/IfSharp) already, which even includes it's own custom charting tools and more -- they're even working with the Python folks at Microsoft to host [notebooks.azure.com](https://notebooks.azure.com).
And of course, the aforementioned [iCSharp kernel](https://github.com/zabirauf/icsharp) is currently far enough ahead of me that I'm copying their code...