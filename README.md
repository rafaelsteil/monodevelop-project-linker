## Project Linker for MonoDevelop / Xamarin Studio


This addin was developed in order to automatically create and maintain links from a source project to a set of target projects, in order to make code sharing between different plataforms a more friendly task. It is based on the [Visual Studio Project Linker](http://visualstudiogallery.msdn.microsoft.com/273dbf44-55a1-4ac6-a1f3-0b9741587b9a) extension. 

The main reason behind the development of this addin was that I was working on a project that targets multiple plataforms, including iOS, Android, Windows, Mac and Linux, and PCL support was very lacky on Xamarin Studio, and even on Visual Studio 2012 it had its problems. Also, PCL programming imposes some restrictions that I found harder to workaround than using a third party plugin to do the file linking between the projects. 

The inner workings of this approach are also described in Xamarin's document [Sharing Code Options](http://docs.xamarin.com/guides/cross-platform/application_fundamentals/building_cross_platform_applications/sharing_code_options), item " _2. File Linking to Separate Projects_ ", and also applies to item " _3. Clone Project Files_ ", which I personally prefer. 

### More information
For more information about the project, including install instructions, please check the project page at http://rafaelsteil.github.io/monodevelop-project-linker/
