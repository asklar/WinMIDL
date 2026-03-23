import * as vscode from 'vscode';
import { execSync } from 'child_process';
import { getServerBinaryPath } from './server';

/**
 * Provides read-only decompiled IDL content for WinMD files.
 * When go-to-definition targets a WinMD type, the LSP returns a midl-winmd:// URI.
 * VS Code calls this provider to get the decompiled IDL text.
 */
export class WinMdContentProvider implements vscode.TextDocumentContentProvider {
    async provideTextDocumentContent(uri: vscode.Uri): Promise<string> {
        const serverBinaryPath = getServerBinaryPath();
        if (!serverBinaryPath) {
            return '// MIDL server binary not found — cannot decompile WinMD';
        }

        // URI format: midl-winmd:///path/to/file.winmd/Namespace.Name
        let fullPath = uri.path;
        if (process.platform === 'win32' && fullPath.startsWith('/')) {
            fullPath = fullPath.substring(1);
        }

        // Split path at .winmd boundary to extract namespace
        let winmdPath: string;
        let nsFilter: string | undefined;
        const winmdIdx = fullPath.toLowerCase().indexOf('.winmd');
        if (winmdIdx >= 0) {
            winmdPath = fullPath.substring(0, winmdIdx + 6);
            const remainder = fullPath.substring(winmdIdx + 6);
            if (remainder.startsWith('/') && remainder.length > 1) {
                nsFilter = remainder.substring(1);
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
