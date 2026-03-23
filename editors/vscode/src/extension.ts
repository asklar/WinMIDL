import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';
import { execSync } from 'child_process';
import { LanguageClient, LanguageClientOptions, ServerOptions, TransportKind } from 'vscode-languageclient/node';

let client: LanguageClient | undefined;
let serverBinaryPath: string | undefined;

/**
 * Attempt to find the MIDL CLI binary by searching known locations.
 * In dev mode, the binary is at <repoRoot>/src/Midl.Cli/bin/Debug/net10.0/Midl.Cli.exe
 * The extension lives at <repoRoot>/editors/vscode/, so repoRoot is ../../ from here.
 */
function findServerBinary(context: vscode.ExtensionContext): string | undefined {
    // 1. Explicit setting takes priority
    const config = vscode.workspace.getConfiguration('midl');
    const configured = config.get<string>('serverPath');
    if (configured && configured !== 'midl' && configured.trim() !== '') {
        return configured;
    }

    const extensionDir = context.extensionPath;

    // 2. Bundled server binary (shipped inside the VSIX, per-platform)
    const arch = process.arch === 'arm64' ? 'win-arm64' : 'win-x64';
    const bundledExe = path.join(extensionDir, 'server', arch, 'Midl.Cli.exe');
    if (fs.existsSync(bundledExe)) {
        console.log(`MIDL: Found bundled server binary at ${bundledExe}`);
        return bundledExe;
    }
    // Fallback: non-RID bundled path
    const bundledFlat = path.join(extensionDir, 'server', 'Midl.Cli.exe');
    if (fs.existsSync(bundledFlat)) {
        console.log(`MIDL: Found bundled server binary at ${bundledFlat}`);
        return bundledFlat;
    }

    // 3. Dev inner loop: look relative to repo root
    const repoRoot = path.resolve(extensionDir, '..', '..');

    const candidates = [
        path.join(repoRoot, 'src', 'Midl.Cli', 'bin', 'Debug', 'net10.0', 'Midl.Cli.exe'),
        path.join(repoRoot, 'src', 'Midl.Cli', 'bin', 'Debug', 'net10.0', 'win-x64', 'Midl.Cli.exe'),
        path.join(repoRoot, 'src', 'Midl.Cli', 'bin', 'Release', 'net10.0', 'Midl.Cli.exe'),
        path.join(repoRoot, 'src', 'Midl.Cli', 'bin', 'Release', 'net10.0', 'win-x64', 'Midl.Cli.exe'),
        path.join(repoRoot, 'publish', 'Midl.Cli.exe'),
        path.join(repoRoot, 'src', 'Midl.Cli', 'bin', 'Debug', 'net10.0', 'Midl.Cli'),
    ];

    for (const candidate of candidates) {
        if (fs.existsSync(candidate)) {
            console.log(`MIDL: Found server binary at ${candidate}`);
            return candidate;
        }
    }

    // 3. Check workspace folders (user opened a different folder but has the MIDL repo somewhere)
    const workspaceFolders = vscode.workspace.workspaceFolders;
    if (workspaceFolders) {
        for (const folder of workspaceFolders) {
            const wsCandidate = path.join(folder.uri.fsPath, 'src', 'Midl.Cli', 'bin', 'Debug', 'net10.0', 'Midl.Cli.exe');
            if (fs.existsSync(wsCandidate)) {
                console.log(`MIDL: Found server binary in workspace at ${wsCandidate}`);
                return wsCandidate;
            }
        }
    }

    return undefined;
}

/**
 * Provides read-only decompiled IDL content for WinMD files.
 * When go-to-definition targets a WinMD type, the LSP returns a midl-winmd:// URI.
 * VS Code calls this provider to get the decompiled IDL text.
 */
