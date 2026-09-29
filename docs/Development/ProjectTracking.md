# Project Tracking

## Current Iteration
- Iteration: [Iter1-EntityRelationship](Iter1-EntityRelationship/PT0001-EntityRelationship.md)
- Overall status: `blocked`
- Progress: `2/3` tasks done
- Current task: `TASK-003` integrated AdminService build validation
- Blocker: untracked `src/Services/AdminService/Controllers/SystemMod/SystemUserController.cs` declares a duplicate `SysUserController` and causes duplicate type/member compile errors.

## Iterations
| Iteration | PT | Status | Progress |
|---|---|---|---|
| [Iter1-EntityRelationship](Iter1-EntityRelationship/PT0001-EntityRelationship.md) | [PT0001](Iter1-EntityRelationship/PT0001-EntityRelationship.md) | blocked | 2/3 |

## Recent Implementation
- Moved EF Core relationship/default-value mappings into module-scoped entity configurations.
- Replaced migration history with a fresh PostgreSQL `Initial` baseline; SQL script generation passed.
- EntityFramework project builds; full AdminService validation remains blocked by the duplicate untracked controller. The migration has not been applied to a database.
