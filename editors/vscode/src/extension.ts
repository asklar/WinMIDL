import * as vscode from 'vscode';
import { findServerBinary, startLanguageServer, stopLanguageServer, getClient } from './server';
import { WinMdContentProvider } from './winmdProvider';
import { createWinMdDefinitionProvider } from './winmdDefinition';

let diagramPanel: vscode.WebviewPanel | undefined;

export function activate(context: vscode.ExtensionContext) {
    console.log('MIDL Language Support activated');

    const outputChannel = vscode.window.createOutputChannel('MIDL Language Server');

    // Commands
    context.subscriptions.push(
        vscode.commands.registerCommand('midl.toggleStrictMode', async () => {
            const config = vscode.workspace.getConfiguration('midl');
            const current = config.get<boolean>('strictMode', true);
            await config.update('strictMode', !current, vscode.ConfigurationTarget.Workspace);
            vscode.window.showInformationMessage(`MIDL: Switched to ${!current ? 'Strict MIDL3' : 'Compatibility'} mode`);
        }),

        vscode.commands.registerCommand('midl.restartServer', async () => {
            const client = getClient();
            if (client) {
                vscode.window.showInformationMessage('MIDL: Restarting language server...');
                await client.stop();
                await client.start();
                vscode.window.showInformationMessage('MIDL: Language server restarted');
            } else {
                vscode.window.showWarningMessage('MIDL: No language server running');
            }
        }),

        vscode.commands.registerCommand('midl.showServerLog', () => {
            outputChannel.show();
        }),

        vscode.commands.registerCommand('midl.selectSdkVersion', async () => {
            // Discover installed SDK versions by scanning UnionMetadata
            const sdkRoot = 'C:\\Program Files (x86)\\Windows Kits\\10\\UnionMetadata';
            const fs = await import('fs');
            const path = await import('path');
            let versions: string[] = [];
            try {
                const dirs = fs.readdirSync(sdkRoot, { withFileTypes: true });
                versions = dirs
                    .filter(d => d.isDirectory() && d.name !== 'Facade')
                    .filter(d => fs.existsSync(path.join(sdkRoot, d.name, 'Windows.winmd')))
                    .map(d => d.name)
                    .sort()
                    .reverse();
            } catch { }

            if (versions.length === 0) {
                vscode.window.showWarningMessage('MIDL: No Windows SDK installations found.');
                return;
            }

            const items: vscode.QuickPickItem[] = [
                { label: 'latest', description: `Auto-detect newest (${versions[0]})` },
                ...versions.map(v => ({ label: v, description: `Windows SDK ${v}` })),
                { label: 'none', description: 'Do not reference any Windows SDK' }
            ];

            const selected = await vscode.window.showQuickPick(items, {
                title: 'Select Windows SDK Version',
                placeHolder: 'Choose which SDK version to use for type completion and F12'
            });

            if (selected) {
                const config = vscode.workspace.getConfiguration('midl');
                if (selected.label === 'none') {
                    await config.update('useWindowsSdk', false, vscode.ConfigurationTarget.Global);
                } else {
                    await config.update('useWindowsSdk', true, vscode.ConfigurationTarget.Global);
                    await config.update('windowsSdkVersion', selected.label, vscode.ConfigurationTarget.Global);
                }
                vscode.window.showInformationMessage(
                    `MIDL: SDK set to ${selected.label}. Restart the language server to apply.`,
                    'Restart Server'
                ).then(choice => {
                    if (choice === 'Restart Server') {
                        vscode.commands.executeCommand('midl.restartServer');
                    }
                });
            }
        }),

        vscode.commands.registerCommand('midl.showClassDiagram', async () => {
            const editor = vscode.window.activeTextEditor;
            if (!editor || editor.document.languageId !== 'midl') {
                vscode.window.showInformationMessage('Open an IDL file first.');
                return;
            }
            const client = getClient();
            if (!client) {
                vscode.window.showWarningMessage('MIDL: Language server not running.');
                return;
            }

            const result = await client.sendRequest('midl/classDiagram', {
                uri: editor.document.uri.toString()
            });

            if (result && (result as any).content) {
                const markdown = (result as any).content as string;
                // Extract raw Mermaid source from the markdown code fence
                const match = markdown.match(/```mermaid\n([\s\S]*?)```/);
                const mermaidSource = match ? match[1].trim() : '';
                if (!mermaidSource) { return; }

                if (diagramPanel) {
                    diagramPanel.webview.html = getClassDiagramHtml(mermaidSource);
                    diagramPanel.reveal(vscode.ViewColumn.Beside);
                } else {
                    diagramPanel = vscode.window.createWebviewPanel(
                        'midlClassDiagram', 'Class Diagram',
                        vscode.ViewColumn.Beside,
                        { enableScripts: true, retainContextWhenHidden: true }
                    );
                    diagramPanel.webview.html = getClassDiagramHtml(mermaidSource);
                    diagramPanel.onDidDispose(() => { diagramPanel = undefined; });
                }
            }
        }),

        vscode.commands.registerCommand('midl.openDocs', async (...args: any[]) => {
            // Args come from command URI: may be a single URL string or parsed from JSON array
            let url: string | undefined;
            if (args.length > 0) {
                const first = args[0];
                if (typeof first === 'string') {
                    url = first;
                } else if (first && typeof first === 'object' && first.toString) {
                    url = first.toString();
                }
            }
            console.log('MIDL openDocs:', JSON.stringify(args), '→ url:', url);
            if (!url) return;

            const config = vscode.workspace.getConfiguration('midl');
            const inline = config.get<boolean>('inlineDocsBrowser', true);
            if (inline) {
                // Open in a side column so the IDL editor stays visible
                await vscode.commands.executeCommand('simpleBrowser.show', url);
                // Move the browser tab to the side
                await vscode.commands.executeCommand('workbench.action.moveEditorToNextGroup');
            } else {
                vscode.env.openExternal(vscode.Uri.parse(url));
            }
        })
    );

    try {
        const serverPath = findServerBinary(context);

        // WinMD decompiled view content provider
        context.subscriptions.push(
            vscode.workspace.registerTextDocumentContentProvider('midl-winmd', new WinMdContentProvider())
        );

        // F12 within decompiled WinMD views
        context.subscriptions.push(
            vscode.languages.registerDefinitionProvider(
                { scheme: 'midl-winmd' },
                createWinMdDefinitionProvider(() => getClient())
            )
        );

        if (serverPath) {
            console.log(`MIDL: Starting language server: ${serverPath}`);
            startLanguageServer(serverPath, context, outputChannel);

            // Send settings to server when they change (no restart needed)
            context.subscriptions.push(
                vscode.workspace.onDidChangeConfiguration(e => {
                    if (e.affectsConfiguration('midl')) {
                        const client = getClient();
                        if (client) {
                            const config = vscode.workspace.getConfiguration('midl');
                            client.sendNotification('workspace/didChangeConfiguration', {
                                settings: {
                                    'midl.strictMode': config.get<boolean>('strictMode', true),
                                    'midl.codeQualityHints': config.get<boolean>('codeQualityHints', true),
                                    'midl.useWindowsSdk': config.get<boolean>('useWindowsSdk', true),
                                    'midl.flatSdkTypeCompletions': config.get<boolean>('flatSdkTypeCompletions', false),
                                    'midl.arrays':config.get<boolean>('arrays', false),
                                    'midl.defines': config.get<string[]>('defines', []),
                                }
                            });
                        }
                    }
                })
            );
        } else {
            console.log('MIDL: Language server binary not found.');
            vscode.window.showInformationMessage(
                'MIDL: Language server not found. Run "dotnet build" in the repo root, or set "midl.serverPath". Syntax highlighting is still active.',
                'Open Settings'
            ).then(selection => {
                if (selection === 'Open Settings') {
                    vscode.commands.executeCommand('workbench.action.openSettings', 'midl.serverPath');
                }
            });
        }
    } catch (err) {
        console.error('MIDL: Failed to start language server:', err);
    }
}

