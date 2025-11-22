using System;
using System.Linq;
using Godot;
using Janphe;
using Janphe.Fantasy.Map;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FantasyMap
{
    using Random = Janphe.Random;

    partial class MapJobsScreen : Panel
    {
        private MapJobs _mapJobs;
        private bool _needUpdate;

        private Image image;
        private ImageTexture texture;

        private Vector2 areaSize;

        [Export] private NodePath serverApi { get; set; }

        public override void _Ready()
        {
            var api = GetNode<FileServerApi>(serverApi);
            api.AddPath("on_layers_toggled", req =>
            {
                Debug.Log($"on_layers_toggled method:{req.method} query:{req.query} body:{req.body}");
                if (req.method.Equals("GET"))
                {
                    return JsonConvert.SerializeObject(_mapJobs.Get_Layers());
                }
                if (req.method.Equals("POST"))
                {
                    if (string.IsNullOrEmpty(req.body))
                    {
                        var layers = _mapJobs.Get_On_Layers();
                        return JsonConvert.SerializeObject(layers);
                    }
                    else
                    {
                        var layers = JsonConvert.DeserializeObject<int[]>(req.body);
                        _mapJobs.On_Layers_Toggled(layers);
                        generate();
                        return JsonConvert.SerializeObject(layers);
                    }
                }
                return "hello world!";
            });

            api.AddPath("on_options_toggled", req =>
            {
                if (req.method.Equals("GET"))
                {
                    return _mapJobs.Get_Options();
                }
                if (req.method.Equals("POST"))
                {
                    if (string.IsNullOrEmpty(req.body))
                    {
                        var opts = _mapJobs.Get_On_Options();
                        return JsonConvert.SerializeObject(opts);
                    }
                    else
                    {
                        var d = JObject.Parse(req.body);
                        _mapJobs.On_Options_Toggled(d);

                        generate();
                        return req.body;
                    }
                }
                return "hello world!";
            });

            api.AddPath("on_quit", req =>
            {
                GetTree().Quit();
                Free();
                return "";
            });

            CallDeferred("start");
        }

        public override void _Process(double delta)
        {
            if (_needUpdate)
            {
                _needUpdate = false;

                if (texture == null)
                {
                    texture = ImageTexture.CreateFromImage(image);
                }
                else
                {
                    if (image.GetFormat() == texture.GetFormat())
                        texture.Update(image);
                }
                QueueRedraw();// for call _Draw
            }
        }

        public override void _Draw()
        {
            if (texture == null)
                return;

            var rr = GetParentAreaSize();
            var ts = texture.GetSize();
            DrawTexture(texture, (rr - ts) / 2);
        }

        private void start()
        {
            areaSize = GetParentAreaSize();// cannot be read during _Ready

            _mapJobs = new MapJobs();
            _mapJobs.Options.Width = (int)areaSize.X;
            _mapJobs.Options.Height = (int)areaSize.Y;
            Debug.Log($"GetParentAreaSize w:{areaSize.X} h:{areaSize.Y}");

            generate();
        }

        public void on_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton)
            {
                //var evt = (InputEventMouseButton)@event;
                //switch ((ButtonList)evt.ButtonIndex)
                //{
                //    case ButtonList.Right:
                //        if (_gui != null && evt.Pressed)
                //            _gui.Visible = !_gui.Visible;
                //        break;
                //}
            }
        }

        private void generate()
        {
            _mapJobs.processAsync(t =>
            {
                Debug.Log($"MapJobs.processAsync t:{t}ms");

                var bitmap = _mapJobs.Bitmap;
                image = Image.CreateFromData(bitmap.Width, bitmap.Height, false, Image.Format.Rgba8, bitmap.Bytes);
                _needUpdate = true;
            });
        }


        public void _on_ViewportContainer_MoveTo(Vector2 position)
        {
            _mapJobs.Translate(position.X, position.Y);
            generate();
        }


        public void _on_ViewportContainer_ZoomTo(Vector2 position, float scale)
        {
            _mapJobs.Scale(scale, scale);
            _mapJobs.Translate(position.X, position.Y);
            generate();
        }

    }
}
