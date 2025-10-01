# Comment To Tooltip

> This is the public repository of the Comment To Tooltip asset for Unity: https://assetstore.unity.com/packages/tools/utilities/comment-to-tooltip-120659

## Overview
Comment To Tooltip automatically generates new tooltips, or updates existing ones, directly from your existing code comments.

## USAGE

The Comment To Tooltip menu is available under Tools -> ToolBuddy in the Unity menu bar.  
It includes the following options:  
  * Process a file: Processes a single .cs file.  
  * Process a folder: Recursively processes all .cs files in a folder.  
  * Preferences: Choose which types of comments to process. The more options you select, the longer the processing time will be.  
  * Help: Opens [ReadMe.txt](Assets/Plugins/ToolBuddy/Assets/CommentToTooltip/ReadMe.txt)

## Supported Comment Types

- Single-line documentation (///): Extracts text from \<summary\>…\</summary\> blocks.
- Delimited documentation (/* … */): Extracts text from \<summary\>…</summary\> blocks.
- Single-line comments (//): Uses contiguous lines of comments immediately above a field.

## How It Works (Technical Summary)

Regular expressions are used to capture:
- documentation lines
- an existing Tooltip attribute if present
- the public field declaration line

The code then updates existing tooltip or inserts a new one if none existing. Tooltip strings are escaped so that embedded quotes, slashes, and newline/tab characters produce valid C# string literals.

## Known Limitations

- The code relies on regexes to do the parsing. Although relatively robust, it is not a as good as a C# parser such as [Roslyn](https://github.com/dotnet/roslyn). This choice was done to support single-line comments (//). In the future, I might implement a hybrid appraoch: Using Roslyn for single-line/delimited documentation, and using regexes for single-line comments.

## Testing

- Test data lives under:
  Assets/Plugins/ToolBuddy/Assets/CommentToTooltip/Tests/TestData/
  Each test has an input (.input.cs.txt) and expected (.expected.cs.txt).
- Run tests via Unity Test Runner under the ToolBuddy.CommentToTooltip.Tests assembly.

## LICENSE

See [LICENSE.md](LICENSE.md)

## Changelog

See [CHANGELOG.md](Assets/Plugins/ToolBuddy/Assets/CommentToTooltip/CHANGELOG.md)