export function deactivate(): Thenable<void> | undefined {
    return stopLanguageServer();
}

function getNonce(): string {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let result = '';
    for (let i = 0; i < 32; i++) {
        result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return result;
}

function getClassDiagramHtml(mermaidSource: string): string {
    const nonce = getNonce();
    const isDark = vscode.window.activeColorTheme.kind === vscode.ColorThemeKind.Dark
        || vscode.window.activeColorTheme.kind === vscode.ColorThemeKind.HighContrast;
    const theme = isDark ? 'dark' : 'default';

    return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta http-equiv="Content-Security-Policy"
          content="default-src 'none'; script-src 'nonce-${nonce}' https://cdn.jsdelivr.net; style-src 'unsafe-inline';">
    <style>
        body {
            background: var(--vscode-editor-background);
            color: var(--vscode-editor-foreground);
            margin: 0;
            padding: 20px;
            overflow: auto;
        }
        #diagram svg {
            max-width: none !important;
        }
    </style>
</head>
<body>
    <div id="diagram"></div>
    <script src="https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.min.js" nonce="${nonce}"></script>
    <script nonce="${nonce}">
        mermaid.initialize({ startOnLoad: false, theme: '${theme}' });
        (async () => {
            const { svg } = await mermaid.render('class-diagram', ${JSON.stringify(mermaidSource)});
            document.getElementById('diagram').innerHTML = svg;
        })();
    </script>
</body>
</html>`;
}
