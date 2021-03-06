﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using imgL.Windows;

namespace imgL.Services
{
    internal class Categorizer
    {
        private readonly string _catFile = Core.FilesRoute + "Cat";

        private readonly ImgL _sender;

        internal readonly List<(string, StringBuilder)> CategoryList = new();

        public Categorizer(ImgL sender)
        {
            _sender = sender;

            if (!File.Exists(_catFile))
            {
                File.WriteAllText(_catFile, "");
                return;
            }

            var content = File.ReadAllText(_catFile);

            foreach (var cat in content.Split("},", StringSplitOptions.RemoveEmptyEntries))
            {
                CategoryList.Add((cat.Split(':')[0], new StringBuilder().Append(cat.Split('{')[1])));
            }
        }

        internal void AddCategory(string catName)
        {
            CategoryList.Add((catName, new StringBuilder()));
        }

        internal void RemoveCategory(string catName)
        {
            for (int i = 0; i < CategoryList.Count; i++)
            {
                if (CategoryList[i].Item1 != catName) continue;
                CategoryList.Remove(CategoryList[i]);
            }
        }

        internal void AddToCategory(string catName, string number)
        {
            for (int i = 0; i < CategoryList.Count; i++)
            {
                if (CategoryList[i].Item1 != catName) continue;
                CategoryList[i].Item2.Append(',').Append(number);
            }
        }

        internal void RemoveFromCategory(string catName, string number)
        {
            for (int i = 0; i < CategoryList.Count; i++)
            {
                if (CategoryList[i].Item1 != catName) continue;
                CategoryList[i].Item2.Replace($",{number}", "");
            }
        }

        internal void SaveToFile()
        {
            var sb = new StringBuilder();
            foreach (var (catName, numbers) in CategoryList)
            {
                sb.Append(catName).Append(":{").Append(numbers).Append("},");
            }

            File.WriteAllText(_catFile, sb.ToString());
        }
    }
}
