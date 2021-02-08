﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace imgLoader_WPF.Sites
{
    public class Hiyobi : ISite
    {
        public string Number { get; set; }

        private readonly string _src_cdn, _src_api, _title, _artist, _group;

        public Hiyobi(string mNumber)
        {
            var sb = new StringBuilder();
            var wc = new WebClient {Encoding = Encoding.UTF8};

            try
            {
                // 썸네일 https://cdn.hiyobi.me/tn/(갤러리id).(확장자)
                _src_api = wc.DownloadString($"https://api.hiyobi.me/gallery/{mNumber}");
                _src_cdn = wc.DownloadString($"https://cdn.hiyobi.me/json/{mNumber}_list.json");

                if (_src_api.Contains("\\")) _src_api = _src_api.Replace("\\", "");
                _title = _src_api.Split("title\":\"")[1].Split("\",")[0];

                if (_src_api.Contains("artists") && StrTools.GetValue(_src_api, "artists").Contains("value"))
                {
                    var temp = _src_api.Split("artists\":[")[1].Split(']')[0].Split("\"value\":\"");

                    for (var i = 1; i < temp.Length; i++) sb.Append(temp[i].Split('"')[0]).Append(';');
                    _artist = sb.ToString();
                }

                sb.Clear();

                if (_src_api.Contains("groups") && StrTools.GetValue(_src_api, "groups").Contains("value"))
                {
                    var temp = _src_api.Split("groups\":[")[1].Split(']')[0].Split("\"value\":\"");

                    for (var i = 1; i < temp.Length; i++) sb.Append(temp[i].Split('"')[0]).Append(';');
                    _group = sb.ToString();
                }

                Number = mNumber;
            }
            catch
            {
                throw new Exception("failed to initiate");
            }
        }

        public string GetArtist()
        {
            return $"{_artist}|{_group}";
        }

        public Dictionary<string, string> GetImgUrls()
        {
            var js = _src_cdn.Split('{');
            var imgList = new Dictionary<string, string>();

            for (var i = 1; i < js.Length; i++)
            {
                if (!js[i].Contains("name")) continue;

                var name = StrTools.GetStringValue(js[i], "name");
                imgList.Add(name, $"http://cdn.hiyobi.me/data/{Number}/{name}");
            }

            return imgList;
        }

        public string GetTitle()
        {
            return _title;
        }

        public string[] ReturnInfo()
        {
            var info = new string[6];

            info[0] = "Hiyobi";
            info[1] = _title;
            info[2] = $"{_artist}|{_group}";
            info[3] = _src_cdn.StrLen("name").ToString();

            var sb = new StringBuilder();
            sb.Append("tags:");
            foreach (var item in StrTools.GetValue(_src_api, "tags", '[', ']').Split('{'))
            {
                if (item.Length == 0) continue;
                sb.Append(StrTools.GetStringValue(item.Split('}')[0],"value")).Append(';');
            }

            info[4] = sb.ToString().Trim();
            info[5] = DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture);

            return info;
        }

        public bool IsValidated()
        {
            return Number != null;
        }
    }
}