# Intentum Snippets

Code snippets for [Intentum](https://github.com/keremvaris/Intentum) intent-driven development.

## Installation

### From VSIX

1. Build the VSIX package:
   ```bash
   cd extensions/vscode-intentum
   npm install -g @vscode/vsce
   vsce package
   ```

2. Install in VS Code:
   - Open VS Code
   - Press `Cmd+Shift+P` (Mac) or `Ctrl+Shift+P` (Windows/Linux)
   - Type "Extensions: Install from VSIX"
   - Select the `intentum-snippets-0.0.1.vsix` file

### From Source

1. Clone this repository
2. Open the `extensions/vscode-intentum` folder in VS Code
3. Press `F5` to launch the Extension Development Host

## Snippets

| Prefix | Description |
|--------|-------------|
| `intent-space` | Create a new BehaviorSpace with observed events |
| `intent-policy` | Create a new IntentPolicy with rules |
| `intent-model` | Implement IIntentModel interface |
| `intent-test` | Create an Intentum test method |
| `intent-confidence` | Create IntentConfidence from score |
| `intent-rule-based` | Create a RuleBasedIntentModel |
| `intent-catalog` | Create an IntentCatalog with definitions |
| `intent-event` | Create a new BehaviorEvent |

## Usage

Type the prefix in a C# file and press Tab to expand the snippet. Use Tab stops (numbered placeholders) to fill in the details.

Example:
1. Type `intent-space` and press Tab
2. Fill in the actor and action values
3. Press Tab to move to the next placeholder

## License

MIT
