# Comment To Tooltip

> This is the public repository of the **Comment To Tooltip** asset for Unity: https://assetstore.unity.com/packages/tools/utilities/comment-to-tooltip-120659

## Overview
**Comment To Tooltip** automatically generates new tooltips, or updates existing ones, directly from your code comments.

## Usage

The **Comment To Tooltip** menu is available under **Tools → ToolBuddy** in the Unity menu bar.  
It provides the following options:  
- **Process a file**: Processes a single `.cs` file.  
- **Process a folder**: Recursively processes all `.cs` files in a folder.  
- **Preferences**: Choose which types of comments to process. The more options you select, the longer the processing time will be.  
- **Help**: Opens [ReadMe.txt](Assets/Plugins/ToolBuddy/Assets/CommentToTooltip/ReadMe.txt)

## Supported Comment Types

- **Single-line documentation (`///`)**: Extracts text from `<summary>…</summary>` blocks.  
- **Delimited documentation (`/* … */`)**: Extracts text from `<summary>…</summary>` blocks.  
- **Single-line comments (`//`)**: Uses contiguous lines of comments immediately above a field.  

## How It Works

### Regular expressions

Regular expressions are used to capture:  
- documentation lines  
- any existing `Tooltip` attribute  
- the public field declaration line  

The code then updates an existing tooltip or inserts a new one if none is present.

### Dependency Injection

[VContainer](https://vcontainer.hadashikick.jp/) is used for DI. The composition root is defined in [EditorCompositionRoot](Assets/Plugins/ToolBuddy/Assets/CommentToTooltip/Editor/EditorCompositionRoot.cs).

## Known Limitations

- The code relies on regex for parsing. Although relatively robust, it is not as accurate as a full C# parser such as [Roslyn](https://github.com/dotnet/roslyn).  
- This design choice was made to support single-line comments (`//`).  
- In the future, a hybrid approach may be implemented: using Roslyn for documentation comments, and regex for single-line comments.  

## Testing

- Test data is located under:  
  `Assets/Plugins/ToolBuddy/Assets/CommentToTooltip/Tests/TestData/`  
  Each test includes an **input** file (`.input.cs.txt`) and an **expected** output file (`.expected.cs.txt`).  
- Run tests via the Unity Test Runner under the `ToolBuddy.CommentToTooltip.Tests` assembly.  

## License

See [LICENSE.md](LICENSE.md)

## Changelog

See [CHANGELOG.md](Assets/Plugins/ToolBuddy/Assets/CommentToTooltip/CHANGELOG.md)
