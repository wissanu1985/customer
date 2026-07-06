# WebUi Component Conventions

## File Separation

Blazor component files MUST be split into three separate files for maintainability and clean separation of concerns:

| File | Purpose |
|---|---|
| `*.razor` | Markup only — HTML, components, directives (`@page`, `@inject`, `@attribute`). No `@code` block. |
| `*.razor.cs` | C# logic — partial class containing fields, lifecycle methods, event handlers, DI-injected services. |
| `*.razor.css` | Component-scoped CSS — styles isolated to this component via Blazor CSS isolation. |

### Rules

1. **No inline `@code` blocks** in `.razor` files. All C# logic goes in the `.razor.cs` partial class.
2. The partial class in `.razor.cs` must use the same root namespace and class name as the component.
3. Create `.razor.css` only when the component has custom styles. Do not create empty CSS files.
4. Keep markup in `.razor` clean and declarative — move complex expressions into properties/methods in `.razor.cs`.
5. All three files must live in the same directory and share the same base filename.

### Example

```
SearchClitizen.razor       # markup
SearchClitizen.razor.cs    # logic (partial class)
SearchClitizen.razor.css   # scoped styles (optional, when needed)
```

### When editing existing components

If a component currently has an inline `@code` block, refactor it into a `.razor.cs` partial class as part of the change.