class WinMdContentProvider implements vscode.TextDocumentContentProvider {
    async provideTextDocumentContent(uri: vscode.Uri): Promise<string> {
        if (!serverBinaryPath) {
            return '// MIDL server binary not found — cannot decompile WinMD';
        }

        // URI format: midl-winmd:///path/to/file.winmd/Namespace.Name
        // or: midl-winmd:///path/to/file.winmd (no namespace filter)
        let fullPath = uri.path;
        // On Windows, strip leading slash from /C:/path
        if (process.platform === 'win32' && fullPath.startsWith('/')) {
            fullPath = fullPath.substring(1);
        }

        // Split path at .winmd boundary to extract namespace
        let winmdPath: string;
        let nsFilter: string | undefined;
        const winmdIdx = fullPath.toLowerCase().indexOf('.winmd');
        if (winmdIdx >= 0) {
            winmdPath = fullPath.substring(0, winmdIdx + 6); // include .winmd
            const remainder = fullPath.substring(winmdIdx + 6);
            if (remainder.startsWith('/') && remainder.length > 1) {
                nsFilter = remainder.substring(1); // strip leading /
            }
        } else {
            winmdPath = fullPath;
        }

        const nsArgs = nsFilter ? ` --namespace "${nsFilter}"` : '';

        try {
            const result = execSync(
                `"${serverBinaryPath}" --decompile "${winmdPath}"${nsArgs}`,
                { encoding: 'utf-8', timeout: 30000, maxBuffer: 50 * 1024 * 1024 }
            );
            return result;
        } catch (err: any) {
            return `// Failed to decompile ${winmdPath}\n// ${err.message || err}`;
        }
    }
}

