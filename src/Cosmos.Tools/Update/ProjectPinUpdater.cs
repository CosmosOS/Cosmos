using System.Text.RegularExpressions;

namespace Cosmos.Tools.Update;

/// <summary>
/// Rewrites the Cosmos version pins a kernel project carries. Regex-based text
/// replacement rather than XML/JSON round-tripping so user formatting survives.
/// Covers every pin syntax the SDK supports:
/// <code>
///   &lt;Project Sdk="Cosmos.Sdk/3.0.71"&gt;                          (template output)
///   &lt;Sdk Name="Cosmos.Sdk" Version="3.0.71" /&gt;                  (element form)
///   "msbuild-sdks": { "Cosmos.Sdk": "3.0.71" }                  (global.json)
///   &lt;PackageReference Include="Cosmos.*" Version="3.0.71" /&gt;    (and CPM PackageVersion)
/// </code>
/// The pins must move as one set: the Sdk pin controls the versions of every
/// transitive Cosmos package via the baked Sdk.props, so a partial bump restores
/// a mixed-version graph.
/// </summary>
public static class ProjectPinUpdater
{
    private static readonly Regex s_sdkAttribute = new Regex(
        "(?<pre>\\bSdk\\s*=\\s*\"Cosmos\\.Sdk/)(?<ver>[^\"]+)(?=\")",
        RegexOptions.Compiled);

    private static readonly Regex s_sdkElement = new Regex(
        "(?<pre><Sdk\\b(?=[^>]*\\bName\\s*=\\s*\"Cosmos\\.Sdk\")[^>]*?\\bVersion\\s*=\\s*\")(?<ver>[^\"]+)(?=\")",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex s_packagePin = new Regex(
        "(?<pre><(?:PackageReference|PackageVersion)\\b(?=[^>]*\\bInclude\\s*=\\s*\"Cosmos\\.[^\"]*\")[^>]*?\\bVersion\\s*=\\s*\")(?<ver>[^\"]+)(?=\")",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex s_globalJsonSdk = new Regex(
        "(?<pre>\"Cosmos\\.Sdk\"\\s*:\\s*\")(?<ver>[^\"]+)(?=\")",
        RegexOptions.Compiled);

    /// <param name="NewContent">File content with every Cosmos pin set to the target version.</param>
    /// <param name="PinCount">Total Cosmos pins found in the file.</param>
    /// <param name="ChangedCount">Pins whose value actually changed.</param>
    /// <param name="PreviousVersions">Distinct versions the file pinned before the edit.</param>
    public sealed record PinEdit(string NewContent, int PinCount, int ChangedCount, IReadOnlyList<string> PreviousVersions);

    /// <summary>
    /// Finds the files under <paramref name="rootDir"/> that carry Cosmos version
    /// pins: *.csproj and *.vbproj, Directory.Packages.props (CPM), and global.json
    /// (msbuild-sdks form). Build output is skipped and recursion is depth-capped
    /// so running from an unexpectedly broad directory stays cheap.
    /// </summary>
    public static List<string> FindPinFiles(string rootDir)
    {
        EnumerationOptions options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            MaxRecursionDepth = 2,
            IgnoreInaccessible = true
        };

        List<string> files = new List<string>();
        foreach (string pattern in (string[])["*.csproj", "*.vbproj", "Directory.Packages.props", "global.json"])
        {
            foreach (string file in Directory.EnumerateFiles(rootDir, pattern, options))
            {
                if (IsInBuildOutput(rootDir, file))
                {
                    continue;
                }

                string content;
                try
                {
                    content = File.ReadAllText(file);
                }
                catch
                {
                    continue;
                }

                if (SelectRegexes(file).Any(regex => regex.IsMatch(content)))
                {
                    files.Add(file);
                }
            }
        }

        files.Sort(StringComparer.Ordinal);
        return files;
    }

    /// <summary>
    /// Computes the rewritten content of one pin file. Pure — does not touch disk.
    /// Pins inside XML comments and non-literal pins (MSBuild properties like
    /// $(VersionPrefix), pack-time tokens like @CosmosPackageVersion@) are left
    /// untouched and not counted.
    /// </summary>
    public static PinEdit ComputeEdit(string filePath, string content, string newVersion)
    {
        bool isXml = !Path.GetFileName(filePath).Equals("global.json", StringComparison.OrdinalIgnoreCase);
        List<(int Start, int End)> commentSpans = isXml ? FindCommentSpans(content) : [];

        int pins = 0;
        int changed = 0;
        HashSet<string> previous = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        string result = content;
        foreach (Regex regex in SelectRegexes(filePath))
        {
            // Span positions are relative to the string a Replace pass runs on;
            // a previous rule's replacements shift them, so recompute.
            if (isXml && !ReferenceEquals(result, content))
            {
                commentSpans = FindCommentSpans(result);
            }

            result = regex.Replace(result, match =>
            {
                string oldVersion = match.Groups["ver"].Value;
                if (IsInsideSpan(commentSpans, match.Index)
                    || oldVersion.Contains("$(")
                    || oldVersion.Contains('@'))
                {
                    return match.Value;
                }

                pins++;
                previous.Add(oldVersion);
                if (!string.Equals(oldVersion, newVersion, StringComparison.OrdinalIgnoreCase))
                {
                    changed++;
                }

                return match.Groups["pre"].Value + newVersion;
            });
        }

        return new PinEdit(result, pins, changed, previous.ToList());
    }

    private static List<(int Start, int End)> FindCommentSpans(string content)
    {
        List<(int Start, int End)> spans = [];
        int index = 0;
        while ((index = content.IndexOf("<!--", index, StringComparison.Ordinal)) >= 0)
        {
            int end = content.IndexOf("-->", index + 4, StringComparison.Ordinal);
            if (end < 0)
            {
                spans.Add((index, content.Length));
                break;
            }

            spans.Add((index, end + 3));
            index = end + 3;
        }

        return spans;
    }

    private static bool IsInsideSpan(List<(int Start, int End)> spans, int index)
    {
        foreach ((int start, int end) in spans)
        {
            if (index >= start && index < end)
            {
                return true;
            }
        }

        return false;
    }

    private static Regex[] SelectRegexes(string filePath)
    {
        return Path.GetFileName(filePath).Equals("global.json", StringComparison.OrdinalIgnoreCase)
            ? [s_globalJsonSdk]
            : [s_sdkAttribute, s_sdkElement, s_packagePin];
    }

    private static bool IsInBuildOutput(string rootDir, string file)
    {
        string relative = Path.GetRelativePath(rootDir, file);
        foreach (string segment in relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            if (segment is "bin" or "obj" or ".git"
                || segment.StartsWith("output-", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
