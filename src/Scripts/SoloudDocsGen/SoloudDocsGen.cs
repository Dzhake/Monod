// ---------------------------------------------------------------------------
// SoloudDocGen.cs
//
// Single-file tool that adds XML <summary> documentation comments to the
// public functions of the SoloudObject-derived classes in the SoLoud C#
// bindings (soloud.cs), using the official SoLoud HTML documentation.
//
// How it works:
//   1. Scans soloud.cs for every "class X : SoloudObject" declaration.
//   2. Inside each class body, scans for functions (methods and
//      constructors). Fields, properties and the internal P/Invoke
//      declarations are ignored.
//   3. For every *public* function, the matching section in the HTML docs
//      is found by heading id:
//          methods      ->  <hX id="class.method">     e.g. "soloud.init"
//          constructors ->  <hX id="soloudclass">       e.g. "soloudwavstream"
//      Matching is case-insensitive. Hyphen-bundled ids such as
//      "wavstream.setlooppoint-wavstream.getlooppoint" match both of the
//      methods they cover. The section text is everything between the
//      heading's </hX> and the next <hX> tag (the text inside the heading
//      tag itself is NOT used).
//   4. That HTML is converted to plain text, XML-escaped, and written as a
//      "/// <summary>" block right above the function. A pre-existing
//      /// comment on that function is removed first.
//   5. The result of every method is printed to the console; failures come
//      with a reason.
//
// Usage:
//   dotnet run SoloudDocGen.cs soloud.cs soloud_docs.html [more.html|folder ...]
//                          [-o output.cs] [-inplace]
//   (also works as Program.cs of an ordinary console project)
//
// By default the result is written to <soloud>.documented.cs next to the
// input file; the input file is never modified unless -inplace is given
// (in which case a .bak copy is created first).
//
// Exit codes: 0 = finished (see the report for misses), 1 = fatal error.
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

#nullable disable

internal static class SoloudDocGen
{
    // =====================================================================
    //  Entry point / command line
    // =====================================================================

    private sealed class Options
    {
        public string CsPath;
        public readonly List<string> DocInputs = new List<string>();
        public string OutputPath;          // null/empty = default
        public bool InPlace;
    }

