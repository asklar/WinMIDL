import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';
import { LanguageClient, LanguageClientOptions, ServerOptions, TransportKind } from 'vscode-languageclient/node';

let client: LanguageClient | undefined;

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

    // 2. Look relative to the extension install path (dev inner loop)
    //    Extension is at <repo>/editors/vscode, so repo root is ../..
    const extensionDir = context.extensionPath;
    const repoRoot = path.resolve(extensionDir, '..', '..');

    const candidates = [
        // Dev build (dotnet build)
        path.join(repoRoot, 'src', 'Midl.Cli', 'bin', 'Debug', 'net10.0', 'Midl.Cli.exe'),
        // Dev build with RID subfolder (self-contained or SDK-style)
        path.join(repoRoot, 'src', 'Midl.Cli', 'bin', 'Debug', 'net10.0', 'win-x64', 'Midl.Cli.exe'),
        // Release build
        path.join(repoRoot, 'src', 'Midl.Cli', 'bin', 'Release', 'net10.0', 'Midl.Cli.exe'),
        path.join(repoRoot, 'src', 'Midl.Cli', 'bin', 'Release', 'net10.0', 'win-x64', 'Midl.Cli.exe'),
        // Published output
        path.join(repoRoot, 'publish', 'Midl.Cli.exe'),
        // Linux/macOS (no .exe)
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

export function activate(context: vscode.ExtensionContext) {
    console.log('MIDL Language Support activated');

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

    const clientOptions: LanguageClientOptions = {
        documentSelector: [
            { scheme: 'file', language: 'midl' },
            { scheme: 'untitled', language: 'midl' }
        ],
        synchronize: {
            fileEvents: vscode.workspace.createFileSystemWatcher('**/*.idl')
        },
        outputChannel: outputChannel
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
