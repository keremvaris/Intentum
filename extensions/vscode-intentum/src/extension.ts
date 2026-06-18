import * as vscode from 'vscode';
import * as fs from 'node:fs';
import * as path from 'node:path';

export function activate(context: vscode.ExtensionContext) {
    console.log('Intentum VS Code extension activated');

    const disposable = vscode.commands.registerCommand('intentum.explorePackage', async () => {
        const uris = await vscode.window.showOpenDialog({
            canSelectFiles: true,
            canSelectFolders: false,
            canSelectMany: false,
            filters: { 'NuGet Packages': ['nupkg'] }
        });

        if (!uris || uris.length === 0) return;

        const nupkgPath = uris[0].fsPath;

        const tempDir = path.join(context.globalStorageUri.fsPath, 'temp-explorer');
        if (!fs.existsSync(tempDir)) {
            fs.mkdirSync(tempDir, { recursive: true });
        }

        const result = await vscode.window.showInformationMessage(
            `Selected package: ${path.basename(nupkgPath)}`,
            'View Contents',
            'Extract'
        );

        if (result === 'View Contents') {
            // Show package info
            const panel = vscode.window.createWebviewPanel(
                'intentumPackageExplorer',
                `Package: ${path.basename(nupkgPath)}`,
                vscode.ViewColumn.One,
                { enableScripts: false }
            );

            panel.webview.html = `<!DOCTYPE html>
<html>
<head><meta charset="utf-8"></head>
<body>
    <h1>NuGet Package Explorer</h1>
    <p><strong>File:</strong> ${path.basename(nupkgPath)}</p>
    <p><strong>Size:</strong> ${(fs.statSync(nupkgPath).size / 1024).toFixed(1)} KB</p>
    <h2>Contents</h2>
    <p>To view contents, extract the package using the "Extract" action.</p>
    <p>The .nupkg file is a ZIP archive containing:</p>
    <ul>
        <li><code>_rels/</code> - Relationship files</li>
        <li><code>package/</code> - Package metadata</li>
        <li><code>lib/</code> - Compiled assemblies</li>
        <li><code>content/</code> - Content files</li>
    </ul>
</body>
</html>`;
        } else if (result === 'Extract') {
            const destDir = await vscode.window.showOpenDialog({
                canSelectFiles: false,
                canSelectFolders: true,
                canSelectMany: false,
                openLabel: 'Select Extraction Directory'
            });

            if (destDir) {
                const extractPath = path.join(destDir[0].fsPath, path.basename(nupkgPath, '.nupkg'));
                vscode.window.showInformationMessage(`Extract to: ${extractPath}`);
                vscode.commands.executeCommand('revealInExplorer', vscode.Uri.file(extractPath));
            }
        }
    });

    context.subscriptions.push(disposable);
}

export function deactivate(): void {
    // Cleanup if needed
}
