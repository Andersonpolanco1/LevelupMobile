using Microsoft.Maui.Handlers;
using LevelUp.Mobile.Extensions;

using LevelUp.Mobile;

#if ANDROID
using Android.Content.Res;
#endif

namespace LevelUp.Mobile.Extensions
{
    public static class PlatformHandlersExtensions
    {
        public static MauiAppBuilder ConfigurePlatformHandlers(this MauiAppBuilder builder)
        {
            builder.ConfigureMauiHandlers(handlers =>
            {
                #if ANDROID
                    EntryHandler.Mapper.AppendToMapping("BrandUnderline", (handler, _) =>
                        ApplyBrandUnderline(handler.PlatformView));

                    EditorHandler.Mapper.AppendToMapping("BrandUnderline", (handler, _) =>
                        ApplyBrandUnderline(handler.PlatformView));
                #endif
            });

            return builder;
        }

        #if ANDROID
        private static void ApplyBrandUnderline(Android.Widget.EditText editText)
        {
            var states = new[]
            {
                new[] { Android.Resource.Attribute.StateFocused },
                new[] { -Android.Resource.Attribute.StateFocused }
            };
            var colors = new[]
            {
                Android.Graphics.Color.ParseColor("#FF7A00").ToArgb(),
                Android.Graphics.Color.ParseColor("#52525B").ToArgb()
            };
            editText.BackgroundTintList = new ColorStateList(states, colors);
        }
        #endif
    }
}