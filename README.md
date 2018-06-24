# Video Effects
A implementation of the chroma key, saturation and zoom effect for Windows.Media.Effects and UWP.

## Getting started
Right now, submodule them into your repo and add a project reference.
If Windows Runtime components gets easier to package, I'll publish them on nuget.org.

```csharp
var media = new MediaCapture();
var settings = new MediaCaptureInitializationSettings();
await media.InitializeAsync();

var definition = new VideoEffectDefinition(typeof(ChromaKeyVideoEffect).FullName);
var cfg = new PropertySet();
cfg[nameof(ChromaKeyMediaExtension.Color)] = Colors.Green;

var effect = await media.AddVideoEffectAsync(definition, MediaStreamType.VideoPreview);
effect.SetProperties(cfg);

captureElement.Source = media;
await media.StartPreviewAsync();
```
