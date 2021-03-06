﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;

using imgL.LoaderListCtrl;

using ThreadState = System.Threading.ThreadState;

namespace imgL.Services
{
    internal class PaginationService
    {
        private const int Interval = 3000;

        private Thread _service;

        private readonly Windows.ImgL _sender;

        private readonly IndexItem _separator;

        private int _counter;
        private int _separatorCount;

        public PaginationService(Windows.ImgL sender)
        {
            _sender = sender;

            _separator = new IndexItem { View = -1, ImgCount = -1, IsSeparator = true };
        }

        internal void Paginate(DispatcherProcessingDisabled disableProcessing)
        {
            var num = (int)Math.Ceiling(_sender.Scroll.ActualHeight / LoaderItem.MHeight);

            _sender.Dispatcher.BeginInvoke(() =>
            {
                var oriCnt = GetCntItemOnly();

                var listCpy = new List<IndexItem>(_sender.List);
                for (var i = 0; i < num; i++)
                {
                    if (oriCnt + i >= listCpy.Count)
                        break;

                    if (listCpy[oriCnt + i] == null)
                        continue;

                    _sender.ShowItems.Add(listCpy[oriCnt + i]);

                    InsertCounter();
                }
                _sender.Scroll.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
            });

            disableProcessing.Dispose();
        }

        internal void Paginate()
        {
            if (_service != null && _service.ThreadState != ThreadState.Stopped) return;

            _service = new Thread(() =>
            {
                var oriCnt = GetCntItemOnly();

                _sender.Dispatcher.Invoke(() =>
                {
                    for (var i = 0; i < Math.Ceiling(_sender.Scroll.ActualHeight / LoaderItem.MHeight); i++)
                    {
                        if (oriCnt + i >= _sender.List.Count)
                            return;

                        _sender.ShowItems.Add(_sender.List[oriCnt + i]);
                        InsertCounter();
                    }
                });
            });
            _service.Name         = "PgSvc_NoDisableDispatcher";
            _service.IsBackground = true;
            _service.Start();
        }

        private void InsertCounter()
        {
            return;

            _counter++;
            if (_counter != 5) return;

            _counter = 0;
            _sender.ShowItems.Add(_separator);
            _separatorCount++;
        }

        internal void RefreshCounter()
        {
            _counter        = 0;
            _separatorCount = 0;
        }

        internal int GetCntItemOnly()
        {
            return _sender.ShowItems.Count - _separatorCount;
        }

        internal void Clear()
        {
            RefreshCounter();
            if (_sender.Scroll.Dispatcher.CheckAccess())
            {
                _sender.ShowItems.Clear();
            }
            else
            {
                _sender.Scroll.Dispatcher.Invoke(() => _sender.ShowItems.Clear());
            }

            _sender.ShowItemsCnt();
        }

        internal void Remove(IndexItem item)
        {
            if (_sender.ItemCtrl.Dispatcher.CheckAccess())
            {
                _sender.ShowItems.Remove(item);
            }
            else
            {
                _sender.ItemCtrl.Dispatcher.Invoke(() => _sender.ShowItems.Remove(item));
            }

            _sender.ShowItemsCnt();
        }

        internal void Refresh()
        {
            _sender.Dispatcher.Invoke(() =>
            {
                var disableProcessing = Dispatcher.CurrentDispatcher.DisableProcessing();
                Clear();
                Paginate(disableProcessing);
                _sender.ShowItemsCnt();
            });
        }

        internal void Insert(int index, IndexItem item)
        {
            if (_sender.ItemCtrl.Dispatcher.CheckAccess())
            {
                _sender.ShowItems.Insert(index, item);
            }
            else
            {
                _sender.ItemCtrl.Dispatcher.Invoke(() => _sender.ShowItems.Insert(index, item));
            }
        }

        internal void Add(IndexItem item)
        {
            _sender.ShowItems.Add(item);
        }

        internal void Modify(int index)
        {
        }
    }
}