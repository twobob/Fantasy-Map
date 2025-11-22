using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;
using FileAccess = Godot.FileAccess;

namespace Janphe
{
    public partial class App
    {
        public static byte[] LoadData(string path)
        {
            using var f = FileAccess.Open($"res://public/{path}", FileAccess.ModeFlags.Read);

            var buffer = f.GetBuffer((int)f.GetLength());
            return buffer;
        }

        public static void LoadRes(string path, Action<Stream> callback)
        {
            var bytes = LoadData(path);
            var stream = new MemoryStream(bytes);
            callback?.Invoke(stream);

            stream.Dispose();
        }

        public static void LoadRes(string path, Action<IntPtr> callback)
        {
            var bytes = LoadData(path);

            var unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
            // Call unmanaged code
            callback?.Invoke(unmanagedPointer);

            Marshal.FreeHGlobal(unmanagedPointer);
        }

        public static string[] GetLocales()
        {
            var loaded = TranslationServer.GetLoadedLocales();
            var localeCount = loaded.Length;
            var locales = new string[localeCount];
            for (var i = 0; i < locales.Length; ++i)
                locales[i] = loaded[i] as string;
            return locales;
        }
        public static string GetLocale() => TranslationServer.GetLocale();
        public static void SetLocale(string locale) => TranslationServer.SetLocale(locale);
        public static string Tr(string msg) => TranslationServer.Translate(msg);
    }
}
