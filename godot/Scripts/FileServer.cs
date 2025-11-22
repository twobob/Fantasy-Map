using System;
using System.Net;
using System.IO;
using Godot;

namespace Janphe
{
    public partial class FileServer : Node, IWebResource
    {
        static string GetLocalIp()
        {
            var hostname = Dns.GetHostName();
            var localhost = Dns.GetHostEntry(hostname);

            //localhost.AddressList.forEach((d, i) =>
            //{
            //    /**
            //    dphe-i7 0 fe80::d43e:583b:86b7:6391 False True False False False
            //    dphe-i7 1 192.168.0.96 False False False False False
            //     */
            //    Debug.Log($"{hostname} {i} {d.ToString()} {d.IsIPv6SiteLocal} {d.IsIPv6LinkLocal} {d.IsIPv6Multicast} {d.IsIPv4MappedToIPv6} {d.IsIPv6Teredo}");
            //});

            var i = localhost.AddressList.findIndex(d => !d.IsIPv6LinkLocal);
            return i < 0 ?
                IPAddress.Loopback.ToString() :
                localhost.AddressList[i].ToString();
        }

        EmbeddedWebServerComponent server;

        public override void _Ready()
        {
            var h5 = "ui";

            server = GetParent<EmbeddedWebServerComponent>();
            server.AddResource($"/{h5}", this);
            //OS.ShellOpen($"http://{GetLocalIp()}:8079/{h5}/index.html");
        }

        public void HandleRequest(Request request, Response response)
        {
            // check if file exist at folder (need to assume a base local root)
            var fullPath = "res://public" + Uri.UnescapeDataString(request.uri.LocalPath);
            // get file extension to add to header
            var fileExt = System.IO.Path.GetExtension(fullPath);
            //Debug.Log($"fullPath:{fullPath} fileExt:{fileExt}");

            // not found
            if (!FileAccess.FileExists(fullPath))
            {
                response.statusCode = 404;
                response.message = "Not Found";
                return;
            }

            // serve the file
            response.statusCode = 200;
            response.message = "OK";
            response.headers.Add("Content-Type", MimeTypeMap.GetMimeType(fileExt));

            using var f = FileAccess.Open(fullPath, FileAccess.ModeFlags.Read);
            if (f != null)
            {
                var length = (int)f.GetLength();
                response.headers.Add("Content-Length", length.ToString());
                response.SetBytes(f.GetBuffer(length));
            }
        }

    }
}
