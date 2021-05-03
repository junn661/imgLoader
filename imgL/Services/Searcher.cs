﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using imgL.Windows;

namespace imgL.Services
{
    internal class Searcher
    {
        private const int IndexCount = 5;
        private readonly ImgLoader _sender;

        public Searcher(ImgLoader sender)
        {
            _sender = sender;
        }

        internal void SearchRefresh(string search, SearchOption option)
        {
            _sender.PgSvc.Clear();

            var index = SearchIndex(_sender.List);
            var result = SearchFrom(_sender.List, index, search, option);

            _sender.List.Clear();

            foreach (var item in result)
            {
                if (item == null) continue;

                _sender.List.Add(item);
            }

            _sender.PgSvc.RefreshCounter();
            _sender.PgSvc.Paginate();
            _sender.ShowItemsCnt();
        }

        internal static string[,] SearchIndex(IReadOnlyList<IndexItem> searchFrom)
        {
            var result = new string[searchFrom.Count, IndexCount];
            var sb = new StringBuilder();

            for (var i = 0; i < searchFrom.Count; i++)
            {
                var indexItem = searchFrom[i];
                if (indexItem.Tags == null || indexItem.Title?.Length == 0) continue;

                result[i, (int)SearchOption.Author] = indexItem.Author;
                result[i, (int)SearchOption.Number] = indexItem.Number;

                foreach (var tag in indexItem.Tags) sb.Append(tag).Append(';');
                result[i, (int)SearchOption.Tag] = sb.ToString();
                sb.Clear();

                result[i, (int)SearchOption.SiteName] = indexItem.SiteName;
                result[i, (int)SearchOption.Title]    = indexItem.Title;
            }

            return result;
        }

        internal static IndexItem[] SearchFrom(IEnumerable<IndexItem> searchFrom, string[,] index, string search, SearchOption option)
        {
            var searchResult = searchFrom.ToArray();
            var opt = (int)option;

            switch (option)
            {
                case SearchOption.All:
                    foreach (var srch in search.Split(','))
                    {
                        if (srch.Contains("//seen/"))
                        {
                            for (var i = 0; i < searchResult.Length; i++)
                            {
                                if (!searchResult[i].IsRead)
                                {
                                    searchResult[i] = null;
                                }
                            }

                            continue;
                        }

                        for (var i = 0; i < index.Length / IndexCount; i++)
                        {
                            if (index[i, 0]?.Contains(srch, StringComparison.OrdinalIgnoreCase)    != true
                                && index[i, 1]?.Contains(srch, StringComparison.OrdinalIgnoreCase) != true
                                && index[i, 2]?.Contains(srch, StringComparison.OrdinalIgnoreCase) != true
                                && index[i, 3]?.Contains(srch, StringComparison.OrdinalIgnoreCase) != true
                                && index[i, 4]?.Contains(srch, StringComparison.OrdinalIgnoreCase) != true)
                            {
                                searchResult[i] = null;
                            }
                        }
                    }

                    break;

                case SearchOption.Title:
                case SearchOption.Number:
                case SearchOption.SiteName:
                    foreach (var srch in search.Split(','))
                    {
                        for (var i = 0; i < index.Length / IndexCount; i++)
                        {
                            if (index[i, opt] == null)
                            {
                                searchResult[i] = null;
                                continue;
                            }

                            if (!index[i, opt].Contains(srch, StringComparison.OrdinalIgnoreCase))
                            {
                                searchResult[i] = null;
                            }
                        }
                    }

                    break;

                case SearchOption.Author:
                case SearchOption.Tag:
                    foreach (var srch in search.Split(','))
                    {
                        for (var i = 0; i < index.Length / IndexCount; i++)
                        {
                            if (index[i, opt] == null)
                            {
                                searchResult[i] = null;
                                continue;
                            }

                            if (srch.Contains("//count="))
                            {
                                _ = int.TryParse(srch.Split("//count=")[1].Split('/')[0], out var temp);

                                if (index[i, opt].StrLen(';') != temp)
                                {
                                    searchResult[i] = null;
                                }

                                continue;
                            }

                            if (!index[i, opt].Contains(srch, StringComparison.OrdinalIgnoreCase))
                            {
                                searchResult[i] = null;
                            }
                        }
                    }

                    break;

                case SearchOption.ImgCount:
                case SearchOption.Vote:
                    foreach (var srch in search.Split(','))
                    {
                        for (var i = 0; i < index.Length / IndexCount; i++)
                        {
                            if (index[i, opt] == null)
                            {
                                searchResult[i] = null;
                                continue;
                            }

                            if (!index[i, opt].Contains(srch, StringComparison.OrdinalIgnoreCase))
                            {
                                searchResult[i] = null;
                            }
                        }
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(option), option, null);
            }

            return searchResult;
        }

        internal enum SearchOption
        {
            All = -1,
            Author,
            Number,
            Tag,
            SiteName,
            Title,
            ImgCount,
            Vote
        }
    }
}