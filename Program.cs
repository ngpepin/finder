/*
 * Finder Console Application
 * 
 * Description:
 *     This console application replicates the functionality of a Unix-like 'find' command 
 *     with enhanced features. It allows users to search for files and directories within a 
 *     specified path based on a pattern. It supports regular expressions, wildcard patterns, 
 *     and fuzzy search. Additionally, it can search within the content of text files.
 *    
 *     The application provides the functionality to exclude specific paths in the search 
 *     and to include exceptions to these exclusions. By default, it excludes '/mnt' 
 *     except for '/mnt/drive2'. This behavior can be customized in the code.
 * 
 * Usage:
 *     finder [options] [directory] [pattern]
 *
 *     [directory] - The starting directory for the search. Defaults to the current directory if not specified.
 *     [pattern]   - The pattern to search for. Supports regular expressions and wildcards. Defaults to '*' (all files).
 *
 * Options:
 *     -c, --content  : Search within the content of text files as well as their names.
 *     -f, --fuzzy    : Enable fuzzy search. Matches files/directories that are approximately similar to the pattern.
 *     -h, --help     : Display help information about the command usage and options.  
 * 
 * Examples:
 *     finder . "*.txt"          : Finds all .txt files in the current directory and its subdirectories.
 *     finder /usr "config" -c   : Searches for files named 'config' and files containing 'config' in their content within /usr.
 *     finder -f "readme"        : Performs a fuzzy search for files/directories similar to 'readme' in the current directory.
 *
 * Note:
 *     The fuzzy search option (-f) uses the Levenshtein distance algorithm to find matches
 *     that are similar to the specified pattern. The sensitivity of the fuzzy search can be 
 *     adjusted in the code.
 * 
 * Author:
 *     Nicolas Pepin
 * 
 * Date:
 *     2023
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class Program
{
    static readonly string[] DefaultExclusions = new[] { "/mnt" };
    static readonly string[] ExclusionExceptions = new[] { "/mnt/drive2" };

    static void Main(string[] args)
    {
        bool searchContent = false;
        bool fuzzySearch = false;
        int fuzzyThreshold = 4; // adjust this threshold
        string pattern = "*";
        string startDir = Directory.GetCurrentDirectory();
        string originalPattern = pattern; // Store the original pattern

        // Parse arguments
        foreach (var arg in args)
        {
            if (arg == "-c" || arg == "--content")
                searchContent = true;
            else if (arg == "-f" || arg == "--fuzzy")
                fuzzySearch = true;
            else if (arg == "-h" || arg == "--help")
            {
                PrintHelp();
                return;
            }
            else if (Directory.Exists(arg)) // Check if the argument is a directory
                startDir = arg;
            else
                pattern = arg; // Treat the argument as a pattern
        }

        var regexPattern = ConvertWildcardToRegex(pattern);
        var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

        TraverseDirectory(startDir, regex, searchContent, fuzzySearch, fuzzyThreshold, startDir, originalPattern);
    }
  
    static void TraverseDirectory(string directory, Regex pattern, bool searchContent, bool fuzzySearch, int fuzzyThreshold, string startDir, string originalPattern)
    {
        // Debug: remove to debug traversal: Console.WriteLine($"Searching in: {directory}"); // Debug: Show current directory

        if (ShouldExclude(directory, startDir))
        {
            Console.WriteLine($"Excluded: {directory}"); // Show excluded directory
            return;
        }

        try
        {
            var files = Directory.EnumerateFiles(directory);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                if (fuzzySearch)
                {
                    int distance = LevenshteinDistance(fileName, originalPattern);
                    if (distance <= fuzzyThreshold)
                    {
                        Console.WriteLine($"{file} (fuzzy match, distance: {distance})");
                    }
                }
                else if (pattern.IsMatch(fileName))
                {
                    Console.WriteLine(file);
                }

                if (searchContent && IsTextFile(file))
                {
                    var fileContent = File.ReadAllLines(file);
                    if (fileContent.Any(line => fuzzySearch ?
                        LevenshteinDistance(line, originalPattern) <= fuzzyThreshold :
                        pattern.IsMatch(line)))
                    {
                        Console.WriteLine(file + " (content match)");
                    }
                }

            }

            var directories = Directory.EnumerateDirectories(directory);
            foreach (var dir in directories)
            {
                TraverseDirectory(dir, pattern, searchContent, fuzzySearch, fuzzyThreshold, startDir, originalPattern);
            }
        }
        catch (UnauthorizedAccessException e)
        {
            Console.WriteLine($"Access denied: {e.Message}"); // Show access denied errors
        }
    }

    static bool IsMatch(string text, Regex pattern, bool fuzzySearch, int fuzzyThreshold)
    {
        return fuzzySearch ?
            LevenshteinDistance(text, pattern.ToString()) <= fuzzyThreshold :
            pattern.IsMatch(text);
    }

    static int LevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a))
            return string.IsNullOrEmpty(b) ? 0 : b.Length;
        if (string.IsNullOrEmpty(b))
            return a.Length;

        int lengthA = a.Length;
        int lengthB = b.Length;
        var distances = new int[lengthA + 1, lengthB + 1];

        for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
        for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

        for (int i = 1; i <= lengthA; i++)
            for (int j = 1; j <= lengthB; j++)
            {
                int cost = (b[j - 1] == a[i - 1]) ? 0 : 1;
                distances[i, j] = Math.Min(
                    Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                    distances[i - 1, j - 1] + cost);
            }
        return distances[lengthA, lengthB];
    }
    
    static bool ShouldExclude(string directory, string startDir)
    {
        // If the start directory is within an excluded path, ignore the exclusion
        if (directory.StartsWith(startDir))
            return false;

        return DefaultExclusions.Any(excl => directory.StartsWith(excl)) &&
               !ExclusionExceptions.Any(excpt => directory.StartsWith(excpt));
    }

    static string ConvertWildcardToRegex(string pattern)
    {
        return "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
    }

    static bool IsTextFile(string path)
    {
        var fileType = Path.GetExtension(path).ToLower();
        var textFileTypes = new[] { ".txt", ".log", ".xml", ".json", ".cs", ".md" }; // Extend as needed
        return textFileTypes.Contains(fileType);
    }

    static void PrintHelp()
    {
        Console.WriteLine("Usage: finder [options] [directory] [pattern]");
        Console.WriteLine();
        Console.WriteLine("This tool recursively searches for files and directories based on the specified pattern.");
        Console.WriteLine("If a directory is not specified, the search starts from the current directory.");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -c, --content   Search within the content of text files as well as their names.");
        Console.WriteLine("  -f, --fuzzy     Enable fuzzy search. Matches files/directories approximately similar to the pattern.");
        Console.WriteLine("                  Fuzzy search uses the Levenshtein distance algorithm and is not case-sensitive.");
        Console.WriteLine("  -h, --help      Display this help message.");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  finder . \"*.txt\"                : Finds all .txt files in the current directory and subdirectories.");
        Console.WriteLine("  finder /usr \"config\" -c         : Searches for files named 'config' and files containing 'config' in /usr.");
        Console.WriteLine("  finder -f \"readme\"              : Performs a fuzzy search for files/directories similar to 'readme'.");
        Console.WriteLine("  finder / \"*.log\" -c -f          : Fuzzy searches for .log files in the root directory and checks their content.");
        Console.WriteLine();
        Console.WriteLine("Note:");
        Console.WriteLine("  The fuzzy search threshold can be adjusted in the code for different levels of matching sensitivity.");
        Console.WriteLine("  Be aware that fuzzy search can be computationally intensive, especially for large directories.");
    }
}