export function activate(context: vscode.ExtensionContext) {
    console.log('MIDL Language Support activated');

    // Debug: log all document opens
    context.subscriptions.push(
        vscode.workspace.onDidOpenTextDocument(doc => {
            if (doc.uri.scheme === 'midl-winmd') {
            }
        })
    );

    // Create output channel for server logs
    const outputChannel = vscode.window.createOutputChannel('MIDL Language Server');

    // Register commands
    context.subscriptions.push(
        vscode.commands.registerCommand('midl.toggleStrictMode', async () => {
            const config = vscode.workspace.getConfiguration('midl');
            const current = config.get<boolean>('strictMode', true);
            await config.update('strictMode', !current, vscode.ConfigurationTarget.Workspace);
            const mode = !current ? 'Strict MIDL3' : 'Compatibility (MIDL2+3)';
            vscode.window.showInformationMessage(`MIDL: Switched to ${mode} mode`);
        }),

        vscode.commands.registerCommand('midl.restartServer', async () => {
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
        })
    );

    try {
        const serverPath = findServerBinary(context);
        serverBinaryPath = serverPath;

        // Register content provider for decompiled WinMD views
        const winmdProvider = new WinMdContentProvider();
        context.subscriptions.push(
            vscode.workspace.registerTextDocumentContentProvider('midl-winmd', winmdProvider)
        );

        // Register a DefinitionProvider for midl-winmd scheme documents.
        // Uses a custom LSP request (midl/decompileDefinition) — no CLI spawning.
        context.subscriptions.push(
            vscode.languages.registerDefinitionProvider(
                { scheme: 'midl-winmd' },
                {
                    async provideDefinition(document: vscode.TextDocument, position: vscode.Position): Promise<vscode.Location | null> {

                        // Get the word/FQN under cursor
                        const wordRange = document.getWordRangeAtPosition(position);
                        const word = document.getText(wordRange);
                        const lineText = document.lineAt(position.line).text;
                        let start = wordRange.start.character;
                        let end = wordRange.end.character;
                        while (start > 0 && (lineText[start - 1] === '.' || /\w/.test(lineText[start - 1]))) start--;
                        while (end < lineText.length && (lineText[end] === '.' || /\w/.test(lineText[end]))) end++;
                        const fqn = lineText.substring(start, end);
                        const typeName = fqn.includes('.') ? fqn : word;

                        // Extract WinMD path and current namespace from URI
                        const uriStr = document.uri.toString();
                        const schemeIdx = uriStr.indexOf('midl-winmd:');
                        let uriPath = decodeURIComponent(uriStr.substring(schemeIdx + 'midl-winmd:'.length)).replace(/^\/+/, '');
                        const winmdIdx = uriPath.toLowerCase().indexOf('.winmd');
                        const winmdPath = uriPath.substring(0, winmdIdx + 6);
                        const currentNs = uriPath.length > winmdIdx + 7 ? uriPath.substring(winmdIdx + 7) : null;

                        // Same namespace? Search current document text directly
                        if (!fqn.includes('.') && currentNs) {
                            const text = document.getText();
                            for (const kw of ['runtimeclass', 'interface', 'struct', 'enum', 'delegate']) {
                                const idx = text.indexOf(`${kw} ${word}`);
                                if (idx >= 0) {
                                    const pos = document.positionAt(idx + kw.length + 1);
                                    return new vscode.Location(document.uri, pos);
                                }
                            }
                        }

                        // Cross-namespace: ask the LSP server
                        try {
                            const result = await client.sendRequest('midl/decompileDefinition', {
                                winmdPath,
                                typeName,
                                currentNamespace: currentNs
                            }) as { namespace: string; text: string; line: number; character: number } | null;

                            if (!result) return null;

                            const targetUri = vscode.Uri.parse(`midl-winmd:///${winmdPath.replace(/\\/g, '/')}/${result.namespace}`);
                            const doc2 = await vscode.workspace.openTextDocument(targetUri);
                            await vscode.languages.setTextDocumentLanguage(doc2, 'midl');
                            const targetPos = new vscode.Position(result.line, result.character);
                            await vscode.window.showTextDocument(doc2, {
                                selection: new vscode.Range(targetPos, targetPos),
                                preview: false,
                                viewColumn: vscode.ViewColumn.Active
                            });
                            return undefined as any;
                        } catch {
                            return null;
                        }
                    }
                }
            )
        );

        if (serverPath) {
            console.log(`MIDL: Starting language server: ${serverPath}`);
            startLanguageServer(serverPath, context, outputChannel);
        } else {
            console.log('MIDL: Language server binary not found.');
            console.log('MIDL: Syntax highlighting, snippets, and bracket matching are still active.');
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

function startLanguageServer(serverPath: string, context: vscode.ExtensionContext, outputChannel: vscode.OutputChannel) {
    const serverOptions: ServerOptions = {
        command: serverPath,
        args: ['--lsp'],
        transport: TransportKind.stdio
    };

    const config = vscode.workspace.getConfiguration('midl');

    const clientOptions: LanguageClientOptions = {
        documentSelector: [
            { scheme: 'file', language: 'midl' },
            { scheme: 'untitled', language: 'midl' },
            { scheme: 'midl-winmd', language: 'midl' }
        ],
        synchronize: {
            fileEvents: vscode.workspace.createFileSystemWatcher('**/*.idl')
        },
        outputChannel: outputChannel,
        initializationOptions: {
            "midl.strictMode": config.get<boolean>('strictMode', true),
            "midl.codeQualityHints": config.get<boolean>('codeQualityHints', true)
        },
        middleware: {
            provideDefinition: async (document, position, token, next) => {
                const result = await next(document, position, token);
                if (result && !Array.isArray(result) && 'uri' in result) {
                    const loc = result as vscode.Location;

                    // WinMD decompiled view: open virtual document with MIDL syntax highlighting
                    if (loc.uri.scheme === 'midl-winmd') {
                        const doc = await vscode.workspace.openTextDocument(loc.uri);
                        await vscode.languages.setTextDocumentLanguage(doc, 'midl');
                        await vscode.window.showTextDocument(doc, {
                            selection: loc.range,
                            preview: true
                        });
                        return null;
                    }

                    const targetPath = loc.uri.fsPath;
                    try {
                        const stat = fs.statSync(targetPath);
                        if (stat.isDirectory()) {
                            // @ref: to a directory — reveal in OS file explorer
                            vscode.commands.executeCommand('revealFileInOS', loc.uri);
                            return null;
                        }
                    } catch {
                        // File doesn't exist locally, fall through to default behavior
                    }
                }
                return result;
            }
        }
    };

    client = new LanguageClient(
        'midl',
        'MIDL Language Server',
        serverOptions,
        clientOptions
    );

    client.start().catch(err => {
        console.error('MIDL: Language server failed to start:', err);
        vscode.window.showWarningMessage(
            `MIDL: Language server failed to start. Check that "${serverPath}" exists and is runnable. Syntax highlighting still active.`
        );
    });
}

export function deactivate(): Thenable<void> | undefined {
    return client?.stop();
}