    private static int Main(string[] args)
    {
        try { return Run(args); }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FATAL: " + ex.Message);
            return 1;
        }
    }

    private static int Run(string[] args)
    {
        Options opts = ParseArgs(args);
        if (opts == null) { PrintUsage(); return 1; }

        if (!File.Exists(opts.CsPath))
        {
            Console.Error.WriteLine("FATAL: C# bindings file not found: " + opts.CsPath);
            return 1;
        }

        List<string> htmlFiles = ExpandDocFiles(opts.DocInputs);
        if (htmlFiles.Count == 0)
        {
            Console.Error.WriteLine("FATAL: no HTML documentation file(s) found.");
            return 1;
        }

        string csText = File.ReadAllText(opts.CsPath);
        string nl = csText.Contains("\r\n") ? "\r\n" : "\n";

        Console.WriteLine("SoLoud C# bindings - documentation comment generator");
        Console.WriteLine("  bindings : " + Path.GetFullPath(opts.CsPath));
        foreach (string f in htmlFiles)
            Console.WriteLine("  docs     : " + Path.GetFullPath(f));

        List<DocSection> sections = BuildDocIndex(htmlFiles);
        Console.WriteLine("  parsed " + sections.Count + " documentation section(s) by heading id");
        if (sections.Count == 0)
        {
            Console.Error.WriteLine("FATAL: no <h1>..<h6 id=\"...\"> headings found in the documentation.");
            return 1;
        }

        List<Edit> edits = new List<Edit>();
        int classes = 0, classesDocumented = 0, publicFunctions = 0, documented = 0, failed = 0;

        foreach (Match m in ClassRx.Matches(csText))
        {
            if (!Regex.IsMatch(m.Groups["bases"].Value, @"\bSoloudObject\b"))
                continue;

            string className = m.Groups["name"].Value;
            int bodyOpen = m.Index + m.Length - 1;                 // the '{' of the class
            int bodyClose = FindMatching(csText, bodyOpen, '}');
            if (bodyClose < 0) bodyClose = csText.Length;

            classes++;

            List<MemberInfo> members = ScanMembers(csText, bodyOpen + 1, bodyClose, className);
            List<MemberInfo> functions = members.FindAll(x => x.IsMethod && x.IsPublic && !x.IsCtor);

            Console.WriteLine();
            Console.WriteLine(className + " : SoloudObject   (" + functions.Count + " public function(s))");

            // ---- class <summary>: the constructor documentation ("soloudclass")
            //      describes the class as a whole, so it goes on the class itself ----
            ClassDeclInfo decl = ScanClassDecl(csText, m.Index);
            string classKey = "soloud" + className.ToLowerInvariant();
            DocSection classDoc = sections.FirstOrDefault(s => MatchesKey(s.Id, classKey));

            if (classDoc == null)
            {
                failed++;
                Console.WriteLine("  [FAIL] " + className + " (class) : no documentation found - " +
                                  "no heading id matching \"" + classKey + "\" in " + htmlFiles.Count + " doc file(s)");
            }
            else if (string.IsNullOrWhiteSpace(classDoc.Text))
            {
                failed++;
                Console.WriteLine("  [FAIL] " + className + " (class) : documentation section \"" +
                                  classDoc.RawId + "\" is empty");
            }
            else
            {
                string indent = GetIndentOf(csText, decl.DeclStart);
                string summary = BuildSummaryComment(classDoc.Text, indent, nl);

                int replaceStart;
                string replacement;
                if (decl.DocCommentStart >= 0 && decl.DocCommentStart < decl.DeclStart)
                {
                    // replace the old /// comment block plus the whitespace up to the declaration
                    replaceStart = decl.DocCommentStart;
                    replacement = summary;
                }
                else
                {
                    int lineStart = LineStartOf(csText, decl.DeclStart);
                    if (IsAllWhitespace(csText, lineStart, decl.DeclStart))
                    {
                        replaceStart = lineStart;      // comment carries its own indentation
                        replacement = summary;
                    }
                    else
                    {
                        replaceStart = decl.DeclStart; // declaration shares its line with code - just insert
                        replacement = nl + summary;
                    }
                }

                edits.Add(new Edit(replaceStart, decl.DeclStart, replacement));
                classesDocumented++;

                string oldCommentInfo = decl.DocCommentLines > 0
                    ? "old comment removed, " + decl.DocCommentLines + " line(s)"
                    : "old comment: none (0 lines)";
                Console.WriteLine("  [OK]   " + className + " (class) : documented from \"" + classDoc.RawId +
                                  "\" (" + CountLines(classDoc.Text) + " doc line(s), " + oldCommentInfo + ")");
            }

            // ---- public functions (constructors are documented on the class, not here) ----
            foreach (MemberInfo f in functions)
            {
                publicFunctions++;

                string key = className.ToLowerInvariant() + "." + f.Name.ToLowerInvariant();

                DocSection doc = sections.FirstOrDefault(s => MatchesKey(s.Id, key));
                if (doc == null)
                {
                    failed++;
                    Console.WriteLine("  [FAIL] " + className + "." + f.Name + "() : no documentation found - " +
                                      "no heading id matching \"" + key + "\" in " + htmlFiles.Count + " doc file(s)");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(doc.Text))
                {
                    failed++;
                    Console.WriteLine("  [FAIL] " + className + "." + f.Name + "() : documentation section \"" +
                                      doc.RawId + "\" is empty");
                    continue;
                }

                string indent = GetIndentOf(csText, f.MemberStart);
                string summary = BuildSummaryComment(doc.Text, indent, nl);

                int replaceStart;
                string replacement;
                if (f.DocCommentStart >= 0 && f.DocCommentStart < f.MemberStart)
                {
                    // replace the old /// comment block plus the whitespace up to the declaration
                    replaceStart = f.DocCommentStart;
                    replacement = summary;
                }
                else
                {
                    int lineStart = LineStartOf(csText, f.MemberStart);
                    if (IsAllWhitespace(csText, lineStart, f.MemberStart))
                    {
                        replaceStart = lineStart;      // comment carries its own indentation
                        replacement = summary;
                    }
                    else
                    {
                        replaceStart = f.MemberStart; // declaration shares its line with code - just insert
                        replacement = nl + summary;
                    }
                }

                edits.Add(new Edit(replaceStart, f.MemberStart, replacement));
                documented++;

                string oldCommentInfo = f.DocCommentLines > 0
                    ? "old comment removed, " + f.DocCommentLines + " line(s)"
                    : "old comment: none (0 lines)";
                Console.WriteLine("  [OK]   " + className + "." + f.Name + "() : documented from \"" + doc.RawId +
                                  "\" (" + CountLines(doc.Text) + " doc line(s), " + oldCommentInfo + ")");
            }
        }

        if (classes == 0)
        {
            Console.Error.WriteLine("FATAL: no \"class X : SoloudObject\" declarations found in " + opts.CsPath);
            return 1;
        }

        string result = ApplyEdits(csText, edits);
        string outPath = opts.InPlace ? opts.CsPath
                                      : (string.IsNullOrEmpty(opts.OutputPath) ? DefaultOutputPath(opts.CsPath)
                                                                               : opts.OutputPath);

        if (opts.InPlace)
        {
            string backup = opts.CsPath + ".bak";
            File.Copy(opts.CsPath, backup, true);
            Console.WriteLine();
            Console.WriteLine("Backup of the original written to: " + Path.GetFullPath(backup));
        }

        File.WriteAllText(outPath, result, new UTF8Encoding(HasUtf8Bom(opts.CsPath)));

        Console.WriteLine();
        Console.WriteLine("----------------------------------------------------------------");
        Console.WriteLine("classes processed  : " + classes);
        Console.WriteLine("classes documented : " + classesDocumented);
        Console.WriteLine("public functions   : " + publicFunctions);
        Console.WriteLine("documented         : " + documented);
        Console.WriteLine("failed             : " + failed);
        return 0;
    }

    private static Options ParseArgs(string[] args)
    {
        if (args.Length < 2) return null;

        Options o = new Options { CsPath = args[0] };
        for (int i = 1; i < args.Length; i++)
        {
            string a = args[i];
            if (a == "-o" || a == "--output")
            {
                if (i + 1 >= args.Length) { Console.Error.WriteLine("ERROR: missing value after " + a); return null; }
                o.OutputPath = args[++i];
            }
            else if (a.StartsWith("-o:", StringComparison.Ordinal) ||
                     a.StartsWith("--output:", StringComparison.Ordinal))
            {
                o.OutputPath = a.Substring(a.IndexOf(':') + 1);
            }
            else if (a == "-inplace" || a == "--inplace")
            {
                o.InPlace = true;
            }
            else if (a.StartsWith("-") && a.Length > 1)
            {
                Console.Error.WriteLine("ERROR: unknown option: " + a);
                return null;
            }
            else
            {
                o.DocInputs.Add(a);
            }
        }
        if (o.DocInputs.Count == 0)
        {
            Console.Error.WriteLine("ERROR: no documentation file(s)/folder(s) given.");
            return null;
        }
        return o;
    }

    private static void PrintUsage()
    {
        Console.WriteLine(
            "Usage:\n" +
            "  SoloudDocGen <soloud.cs> <docs.html|docsFolder> [more.html|moreFolders...]\n" +
            "              [-o <output.cs>] [-inplace]\n" +
            "\n" +
            "  <soloud.cs>             generated C# SoLoud bindings\n" +
            "  <docs.html|docsFolder>  official SoLoud HTML documentation: a single .html file,\n" +
            "                          several files, or a folder (searched recursively for *.html/*.htm)\n" +
            "  -o <output.cs>          output file (default: <soloud>.documented.cs next to the input)\n" +
            "  -inplace                overwrite the input file (a .bak copy is written first)");
    }

    private static List<string> ExpandDocFiles(IEnumerable<string> inputs)
    {
        List<string> files = new List<string>();
        foreach (string input in inputs)
        {
            if (Directory.Exists(input))
            {
                files.AddRange(Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories)
                        .Where(f =>
                        {
                            string ext = Path.GetExtension(f);
                            return ext.Equals(".html", StringComparison.OrdinalIgnoreCase)
                                || ext.Equals(".htm", StringComparison.OrdinalIgnoreCase);
                        })
                        .OrderBy(f => f, StringComparer.OrdinalIgnoreCase));
            }
            else if (File.Exists(input))
            {
                files.Add(input);
            }
            else
            {
                Console.WriteLine("[WARN] documentation input not found, skipping: " + input);
            }
        }
        return files.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    // =====================================================================
    //  HTML documentation parsing
    // =====================================================================

    private sealed class DocSection
    {
        public string Id;      // lower-cased heading id, e.g. "wavstream.setlooppoint"
        public string RawId;   // id exactly as written in the html (for console output)
        public string Text;    // plain-text body of the section
    }

    // <h2 id="soloud.init">Soloud.init()</h2>  - level can be anything from h1 to h6
    private static readonly Regex HeadingRx = new Regex(
        @"<h(?<lvl>[1-6])(?<attrs>[^>]*)>(?<inner>.*?)</h\k<lvl>\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex HeadingIdRx = new Regex(
        @"(?:(?<=\s)|^)id\s*=\s*(?:""(?<v>[^""]*)""|'(?<v>[^']*)'|(?<v>[^\s>]+))",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static List<DocSection> BuildDocIndex(List<string> htmlFiles)
    {
        List<DocSection> sections = new List<DocSection>();
        foreach (string file in htmlFiles)
        {
            string html;
            try { html = File.ReadAllText(file); }
            catch (Exception ex)
            {
                Console.WriteLine("[WARN] could not read " + file + ": " + ex.Message);
                continue;
            }

            MatchCollection matches = HeadingRx.Matches(html);
            for (int i = 0; i < matches.Count; i++)
            {
                Match m = matches[i];
                string rawId = HeadingIdRx.Match(m.Groups["attrs"].Value).Groups["v"].Value.Trim();
                if (rawId.Length == 0) continue;            // heading without id - cannot be referenced

                int bodyStart = m.Index + m.Length;                              // just after </hX>
                int bodyEnd = (i + 1 < matches.Count) ? matches[i + 1].Index     // next <hX> tag
                                                      : html.Length;
                string fragment = bodyEnd > bodyStart
                    ? html.Substring(bodyStart, bodyEnd - bodyStart)
                    : string.Empty;

                sections.Add(new DocSection
                {
                    RawId = rawId,
                    Id = rawId.ToLowerInvariant(),
                    Text = HtmlToDocText(fragment)
                });
            }
        }
        return sections;
    }

    // "wavstream.setlooppoint-wavstream.getlooppoint" matches both
    // "wavstream.setlooppoint" and "wavstream.getlooppoint".
    private static bool MatchesKey(string id, string key)
    {
        foreach (string part in id.Split('-'))
            if (part == key) return true;
		if (key.Contains("set3d") && id == "audiosource."+key.Split('.')[1]) return true;
        return false;
    }

    // =====================================================================
    //  HTML -> plain text
    // =====================================================================

    private static readonly Regex PreBlockRx = new Regex(
        @"<pre\b[^>]*>(?<code>.*?)</pre\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
		
	private static readonly Regex InlineCodeRx = new Regex(
        @"<code\b[^>]*>(?<code>.*?)</code\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex AnyTagRx = new Regex(@"<[^>]*>", RegexOptions.Compiled);

    private static readonly Regex EntityRx = new Regex(
        @"&(?<ent>#[xX]?[0-9a-fA-F]+|[a-zA-Z][a-zA-Z0-9]*);",
        RegexOptions.Compiled);

    private static readonly Regex MultiSpaceRx = new Regex("[ \t\f\v\u00A0]+", RegexOptions.Compiled);

      // Converts an HTML fragment into XML-doc markup: text content is XML-escaped,
    // <p> becomes <para>, <pre><code> becomes a <code> block, inline <code> becomes <c>.
    // All other tags are dropped (their text is kept).
    //
    // Every piece of markup we keep is parked in a \u0001K placeholder while the
    // "drop all tags" and whitespace-normalisation passes run, and restored at the
    // end - <code> always worked that way, and <para> now does too.
    private static string HtmlToDocText(string html)
    {
        if (html == null || html.Trim().Length == 0) return string.Empty;

        // text content of kept elements: strip inner tags, decode entities, escape for XML
        string CleanText(string raw) => XmlEscape(DecodeEntities(AnyTagRx.Replace(raw, "")));

        List<string> parked = new List<string>();              // kept markup, restored at the end
        string Park(string markup)
        {
            parked.Add(markup);
            return "\u0001K" + (parked.Count - 1) + "\u0001";
        }

        // 1) <pre> blocks -> <code> blocks; internal line breaks and indentation are
        //    preserved (the placeholder bypasses whitespace normalisation)
        string s = PreBlockRx.Replace(html, m =>
        {
            string code = CleanText(m.Groups["code"].Value)
                             .Replace("\r\n", "\n").Replace("\r", "\n");
            string[] codeLines = code.Split('\n');
            for (int i = 0; i < codeLines.Length; i++) codeLines[i] = codeLines[i].TrimEnd(' ', '\t');
            return "\n" + Park("<code>\n" + string.Join("\n", codeLines) + "\n</code>") + "\n";
        });

        // 2) inline <code> -> <c>
        s = InlineCodeRx.Replace(s, m => Park("<c>" + CleanText(m.Groups["code"].Value) + "</c>"));

        // 3) <p> / </p> -> <para> / </para>, parked exactly like the code markup above,
        //    so the tag-stripping pass below cannot delete them again
        s = Regex.Replace(s, @"<p\b[^>]*>", m => Park("<para>"), RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"</p\s*>", m => Park("</para>"), RegexOptions.IgnoreCase);

        // 4) remaining layout tags -> plain text markers
        s = Regex.Replace(s, @"<li\b[^>]*>", "- ", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"</li\s*>", "\n", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"</(tr|div|blockquote|dt|dd|caption|figcaption)\s*>", "\n", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"</(td|th)\s*>", " ", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"<hr\b[^>]*/?>", "\n", RegexOptions.IgnoreCase);

        // 5) every remaining tag (<b>, <i>, <a>, ...) is dropped, text kept;
        //    the parked <para>/<code>/<c> markup is immune - placeholders are not tags
        s = AnyTagRx.Replace(s, "");

        // 6) entities -> characters (placeholders contain no '&', so they are safe here)
        s = DecodeEntities(s);

        // 7) normalise whitespace; escape bare text for XML - placeholders contain no
        //    & < >, so escaping the whole line cannot damage them
        List<string> lines = new List<string>();
        foreach (string raw in s.Split('\n'))
        {
            string line = raw.Trim(' ', '\t', '\f', '\v', '\r', '\u00A0');
            line = MultiSpaceRx.Replace(line, " ");
            lines.Add(XmlEscape(line));
        }

        List<string> kept = new List<string>();
        foreach (string line in lines)
        {
            if (line.Length == 0 && (kept.Count == 0 || kept[kept.Count - 1].Length == 0)) continue;
            kept.Add(line);
        }
        while (kept.Count > 0 && kept[kept.Count - 1].Length == 0) kept.RemoveAt(kept.Count - 1);

        s = string.Join("\n", kept);

        // 8) restore the parked markup
        for (int i = 0; i < parked.Count; i++)
            s = s.Replace("\u0001K" + i + "\u0001", parked[i]);

        // 9) put directly adjacent paragraphs on separate lines
        s = s.Replace("</para><para>", "</para>\n<para>");

        // 10) drop empty paragraphs the conversion may have produced
        s = Regex.Replace(s, @"<para>\s*</para>", "", RegexOptions.IgnoreCase);

        return s.Trim('\n');
    }

    private static string DecodeEntities(string s)
    {
        if (s == null || s.IndexOf('&') < 0) return s ?? string.Empty;
        return EntityRx.Replace(s, m =>
        {
            string e = m.Groups["ent"].Value;
            if (e[0] == '#')
            {
                int code = 0;
                bool ok;
                if (e.Length > 2 && (e[1] == 'x' || e[1] == 'X'))
                    ok = int.TryParse(e.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out code);
                else
                    ok = int.TryParse(e.Substring(1), out code);
                if (!ok || code < 0x20 || code > 0x10FFFF || (code >= 0xD800 && code <= 0xDFFF))
                    return " ";                                   // invalid/control -> space
                return char.ConvertFromUtf32(code);
            }
            switch (e)
            {
                case "lt":    return "<";
                case "gt":    return ">";
                case "amp":   return "&";
                case "quot":  return "\"";
                case "apos":  return "'";
                case "nbsp":  return " ";
                case "lsquo":
                case "rsquo":
                case "sbquo": return "'";
                case "ldquo":
                case "rdquo":
                case "bdquo": return "\"";
                case "mdash":
                case "ndash": return "-";
                case "hellip": return "...";
                case "bull":   return "-";
                case "copy":   return "(c)";
                case "reg":    return "(r)";
                case "trade":  return "(tm)";
                case "deg":    return "\u00B0";
                case "plusmn": return "\u00B1";
                case "times":  return "\u00D7";
                case "divide": return "\u00F7";
                default:       return m.Value;   // unknown entity: kept literally
            }
        });
    }

    private static string XmlEscape(string s)
    {
        if (s.IndexOf('&') >= 0) s = s.Replace("&", "&amp;");
        if (s.IndexOf('<') >= 0) s = s.Replace("<", "&lt;");
        if (s.IndexOf('>') >= 0) s = s.Replace(">", "&gt;");
        return s;
    }

    // =====================================================================
    //  C# source scanning
    // =====================================================================

    private static readonly Regex ClassRx = new Regex(
        @"\bclass\s+(?<name>[A-Za-z_]\w*)\s*:\s*(?<bases>[^{;]+)\{",
        RegexOptions.Compiled);

    private static readonly string[] ModifierWords =
    {
        "public", "private", "protected", "internal", "static", "sealed", "abstract",
        "virtual", "override", "readonly", "const", "new", "unsafe", "extern",
        "async", "partial", "volatile", "required", "file"
    };

    private sealed class MemberInfo
    {
        public int MemberStart;          // first significant char of the member ('[' of attributes or first modifier)
        public int MemberEnd;            // one past the last char of the member
        public string Name;
        public bool IsPublic;
        public bool IsCtor;
        public bool IsMethod;            // has a parameter list and a { body }
        public int DocCommentStart = -1; // line start of the first /// line above the member (-1 = none)
        public int DocCommentLines;      // number of /// lines above the member
    }

    private static List<MemberInfo> ScanMembers(string s, int bodyStart, int bodyEnd, string className)
    {
        List<MemberInfo> members = new List<MemberInfo>();
        int pos = bodyStart;

        while (pos < bodyEnd)
        {
            // ---- leading trivia: whitespace, preprocessor lines, comments ----
            int p = pos;
            int docStart = -1, docLines = 0;
            while (p < bodyEnd)
            {
                char c = s[p];
                if (char.IsWhiteSpace(c)) { p++; continue; }
                if (c == '#') { while (p < bodyEnd && s[p] != '\n') p++; continue; }
                if (c == '/' && p + 1 < bodyEnd && s[p + 1] == '/')
                {
                    bool isDoc = p + 2 < bodyEnd && s[p + 2] == '/';
                    if (isDoc)
                    {
                        if (docStart < 0) docStart = LineStartOf(s, p);
                        docLines++;
                    }
                    while (p < bodyEnd && s[p] != '\n') p++;
                    if (p < bodyEnd) p++;
                    continue;
                }
                if (c == '/' && p + 1 < bodyEnd && s[p + 1] == '*')
                {
                    bool isDoc = p + 2 < bodyEnd && s[p + 2] == '*' &&
                                 !(p + 3 < bodyEnd && s[p + 3] == '/');     // "/**/" is not a doc comment
                    int after = SkipBlockComment(s, p);
                    if (isDoc)
                    {
                        if (docStart < 0) docStart = LineStartOf(s, p);
                        docLines += CountLines(s.Substring(p, after - p));
                    }
                    p = after;
                    continue;
                }
                break;
            }
            if (p >= bodyEnd) break;

            MemberInfo info = new MemberInfo
            {
                MemberStart = p,
                DocCommentStart = docStart,
                DocCommentLines = docLines
            };

            // ---- attribute sections (several possible, with nested brackets) ----
            int q = p;
            while (q < bodyEnd && s[q] == '[')
            {
                int after = FindMatching(s, q, ']');
                if (after < 0) { q = bodyEnd; break; }
                int r = after;
                while (r < bodyEnd)
                {
                    if (char.IsWhiteSpace(s[r])) { r++; continue; }
                    if (s[r] == '/' && r + 1 < bodyEnd && s[r + 1] == '/') { while (r < bodyEnd && s[r] != '\n') r++; continue; }
                    if (s[r] == '/' && r + 1 < bodyEnd && s[r + 1] == '*') { r = SkipBlockComment(s, r); continue; }
                    break;
                }
                q = r;   // next attribute block, or the declaration itself
            }
            if (q >= bodyEnd) break;

            // ---- scan the declaration up to the body '{' or the ';' terminator ----
            int i = q;
            while (i < bodyEnd)
            {
                char c = s[i];
                if (c == '/' && i + 1 < bodyEnd && s[i + 1] == '/') { while (i < bodyEnd && s[i] != '\n') i++; continue; }
                if (c == '/' && i + 1 < bodyEnd && s[i + 1] == '*') { i = SkipBlockComment(s, i); continue; }
                if (c == '"') { i = SkipString(s, i); continue; }
                if (c == '\'') { i = SkipChar(s, i); continue; }
                if (c == '{' || c == ';') break;
                i++;
            }
            if (i >= bodyEnd) break;

            if (s[i] == '{')
            {
                int afterBody = FindMatching(s, i, '}');
                info.MemberEnd = afterBody < 0 ? bodyEnd : Math.Min(afterBody, bodyEnd);
                ParseDeclaration(s.Substring(q, i - q), className, info);
            }
            else
            {
                info.MemberEnd = Math.Min(i + 1, bodyEnd);   // ';' - field / P/Invoke / delegate...
                info.IsMethod = false;
            }

            members.Add(info);
            pos = info.MemberEnd;
        }
        return members;
    }

    private static void ParseDeclaration(string declRaw, string className, MemberInfo info)
    {
        string decl = StripComments(declRaw);
        int n = decl.Length;

        // ---- modifier keywords ----
        int i = 0;
        List<string> mods = new List<string>();
        while (i < n)
        {
            int j = i;
            while (j < n && char.IsWhiteSpace(decl[j])) j++;
            if (j >= n) { i = j; break; }
            if (!char.IsLetter(decl[j])) { i = j; break; }         // '~' finalizer, etc.
            int k = j;
            while (k < n && (char.IsLetterOrDigit(decl[k]) || decl[k] == '_')) k++;
            string word = decl.Substring(j, k - j);
            if (Array.IndexOf(ModifierWords, word) < 0) { i = j; break; }
            mods.Add(word);
            i = k;
        }
        info.IsPublic = mods.Contains("public");

        string rest = decl.Substring(i).TrimStart();
        if (rest.Length == 0 || rest[0] == '~') return;            // finalizer: never documented

        // ---- find top-level '(' groups (the parameter list) ----
        List<int> groups = new List<int>();
        int depth = 0, g = 0;
        while (g < rest.Length)
        {
            char c = rest[g];
            if (c == '(') { if (depth == 0) groups.Add(g); depth++; g++; }
            else if (c == ')') { if (depth > 0) depth--; g++; }
            else if (c == '"') g = SkipString(rest, g);
            else if (c == '\'') g = SkipChar(rest, g);
            else g++;
        }
        if (groups.Count == 0) return;                             // property/field-like member - not a function

        // the parameter list is the last group that does not belong to
        // a ": base(...)" / ": this(...)" constructor initializer
        int paren = -1;
        for (int t = groups.Count - 1; t >= 0; t--)
        {
            string owner = IdentifierBefore(rest, groups[t]);
            if (owner != "base" && owner != "this") { paren = groups[t]; break; }
        }
        if (paren < 0) return;

        string name = IdentifierBefore(rest, paren);
        if (name.Length == 0) return;

        info.Name = name;
        info.IsCtor = string.Equals(name, className, StringComparison.Ordinal);
        info.IsMethod = true;
    }

    // The identifier that sits immediately left of s[parenIdx] (skipping
    // whitespace and a possible <...> generic suffix).
    private static string IdentifierBefore(string s, int parenIdx)
    {
        int i = parenIdx - 1;
        while (i >= 0 && char.IsWhiteSpace(s[i])) i--;
        if (i < 0) return "";
        if (s[i] == '>')
        {
            int d = 0;
            while (i >= 0)
            {
                if (s[i] == '>') d++;
                else if (s[i] == '<') { d--; if (d == 0) { i--; break; } }
                i--;
            }
            while (i >= 0 && char.IsWhiteSpace(s[i])) i--;
        }
        int end = i + 1;
        while (i >= 0 && (char.IsLetterOrDigit(s[i]) || s[i] == '_')) i--;
        return s.Substring(i + 1, end - i - 1);
    }

    // Removes // and /* */ comments, but leaves string and char literals intact.
    private static string StripComments(string s)
    {
        StringBuilder sb = new StringBuilder(s.Length);
        int i = 0;
        while (i < s.Length)
        {
            char c = s[i];
            if (c == '/' && i + 1 < s.Length && s[i + 1] == '/')
            {
                while (i < s.Length && s[i] != '\n') i++;
            }
            else if (c == '/' && i + 1 < s.Length && s[i + 1] == '*')
            {
                sb.Append(' ');
                i = SkipBlockComment(s, i);
            }
            else if (c == '"') { int after = SkipString(s, i); sb.Append(s, i, after - i); i = after; }
            else if (c == '\'') { int after = SkipChar(s, i); sb.Append(s, i, after - i); i = after; }
            else { sb.Append(c); i++; }
        }
        return sb.ToString();
    }

    // =====================================================================
    //  Lexical helpers (comment-, string- and char-literal-aware)
    // =====================================================================

    // Index just past the 'close' char matching s[open]; -1 if unbalanced.
    private static int FindMatching(string s, int open, char close)
    {
        char openChar = s[open];
        int depth = 0;
        int i = open;
        int n = s.Length;
        while (i < n)
        {
            char c = s[i];
            if (c == openChar) { depth++; i++; }
            else if (c == close)
            {
                if (--depth == 0) return i + 1;
                i++;
            }
            else if (c == '/' && i + 1 < n && s[i + 1] == '/') { while (i < n && s[i] != '\n') i++; }
            else if (c == '/' && i + 1 < n && s[i + 1] == '*') { i = SkipBlockComment(s, i); }
            else if (c == '"') { i = SkipString(s, i); }
            else if (c == '\'') { i = SkipChar(s, i); }
            else i++;
        }
        return -1;
    }

    private static int SkipBlockComment(string s, int i)     // i at the '/'
    {
        int j = i + 2;
        while (j < s.Length - 1)
        {
            if (s[j] == '*' && s[j + 1] == '/') return j + 2;
            j++;
        }
        return s.Length;
    }

    private static int SkipString(string s, int i)           // i at the opening '"'
    {
        bool verbatim = i > 0 && s[i - 1] == '@';
        int j = i + 1;
        while (j < s.Length)
        {
            if (verbatim)
            {
                if (s[j] == '"')
                {
                    if (j + 1 < s.Length && s[j + 1] == '"') { j += 2; continue; }
                    return j + 1;
                }
                j++;
            }
            else
            {
                if (s[j] == '\\') { j += 2; continue; }
                if (s[j] == '"') return j + 1;
                j++;
            }
        }
        return s.Length;
    }

    private static int SkipChar(string s, int i)             // i at the opening '\''
    {
        int j = i + 1;
        while (j < s.Length)
        {
            if (s[j] == '\\') { j += 2; continue; }
            if (s[j] == '\'') return j + 1;
            j++;
        }
        return s.Length;
    }

    // =====================================================================
    //  Output generation
    // =====================================================================

    private sealed class Edit
    {
        public readonly int Start;
        public readonly int End;
        public readonly string Text;
        public Edit(int start, int end, string text) { Start = start; End = end; Text = text; }
    }

    private static string BuildSummaryComment(string docText, string indent, string nl)
    {
        string[] lines = docText.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
        StringBuilder sb = new StringBuilder();
        sb.Append(indent).Append("/// <summary>").Append(nl);
        foreach (string line in lines)
        {
            if (line.Length == 0) sb.Append(indent).Append("///").Append(nl);
            else sb.Append(indent).Append("/// ").Append(line).Append(nl);
        }
        sb.Append(indent).Append("/// </summary>").Append(nl);
        sb.Append(indent);
        return sb.ToString();
    }

    private static string ApplyEdits(string csText, List<Edit> edits)
    {
        if (edits.Count == 0) return csText;
        StringBuilder sb = new StringBuilder(csText);
        foreach (Edit e in edits.OrderByDescending(e => e.Start))   // back to front
        {
            if (e.End > e.Start) sb.Remove(e.Start, e.End - e.Start);
            if (e.Text.Length > 0) sb.Insert(e.Start, e.Text);
        }
        return sb.ToString();
    }

    private static int LineStartOf(string s, int pos)
    {
        int i = pos - 1;
        while (i >= 0 && s[i] != '\n') i--;
        return i + 1;
    }

    private static string GetIndentOf(string s, int pos)
    {
        int ls = LineStartOf(s, pos);
        int i = ls;
        while (i < pos && (s[i] == ' ' || s[i] == '\t')) i++;
        return s.Substring(ls, i - ls);
    }

    private static bool IsAllWhitespace(string s, int from, int to)
    {
        for (int i = from; i < to; i++)
            if (!char.IsWhiteSpace(s[i])) return false;
        return true;
    }

    private static int CountLines(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        return s.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n').Length;
    }

    private static bool HasUtf8Bom(string path)
    {
        try
        {
            using FileStream fs = File.OpenRead(path);
            return fs.ReadByte() == 0xEF && fs.ReadByte() == 0xBB && fs.ReadByte() == 0xBF;
        }
        catch { return false; }
    }

    private static string DefaultOutputPath(string csPath)
    {
        string dir = Path.GetDirectoryName(Path.GetFullPath(csPath));
        if (string.IsNullOrEmpty(dir)) dir = ".";
        return Path.Combine(dir, Path.GetFileNameWithoutExtension(csPath) + ".documented" +
                                Path.GetExtension(csPath));
    }
	
	// =====================================================================
    //  Class declaration scanning (class-level doc comments)
    // =====================================================================

    private sealed class ClassDeclInfo
    {
        public int DeclStart;          // first non-whitespace char of the FIRST declaration line
        public int DocCommentStart;    // line start of the first doc-comment line above it (-1 = none)
        public int DocCommentLines;    // number of doc-comment lines above it
    }

    // true when every whitespace-separated token of 'text' is a modifier keyword
    // (an empty string counts as "only modifiers")
    private static bool IsModifierList(string text)
    {
        foreach (string tok in text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            if (Array.IndexOf(ModifierWords, tok) < 0) return false;
        return true;
    }

    // Locates the first line of the class declaration: the line holding the
    // 'class' keyword, plus modifier-only lines above it (e.g. "public partial"
    // on its own line) and attribute lines ('[...]') above those. DeclStart is
    // the first non-whitespace character of that first declaration line, so a
    // doc comment inserted before it always starts on its own line, above the
    // whole declaration.
    private static ClassDeclInfo ScanClassDecl(string s, int classKeywordIndex)
    {
        ClassDeclInfo info = new ClassDeclInfo();

        // ---- the line that contains the 'class' keyword ----
        int declLine = LineStartOf(s, classKeywordIndex);

        // everything before the keyword on that line must be modifiers only
        // ("public partial "), otherwise we keep the 'class' line as the anchor
        string head = StripComments(s.Substring(declLine, classKeywordIndex - declLine)).Trim();
        if (IsModifierList(head))
        {
            // ---- absorb declaration lines above: modifier-only and attribute lines ----
            while (declLine > 0)
            {
                int nl = declLine - 1;
                int prevStart = LineStartOf(s, nl);
                string prev = StripComments(s.Substring(prevStart, nl - prevStart)).Trim();
                if (prev.Length == 0) break;                            // blank line ends the declaration
                if (prev.StartsWith("[", StringComparison.Ordinal) ||
                    IsModifierList(prev))
                { declLine = prevStart; continue; }
                break;                                                  // real code above: stop
            }
        }

        // ---- first non-whitespace character of the first declaration line ----
        int declStart = declLine;
        while (declStart < s.Length && (s[declStart] == ' ' || s[declStart] == '\t')) declStart++;
        info.DeclStart = declStart;

        // ---- existing doc comment directly above the declaration ----
        int docStart = -1, docLines = 0;
        int cursor = declLine;
        while (cursor > 0)
        {
            int nl = cursor - 1;                        // the '\n' that ends the line above
            int prevStart = LineStartOf(s, nl);
            string prev = s.Substring(prevStart, nl - prevStart).Trim();

            if (prev.Length == 0) { cursor = prevStart; continue; }
            if (prev.StartsWith("///", StringComparison.Ordinal))
            {
                docStart = prevStart; docLines++;
                cursor = prevStart; continue;
            }
            if (prev.StartsWith("//", StringComparison.Ordinal) ||
                prev.StartsWith("#", StringComparison.Ordinal)) { cursor = prevStart; continue; }
            if (prev.StartsWith("/*", StringComparison.Ordinal))
            {
                // single-line block comment; '/** ... */' is a doc comment
                if (prev.StartsWith("/**", StringComparison.Ordinal) && prev != "/**/")
                { docStart = prevStart; docLines++; }
                cursor = prevStart; continue;
            }
            if (prev.EndsWith("*/", StringComparison.Ordinal))
            {
                // tail of a multi-line block comment: find its opening line
                int p = prevStart;
                while (p > 0)
                {
                    int pNl = p - 1;
                    int pStart = LineStartOf(s, pNl);
                    string pText = s.Substring(pStart, pNl - pStart);
                    if (pText.IndexOf("/*", StringComparison.Ordinal) >= 0)
                    {
                        if (pText.TrimStart().StartsWith("/**", StringComparison.Ordinal) &&
                            !pText.TrimStart().StartsWith("/**/", StringComparison.Ordinal))
                        {
                            docStart = pStart;
                            docLines += CountLines(s.Substring(pStart, prevStart - pStart));
                        }
                        cursor = pStart;
                        break;
                    }
                    p = pStart;
                }
                if (p == 0) break;                      // opening line not found: stop
                continue;
            }
            break;                                      // real code above: comment block ends
        }

        info.DocCommentStart = docStart;
        info.DocCommentLines = docLines;
        return info;
    }
}

