using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileSystemSearcher
{
	/// <summary>
	/// 用于在文件系统中按通配符或正则表达式搜索文件或目录。
	/// </summary>
	public static partial class Searcher
	{
		private const char SEP = '/';

		/// <summary>
		/// 使用通配符匹配方式在指定的目录下搜索文件或目录。
		/// </summary>
		/// <param name="path">用于搜索的目录。</param>
		/// <param name="pattern">搜索模式。
		/// <para>支持的通配符：</para>
		/// <para>1. <b>?</b> - 匹配任意单个字符；</para>
		/// <para>2. <b>*</b> - 匹配任意 0 个或多个字符；</para>
		/// <para>3. <b>**</b> - 匹配任意层数的目录；若搜索目标为 <see cref="SearchTarget.File"/>
		/// 且 <paramref name="pattern"/> 的值为 ** 时，此时 ** 等同于 2.</para>
		/// </param>
		/// <param name="searchTarget">指定搜索目标。</param>
		/// <returns>返回搜索到的文件集合。</returns>
		/// <exception cref="ArgumentNullException"><paramref name="path"/> 或 <paramref name="pattern"/> 为 <see langword="null"/>。</exception>
		/// <exception cref="DirectoryNotFoundException"><paramref name="path"/> 指定的目录不存在。</exception>
		public static IEnumerable<string> WildcardSearch(string path, string pattern, SearchTarget searchTarget)
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

			return WildcardSearchInternal(path, SplitPattern(pattern), searchTarget == SearchTarget.File);
		}


		/****************************************************************************************************************
		 * 通配符搜索的具体实现
		 * 
		 * 1. 对于 patternParts.Length == 1，搜索模式可能为为 文件（a*b.txt 或 **） /  目录（a*b 或 **）
		 *		a. 文件（a*b.txt）：直接在当前目录下使用通配符搜索即可
		 *		b. 文件（**）：** 对于文件而已仅仅只代表普通通配符而已，故同 a.
		 *		c. 目录（a*b）: 直接在当前目录下使用通配符搜索即可
		 *		d. 目录（**）：对于目录而言，2 个或 2个以上的 * 代表任意层数的目录，因此需要搜索所有层的子目录
		 *	
		 *	2. 对于 patternParts.Length == 2，搜索模式可能有以下情形：（不可能出现 ** /**，因为 CleanPatternParts 方法会清理掉）
		 *		a. 若 patternParts[0] == "**"，此时无论是搜索文件还是目录，都等价于在所有层的目录中搜索
		 *		b. 若 patternParts[0] == "a*b"，此时首先在当前目录搜索出符合 a*b 的目录，然后等价于在这些目录中执行 1.
		 *		
		 *	3. 对于 patternParts.Length > 2，搜索模式可能有以下情形：
		 *		a. 若 patternParts[0] == "**"，此时等价于在所有层的目录中搜索 patternParts[1]，
		 *		   然后对于搜索出的所有子目录进行递归 3.，直到执行 1. 或 2.
		 *		   （由于 CleanPatternParts 方法的存在，patternParts[1]）不可能为 **，
		 *		   而且 patternParts.Length == 3 则执行 1.，其余执行 2.）
		 *		b. 若 patternParts[0] == "a*b"，此时首先在当前目录搜索出符合 a*b 的目录，然后等价于在这些目录中递归 3.，
		 *		   直到执行 1. 或 2.
		 *****************************************************************************************************************/
		private static IEnumerable<string> WildcardSearchInternal(string path, string[] patternParts, bool isSearchFiles)
		{
			var result = new List<string>();

			if (patternParts.Length <= 0)
			{
				return result;
			}

			if (patternParts.Length == 1)
			{
				string fn = patternParts[0];
				if (isSearchFiles)
				{
					result.AddRange(Directory.EnumerateFiles(path, fn, SearchOption.TopDirectoryOnly)); // 1.a, 1.b
				}
				else
				{
					if (IsMatchAllLevel(fn)) // **
					{
						result.AddRange(Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories)); // 1.d
					}
					else // a*b
					{
						result.AddRange(Directory.EnumerateDirectories(path, fn, SearchOption.TopDirectoryOnly)); // 1.c
					}
				}
			}
			else if (patternParts.Length == 2)
			{
				string folder = patternParts[0];
				string fn = patternParts[1];

				if (IsMatchAllLevel(folder)) // 2.a
				{
					if (isSearchFiles)
					{
						result.AddRange(Directory.EnumerateFiles(path, fn, SearchOption.AllDirectories));
					}
					else
					{
						result.AddRange(Directory.EnumerateDirectories(path, fn, SearchOption.AllDirectories));
					}
				}
				else // 2.b
				{
					var dirs = Directory.EnumerateDirectories(path, folder, SearchOption.TopDirectoryOnly);
					foreach (var dir in dirs)
					{
						result.AddRange(WildcardSearchInternal(dir, new[] { fn }, isSearchFiles));
					}
				}
			}
			else
			{
				string firstFolder = patternParts[0];
				if (IsMatchAllLevel(firstFolder)) // 3. a
				{
					var dirs = Directory.EnumerateDirectories(path, patternParts[1], SearchOption.AllDirectories);
					foreach (var dir in dirs)
					{
						result.AddRange(WildcardSearchInternal(dir, patternParts.Skip(2).ToArray(), isSearchFiles));
					}
				}
				else // 3.b
				{
					var dirs = Directory.EnumerateDirectories(path, firstFolder, SearchOption.TopDirectoryOnly);
					foreach (var dir in dirs)
					{				
						result.AddRange(WildcardSearchInternal(dir, patternParts.Skip(1).ToArray(), isSearchFiles));
					}
				}
			}

			return result.Distinct(StringComparer.OrdinalIgnoreCase);
		}



		// 2个或2个以上的 *，匹配任意层数的子目录
		// 例如： ** ， ***， ****， ...
		private static bool IsMatchAllLevel(string part)
		{
			return part.Length >= 2 && part.All(c => c == '*');
		}

		// 分割并清理 Pattern
		// 例如： abc/**/**/de*f/*.png => ["abc", "**", "de*f", "*.png"]
		private static string[] SplitPattern(string pattern)
		{
			var patternParts = pattern.Split(SEP).Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
			return CleanPatternParts(patternParts);
		}

		// 清理 PatternParts
		// 例如：[ "**", "**", "abc", "**", "**", "*.js" ] => [ "**", "abc", "**", "*.js" ]
		private static string[] CleanPatternParts(string[] patternParts)
		{
			IList<string> result = new List<string>();
			bool isLastMatchAllLevel = false;
			foreach (var part in patternParts)
			{
				if (IsMatchAllLevel(part))
				{
					if (isLastMatchAllLevel)
					{
						continue;
					}
					else
					{
						result.Add(part);
						isLastMatchAllLevel = true;
					}
				}
				else
				{
					result.Add(part);
					isLastMatchAllLevel = false;
				}
			}

			return result.ToArray();
		}
	}
}