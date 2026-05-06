# Copilot Instructions

## Scope
These instructions apply to the entire WarehouseManagementSystem workspace.

## Required Sub-agent Usage
- For any UI/UX task in the MVC web app, the main agent must invoke sub-agent "UX/UI Agent" before implementing UI changes.
- UI/UX task includes: layout updates, styling, navigation menus, breadcrumbs, responsive behavior, view redesign, component hierarchy, and visual polish.

## Blocking Rule
- If the user asks for UI/UX changes, do not directly edit Razor/CSS first.
- First call runSubagent with agentName "UX/UI Agent", then implement based on its output.
- Backend-only tasks (controllers, repositories, Program.cs, models without UI changes) do not require UX/UI Agent.

## Invocation Rule
- Always call sub-agent name exactly: "UX/UI Agent".
- Ask the sub-agent for: UX intent, files to modify, concrete Razor/CSS edits, validation checklist, and manual click-through test path.
- After receiving the sub-agent output, apply the changes in the main agent flow.

## Standard Sub-agent Prompt Contract
- Include these requirements in each invocation:
	- Keep Index and Details read-only unless explicitly requested otherwise.
	- Preserve MVC routes and all working links.
	- Provide full navigation: top menu, list-to-details links, back links, breadcrumbs.
	- Produce a unique/non-default Bootstrap visual direction.
	- Ensure desktop and mobile usability.
	- Return output in sections: UX intent, files, concrete edits, validation checklist, manual test path.

## Lab Constraints
- Keep pages read-only where required by assignment (Index and Details only, no Create/Edit/Delete unless explicitly requested).
- Preserve ASP.NET MVC routing conventions and working links.
- Keep navigation complete: menu links, list-to-details links, back links, and breadcrumbs.
- Include custom home page (or equivalent custom page) as required by assignment when task scope includes home/navigation.
- Ensure resulting UI is unique/non-standard, not default Bootstrap template styling.

## Evidence and Logging
- Keep hook logging enabled so prompts and tool calls are recorded in lab-2/sub-agent_log.txt.
- Sub-agent invocation must be visible in logs as tool call runSubagent with agentName "UX/UI Agent".
- Treat missing runSubagent log evidence as incomplete UI task.

## Quality Bar
- Prefer maintainable CSS tokens/variables and consistent visual system across pages.
- Validate route correctness, readability, and mobile responsiveness after UI changes.
- Validate breadcrumb correctness and end-to-end navigation path for each entity.
- Prefer semantic HTML and baseline accessibility (focus visibility, heading hierarchy, readable contrast).

## Definition Of Done (UI Tasks)
- UX/UI Agent invoked and visible in logs.
- All required pages render and routes work.
- Navigation complete across menu, list, details, back links, and breadcrumbs.
- Visual style is coherent and non-default.
- No regressions to controller/repository wiring.
