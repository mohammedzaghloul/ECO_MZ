---
name: admin-ui-patterns
description: Build consistent responsive Angular admin screens with safe inline destructive actions.
---

# Admin UI Patterns

Use this pattern for Angular admin list pages in ECO:

- Keep destructive actions inline and reversible until confirmed. Use a two-step row state with confirm and cancel icon buttons; do not use browser `confirm()` or `alert()`.
- Use real Material Icons inside compact buttons with `title` and `aria-label`. Keep view, role, confirm, cancel, and delete actions visually distinct.
- Hide destructive actions that are not valid for protected records, such as deleting users with the `Admin` role.
- Keep table actions in a dedicated flex group and provide a tighter mobile variant. On small screens, prioritize identity and actions while hiding secondary columns.
- Put deletion behind the controller/service boundary: Angular service → API controller → BLL interface/service → `UserManager` or repository. Return an explicit failure when the record is missing or protected.
