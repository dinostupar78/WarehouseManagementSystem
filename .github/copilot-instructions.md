# Copilot Instructions

## Scope
These instructions apply to the entire WarehouseManagementSystem workspace.

## Required Sub-agent Usage
- For any UI/UX task in the MVC web app, the main agent must invoke sub-agent "UX/UI Agent" before implementing UI changes.
- UI/UX task includes: layout updates, styling, navigation menus, breadcrumbs, responsive behavior, view redesign, component hierarchy, and visual polish.

## Invocation Rule
- Always call sub-agent name exactly: "UX/UI Agent".
- Ask the sub-agent for: UX intent, files to modify, concrete Razor/CSS edits, and a validation checklist.
- After receiving the sub-agent output, apply the changes in the main agent flow.

## Lab 2 Constraints
- Keep pages read-only where required by assignment (Index and Details only, no Create/Edit/Delete unless explicitly requested).
- Preserve ASP.NET MVC routing conventions and working links.
- Keep navigation complete: menu links, list-to-details links, back links, and breadcrumbs.
- Ensure resulting UI is unique/non-standard, not default Bootstrap template styling.

## Evidence and Logging
- Keep hook logging enabled so prompts and tool calls are recorded in lab-2/sub-agent_log.txt.
- Sub-agent invocation must be visible in logs as tool call runSubagent with agentName "UX/UI Agent".

## Quality Bar
- Prefer maintainable CSS tokens/variables and consistent visual system across pages.
- Validate route correctness, readability, and mobile responsiveness after UI changes.
