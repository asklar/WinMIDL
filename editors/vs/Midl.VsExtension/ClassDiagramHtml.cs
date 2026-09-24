using Newtonsoft.Json;

namespace Midl.VsExtension
{
    /// <summary>
    /// Builds the WebView2 HTML that renders a Graphviz DOT class diagram as SVG.
    /// </summary>
    internal static class ClassDiagramHtml
    {
        private const string Template = @"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8"">
<style>
 html,body{height:100%;margin:0;background:#1e1e1e;}
 #toolbar{padding:6px 10px;font-family:'Segoe UI';color:#ccc;border-bottom:1px solid #333;}
 #toolbar label{cursor:pointer;display:inline-flex;align-items:center;gap:4px;}
 #diagram{overflow:auto;}
 #diagram svg{max-width:none !important;}
 #diagram svg > polygon{display:none;}
 #error{color:#f48771;font-family:'Segoe UI';white-space:pre-wrap;}
 #loading{color:#ccc;font-family:'Segoe UI';}
</style>
</head>
<body>
<div id=""toolbar""><label><input type=""checkbox"" id=""showRefs"" __CHECKED__>Show references</label></div>
<div id=""diagram""><p id=""loading"">Rendering diagram\u2026</p></div>
<script type=""module"">
 const post = (m) => window.chrome.webview.postMessage(m);
 document.getElementById('showRefs').onchange = function () { post({ command: 'toggleReferences', value: this.checked }); };
 try {
   const { instance } = await import('https://cdn.jsdelivr.net/npm/@viz-js/viz@3.11.0/lib/viz-standalone.mjs');
   const viz = await instance();
   const svgStr = viz.renderString(__DOT_JSON__, { format: 'svg' });
   document.getElementById('diagram').innerHTML = svgStr;
 } catch (e) {
   document.getElementById('diagram').innerHTML = '<p id=""error"">Failed to render diagram:\n' + e.message + '</p>';
 }
 const svgEl = document.querySelector('#diagram svg');
 const graphG = svgEl ? svgEl.querySelector('g.graph') : null;
 const edges = [];
 const nodes = {};
 const offsets = {};
 if (svgEl) {
   const ns = 'http://www.w3.org/2000/svg';
   const defs = document.createElementNS(ns, 'defs');
   defs.innerHTML = '<marker id=""ah"" viewBox=""0 0 10 10"" refX=""9"" refY=""5"" markerWidth=""8"" markerHeight=""6"" orient=""auto""><path d=""M0,0 L10,5 L0,10 z"" fill=""none"" stroke=""#888"" stroke-width=""1.5""/></marker>';
   svgEl.insertBefore(defs, svgEl.firstChild);
   document.querySelectorAll('.edge').forEach(g => {
     const title = g.querySelector('title');
     const raw = (title && title.textContent) || '';
     const m = raw.match(/^(\w+)\s*->\s*(\w+)$/);
     const path = g.querySelector('path');
     const poly = g.querySelector('polygon');
     if (path) path.setAttribute('marker-end', 'url(#ah)');
     if (poly) poly.remove();
     if (m && path) edges.push({ from: m[1], to: m[2], path: path });
   });
   document.querySelectorAll('.node').forEach(g => {
     const title = g.querySelector('title');
     const id = title ? title.textContent : null;
     if (!id) return;
     const bb = g.getBBox();
     nodes[id] = { el: g, cx: bb.x + bb.width / 2, cy: bb.y + bb.height / 2, w: bb.width, h: bb.height };
     offsets[id] = { x: 0, y: 0 };
     g.style.cursor = 'move';
   });
 }

 let dragId = null, startPt = null, startOff = null;
 function toGraph(e) {
   if (!svgEl || !graphG) return { x: 0, y: 0 };
   const pt = svgEl.createSVGPoint();
   pt.x = e.clientX; pt.y = e.clientY;
   return pt.matrixTransform(graphG.getScreenCTM().inverse());
 }
 function nodePos(id) {
   const n = nodes[id], o = offsets[id];
   if (!n || !o) return null;
   return { cx: n.cx + o.x, cy: n.cy + o.y, w: n.w, h: n.h };
 }
 function edgePt(fx, fy, r) {
   const dx = fx - r.cx, dy = fy - r.cy;
   if (Math.abs(dx) < 1 && Math.abs(dy) < 1) return { x: r.cx, y: r.cy };
   const s = Math.min((r.w / 2) / Math.abs(dx), (r.h / 2) / Math.abs(dy));
   return { x: r.cx + dx * s, y: r.cy + dy * s };
 }
 function redrawEdges() {
   edges.forEach(edge => {
     const fp = nodePos(edge.from), tp = nodePos(edge.to);
     if (!fp || !tp) return;
     const a = edgePt(tp.cx, tp.cy, fp);
     const b = edgePt(fp.cx, fp.cy, tp);
     edge.path.setAttribute('d', 'M' + a.x + ',' + a.y + ' L' + b.x + ',' + b.y);
   });
 }
 if (svgEl) {
   svgEl.addEventListener('pointerdown', e => {
     const node = e.target.closest('.node');
     if (!node) return;
     const title = node.querySelector('title');
     const id = title ? title.textContent : null;
     if (!id || !nodes[id]) return;
     dragId = id;
     startPt = toGraph(e);
     startOff = { x: offsets[id].x, y: offsets[id].y };
     node.style.cursor = 'grabbing';
     node.setPointerCapture(e.pointerId);
     e.preventDefault();
   });
   svgEl.addEventListener('pointermove', e => {
     if (!dragId) return;
     const pt = toGraph(e);
     offsets[dragId].x = startOff.x + (pt.x - startPt.x);
     offsets[dragId].y = startOff.y + (pt.y - startPt.y);
     nodes[dragId].el.setAttribute('transform', 'translate(' + offsets[dragId].x + ',' + offsets[dragId].y + ')');
     redrawEdges();
   });
   svgEl.addEventListener('pointerup', () => {
     if (dragId && nodes[dragId]) nodes[dragId].el.style.cursor = 'move';
     dragId = null;
   });
 }
 let zoom = 1;
 const diagramDiv = document.getElementById('diagram');
 document.addEventListener('wheel', e => {
   if (!e.ctrlKey) return;
   e.preventDefault();
   zoom = Math.max(0.2, Math.min(5, zoom + (e.deltaY > 0 ? -0.1 : 0.1)));
   const svg = diagramDiv.querySelector('svg');
   if (svg) { svg.style.transform = 'scale(' + zoom + ')'; svg.style.transformOrigin = 'top left'; }
 }, { passive: false });
</script>
</body>
</html>";

        /// <summary>
        /// Builds an HTML document that renders the supplied DOT source as an SVG class diagram.
        /// </summary>
        /// <param name="dotSource">The Graphviz DOT source returned by the MIDL server.</param>
        /// <param name="showReferences">Whether the "Show references" toggle is checked.</param>
        /// <returns>A self-contained HTML document for a WebView2 control.</returns>
        public static string Build(string dotSource, bool showReferences)
        {
            string dotJson = JsonConvert.SerializeObject(
                dotSource,
                new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml });

            return Template
                .Replace("__DOT_JSON__", dotJson)
                .Replace("__CHECKED__", showReferences ? "checked" : string.Empty);
        }
    }
}
