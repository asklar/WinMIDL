import * as vscode from 'vscode';
import { findServerBinary, startLanguageServer, stopLanguageServer, getClient } from './server';
import { WinMdContentProvider } from './winmdProvider';
import { createWinMdDefinitionProvider } from './winmdDefinition';

let diagramPanel: vscode.WebviewPanel | undefined;
let diagramShowReferences = false;
let diagramUri: string | undefined;

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
                    uri: editor.document.uri.toString(),
                    showReferences: diagramShowReferences
                }) as any;

                if (result?.dot) {
                    diagramUri = editor.document.uri.toString();
                    const webviewHtml = getDiagramHtml(result.dot as string, diagramShowReferences);
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
                        diagramPanel.webview.onDidReceiveMessage(async (msg) => {
                            if (msg.command === 'toggleReferences') {
                                diagramShowReferences = msg.value;
                                if (diagramUri && client) {
                                    const refreshResult = await client.sendRequest('midl/classDiagram', {
                                        uri: diagramUri,
                                        showReferences: diagramShowReferences
                                    }) as any;
                                    if (refreshResult?.dot && diagramPanel) {
                                        diagramPanel.webview.html = getDiagramHtml(refreshResult.dot as string, diagramShowReferences);
                                    }
                                }
                            }
                        });
                        diagramPanel.onDidDispose(() => { diagramPanel = undefined; });
                    }
                } else {
                    vscode.window.showWarningMessage('MIDL: Server returned no diagram.');
                }
            } catch (err: any) {
                vscode.window.showErrorMessage(`MIDL Class Diagram: ${err.message || err}`);
            }
        }),

        vscode.commands.registerCommand('midl.reportIssue', async () => {
            const client = getClient();
            if (!client) {
                // No server running — try to read the persistent log file
                const fs = await import('fs');
                const path = await import('path');
                const logDir = path.join(process.env.USERPROFILE ?? process.env.HOME ?? '', '.midl');
                const logFile = path.join(logDir, 'server.log');
                let logTail = '';
                try {
                    if (fs.existsSync(logFile)) {
                        const lines = fs.readFileSync(logFile, 'utf-8').split('\n');
                        logTail = lines.slice(-100).join('\n');
                    }
                } catch { /* ignore */ }

                const sections = [
                    '<h2>Environment</h2>',
                    '',
                    `- VS Code: ${vscode.version}`,
                    `- OS: ${process.platform} ${process.arch}`,
                    `- Extension: ${vscode.extensions.getExtension('AlexanderSklarMSFT.midl3-language-server')?.packageJSON?.version ?? 'unknown'}`,
                    `- Server: not running`,
                    '',
                ];

                if (logTail) {
                    sections.push('<details><summary>Server Log (last 100 lines from previous session)</summary>');
                    sections.push('');
                    sections.push('```');
                    sections.push(logTail);
                    sections.push('```');
                    sections.push('</details>');
                    sections.push('');
                }

                sections.push('<h2>Description</h2>');
                sections.push('');
                sections.push('_Please describe the issue_');

                openGitHubIssue('Bug: ', sections.join('\n'));
                return;
            }

            try {
                const report = await client.sendRequest('midl/diagnosticReport') as any;

                // Ask about including IDL file contents
                const consent = await vscode.window.showQuickPick(
                    [
                        { label: 'Include IDL files', description: 'Attach IDL file contents for diagnosis', value: true },
                        { label: 'Summary only', description: 'Only include statistics (no file contents)', value: false },
                    ],
                    { title: 'MIDL: Report Issue — what to include?', placeHolder: 'Choose what diagnostic data to share' }
                );
                if (!consent) { return; } // cancelled

                // Build the issue body
                const sections: string[] = [];

                // Environment
                sections.push('<h2>Environment</h2>');
                sections.push('');
                sections.push(`| Key | Value |`);
                sections.push(`|-----|-------|`);
                sections.push(`| VS Code | ${vscode.version} |`);
                sections.push(`| OS | ${report.platform ?? process.platform} |`);
                sections.push(`| Architecture | ${report.architecture ?? process.arch} |`);
                sections.push(`| Runtime | ${report.runtime ?? 'unknown'} |`);
                sections.push(`| Extension | ${vscode.extensions.getExtension('AlexanderSklarMSFT.midl3-language-server')?.packageJSON?.version ?? 'unknown'} |`);
                sections.push(`| Server | ${report.serverVersion ?? 'unknown'} |`);
                sections.push(`| Memory (private) | ${report.privateBytesMB ?? '~'} MB |`);
                sections.push(`| Memory (working set) | ${report.workingSetMB ?? '~'} MB |`);
                sections.push(`| Memory (GC heap) | ${report.gcHeapMB ?? '~'} MB |`);
                sections.push(`| Compilation mode | ${report.compilationMode ?? 'unknown'} |`);
                sections.push(`| Files scanned | ${report.filesScanned ?? 0} |`);
                sections.push(`| Files open | ${report.filesOpen ?? 0} |`);
                sections.push(`| Compilations cached | ${report.compilationsCached ?? 0} |`);
                sections.push(`| WinMD providers cached | ${report.winmdProvidersCached ?? 0} |`);
                sections.push(`| Project file | ${report.hasProjectFile ? 'yes' : 'no'} |`);
                sections.push('');

                // File stats
                const fileStats = report.fileStats as any[] ?? [];
                if (fileStats.length > 0) {
                    sections.push('<h2>File Statistics</h2>');
                    sections.push('');
                    sections.push('| File | Size | Lines | User Types | Resolved Types | Errors | Warnings | MIDL3 |');
                    sections.push('|------|------|-------|------------|----------------|--------|----------|-------|');
                    for (const f of fileStats) {
                        const resolved = f.resolvedFrom ? Object.entries(f.resolvedFrom as Record<string, number>).map(([k, v]) => `${v} from ${k}`).join(', ') : '';
                        const resolvedCell = f.resolvedTypes ? `${f.resolvedTypes} (${resolved})` : '0';
                        sections.push(`| ${f.relativePath} | ${f.sizeBytes}B | ${f.lines} | ${f.userTypes} | ${resolvedCell} | ${f.errors} | ${f.warnings} | ${f.isMidl3 ? '✓' : '✗'} |`);
                    }
                    sections.push('');
                }

                // Event log (last 50 events to stay within URL limits)
                const events = report.events as any[] ?? [];
                if (events.length > 0) {
                    sections.push('<details><summary>Event Log (last ' + Math.min(events.length, 50) + ' events)</summary>');
                    sections.push('');
                    sections.push('```');
                    for (const e of events.slice(-50)) {
                        const meta = e.metadata ? ' ' + JSON.stringify(e.metadata) : '';
                        sections.push(`[${e.timestamp}] [${e.category}] ${e.message}${meta}`);
                    }
                    sections.push('```');
                    sections.push('</details>');
                    sections.push('');
                }

                // IDL file contents (if consented)
                if (consent.label === 'Include IDL files') {
                    const idlContents: string[] = [];
                    for (const f of fileStats) {
                        if (f.content) {
                            const truncated = f.content.length > 5000 ? f.content.substring(0, 5000) + '\n... (truncated)' : f.content;
                            idlContents.push(`<details><summary>${f.relativePath} (${f.sizeBytes}B, ${f.lines} lines)</summary>\n\n\`\`\`idl\n${truncated}\n\`\`\`\n</details>`);
                        }
                    }
                    if (idlContents.length > 0) {
                        sections.push('<h2>IDL Files</h2>');
                        sections.push('');
                        sections.push(idlContents.join('\n'));
                        sections.push('');
                    }
                }

                sections.push('<h2>Description</h2>');
                sections.push('');
                sections.push('_Please describe the issue_');
                sections.push('');

                const body = sections.join('\n');

                // GitHub issue URL — truncate body if needed for URL length limits
                const maxBodyLength = 6000; // conservative limit for URL encoding
                const truncatedBody = body.length > maxBodyLength
                    ? body.substring(0, maxBodyLength) + '\n\n... (truncated — please paste additional details manually)'
                    : body;

                openGitHubIssue('Bug: ', truncatedBody);
            } catch (err: any) {
                vscode.window.showErrorMessage(`MIDL Report Issue: ${err.message || err}`);
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

function openGitHubIssue(title: string, body: string) {
    // Use encodeURIComponent for query values + Uri.parse (same pattern as VS Code's
    // built-in issue reporter). HTML headings (<h2>) are used instead of markdown ##
    // to avoid # and ? encoding issues with vscode.env.openExternal.
    const url = `https://github.com/microsoft/midl/issues/new?body=${encodeURIComponent(body)}&labels=bug`;
    vscode.env.openExternal(vscode.Uri.parse(url));
}

function getNonce(): string {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let result = '';
    for (let i = 0; i < 32; i++) {
        result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return result;
}

function getDiagramHtml(dotSource: string, showReferences: boolean = false): string {
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
        #toolbar {
            padding: 4px 0 8px 0;
            font: 13px var(--vscode-font-family, 'Segoe UI', sans-serif);
            color: var(--vscode-editor-foreground, #ccc);
            user-select: none;
        }
        #toolbar label { cursor: pointer; display: inline-flex; align-items: center; gap: 4px; }
        #toolbar input[type="checkbox"] { accent-color: var(--vscode-focusBorder, #007acc); cursor: pointer; }
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
    <div id="toolbar">
        <label><input type="checkbox" id="showRefs" ${showReferences ? 'checked' : ''}>Show references</label>
    </div>
    <div id="diagram"><p id="loading">Rendering diagram\u2026</p></div>
    <div id="toast"></div>
    <script type="module" nonce="${nonce}">
        const vscode = acquireVsCodeApi();
        document.getElementById('showRefs').onchange = function() {
            vscode.postMessage({ command: 'toggleReferences', value: this.checked });
        };
        try {
            const { instance } = await import('https://cdn.jsdelivr.net/npm/@viz-js/viz@3.11.0/lib/viz-standalone.mjs');
            const viz = await instance();
            const svgStr = viz.renderString(${JSON.stringify(dotSource)}, { format: 'svg' });
            document.getElementById('diagram').innerHTML = svgStr;
        } catch (e) {
            document.getElementById('diagram').innerHTML =
                '<p id="error">Failed to render diagram:\\\\n' + e.message + '</p>';
        }

        const svgEl = document.querySelector('#diagram svg');
        const graphG = svgEl?.querySelector('g.graph');

        // --- Arrow markers (replace Graphviz polygons) ---
        if (svgEl) {
            const ns = 'http://www.w3.org/2000/svg';
            const defs = document.createElementNS(ns, 'defs');
            defs.innerHTML = '<marker id="ah" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="8" markerHeight="6" orient="auto">'
                + '<path d="M0,0 L10,5 L0,10 z" fill="none" stroke="#888" stroke-width="1.5"/></marker>';
            svgEl.insertBefore(defs, svgEl.firstChild);
        }

        // --- Parse edges ---
        const edges = [];
        document.querySelectorAll('.edge').forEach(g => {
            const raw = g.querySelector('title')?.textContent || '';
            const m = raw.match(/^(\\w+)\\s*->\\s*(\\w+)$/);
            if (!m) return;
            const path = g.querySelector('path');
            const poly = g.querySelector('polygon');
            if (path) path.setAttribute('marker-end', 'url(#ah)');
            if (poly) poly.remove();
            edges.push({ from: m[1], to: m[2], path, dashed: !!path?.getAttribute('stroke-dasharray') });
        });

        // --- Parse nodes, store initial bounding boxes ---
        const nodes = {}, offsets = {};
        document.querySelectorAll('.node').forEach(g => {
            const id = g.querySelector('title')?.textContent;
            if (!id) return;
            const bb = g.getBBox();
            nodes[id] = { el: g, cx: bb.x + bb.width/2, cy: bb.y + bb.height/2, w: bb.width, h: bb.height };
            offsets[id] = { x: 0, y: 0 };
            g.style.cursor = 'move';
        });

        // --- Drag state ---
        let dragId = null, startPt = null, startOff = null;

        function toGraph(e) {
            if (!svgEl || !graphG) return { x: 0, y: 0 };
            const pt = svgEl.createSVGPoint();
            pt.x = e.clientX; pt.y = e.clientY;
            return pt.matrixTransform(graphG.getScreenCTM().inverse());
        }

        svgEl?.addEventListener('pointerdown', e => {
            const node = e.target.closest('.node');
            if (!node) return;
            const id = node.querySelector('title')?.textContent;
            if (!id || !nodes[id]) return;
            dragId = id;
            startPt = toGraph(e);
            startOff = { ...offsets[id] };
            node.style.cursor = 'grabbing';
            node.setPointerCapture(e.pointerId);
            e.preventDefault();
        });

        svgEl?.addEventListener('pointermove', e => {
            if (!dragId) return;
            const pt = toGraph(e);
            offsets[dragId].x = startOff.x + (pt.x - startPt.x);
            offsets[dragId].y = startOff.y + (pt.y - startPt.y);
            nodes[dragId].el.setAttribute('transform', 'translate(' + offsets[dragId].x + ',' + offsets[dragId].y + ')');
            redrawEdges();
        });

        svgEl?.addEventListener('pointerup', () => {
            if (dragId && nodes[dragId]) nodes[dragId].el.style.cursor = 'move';
            dragId = null;
        });

        function nodePos(id) {
            const n = nodes[id], o = offsets[id];
            if (!n || !o) return null;
            const cx = n.cx + o.x, cy = n.cy + o.y;
            return { cx, cy, x: cx - n.w/2, y: cy - n.h/2, w: n.w, h: n.h };
        }

        function edgePt(fx, fy, r) {
            const cx = r.x + r.w/2, cy = r.y + r.h/2;
            const dx = fx - cx, dy = fy - cy;
            if (Math.abs(dx) < 1 && Math.abs(dy) < 1) return { x: cx, y: cy };
            const s = Math.min(r.w/2 / Math.abs(dx), r.h/2 / Math.abs(dy));
            return { x: cx + dx * s, y: cy + dy * s };
        }

        function redrawEdges() {
            edges.forEach(({ from, to, path }) => {
                if (!path) return;
                const fp = nodePos(from), tp = nodePos(to);
                if (!fp || !tp) return;
                const a = edgePt(tp.cx, tp.cy, fp);
                const b = edgePt(fp.cx, fp.cy, tp);
                path.setAttribute('d', 'M' + a.x + ',' + a.y + ' L' + b.x + ',' + b.y);
            });
        }

        // --- Ctrl+mousewheel zoom ---
        let zoomLevel = 1;
        const diagramDiv = document.getElementById('diagram');

        document.addEventListener('wheel', e => {
            if (!e.ctrlKey) return;
            e.preventDefault();
            const delta = e.deltaY > 0 ? -0.1 : 0.1;
            zoomLevel = Math.max(0.2, Math.min(5, zoomLevel + delta));
            const svg = diagramDiv?.querySelector('svg');
            if (svg) svg.style.transform = 'scale(' + zoomLevel + ')';
            if (svg) svg.style.transformOrigin = 'top left';
        }, { passive: false });

        // --- Context menu ---
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
