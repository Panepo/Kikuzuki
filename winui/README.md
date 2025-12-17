# Kikuzuki WinUI

C# implementation of optical character recognition and translation by Microsoft Foundry on Windows and Phi Silica

<img src="https://github.com/Panepo/Kikuzuki/blob/master/doc/usage-winui1.png" alt="usage1" height="418" width="750"> <img src="https://github.com/Panepo/Kikuzuki/blob/master/doc/usage-winui2.png" alt="usage2" height="418" width="750">

## Requirements

* .NET 8.0
* Copilot+ PC
* Windows 25H2

## Reference

* [Microsoft Foundry on Windows](https://developer.microsoft.com/en-us/windows/ai/)
* [Windows AI](https://learn.microsoft.com/en-us/windows/ai/)
* [Phi Silica](https://learn.microsoft.com/en-us/windows/ai/apis/phi-silica)
* [AI Text Recognition](https://learn.microsoft.com/en-us/windows/ai/apis/text-recognition)
* [OpenCVSharp](https://github.com/shimat/opencvsharp)

## Develop

### Development Requirements
* Visual Studio 2022

### Phi Silica Cofiguration
* Apply a LAF access toekn for Phi Silica. [Link](https://go.microsoft.com/fwlink/?linkid=2271232&c1cid=04x409)
* Fill `Phi Silica.cs` with the token you requested from Microsoft
```
public static async Task<PhiSilica?> CreateAsync(CancellationToken cancellationToken = default)
{
    // const string featureId = "com.microsoft.windows.ai.languagemodel";

    // IMPORTANT!!
    // This is a demo LAF Token and PublisherId cannot be used for production code and won't be accepted in the Store
    // Please go to https://aka.ms/laffeatures to learn more and request a token for your app
    var demoToken = "LimitedAccessFeaturesHelper.GetAiLanguageModelToken()";
    var demoPublisherId = "LimitedAccessFeaturesHelper.GetAiLanguageModelPublisherId()";
}
```

## Author

[Panepo](https://github.com/Panepo)
