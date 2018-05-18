using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics.Imaging;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.UI;

namespace NickDarvey.ChromaKey
{
    public sealed class ChromaKeyVideoEffect : IBasicVideoEffect
    {
        private readonly ChromaKeyMediaExtension MediaExtension = new ChromaKeyMediaExtension();

        /// <summary>
        /// Gets the optional background canvas to render
        /// </summary>
        public BitmapDecoder Background { get; private set; } = default(BitmapDecoder);

        /// <summary>
        /// Gets a value indicating whether the video effect will modify the contents of the input frame.
        /// </summary>
        public bool IsReadOnly { get; } = false;

        /// <summary>
        /// Gets the encoding properties supported by the custom video effect.
        /// </summary>
        public IReadOnlyList<VideoEncodingProperties> SupportedEncodingProperties { get; } = new List<VideoEncodingProperties>
            {  new VideoEncodingProperties { Subtype = "ARGB32" } };

        /// <summary>
        /// Gets a value that indicates whether the custom video effect supports the use of GPU memory or CPU memory.
        /// </summary>
        public MediaMemoryTypes SupportedMemoryTypes { get; } = MediaMemoryTypes.Gpu;

        private ICanvasImage BackgroundImage { get; set; } = default(ICanvasImage);

        public void ProcessFrame(ProcessVideoFrameContext context)
        {
            using (var inputBitmap = CanvasBitmap.CreateFromDirect3D11Surface(MediaExtension.Device, context.InputFrame.Direct3DSurface))
            using (var renderTarget = CanvasRenderTarget.CreateFromDirect3D11Surface(MediaExtension.Device, context.OutputFrame.Direct3DSurface))
            using (var ds = renderTarget.CreateDrawingSession())
            using (var chromaKeyEffect = MediaExtension.CreateChromaKeyEffect(inputBitmap))
            {
                if (Background == null) ds.FillRectangle(inputBitmap.Bounds, Colors.Black);
                else ds.DrawImage(BackgroundImage);

                ds.DrawImage(chromaKeyEffect);
            }
        }

        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device) =>
           MediaExtension.SetEncodingProperties(encodingProperties, device);

        public void Close(MediaEffectClosedReason reason)
        {
            BackgroundImage?.Dispose();
            MediaExtension.Close(reason);
        }

        public void DiscardQueuedFrames() =>
            MediaExtension.DiscardQueuedFrames();

        public bool TimeIndependent =>
            MediaExtension.TimeIndependent;

        public void SetProperties(IPropertySet configuration)
        {
            MediaExtension.SetProperties(configuration);

            if (configuration.TryGetValue(nameof(Background), out var background))
            {
                Background = (BitmapDecoder)background;

                var props = MediaExtension.EncodingProperties;

                using (var bitmap = Background.GetSoftwareBitmapAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied,
                    new BitmapTransform { ScaledHeight = props.Height, ScaledWidth = props.Width, InterpolationMode = BitmapInterpolationMode.Fant },
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage).AsTask().GetAwaiter().GetResult())
                {
                    BackgroundImage?.Dispose();
                    BackgroundImage = CanvasBitmap.CreateFromSoftwareBitmap(MediaExtension.Device, bitmap);
                }
            }
        }
    }
}
