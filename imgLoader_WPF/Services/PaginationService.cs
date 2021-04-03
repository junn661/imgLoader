﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using imgLoader_WPF.LoaderListCtrl;
using ThreadState = System.Threading.ThreadState;

namespace imgLoader_WPF.Services
{
    internal class PaginationService
    {
        private const int Interval = 3000;

        private Thread _service;
        //private Action _PaginateEnded;
        //public event EventHandler PaginateDone;
        //private DispatcherProcessingDisabled _disableProcessing;

        private readonly Windows.ImgLoader _sender;
        public double ScrollHeight;
        private readonly ObservableCollection<IndexItem> _showItems;
        private readonly List<IndexItem> _list;

        public PaginationService(Windows.ImgLoader sender, double scrollHeight, ObservableCollection<IndexItem> showItems, List<IndexItem> list)
        {
            _sender = sender;
            _showItems = showItems;
            _list = list;
            ScrollHeight = scrollHeight;
        }

        internal void Paginate(Dispatcher dispatcher, DispatcherProcessingDisabled disableProcessing)
        {
            if (_service != null && _service.ThreadState != ThreadState.Stopped) return;

            _service = new Thread(() =>
            {
                var num = (int)Math.Ceiling(ScrollHeight / LoaderItem.MHeight);

                var oriCnt = _showItems.Count;
                var listCount = _list.Count;

                var itemToAdd = new IndexItem[num];
                for (var i = 0; i < num; i++)
                {
                    var i1 = i;
                    if (oriCnt + i1 + 1 > listCount) return;

                    itemToAdd[i] = _list[oriCnt + i1];
                } 

                var temp = dispatcher.Thread.Priority;
                dispatcher.Thread.Priority = ThreadPriority.Highest;
                dispatcher.BeginInvoke(() =>
                {
                    foreach (var item in itemToAdd)
                    {
                        _showItems.Add(item);
                    }
                });
                 dispatcher.Thread.Priority = temp;
             });
            _service.Name = "PgSvc";
            _service.IsBackground = true;
            _service.Start();
            _service.Join();

            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(disableProcessing.Dispose);
            }
            else
            {
                disableProcessing.Dispose();
            }
        }
        internal void Paginate(DispatcherProcessingDisabled disableProcessing)
        {
            if (_service != null && _service.ThreadState != ThreadState.Stopped) return;

            _service = new Thread(() =>
            {
                var num = (int)Math.Ceiling(ScrollHeight / LoaderItem.MHeight);

                var oriCnt = _showItems.Count;
                var listCount = _list.Count;

                var itemToAdd = new IndexItem[num];
                for (var i = 0; i < num; i++)
                {
                    var i1 = i;
                    if (oriCnt + i1 + 1 > listCount) return;

                    itemToAdd[i] = _list[oriCnt + i1];
                }

                _sender.Dispatcher.BeginInvoke(() =>
                {
                    foreach (var item in itemToAdd)
                    {
                        _showItems.Add(item);
                    }
                });
            });
            _service.Name = "PgSvc";
            _service.IsBackground = true;
            _service.Start();
            _service.Join();

            if (!_sender.Dispatcher.CheckAccess())
            {
                _sender.Dispatcher.Invoke(() =>
                {
                    disableProcessing.Dispose();
                });
            }
            else
            {
                disableProcessing.Dispose();
            }
        }

        internal void Paginate()
        {
            if (_service != null && _service.ThreadState != ThreadState.Stopped) return;

            _service = new Thread(() =>
            {
                var oriCnt = _showItems.Count;

                _sender.Dispatcher.Invoke(() =>
                {
                    for (var i = 0; i < Math.Ceiling(ScrollHeight / LoaderItem.MHeight); i++)
                    {
                        var i1 = i;
                        if (oriCnt + i1 + 1 > _list.Count) return;

                        var temp = _list[oriCnt + i1];
                        _showItems.Add(temp);
                    }
                });
            });
            _service.Name = "PgSvc_NoDisableDispatcher";
            _service.IsBackground = true;
            _service.Start();
        }

        internal void PaginateToEnd()
        {
            if (_service == null) goto page;
            if (_service.ThreadState != ThreadState.Stopped) return;

            page: _service = new Thread(() =>
            {
                _sender.Dispatcher.Invoke(() =>
                {
                    _showItems.Clear();
                    foreach (var item in _list)
                    {
                        _showItems.Add(item);
                    }
                });

            });
            _service.Name = "PgSvc";
            _service.Start();
        }
    }
}
