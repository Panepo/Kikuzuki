# Kikuzuki

C# implementation of Tesseract optical character recognition with pre-recongizing image processing and translation.

<img src="https://github.com/Panepo/Kikuzuki/blob/master/doc/usage1.png" alt="usage1" height="418" width="750"> <img src="https://github.com/Panepo/Kikuzuki/blob/master/doc/usage2.png" alt="usage2" height="418" width="750">

Here's the readme for implementation by Microsoft Foundry on Windows. [Link](https://github.com/Panepo/Kikuzuki/blob/master/winui/README.md)

## Requirements

* .NET Framework 4.6.1 runtime

## Reference

* [Tesseract](https://github.com/tesseract-ocr/tesseract)
* [Tesseract CSharp wrapper](https://github.com/charlesw/tesseract)
* [OpenCVSharp](https://github.com/shimat/opencvsharp)
* [PRLib](https://github.com/leha-bot/PRLib)
* [Azure Cognitive Translator](https://azure.microsoft.com/en-us/products/cognitive-services/translator/)

## Develop

### Development Requirements
* Visual Studio 2022
* Azure account (For Azure translator)

### Azure Translator Cofiguration
* Create an Azure subscription and translator resource. [Link](https://learn.microsoft.com/en-us/azure/cognitive-services/translator/quickstart-translator?tabs=csharp)
* Create `Secret.cs` into `csharp\Common` folder
* Fill `Secret.cs` with these following
```
namespace Kikuzuki
{
    class TranslatorConfig
    {
        public static readonly string TranslatorKey = <Your Translator Key>;
        public static readonly string TranslatorRegion = <Your Translator Region>;
        public static readonly string TranslatorEndpoint = "https://api.cognitive.microsofttranslator.com/";
    }
}
```

## Author

[Panepo](https://github.com/Panepo)
