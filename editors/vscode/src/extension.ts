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
            try {
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
                }) as any;

                if (result?.dot) {
                    const webviewHtml = getDiagramHtml(result.dot as string);
                    if (diagramPanel) {
                        diagramPanel.webview.html = webviewHtml;
                        diagramPanel.reveal(vscode.ViewColumn.Beside);
                    } else {
                        diagramPanel = vscode.window.createWebviewPanel(
                            'midlClassDiagram', 'Class Diagram',
                            vscode.ViewColumn.Beside,
                            { enableScripts: true, retainContextWhenHidden: true }
                        );
                        diagramPanel.webview.html = webviewHtml;
                        diagramPanel.onDidDispose(() => { diagramPanel = undefined; });
                    }
                } else {
                    vscode.window.showWarningMessage('MIDL: Server returned no diagram.');
                }
            } catch (err: any) {
                vscode.window.showErrorMessage(`MIDL Class Diagram: ${err.message || err}`);
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

function getDiagramHtml(dotSource: string): string {
    const nonce = getNonce();
    return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta http-equiv="Content-Security-Policy"
          content="default-src 'none'; script-src 'nonce-${nonce}' https://cdn.jsdelivr.net 'wasm-unsafe-eval'; style-src 'unsafe-inline'; img-src blob:;">
    <style>
        body {
            background: transparent;
            margin: 0;
            padding: 16px;
            overflow: auto;
        }
        #diagram { overflow: auto; }
        #diagram svg { max-width: none !important; }
        #diagram svg > polygon { display: none; }
        #loading { color: var(--vscode-editor-foreground, #ccc); font-family: var(--vscode-font-family); }
        #error { color: #f48771; font-family: var(--vscode-font-family); white-space: pre-wrap; }
        #ctx-menu {
            position: fixed; z-index: 9999;
            background: var(--vscode-menu-background, #2d2d2d);
            border: 1px solid var(--vscode-menu-border, #555);
            border-radius: 4px; padding: 4px 0;
            box-shadow: 0 2px 8px rgba(0,0,0,.4);
        }
        #ctx-menu div {
            padding: 6px 24px; cursor: pointer;
            color: var(--vscode-menu-foreground, #ccc);
            font: 13px var(--vscode-font-family, 'Segoe UI', sans-serif);
        }
        #ctx-menu div:hover { background: var(--vscode-menu-selectionBackground, #094771); }
        #toast {
            position: fixed; bottom: 16px; left: 50%; transform: translateX(-50%);
            background: var(--vscode-editorWidget-background, #333); color: var(--vscode-editor-foreground, #ccc);
            font: 13px var(--vscode-font-family, 'Segoe UI', sans-serif);
            padding: 6px 16px; border-radius: 4px; opacity: 0; transition: opacity .3s;
            pointer-events: none;
        }
    </style>
</head>
<body>
    <div id="diagram"><p id="loading">Rendering diagram\u2026</p></div>
    <div id="toast"></div>
    <script type="module" nonce="${nonce}">
        try {
            const { instance } = await import('https://cdn.jsdelivr.net/npm/@viz-js/viz@3.11.0/lib/viz-standalone.mjs');
            const viz = await instance();
            const svg = viz.renderString(${JSON.stringify(dotSource)}, { format: 'svg' });
            document.getElementById('diagram').innerHTML = svg;
        } catch (e) {
            document.getElementById('diagram').innerHTML =
                '<p id="error">Failed to render diagram:\\\\n' + e.message + '</p>';
        }

        document.addEventListener('contextmenu', e => {
            e.preventDefault();
            document.getElementById('ctx-menu')?.remove();
            const menu = document.createElement('div');
            menu.id = 'ctx-menu';
            menu.style.left = e.clientX + 'px';
            menu.style.top = e.clientY + 'px';
            const item = document.createElement('div');
            item.textContent = 'Copy as PNG';
            item.onclick = () => { menu.remove(); copyAsPng(); };
            menu.appendChild(item);
            document.body.appendChild(menu);
            setTimeout(() => document.addEventListener('click', () => menu.remove(), { once: true }), 0);
        });

        function toast(msg) {
            const el = document.getElementById('toast');
            el.textContent = msg; el.style.opacity = '1';
            setTimeout(() => el.style.opacity = '0', 1500);
        }

        async function copyAsPng() {
            const svgEl = document.querySelector('#diagram svg');
            if (!svgEl) return;
            const clone = svgEl.cloneNode(true);
            const bg = clone.querySelector(':scope > polygon');
            if (bg) bg.remove();
            const data = new XMLSerializer().serializeToString(clone);
            const blob = new Blob([data], { type: 'image/svg+xml;charset=utf-8' });
            const url = URL.createObjectURL(blob);
            const img = new Image();
            img.onload = async () => {
                const c = document.createElement('canvas');
                c.width = img.naturalWidth * 2;
                c.height = img.naturalHeight * 2;
                const ctx = c.getContext('2d');
                ctx.scale(2, 2);
                ctx.drawImage(img, 0, 0);
                URL.revokeObjectURL(url);
                c.toBlob(async b => {
                    try {
                        await navigator.clipboard.write([new ClipboardItem({ 'image/png': b })]);
                        toast('Copied to clipboard');
                    } catch (e) { toast('Copy failed: ' + e.message); }
                }, 'image/png');
            };
            img.src = url;
        }
    </script>
</body>
</html>`;
}
