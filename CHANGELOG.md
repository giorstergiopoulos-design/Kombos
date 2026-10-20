# Changelog

All notable changes to Kombos, summarized here for GitHub. The full changelog - in all 4 supported
languages (Greek, English, French, German) - is always available inside the app itself, via the Help panel's
"Version history" tab.

## v4.8.0 — Sidebar collapse, MotionDeskTop-style

the "expanded" state (default) now shows an icon+label on every button (Theme/Language/Appearance/Artifacts/Projects/Settings) instead of bare icons - the "collapsed" state stays the familiar narrow icon-only rail. The collapse button now lives inside that same column (below the icons) and automatically follows the sidebar whether it's on the left or right, no more manual cross-column position math.

## v4.7.0 — Visual & usability pass

**vector icons** instead of emoji in window chrome, the main toolbar, and the sidebar (clean line-icon style) · **dark mode in neutral charcoal #171717** (instead of the previous warm brownish-black) · **sidebar collapse** with a toggle button below the icons, centered with them · **📋 Paste** buttons on the API key/username/password fields (a safe alternative to reading saved browser passwords, which was explicitly declined for security reasons) · the auto-continue-with-another-agent feature (v4.6.0) got more correct: a Yes/No pop-up the FIRST time, then it remembers the choice and continues automatically without asking again · Agent settings were reorganized into collapsible sections (less clutter). Live-tested on a real running instance (UI Automation, no keys): the Gateway endpoint was confirmed working end-to-end; found that Ctrl+Alt+Space (Kombos Anywhere) can silently conflict with another app on some machines (known issue, open for a fix).

## v4.6.0 — Kombos Gateway + automatic agent continuation

Kombos Gateway (an explicit request after the "final" v4.5.0 - an exception, not scope creep): a new local HTTP gateway, inspired by the open-source OmniRoute (github.com/diegosouzapw/OmniRoute, MIT) - an OpenAI-compatible endpoint (`http://127.0.0.1:PORT/v1/chat/completions`) other tools (VS Code, Cursor, Claude Code, etc.) can point at instead of talking to a provider directly. Routes automatically to Kombos's central agent, with automatic **fallback** to the next configured agent if the first fails (rate limit/quota/network) - the same spirit as OmniRoute's "automatic fallback across 359 providers," but configured entirely through Kombos's already-friendly GUI (Settings → Behavior: one checkbox + one port), not .env files/a CLI/Docker/a separate dashboard - this is the "friendlier OmniRoute" that was asked for. Secure by design: listens ONLY on 127.0.0.1, never on the network. No code was copied from OmniRoute (different language/architecture - Node.js there, C#/.NET here) - only the idea of a single local gateway with automatic fallback was built from scratch, using the keys you've already entered into Kombos. Alongside it, new **automatic continuation when Claude nears/hits its limit** - explicit user request: the FIRST time, a pop-up proposes the first already-configured agent (Yes/No), and your choice is remembered from then on - Kombos automatically draws on that same agent next time, without asking again, so the conversation/project keeps going uninterrupted. Settings → Behavior has the enable checkbox plus a "Reset" button to pick a different agent later. The reply bubble always shows which agent actually answered - no silent/hidden switching.

## v4.5.0 — FINAL VERSION

