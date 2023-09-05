using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FileSystemSearcher
{
	public static partial class Searcher
	{
		private static readonly Regex SepRegex = new Regex("/+", RegexOptions.Compiled);


		/// <summary>
		/// 使用正则表达式方式在指定的目录下搜索文件或目录。
		/// </summary>
		/// <param name="path">用于搜索的目录。</param>
		/// <param name="pattern">搜索模式（正则表达式）。</param>
		/// <param name="searchTarget">指定搜索目标。</param>
		/// <returns>返回搜索到的文件集合。</returns>
		/// <exception cref="ArgumentException"><paramref name="pattern"/> 指定的正则表达式不合法。</exception>
		/// <exception cref="ArgumentNullException"><paramref name="path"/> 或 <paramref name="pattern"/> 为 <see langword="null"/>。</exception>
		/// <exception cref="DirectoryNotFoundException"><paramref name="path"/> 指定的目录不存在。</exception>
		public static IEnumerable<string> RegexSearch(string path, string pattern, SearchTarget searchTarget)
		{
			if (path is null)
			{
				throw new ArgumentNullException(nameof(path));
			}
			if (!Directory.Exists(path))
			{
				throw new DirectoryNotFoundException();
			}
			if (pattern is null)
			{
				throw new ArgumentNullException(nameof(pattern));
			}

			// 正则表达式错误
			Regex regex;
			try
			{
				// 将正则表达式中的 / 替换为 \
				if (Path.DirectorySeparatorChar == '\\')
				{
					pattern = SepRegex.Replace(pattern, @"\\");
				}
				regex = new Regex(pattern, RegexOptions.IgnoreCase);
			}
			catch 
			{
				throw new ArgumentException("Invalid regular expression", nameof(pattern));
			}

			return RegexSearchInternal(path, regex, searchTarget == SearchTarget.File);
		}

		private static IEnumerable<string> RegexSearchInternal(string path, Regex regex, bool isSearchFiles)
		{
			var result = new List<string>();

			IEnumerable<string> fns = isSearchFiles ? Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
				: Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories);
			foreach (var fn in fns)
			{
				string s = fn.Remove(0, path.Length);
				while (s[0] == '/' || s[0] == '\\')
				{
					s = s.Remove(0, 1);
				}

				if (regex.IsMatch(s))
				{
					result.Add(fn);
				}
			}

			return result.Distinct(StringComparer.OrdinalIgnoreCase);
		}
	}
}
