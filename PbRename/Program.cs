using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PbRename
{
    class Program
    {
        static void Main(string[] args)
        {
            //args = new[] { "*.png", "9", "4" };
            //args = new[] { "*.png", "9", "4", "9" };

            List<int> indexes = new List<int>(args.Length);
            if (args.Length < 1) { Console.WriteLine("Ошибка при передаче параметров."); Console.WriteLine("Для выхода нажмите любую клавишу..."); return; }

            for (var i = 1; i < args.Length; i++)
            {
                int val = 0;
                if (int.TryParse(args[i].Trim(), out val)) { indexes.Add(val); }
                else { Console.WriteLine("Ошибка при передаче параметров. Параметр {0} - не числовой", args[i]); Console.WriteLine("Для выхода нажмите любую клавишу..."); return; }
            }

            if (indexes.Count == 0) { indexes.Add(int.MaxValue); }
            var patern = args[0];

            Console.WriteLine("Начало обработки файлов по патерну: {0}, где последний возможный файл {1}...", patern, string.Join("_", indexes));

            var max = 1;
            for (var i = 0; i < indexes.Count; i++) { max = max * indexes[i]; }

            var lastIndex = indexes[indexes.Count - 1];
            var lastIndexCol = (int)Math.Sqrt(lastIndex);
            var ta = Enumerable.Range(1, max).ToArray().Split(lastIndexCol).ToArray();

            var lr = new List<List<int>>();
            var skipCount = (int)Math.Sqrt(max / indexes[indexes.Count -1]);

            for (var i = 0; i < ta.Length; i++)
            {
                if (lr.Count > 0 && lr.Where(x => x.Count != lastIndex).Count() == 0) { lr.Add(ta[i].ToList()); continue; }
                if (lr.Count == 0 || lr.Count % skipCount != 0) { lr.Add(ta[i].ToList()); continue; }

                lr[lr.Count - skipCount + i % skipCount].AddRange(ta[i]);
            }

            var hash = new Dictionary<int, int>();
            foreach (var els in lr.Select((x, i) => new { x = x, i = i }))
            {
                foreach (var ind in els.x) { hash[ind] = els.i; }
            }

            string dPath = Directory.GetCurrentDirectory();
            string[] files = Directory.GetFiles(dPath, patern);


            var resultFolder = string.Format("result_{0}_{1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString().Replace(":", "-"));
            if (Directory.Exists(resultFolder)) { Directory.Delete(resultFolder); }
            Directory.CreateDirectory(resultFolder);

            int[] curIndesex = indexes.Select(x => 1).ToArray();
            curIndesex[curIndesex.Length - 1] = 0;

            //files = Enumerable.Range(100, 500).Select(x => string.Format("assss\asdsadsad__{0}.png", x.ToString())).ToArray();
            //var paths = new List<string>(files.Length);

            files = files.OrderBy(x => x)
                .Select((x, i) => new { x = x, i = i })
                .OrderBy(x => hash.ContainsKey(x.i) ? hash[x.i] : hash.Count + 1)
                .Select(x => x.x)
                .ToArray();

            foreach (var f in files)
            {
                for (var i = curIndesex.Length - 1; i >= 0; i--)
                {
                    if (curIndesex[i] + 1 > indexes[i]) { curIndesex[i] = 1; if (i == 0) { Console.WriteLine("Закончился диапазон значений"); break; } }
                    else
                    {
                        curIndesex[i] = curIndesex[i] + 1;
                        break;
                    }
                }

                var splPath = f.Split(Path.PathSeparator).ToList();
                splPath.RemoveAt(splPath.Count - 1);
                string ext = f.Split('.').LastOrDefault();

                var newPth = splPath.Union(new[] { resultFolder, string.Join("_", curIndesex) + (string.IsNullOrWhiteSpace(ext) ? "" : "." + ext.Trim()) }).ToArray();
               // paths.Add(string.Join("\\", newPth));
                File.Copy(f, Path.Combine(newPth), true);
            }

            Console.WriteLine("Количество файлов: {0}", files.Length);
            Console.WriteLine("Для выхода нажмите любую клавишу...");
            Console.ReadKey();
        }
    }

    public static class Ext
    {
        /// <summary>
        /// Splits an array into several smaller arrays.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="array">The array to split.</param>
        /// <param name="size">The size of the smaller arrays.</param>
        /// <returns>An array containing smaller arrays.</returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
        {
            for (var i = 0; i < (float)array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size);
            }
        }
    }
}
