import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';
import { LanguageClient, LanguageClientOptions, ServerOptions, TransportKind } from 'vscode-languageclient/node';

let client: LanguageClient | undefined;
let serverBinaryPath: string | undefined;

export function getClient(): LanguageClient | undefined { return client; }
export function getServerBinaryPath(): string | undefined { return serverBinaryPath; }

/**
 * Attempt to find the MIDL CLI binary by searching known locations.
 */
export function findServerBinary(context: vscode.ExtensionContext): string | undefined {
    const config = vscode.workspace.getConfiguration('midl');
    const configured = config.get<string>('serverPath');
    if (configured && configured !== 'midl' && configured.trim() !== '') {
        return configured;
    }

    const extensionDir = context.extensionPath;

    // Bundled server binary (shipped inside the VSIX, per-platform)
    const arch = process.arch === 'arm64' ? 'win-arm64' : 'win-x64';
    const bundledExe = path.join(extensionDir, 'server', arch, 'Midl.Cli.exe');
    if (fs.existsSync(bundledExe)) {
        console.log(`MIDL: Found bundled server binary at ${bundledExe}`);
        return bundledExe;
    }
    const bundledFlat = path.join(extensionDir, 'server', 'Midl.Cli.exe');
    if (fs.existsSync(bundledFlat)) {
        console.log(`MIDL: Found bundled server binary at ${bundledFlat}`);
        return bundledFlat;
    }

    // Dev inner loop: look relative to repo root
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

    // Check workspace folders
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
 * Start the MIDL Language Server and configure the LSP client.
 */
export function startLanguageServer(serverPath: string, context: vscode.ExtensionContext, outputChannel: vscode.OutputChannel) {
    serverBinaryPath = serverPath;

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
            "midl.codeQualityHints": config.get<boolean>('codeQualityHints', true),
            "midl.useWindowsSdk": config.get<boolean>('useWindowsSdk', true),
            "midl.windowsSdkVersion": config.get<string>('windowsSdkVersion', 'latest')
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

                    try {
                        const targetPath = loc.uri.fsPath;
                        const stat = fs.statSync(targetPath);
                        if (stat.isDirectory()) {
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

export function stopLanguageServer(): Thenable<void> | undefined {
    return client?.stop();
}
