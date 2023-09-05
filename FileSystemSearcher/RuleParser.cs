using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileSystemSearcher
{
	/// <summary>
	/// 规则解析器。
	/// </summary>
	/// <remarks>
	/// 规则定义：
	/// <para>1. 以“#”开头：注释；</para>
	/// <para>2. 以“+wf:”开头：指定用以匹配文件的通配符，通过该通配符匹配到的文件集合会被添加到解析结果中；</para>
	/// <para>3. 以“-wf:”开头：指定用以匹配文件的通配符，通过该通配符匹配到的文件集合会从解析结果中移除；</para>
	/// <para>4. 以“+rf:”开头：指定用以匹配文件的正则表达式，通过该正则表达式匹配到的文件集合会被添加到解析结果中；</para>
	/// <para>5. 以“-rf:”开头：指定用以匹配文件的正则表达式，通过该正则表达式匹配到的文件集合会从解析结果中移除；</para>
	/// <para>6. 以“+wd:”开头：指定用以匹配目录的通配符，通过该通配符匹配到的目录集合会被添加到解析结果中；</para>
	/// <para>7. 以“-wd:”开头：指定用以匹配目录的通配符，通过该通配符匹配到的目录集合会从解析结果中移除；</para>
	/// <para>8. 以“+rd:”开头：指定用以匹配目录的正则表达式，通过该正则表达式匹配到的目录集合会被添加到解析结果中；</para>
	/// <para>9. 以“-rd:”开头：指定用以匹配目录的正则表达式，通过该正则表达式匹配到的目录集合会从解析结果中移除；</para>
	/// <para>可以通过继承该类并重写 <see cref="ParseCustomRule(string, string)"/> 方法实现解析自定义的规则。</para>
	/// </remarks>
	public class RuleParser
	{
		private readonly HashSet<string> _fs = new HashSet<string>();
		private readonly HashSet<string> _ds = new HashSet<string>();

		/// <summary>
		/// 获取解析出的文件集合。
		/// </summary>
		public IReadOnlyCollection<string> Files
		{
			get
			{
				return _fs.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
			}
		}

		/// <summary>
		/// 获取解析出的目录集合。
		/// </summary>
		public IReadOnlyCollection<string> Directories
		{
			get
			{
				return _ds.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
			}
		}


		/// <summary>
		/// 解析规则集并在指定的目录下按解析出的规则集搜索文件或目录。
		/// </summary>
		/// <param name="path">用于搜索的目录。</param>
		/// <param name="rules">规则集。</param>
		/// <exception cref="ArgumentNullException"><paramref name="path"/> 或 <paramref name="rules"/> 为 <see langword="null"/>。</exception>
		/// <exception cref="DirectoryNotFoundException"><paramref name="path"/> 指定的目录不存在。</exception>
		public void ParseRules(string path, params string[] rules)
		{
			if (path is null)
			{
				throw new ArgumentNullException(nameof(path));
			}
			if (!Directory.Exists(path))
			{
				throw new DirectoryNotFoundException();
			}
			if (rules is null)
			{
				throw new ArgumentNullException(nameof(rules));
			}

			foreach (var rule in rules)
			{
				string s = (rule ?? string.Empty).Trim();
				if (s.Length <= 0) // 空行
				{
					return;
				}
				else if (s.StartsWith("#")) // # 开头的是注释
				{
					return;
				}
				else if (s.StartsWith("+wf:", StringComparison.OrdinalIgnoreCase)) // 增加使用通配符匹配到的文件
				{
					var files = Searcher.WildcardSearch(path, s.Substring(4), SearchTarget.File);
					AddFiles(files.ToArray());
				}
				else if (s.StartsWith("-wf:", StringComparison.OrdinalIgnoreCase)) // 移除使用通配符匹配到的文件
				{
					var files = Searcher.WildcardSearch(path, s.Substring(4), SearchTarget.File);
					RemoveFiles(files.ToArray());
				}
				else if (s.StartsWith("+rf:", StringComparison.OrdinalIgnoreCase)) // 增加使用正则表达式匹配到的文件
				{
					var files = Searcher.RegexSearch(path, s.Substring(4), SearchTarget.File);
					AddFiles(files.ToArray());
				}
				else if (s.StartsWith("-rf:", StringComparison.OrdinalIgnoreCase)) // 移除使用正则表达式匹配到的文件
				{
					var files = Searcher.RegexSearch(path, s.Substring(4), SearchTarget.File);
					RemoveFiles(files.ToArray());
				}
				else if (s.StartsWith("+wd:", StringComparison.OrdinalIgnoreCase)) // 增加使用通配符匹配到的目录
				{
					var dirs = Searcher.WildcardSearch(path, s.Substring(4), SearchTarget.Directory);
					AddDirectories(dirs.ToArray());
				}
				else if (s.StartsWith("-wd:", StringComparison.OrdinalIgnoreCase)) // 移除使用通配符匹配到的目录
				{
					var dirs = Searcher.WildcardSearch(path, s.Substring(4), SearchTarget.Directory);
					RemoveDirectories(dirs.ToArray());
				}
				else if (s.StartsWith("+rd:", StringComparison.OrdinalIgnoreCase)) // 增加使用正则表达式匹配到的目录
				{
					var dirs = Searcher.RegexSearch(path, s.Substring(4), SearchTarget.Directory);
					AddDirectories(dirs.ToArray());
				}
				else if (s.StartsWith("-rd:", StringComparison.OrdinalIgnoreCase)) // 移除使用正则表达式匹配到的目录
				{
					var dirs = Searcher.RegexSearch(path, s.Substring(4), SearchTarget.Directory);
					RemoveDirectories(dirs.ToArray());
				}
				else
				{
					ParseCustomRule(path, s);
				}
			}
		}

		/// <summary>
		/// 对解析出的文件和目录的集合进行重置。
		/// </summary>
		public void Reset()
		{
			_fs.Clear();
			_ds.Clear();
		}

		/// <summary>
		/// 对解析出的文件或目录的集合进行重置。
		/// </summary>
		/// <param name="searchTarget">指定重置目标。</param>
		public void Reset(SearchTarget searchTarget)
		{
			if (searchTarget == SearchTarget.File)
			{
				_fs.Clear();
			}
			else if (searchTarget == SearchTarget.Directory)
			{
				_ds.Clear();
			}
		}


		/// <summary>
		/// 解析自定义的规则。
		/// </summary>
		/// <param name="path">解析所作用的路径。</param>
		/// <param name="rule">自定义的规则。</param>
		protected virtual void ParseCustomRule(string path, string rule) { }


		/// <summary>
		/// 新增多个文件。
		/// </summary>
		/// <param name="files">文件集合。</param>
		protected void AddFiles(params string[] files)
		{
			AddRange(_fs, files);
		}

		/// <summary>
		/// 移除多个文件。
		/// </summary>
		/// <param name="files">文件集合。</param>
		protected void RemoveFiles(params string[] files)
		{
			RemoveRange(_fs, files);
		}

		/// <summary>
		/// 新增多个目录。
		/// </summary>
		/// <param name="directories">目录集合。</param>
		protected void AddDirectories(params string[] directories)
		{
			AddRange(_ds, directories);
		}

		/// <summary>
		/// 移除多个目录。
		/// </summary>
		/// <param name="directories">目录集合。</param>
		protected void RemoveDirectories(params string[] directories)
		{
			RemoveRange(_ds, directories);
		}


		private static void AddRange(HashSet<string> hs, IEnumerable<string> items)
		{
			foreach (var item in items)
			{
				hs.Add(item);
			}
		}

		private static void RemoveRange(HashSet<string> hs, IEnumerable<string> items)
		{
			foreach (var item in items)
			{
				hs.Remove(item);
			}
		}
	}
}
