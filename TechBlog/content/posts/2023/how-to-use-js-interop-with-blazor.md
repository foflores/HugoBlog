---
title: "How to use JS Interop with Blazor"
date: 2023-09-19T00:00:00-04:00
# weight: 1
# aliases: [""]
tags: ["Blazor"]
author: "Favian Flores"
showToc: true
TocOpen: false
draft: false
hideMeta: false
comments: false
canonicalURL: "https://favianflores.com/posts/2023/how-to-use-js-interop-with-blazor/"
disableHLJS: false
disableShare: false
hideSummary: false
searchHidden: false
ShowReadingTime: true
ShowBreadCrumbs: true
ShowPostNavLinks: true
ShowWordCount: false
ShowRssButtonInSectionTermList: false
UseHugoToc: true
cover:
    image: "" # image path/url
    alt: "" # alt text
    caption: "" # display caption under cover
    relative: false # when using page bundles set this to true
    hidden: true # only hide on current single page
---

One of the main benefits of working with blazor is that you can use C# both server side and client side. It allows frontend and backend developers to share a single codebase (something that was only possible with javascript before). This feature is possible because of Web Assembly which gives .NET a low level language to target when compiling C#. 

One issue with Web Assembly is that it is still in development and doesn't have direct access to the DOM like javascript does. So calls to the DOM need to be handled with an *IJSRuntime* interface which allows developers to call JavaScript functions from .NET methods and vice-versa.

## Injecting the IJSRuntime interface

When working inside of a blazor project, the framework automatically registers the IJSRuntime interface for dependency injection. This allows you to use the interface by calling the @inject directive inside a razor page:

```csharp
@page "/example"
@inject IJSRuntime Js // <-- injected here

<h1>Example Title</h1>

@code {
    ...
    await JS.InvokeVoidAsync(...) // <-- used here
    ...
}
```

## Calling JavaScript functions from .NET

The IJSRuntime interface exposes 2 methods, InvokeVoidAsync and InvokeAsync. The InvokeVoidAsync function is used when you need call a javascript function that doesn't return a result and InvokeAsync is for when it does. They both take a string parameter which is the name for the javascript function you want to call. You can also add parameters after the identifier and .NET will pass those as parameters to the javascript function.

For example, if you need to remove focus from an html element, you can write:

```csharp
await Js.InvokeVoidAsync("document.activeElement.blur");
```

If you need to determine the current inner height of the window, you can write:

```csharp
var height = await Js.InvokeAsync<double>("window.innerHeight");
```

**Keep in mind you need to provide the return type in order for .NET to properly return the result (a *double* in this case).**

If the function you want to call requires parameters, you can write:

```csharp
await Js.InvokeVoidAsync("window.alert", "This is an alert!");
```

The string "This is an alert!" will be passed to the alert function.

**You can provide multiple parameters in this way and all of them will be passed into the function.**

These examples are calling existing Web API functions, but you can also create your own functions and execute them using these methods.

## Calling .NET methods from JavaScript

### Calling a static method

.NET provides the invokeMethod and invokeMethodAsync methods to call static .NET methods from javascript. Both functions take in a string which is the name of the assembly containing the method you want to call and a string containing the name of the method you want to call.

```csharp
@page "/example"
@inject IJSRuntime Js

<button onclick="invokeDotNet()">Click Me</button>

<script>
  function invokeDotNet() 
  {
    DotNet.invokeMethodAsync("JSInteropExample", "InvokeDotNet");
  }
</script>

@code {
  [JSInvokable]
  public static void InvokeDotNet()
  {
    Console.WriteLine("Called from javascript");
  }
}
```

In this example, the button triggers the javascript invokeDotNet function which then triggers the InvokeDotNet method.

### Calling an instance method

.NET also provides a way for you to pass a reference to an instance of a class to javascript. Then you can use that instance to call non-static methods.

For example, if I define this class:
```csharp
  public class ExampleClass
  {
    [JSInvokable]
    public void InvokeDotNet()
    {
      Console.WriteLine("Called from javascript");
    }
  }
```

Then I can call the instance method InvokeDotNet in javascript, by creating an object reference like so:

```csharp
@page "/example"
@inject IJSRuntime Js

<button @onclick="InvokeDotNet">Click Me</button>

<script>
  function invokeDotNet(objRef) 
  {
    objRef.invokeMethodAsync("InvokeDotNet"); // Calls instance method from object reference
  }
</script>

@code {
  public static void InvokeDotNet()
  {
    ExampleClass obj = new(); // Creates instance of ExampleClass
    objRef = DotNetObjectReference.Create(obj); // Creates reference to object
    await Js.InvokeVoidAsync("invokeDotNet", objRef); // Calls javascript function and passes object reference as a parameter
  }
}

```

In this example, the button triggers the .NET static method InvokeDotNet. The method creates an instance of ExampleClass and uses it to create an object reference. The object reference is then passed to javascript as a parameter to the invokeDotNet function.

You can now invoke instance methods from the ExampleClass object through the invokeMethodAsync method.