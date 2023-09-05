# FileSystemSearcher

用于在文件系统中按通配符或正则表达式搜索文件或目录。[nuget.org](https://www.nuget.org/packages/FileSystemSearcher/1.0.2)

It helps to search files or directories by using wildcard or regex. [nuget.org](https://www.nuget.org/packages/FileSystemSearcher/1.0.2)

## Searcher

### 通配符搜索（Wildcard Search）

```cs
public static IEnumerable<string> WildcardSearch(string path, string pattern, SearchTarget searchTarget);
```

1. path: 搜索路径（the path to be searched）；
2. pattern: 通配符模式（the wildcard pattern）；
    
    - ?：指代单个字符，不含路径分隔符（matches any one character (excluding directory separators)）
    
    - *：指代任意字符，不含路径分隔符（matches zero or more characters (excluding directory separators)）
    
    - **：指定任意目录层级（matches any directory nested to any level）

3. searchTarget: 搜索目标（the search targe: file or directory）


### 正则表达式搜索（Regex Search）

```cs
public static IEnumerable<string> RegexSearch(string path, string pattern, SearchTarget searchTarget);
```

1. path: 搜索路径（the path to be searched）；
2. pattern: 正则表达式模式（the regex pattern）；
3. searchTarget: 搜索目标（the search targe: file or directory）


## RuleParser

### 按规则集解析（Parse With Rules）

```cs
public void ParseRules(string path, params string[] rules);
```

1. path: 搜索路径（the path to be searched）；
2. rules: 规则集

### 解析结果（Parse Result）

1. `RuleParser.Files`: 通过规则集搜索出的文件集合（Files searched by rules）
2. `RuleParser.Directories: 通过规则集搜索出的目录集合（Directories list searched by rules）

### 重置结果（Reset Result）

```cs
public void Reset();

public void Reset(SearchTarget searchTarget);
```


### 关于规则

1. 以“#”开头：注释内容；
2. 以“+wf:”开头：指定用以匹配文件的通配符，通过该通配符匹配到的文件集合会被添加到解析结果中；
3. 以“-wf:”开头：指定用以匹配文件的通配符，通过该通配符匹配到的文件集合会从解析结果中移除；
4. 以“+rf:”开头：指定用以匹配文件的正则表达式，通过该正则表达式匹配到的文件集合会被添加到解析结果中；
5. 以“-rf:”开头：指定用以匹配文件的正则表达式，通过该正则表达式匹配到的文件集合会从解析结果中移除；
6. 以“+wd:”开头：指定用以匹配目录的通配符，通过该通配符匹配到的目录集合会被添加到解析结果中；
7. 以“-wd:”开头：指定用以匹配目录的通配符，通过该通配符匹配到的目录集合会从解析结果中移除；
8. 以“+rd:”开头：指定用以匹配目录的正则表达式，通过该正则表达式匹配到的目录集合会被添加到解析结果中；
9. 以“-rd:”开头：指定用以匹配目录的正则表达式，通过该正则表达式匹配到的目录集合会从解析结果中移除；


### About Rule

1. Starts With "#": comment;
2. Starts With "+wf:": Specify a wildcard rule to match the files will be included in result;
3. Starts With "-wf:": Specify a wildcard rule to match the files will be excluded from result;
4. Starts With "+rf:": Specify a regex rule to match the files will be included in result;
5. Starts With "-rf:": Specify a regex rule to match the files will be excluded from result;
6. Starts With "+wd:": Specify a wildcard rule to match the directories will be included in result;
7. Starts With "-wd:": Specify a wildcard rule to match the directories will be excluded from result;
8. Starts With "+rd:": Specify a regex rule to match the directories will be included in result;
9. Starts With "-rd:": Specify a regex rule to match the directories will be excluded from result;



你可以通过继承 `RuleParser` 类并重写 ParseCustomRule 方法来解析自定义规则。

You can parse the custom rule by overriding the `ParseCustomRule` method in a subclass inherited from ``RuleParser``.
