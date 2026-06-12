// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import * as vscode from 'vscode';
import { LanguageClient } from 'vscode-languageclient/node';

/** WinRT primitive types that have no navigable definition. */
const PRIMITIVES = new Set([
    'Object', 'String', 'Boolean', 'Char16', 'Guid',
    'Int16', 'Int32', 'Int64', 'UInt8', 'UInt16', 'UInt32', 'UInt64',
    'Single', 'Double', 'void',
]);

/**
 * Creates a DefinitionProvider for midl-winmd scheme documents.
 * Handles F12 within decompiled WinMD views:
 * - Same-namespace short names: searches current document text
 * - Cross-namespace FQNs: sends midl/decompileDefinition request to LSP server
 */
export function createWinMdDefinitionProvider(getClient: () => LanguageClient | undefined): vscode.DefinitionProvider {
    return {
        async provideDefinition(document: vscode.TextDocument, position: vscode.Position): Promise<vscode.Location | null> {
            const client = getClient();
            if (!client) return null;

            const wordRange = document.getWordRangeAtPosition(position);
            if (!wordRange) return null;
            const word = document.getText(wordRange);

            if (PRIMITIVES.has(word)) return null;

            // Extract FQN by walking through dots
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
            const uriPath = decodeURIComponent(uriStr.substring(schemeIdx + 'midl-winmd:'.length)).replace(/^\/+/, '');
            const winmdIdx = uriPath.toLowerCase().indexOf('.winmd');
            if (winmdIdx < 0) return null;
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

            // Cross-namespace: ask the LSP server via custom request
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
    };
}
