---
description: Keep the admin settings form compact, readable, and mobile-safe.
applyTo: 'Client/src/app/admin/settings/**/*.{html,scss,ts}'
---

# Admin settings UI

- Keep each editable control visually identifiable with a visible border, white background, and a clear focus state. Do not remove the border from both the input and its wrapper.
- The SMTP form must fit the common mobile viewport without unnecessary vertical repetition: use a compact two-column grid on phones, make password full width, and stack only when the content would become unreadable.
- Treat the admin bottom navigation as part of the available viewport; avoid large vertical gaps that push primary save/test actions below it.
- After layout changes, validate both the desktop and mobile breakpoints and confirm the built page contains only one instance of each SMTP field.
