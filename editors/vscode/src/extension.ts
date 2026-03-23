import * as vscode from 'vscode';
import { findServerBinary, startLanguageServer, stopLanguageServer, getClient } from './server';
import { WinMdContentProvider } from './winmdProvider';
import { createWinMdDefinitionProvider } from './winmdDefinition';

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
