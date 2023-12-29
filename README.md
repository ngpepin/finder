
# Finder application for Linux

## Overview
Finder is a powerful and flexible console application for searching files and directories in a specified path. 
It supports regular expressions, wildcard patterns, fuzzy search, and can also search within the content of text files 
(note: that it has only been tested with Ubuntu).

## Features
- **Pattern Matching**: Supports regular expressions and wildcards for matching file names.
- **Fuzzy Search**: Implements fuzzy search using the Levenshtein distance algorithm.
- **Content Search**: Ability to search within the content of text files.
- **Customizable Exclusions**: Excludes specific paths in the search with the option to include exceptions.

## Usage
```bash
finder [options] [directory] [pattern]
````

* `[directory]`: The starting directory for the search (defaults to the current directory if not specified).
* `[pattern]`: The pattern to search for (supports regular expressions and wildcards).

### Options

* `-c`, `--content`: Search within the content of text files as well as their names.
* `-f`, `--fuzzy`: Enable fuzzy search for approximately similar matches.
* `-h`, `--help`: Display help information about the command usage and options.

### Examples

    finder . "*.txt"                  # Finds all .txt files in the current directory and subdirectories.
    finder /usr "config" -c           # Searches for files named 'config' and files containing 'config' in /usr.
    finder -f "readme"                # Performs a fuzzy search for files/directories similar to 'readme'.
    finder / "log*.txt" -c -f         # Fuzzy searches for .log files in the root directory and checks their content.

## Installation

Compile and deploy as a single-file standalone console app with .NET 8.

## Contributing

Contributions to the Finder project are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) for how to contribute.

## License

This project is licensed under the [MIT License](LICENSE).

## Acknowledgments
