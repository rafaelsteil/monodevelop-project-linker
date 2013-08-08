## Project Linker for MonoDevelop / Xamarin Studio


This addin was developed in order to automatically create and maintain links from a source project to a set of target projects, in order to make code sharing between different plataforms a more friendly task. It is based on the [Visual Studio Project Linker](http://visualstudiogallery.msdn.microsoft.com/273dbf44-55a1-4ac6-a1f3-0b9741587b9a) extension. 

The main reason behind the development of this addin was that I was working on a project that targets multiple plataforms, including iOS, Android, Windows, Mac and Linux, and PCL support was very lacky on Xamarin Studio, and even on Visual Studio 2012 it had its problems. Also, PCL programming imposes some restrictions that I found harder to workaround than using a third party plugin to do the file linking between the projects. 

The inner workings of this approach are also described in Xamarin's document [Sharing Code Options](http://docs.xamarin.com/guides/cross-platform/application_fundamentals/building_cross_platform_applications/sharing_code_options), item " _2. File Linking to Separate Projects_ ", and also applies to item " _3. Clone Project Files_ ", which I personally prefer. 

### Benefits
> **Disclaimer**: The following information was copied from Xamarin's [Sharing Code Options](http://docs.xamarin.com/guides/cross-platform/application_fundamentals/building_cross_platform_applications/sharing_code_options) document. 

* Allows you to share code across multiple projects.
* Platform-specific projects can each have different compiler directives defined, so you can branch code based on the platform (eg. using `#if __ANDROID__` or `#if __IOS__`).
* Platform-specific projects can include platform-specific references. The code using these references can be managed with compiler directives.


### How to use
After the addin is instaled, open any solution with more than one project, and select the menu `Project -> Configure project link...`, and select the source and target projects. Then, when any add, remove or rename operation is performed on the source project, it will be automatically replicated to all target projects. 

To stop the linking projects just open the configuration dialog again and select the option `Do not link any projects`

### Example
> **Disclaimer**: The following information was copied from Xamarin's [Sharing Code Options](http://docs.xamarin.com/guides/cross-platform/application_fundamentals/building_cross_platform_applications/sharing_code_options) document. 

The project structure usually look like this: 

![Project structure](http://docs.xamarin.com/static/guides/cross-platform/application_fundamentals/building_cross_platform_applications/sharing_code_options/Images/image9.png)

Note that the linked projects are the `Linked` prefix, which is not mandatory but helps to identify them on the solution. The conceptual architecture is shown in this diagram:

![Conceptual architecture](http://docs.xamarin.com/static/guides/cross-platform/application_fundamentals/building_cross_platform_applications/sharing_code_options/Images/LinkedFiles.png)
