# Plugin.Maui.Feature Template

The `Plugin.Maui.Feature` repository is a template repository that can be used to bootstrap your own .NET MAUI plugin project. You can use this project structure as a blueprint for your own work.

Learn how to get started with your plugin in this [YouTube video](https://www.youtube.com/watch?v=ZCQrlGT7MhI&list=PLfbOp004UaYVgzmTBNVI0ql2qF0LhSEU1&index=27).

This template contains:

- A [sample .NET MAUI app](samples) where you can demonstrate how your plugin works and test your plugin with while developing
- The [source](src) of the plugin
- A boilerplate [README file](README_Feature.md) you can use (don't forget to rename to `README.md` and remove this one!)
- [GitHub Actions for CI](.github/workflows) of the library and the sample app
- [GitHub Action for releasing](.github/workflows) your package to NuGet
- A [generic icon](nuget.png) for your project, feel free to adapt and be creative!
- A [.editorconfig](.editorconfig) file to standardize the code syntax. Feel free to adapt or remove.
- The [LICENSE](LICENSE) file with the MIT license. If you want this to be different, please change it. At the very least add your name in there!

## Getting Started

1. Create your own GitHub repository from this one by clicking the "Use this template" button and then "Create a new repository". More information in the [documentation](https://docs.github.com/repositories/creating-and-managing-repositories/creating-a-repository-from-a-template). After that, clone the repo to your local machine.

2. Replace all occurrences of `Plugin.Maui.Feature` with whatever your feature or functionality will be. For instance: `Plugin.Maui.ScreenBrightness` or `Plugin.Maui.Audio`. Of course the name can be anything, but to make it more discoverable it could be a great choice to stick to this naming scheme. You can easily do this with your favorite text-editor and do a replace all on all files.

   2.1 Don't forget to also rename the files and folders on your filesystem.

3. In the csproj file of the plugin project (under `src`), make sure that you replace all relevant values to your project. This means the author of this project, the description of the project, the target framework (.NET 7, 8 or something else). If you don't want to or can't support a certain platform, remove that target platform altogether.

4. Delete this `README.md` file and rename `README_Feature.md` to `README.md`. Fill that README file with all the relevant details of your project.

5. Check the LICENSE file if this reflects the license that you want to distribute your project under. At the very least add your name there and the current year we live in.

6. Create a nice icon in the `nuget.png` file that will show up on nuget.org and in the NuGet manager in Visual Studio.

7. Write your plugin code (under `src`) and add samples to the .NET MAUI sample app (under `samples` folder)

8. Make super sure that your package won't show up as `Plugin.Maui.Feature` on NuGet! If one does, you owe me a drink!

9. Publish your package to NuGet, a nice guide to do that can be found [here](https://learn.microsoft.com/nuget/nuget-org/publish-a-package). Also see [Publish to NuGet](#publish-to-nuget) below.

10. Enjoy life as a .NET MAUI plugin author! ✨

As an example of all of this you can have a look at:

- [Plugin.Maui.Audio](https://github.com/jfversluis/Plugin.Maui.Audio)
- [Plugin.Maui.Pedometer](https://github.com/jfversluis/Plugin.Maui.Pedometer)
- [Plugin.Maui.ScreenBrightness](https://github.com/jfversluis/Plugin.Maui.ScreenBrightness)

## Publish to NuGet

If you want to publish your package to NuGet, you totally can! Included in this template are a couple of GitHub Actions. One of them goes of when you create a new tag with this pattern: `v1.0.0` or `v1.0.0-preview1`. Obviously the `1.0.0` part can be determined by you as you see fit, as long as you follow the pattern of 3 integers separated by dots.

You will also want to set a secret for this repository which contains your NuGet API key. Follow the documentation on that [here](https://docs.github.com/actions/security-guides/encrypted-secrets#creating-encrypted-secrets-for-a-repository), and add a secret with the key `NUGET_API_KEY` and value of your NuGet API key. The API key should be authorized to push a NuGet package with the given identifier. 

From there, after [creating a GitHub release](https://docs.github.com/repositories/releasing-projects-on-github/managing-releases-in-a-repository) your plugin will be automatically released on NuGet!