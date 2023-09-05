using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileSystemSearcher.Tests
{
	[TestClass()]
	public class SearcherTests
	{
		private readonly static string Root = GetPath();
		private readonly static string NotExistsDirectory = GetPath("test_directory_not_exists");

		[TestMethod()]
		public void WildcardSearch_F_PathNotExists_Test()
		{
			Assert.ThrowsException<DirectoryNotFoundException>(
				() => Searcher.WildcardSearch(NotExistsDirectory, "*", SearchTarget.File));
		}

		[TestMethod()]
		public void WildcardSearch_D_PathNotExists_Test()
		{
			Assert.ThrowsException<DirectoryNotFoundException>(
				() => Searcher.WildcardSearch(NotExistsDirectory, "*", SearchTarget.Directory));
		}


		[TestMethod()]
		public void WildcardSearch_F_EmptyPattern_Test()
		{
			var result = Searcher.WildcardSearch(Root, " / ", SearchTarget.File);
			Assert.AreEqual(0, result.Count());
		}

		[TestMethod()]
		public void WildcardSearch_D_EmptyPattern_Test()
		{
			var result = Searcher.WildcardSearch(Root, " ", SearchTarget.Directory);
			Assert.AreEqual(0, result.Count());
		}



		[TestMethod()]
		public void WildcardSearch_F_1_Test()
		{
			var result = Searcher.WildcardSearch(Root, "a*b.t*t", SearchTarget.File);
			Assert.IsTrue(SetEquals(result, new[]
			{
				GetPath("a1b.txt"), 
				GetPath("a2b.txt"),
				GetPath("a1b.tvt")
			}));
		}

		[TestMethod()]
		public void WildcardSearch_F_2_Test()
		{
			var result = Searcher.WildcardSearch(Root, "**", SearchTarget.File);
			Assert.IsTrue(SetEquals(result, new[] 
			{
				GetPath("a1b.txt"),
				GetPath("a2b.txt"), 
				GetPath("2ab.txt"), 
				GetPath("a1b.tvt") 
			}));
		}

		[TestMethod()]
		public void WildcardSearch_F_3_Test()
		{
			var result = Searcher.WildcardSearch(Root, "**/a*b.t*t", SearchTarget.File);
			Assert.IsTrue(SetEquals(result, new[]
			{
				GetPath("a1b.txt"),
				GetPath("a2b.txt"),
				GetPath("a1b.tvt"),
				GetPath("f1", "a1b.txt"),
				GetPath("f1", "a2b.txt"),
				GetPath("f1", "a1b.tvt"),
				GetPath("f2", "a1b.txt"),
				GetPath("f2", "a2b.txt"),
				GetPath("f2", "a1b.tvt"),
				GetPath("f1", "f2", "a1b.txt"),
				GetPath("f1", "f2", "a2b.txt"),
				GetPath("f1", "f2", "a1b.tvt"),
			}));
		}

		[TestMethod()]
		public void WildcardSearch_F_4_Test()
		{
			var result = Searcher.WildcardSearch(Root, "f*/**", SearchTarget.File);
			Assert.IsTrue(SetEquals(result, new[]
			{
				GetPath("f1", "a1b.txt"),
				GetPath("f1", "a2b.txt"),
				GetPath("f1", "2ab.txt"),
				GetPath("f1", "a1b.tvt"),
				GetPath("f2", "a1b.txt"),
				GetPath("f2", "a2b.txt"),
				GetPath("f2", "2ab.txt"),
				GetPath("f2", "a1b.tvt"),
			}));
		}

		[TestMethod()]
		public void WildcardSearch_F_5_Test()
		{
			var result = Searcher.WildcardSearch(Root, "f*/**/a*.t*t", SearchTarget.File);
			Assert.IsTrue(SetEquals(result, new[]
			{
				GetPath("f1", "a1b.txt"),
				GetPath("f1", "a2b.txt"),
				GetPath("f1", "a1b.tvt"),
				GetPath("f2", "a1b.txt"),
				GetPath("f2", "a2b.txt"),
				GetPath("f2", "a1b.tvt"),
				GetPath("f1", "f2", "a1b.txt"),
				GetPath("f1", "f2", "a2b.txt"),
				GetPath("f1", "f2", "a1b.tvt"),
			}));
		}

		[TestMethod()]
		public void WildcardSearch_F_6_Test()
		{
			var result = Searcher.WildcardSearch(Root, "**/f*/a*.t*t", SearchTarget.File);
			Assert.IsTrue(SetEquals(result, new[]
			{
				GetPath("f1", "a1b.txt"),
				GetPath("f1", "a2b.txt"),
				GetPath("f1", "a1b.tvt"),
				GetPath("f2", "a1b.txt"),
				GetPath("f2", "a2b.txt"),
				GetPath("f2", "a1b.tvt"),
				GetPath("f1", "f2", "a1b.txt"),
				GetPath("f1", "f2", "a2b.txt"),
				GetPath("f1", "f2", "a1b.tvt"),
			}));
		}

		[TestMethod()]
		public void WildcardSearch_F_7_Test()
		{
			var result = Searcher.WildcardSearch(Root, "**/f2/**/2ab.t*t", SearchTarget.File);
			Assert.IsTrue(SetEquals(result, new[]
			{
				GetPath("f2", "2ab.txt"),
				GetPath("f1", "f2", "2ab.txt"),
				GetPath("f1", "f2", "f1", "2ab.txt"),
			}));
		}



		[TestMethod()]
		public void WildcardSearch_D_1_Test()
		{
			var result = Searcher.WildcardSearch(Root, "f*", SearchTarget.Directory);
			Assert.IsTrue(SetEquals(result, new[]
			{
				GetPath("f1"),
				GetPath("f2"),
			}));
		}

		[TestMethod()]
		public void WildcardSearch_D_2_Test()
		{
			var result = Searcher.WildcardSearch(Root, "*", SearchTarget.Directory);
			Assert.IsTrue(SetEquals(result, new[]
			{
				GetPath("f1"),
				GetPath("f2"),
			}));
		}


		[TestMethod()]
		public void WildcardSearch_D_3_Test()
		{
			var result = Searcher.WildcardSearch(Root, "**", SearchTarget.Directory);
			Assert.IsTrue(SetEquals(result, new[]
			{
				GetPath("f1"),
				GetPath("f2"),
				GetPath("f1", "f2"),
				GetPath("f1", "f2", "f1"),
			}));
		}

		[TestMethod()]
		public void WildcardSearch_D_4_Test()
		{
			var result = Searcher.WildcardSearch(Root, "**/f1", SearchTarget.Directory);
			Assert.IsTrue(SetEquals(result, new[]
			{
				GetPath("f1"),
				GetPath("f1", "f2", "f1"),
			}));
		}

		[TestMethod()]
		public void WildcardSearch_D_5_Test()
		{
			var result = Searcher.WildcardSearch(Root, "**/f1/**/f*", SearchTarget.Directory);
			Assert.IsTrue(SetEquals(result, new[]
			{
				GetPath("f1", "f2"),
				GetPath("f1", "f2", "f1"),
			}));
		}



		[TestMethod()]
		public void RegexSearch_F_PathNotExists_Test()
		{
			Assert.ThrowsException<DirectoryNotFoundException>(
				() => Searcher.RegexSearch(NotExistsDirectory, "*", SearchTarget.File));
		}


		[TestMethod()]
		public void RegexSearch_D_PathNotExists_Test()
		{
			Assert.ThrowsException<DirectoryNotFoundException>(
				() => Searcher.RegexSearch(NotExistsDirectory, "*", SearchTarget.Directory));
		}


		[TestMethod()]
		public void RegexSearchTest()
		{
			var result = Searcher.RegexSearch(Root, @"(f.+/)*a1b\.t.+t", SearchTarget.File);
			Assert.IsTrue(SetEquals(result, new[]
			{
				GetPath("a1b.tvt"),
				GetPath("a1b.txt"),
				GetPath("f1", "a1b.tvt"),
				GetPath("f1", "a1b.txt"),
				GetPath("f2", "a1b.tvt"),
				GetPath("f2", "a1b.txt"),
				GetPath("f1", "f2", "a1b.tvt"),
				GetPath("f1", "f2", "a1b.txt"),
			}));
		}



		[TestMethod()]
		public void Parse_F1_Test()
		{
			var parser = new RuleParser();
			parser.ParseRules(Root, new[] { "+wf:**/a*b.txt" });
			Assert.IsTrue(SetEquals(parser.Files, new[]
			{
				GetPath("a1b.txt"),
				GetPath("a2b.txt"),
				GetPath("f1", "a1b.txt"),
				GetPath("f1", "a2b.txt"),
				GetPath("f2", "a1b.txt"),
				GetPath("f2", "a2b.txt"),
				GetPath("f1", "f2", "a1b.txt"),
				GetPath("f1", "f2", "a2b.txt"),
			}));
		}

		[TestMethod()]
		public void Parse_F2_Test()
		{
			var parser = new RuleParser();
			parser.ParseRules(Root, new[] { "+wf:**/a*b.txt", "-wf:a*b.txt" });
			Assert.IsTrue(SetEquals(parser.Files, new[]
			{
				GetPath("f1", "a1b.txt"),
				GetPath("f1", "a2b.txt"),
				GetPath("f2", "a1b.txt"),
				GetPath("f2", "a2b.txt"),
				GetPath("f1", "f2", "a1b.txt"),
				GetPath("f1", "f2", "a2b.txt"),
			}));
		}


		[TestMethod()]
		public void Parse_D1_Test()
		{
			var parser = new RuleParser();
			parser.ParseRules(Root, new[] { "+wd:**/f*" });
			Assert.IsTrue(SetEquals(parser.Directories, new[]
			{
				GetPath("f1"),
				GetPath("f1"),
				GetPath("f2"),
				GetPath("f2"),
				GetPath("f1", "f2"),
				GetPath("f1", "f2"),
				GetPath("f1", "f2", "f1"),
				GetPath("f1", "f2", "f1"),
			}));
		}

		[TestMethod()]
		public void Parse_D2_Test()
		{
			var parser = new RuleParser();
			parser.ParseRules(Root, new[] { "+wd:**/f*", "-wd:f*" });
			Assert.IsTrue(SetEquals(parser.Directories, new[]
			{
				GetPath("f1", "f2"),
				GetPath("f1", "f2"),
				GetPath("f1", "f2", "f1"),
				GetPath("f1", "f2", "f1"),
			}));
		}



		static string GetPath(params string[] paths)
		{
			string root = AppDomain.CurrentDomain.BaseDirectory;
			string[] ps = new string[paths.Length + 2];
			ps[0] = root;
			ps[1] = "test";
			Array.Copy(paths, 0, ps, 2, paths.Length);
			return Path.Combine(ps);
		}


		static bool SetEquals(IEnumerable<string> list1, IEnumerable<string> list2)
		{
			return new HashSet<string>(list1).SetEquals(new HashSet<string>(list2));
		}

		//static T? Invoke<T>(string name, params object[] parameters)
		//{
		//	return (T?)typeof(Searcher).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, parameters);
		//}
	}
}