Kombos is now complete as a multi-tool - no version is planned after this one. A large final wave: **🔄 Check for updates** (Settings → About, manual only, never a silent download/install) · **Import from ChatGPT/Claude.ai** (their conversations.json export files) · **Kombos Anywhere** (Ctrl+Alt+Space opens a mini quick-capture bar from anywhere, with an option to add the answer to the main conversation) · **Multi-window** (detach any conversation into its own window, with "Copy all as Markdown") · **/image** (real image generation inside the conversation via the OpenAI Images API, using the already-saved ChatGPT key) · **Real-time collaboration** (two computers pointed at the same sync folder see each other's changes automatically, via file watching) · **Local analytics dashboard** (tokens-per-session chart plus a per-tool breakdown, no telemetry) · **Plugin SDK v2** (a "New Plugin" wizard scaffolds a ready-to-build project) · **Nexus Marketplace** (install a plugin from a folder or Git URL) · **Artifact live preview** (real HTML/SVG/Markdown rendering in its own window, not just a code list) · an accessibility pass (AutomationProperties.Name on key icons/window controls). On "response speed/cost": the standing cache-friendly system prompt (since v2.8.0) already applies to every agent; real automatic routing to a cheaper tool or batching were evaluated and deliberately NOT built (it would mean Kombos silently overriding the user's own agent choice, or doesn't fit real-time chat) - an honest choice, not an oversight.

## v4.3.0 — Local project search + message actions (second wave toward the final v4.5.0)

new `/search <keywords>` command - searches the active project's full content (not just the file tree like /readfile), locally, with term-frequency scoring, zero token cost (a "local RAG-lite" without embeddings/vector DB). Right-click on any message now offers "Copy as Markdown", "Copy as plain text", and "Quote in new message".

## v4.2.0 — Voice + Presets + Focus mode (first wave toward the final v4.5.0)

new 🔊 "Read aloud" button on every agent reply (local, free text-to-speech via System.Speech, no cloud/API key), and a new 🎤 voice-input button next to the message field (push-to-talk: press, speak once, press again - not continuous listening). New "Presets" section in Agent settings: save the current agent + instructions combo under a name, apply it again later with one click instead of retyping the same instructions. New "Focus mode" (F11, or the View menu on Windows Classic): hides sidebar/menu bar/header/tool catalog/status bar, leaving only the conversation - Esc exits it. v4.5.0 has been set as Kombos's FINAL version - everything from here builds toward that point; see the full roadmap for what's left.

## v4.1.0 — Real Light/Dark fix + conversation forking + Prompt Library

dark mode visibly "didn't work" on the Windows Classic skin - the real cause was found (Classic's palette completely ignored dark mode) and a real dark variant was added. New "Theme" option in Appearance with 3 states: Light / Dark / System (automatically and live follows the Windows theme). Also fixed the language/Artifacts popup position on Windows Classic (correctly anchored but with the wrong Placement, landing outside the window) and the too-narrow title field in "+ New Project". Research into similar open-source apps (LibreChat, NextChat, Cherry Studio) produced two new, original implementations: "🔀 Fork" a conversation from any message (a new independent conversation with the history up to that point, without altering the original), and a "📚 Prompt Library" (10 original starter prompts across 5 categories - zero token cost until you send one). Search across ALL conversations (title + content) was already implemented earlier.

## v4.0.0 — Milestone: skin/window fixes, Accounts, catalog pagination

skin/window fixes + Accounts + catalog pagination. The Windows Classic skin had forgotten popups (language, Artifacts, Projects/history) that stayed rounded and appeared at the sidebar's now-hidden position - fully fixed, along with a deeper cause: a MenuItem in the classic menu bar isn't a safe anchor once its menu closes, so popups were silently failing to open. The Nexus skin in dark mode now uses a neutral charcoal/graphite palette (Adobe Acrobat Pro dark-mode style) instead of the previous very dark one. Maximizing now respects the exact screen boundaries (taskbar/multi-monitor setups) instead of overflowing. The tool catalog now pages through tiles with side arrows instead of a scrollbar. New "Accounts" window (from the tray icon) showing which tools have saved details and opening the tool's real login/signup page in your browser - Kombos never logs in or signs up on your behalf.

## v3.9.0 — Projects (Claude Desktop style) + fixes

new "PROJECTS" section above the conversation list with a "+ New Project" button - group conversations under a name, new chats started inside a Project are auto-assigned to it. The attach button now accepts ANY file type (not just images) - .md/.txt/.json/code get added as text context to the chat. New optional username/password fields per agent in settings (DPAPI-encrypted, for your own reference only - never auto-fills any website login). New catalog tool "OmniRoute" (a third-party local AI gateway, community project - you need to start it yourself). GitHub's MCP connector is pre-filled as an example in the MCP servers list (needs OAuth that isn't automated yet). Fixes: the "Orbit" animated background now shows the orbit path line, and the leftmost gear in "Gears" no longer overlaps the animated title icon.

## v3.8.0 — Command Palette + Business Toolkit + Shared Projects

new Ctrl+K quick-command window searching across tools/commands/settings. New Business Toolkit (local plugins, zero token cost): /vat, /loan, /percent, /workdays (business days aware of Greek public holidays), /iban, /invoice-number. New "Shared Projects": publish a Project Memory project as a ZIP inside the sync folder - every computer with the same folder sees the list with a status (Pending/In progress/Completed) and can download the ZIP (there's no public API for claude.ai's Projects, so this is the real alternative). Fixes: the Windows Classic skin now has the correct gray background/navy titlebar everywhere, and the Nexus skin's vertical tab labels no longer get cut off.

## v3.7.0 — Visual upgrade + Creative Toolkit

two new skins (Windows Classic — square corners everywhere, raised 3D-bevel tiles, navy titlebar, and a CLASSIC MENU BAR — File/Edit/View/Tools/Help — replacing the icon sidebar entirely; Nexus — original orange branding, vertical tab rail in Settings). Every skin now has an authentic period accent color (Aero blue, Metro blue, Fluent blue, Classic navy). New Creative Toolkit (local plugins, zero token cost): /gradient, /aspect-ratio, /palette, /contrast (WCAG contrast checker), /favicon-checklist.

## v3.6.0 — Big "multi-tool" update

a new "Download & Install" gallery in Settings → Extensions for one-click install/uninstall of official plugins (Dev Toolkit with /json /base64 /hash /uuid /color /timestamp /regex /git /http /cron, Office Toolkit with /wordcount /lorem /password /diff /case, a Weather plugin) - all local, zero token cost. New "Hive Memory": whatever one agent learns (/remember, /hive) every agent now knows. New Handoff pipeline (🔗 button next to the message box): a draft reply from one agent, refined by a second. New "Project Memory": import a project as a lightweight index (file tree) instead of full content, pull in a specific file on demand with /readfile, and run a local compatibility check with /verify (runs the project's own build/lint, no AI call). A prompt-cache hit-rate indicator in the status bar, the catalog JSON is now built once instead of on every message, and older messages are automatically trimmed in very long conversations to save tokens.

## v3.0.0 — First complete, "shipped" release

an installer (KombosSetup.exe), no admin rights needed, with an optional desktop shortcut and start-with-Windows during setup. Full plugin system, prompt caching + MCP servers for Claude, safe message delete/conversation duplicate, an Artifacts panel, a conversations sync folder across computers, a first-run guide, automated tests, and major memory/CPU fixes - everything built since v2.7.0, bundled into one stable, tested release.

## v2.9.9 — Reduced memory/CPU usage

tool logos are now cached instead of being re-downloaded from the web on every theme/language/search change, the tool search box no longer rebuilds the whole catalog on every keystroke, and animated backgrounds now fully stop while Kombos is running hidden in the system tray instead of continuing indefinitely in the background.

## v2.9.5 — Clear split of settings entry points

the gear opens general app settings (Behavior), the "C" avatar opens the central agent's own personalized settings and collaboration with other agents (MCP) - still the same two, and only two, entry points into settings; a new "Conversations" sidebar icon for quick access to current and older chats.

## v2.9.0

A first-run onboarding guide with tutorial pages, safe deletion of a single message, duplicate a conversation, an "Artifacts" panel in the sidebar listing every code block from every conversation (copy/save), a conversations sync folder (OneDrive/Dropbox/network) for sharing across computers, and a fix for a bug where "Regenerate" on an error bubble could erase real history.

## v2.8.0

Prompt caching for Claude (cheaper turns whenever the system prompt/catalog doesn't change), remote MCP server support in Settings → Agent (Claude only), and a minimum-token-cost rule applied to every agent without exception (documented in code per provider).

## v2.7.0 — Plugin system

a new "Plugins" tab in Settings, an %AppData%\Kombos\plugins\ folder for third-party extensions, "/something" chat commands registered by plugins (sample included: /weather <city>).

## v2.6.0

A tool search box next to the category tabs - searches across all categories at once.

## v2.5.0

Pin favorite tools (⭐ on every tile and in the details panel - pinned ones show first within their category), a new "Behavior" tab in Settings (always on top, start with Windows, start minimized, notification sound), export/import a full JSON backup of your conversations, keyboard shortcuts (Ctrl+N new chat, Ctrl+, settings, Esc to close), and a "jump to bottom" button when you scroll up to older messages.

## v2.4.0

A system-tray notification when a reply arrives while the window isn't in the foreground, searching saved conversations now also matches their content (not just the title), an unsent draft is kept when you switch conversations, adjustable chat text size (Small/Normal/Large, in Appearance) for accessibility, and a "delete all saved conversations" button (with confirmation) in Settings.

## v2.3.0

API keys are now encrypted with Windows DPAPI (tied to your Windows account) before being written to disk, instead of plain text.

## v2.2.0

Markdown rendering in replies (bold/italic/inline code, code blocks with their own copy button), edit your own message before resending it (✏ on every user bubble), search within saved conversations, and several stability fixes: "Regenerate" now correctly replaces the old reply instead of adding a second one below it and no longer erases real history on failure; API keys now persist across restarts; fixed a possible conversation mix-up if a delayed reply arrived after switching to another conversation.

## v2.1.0

Rename saved conversations (✏ in the history list), custom instructions for the central agent (Settings → Agent), bigger and more distinguishable buttons next to the message box, Help and Version history fully split into separate, bold-headed sections.

## v2.0.0

Regenerate a reply, compare answers from multiple agents at once (⚖ next to the message box), multiple saved conversations with their own history (🕘) instead of one continuous chat, a system tray icon so Kombos keeps running in the background when the window is closed, and a global hotkey (Ctrl+Alt+K) to summon it from anywhere in Windows.

## v1.9.0

Real web search (toggleable, 🌐 next to the message box) via Claude's official server-side web search tool — it decides on its own when it needs to search, and the sources it found show up as clickable links under the reply.

## v1.8.0

Paste an image from the clipboard with Ctrl+V (e.g. a screenshot) straight into the chat, drag & drop an image/file/folder onto the chat panel (auto-detects whether it's an image attachment or context to import), boosted contrast on every animated background so they read clearly in any theme/skin.

## v1.7.0

10 selectable animated header backgrounds (Gears/Orbit/Waves/Grid Pulse inspired by Windows Optimizer, plus Node Network/Bokeh/Aurora/Starfield), a "New chat" button, a copy button on every reply, the ability to stop a reply in progress, a multi-line input box (Shift+Enter now actually inserts a newline), a fixed palette icon in the Appearance window.

## v1.6.0

Image attachments in chat (Claude actually "sees" them via the vision API), a toggleable visible extended-thinking panel above replies, a new dedicated gear button for general app settings (Agent/Project/Info) while the palette button now opens its own smaller window just for Appearance/Themes/Skins.

## v1.5.0

Appearance Settings is now a separate tabbed window (Agent/Appearance/Project/Info) instead of a popup menu that closed itself whenever another window got focus; fixed a bug where tool logos would show a plain letter instead of the real icon after switching the Windows theme.

## v1.4.0

Selectable visual "themes" inspired by Windows Vista/7/8/10-11 (corners, shadows, glossy buttons, Windows 8 Metro tiles) in the Appearance menu.

## v1.3.0

Real window-level transparency (no longer on panels/buttons) with a custom title bar, a startup splash that shrinks into the title's position, custom vector icons on category tabs (instead of emoji), a central AI agent picker (beyond Claude) with real chat for ChatGPT/Mistral/DeepSeek/Grok/Perplexity/Gemini, adjustable transparency levels & animated background options, full catalog translations in French/German.

## v1.2.0

Grouped settings menu (Agent, Appearance, Project, Info), a globe icon for language selection (more universal than a flag), a corrected UK flag, a white backing plate behind every logo for contrast in dark mode, an Aero visual style (transparency/shadow/animated background), whole-folder project import, chat export to file, real usage tracking for ElevenLabs, settings persisted across restarts.

## v1.1.0

Multilingual UI in 4 languages (Greek/English/French/German) with a flag picker, per-tool logos (with a fix for Microsoft Copilot), this help/version panel, project import from file, sidebar position setting, animated title icon & title font, general contrast/rendering fixes.

## v1.0.0 — First complete release

confirmed build/run on Windows, app icon, category tabs with real content switching, agent/login menu on "C" with per-tool key storage.
