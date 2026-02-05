---
title: "How to use Microsoft Library Manager (LibMan)"
date: 2023-09-14T00:00:00-04:00
# weight: 1
# aliases: [""]
tags: ["libman"]
author: "Favian Flores"
showToc: false
TocOpen: false
draft: false
hideMeta: false
comments: false
canonicalURL: "https://favianflores.com/posts/2023/how-to-use-libman/"
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

LibMan is a lightweight tool for managing client side libraries. It is integrated in Visual Studio 2017+ or it can be installed as a .NET tool using the following command:

```shell
dotnet tool install --global Microsoft.Web.LibraryManager.Cli
```

To set up LibMan, go to your project's root directory and run this:

```shell
libman init
```

This creates a libman.json file which looks like this:

```json
{
  "version": "1.0",
  "defaultProvider": "cdnjs",
  "libraries": []
}
```

You can then customize the libman file with the libraries you want to use:

```json
{
  "version": "1.0",
  "defaultProvider": "cdnjs",
  "defaultDestination": "wwwroot",
  "libraries": 
  [
    {
      "library": "bootstrap@5.3.1",
      "files": 
      [
        "css/bootstrap.min.css",
        "js/bootstrap.bundle.min.js"
      ],
      "destination": "wwwroot/lib"
    }
  ]
}
```

In this example, I've configured libman to source the files from [cdnjs](https://cdnjs.com) and place them into the wwwroot folder by default. 

However, in the libraries array, I'm overriding the default destination and I'm specifying that I only want the minified bootstrap css and js bundle.

Now, you need to run this:

```shell
libman restore
```

And libman will download the specified files to their destination.

libman also provides several utility commands such as:

- `libman clean` which deletes all files defined in libman.json
- `libman install` which lets you add a new library through the command line
- `libman uninstall` which lets you delete a library through the command line
- `libman update` which lets you update a library through the command line

LibMan is great if you need to manage a small number of libraries and are not currently using any other package manager. However, if you need more advanced configurations, you will most likely need to use a full featured package manager such as npm or yarn.