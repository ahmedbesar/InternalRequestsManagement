# Internal Requests Management

A feature for creating, tracking, and reviewing **internal requests** inside an
organization (bug reports, leave requests, access requests, deployments, and
more). It is built on the **ABP Framework** (ASP.NET Core MVC / Razor Pages,
Entity Framework Core, SQL Server) using a layered solution structure.

The core idea: every request belongs to an **Organization Unit (OU)**, and what
a user can see and do is driven by the OU they belong to and the role they have.

---

## 1. What the feature does

A request captures the work item that one person asks another part of the
company to handle. Each request has:

- **Title** and **description**
- **Request type** (e.g. *Bug Report*, *Leave Request*, *Deployment Request*) –
  the available types depend on the Organization Unit
- **Priority** – `Low`, `Normal`, `High`, `Critical`
- **Status** – the request lifecycle (see below)
- **Requester** – the user who created it (captured automatically)
- **Assigned person** – who is responsible for resolving it
- **Due date**
- **Organization Unit context** – the OU the request belongs to
- **Justification / status notes** – required in certain situations

The feature includes a **requests list** (search, filter, sort, paging), a
**detail view** with full status history, modal-based **create / edit / assign /
change-status** screens, and a **dashboard** that summarizes the current state.

### Request lifecycle (statuses)

```
Draft ──▶ Submitted ──▶ In Progress ──▶ Resolved ──▶ Closed
   │           │              │   ▲           │
   │           │              ▼   │           └──▶ (re-open) In Progress
   │           │           On Hold┘
   ▼           ▼              │
Cancelled   Rejected ◀────────┘
```

- Allowed transitions are enforced in the domain layer; invalid jumps are
  rejected.
- `Closed`, `Cancelled`, and `Rejected` are **terminal** – a request in these
  states can no longer be edited or transitioned.
- Moving to **On Hold**, **Rejected**, or **Cancelled** **requires a note**.
- Every status change is recorded in the request's **status history** (who,
  when, from → to, and the note).

### Dynamic business rules

The form is not static – its requirements change with the data entered and the
selected request type:

- **Request type drives requirements** – some types require a **justification**
  (e.g. *Feature Request*, *Access Request*) and some require a **due date**
  (e.g. *Leave Request*, *Deployment Request*).
- **`Critical` priority** always forces both a **justification** and a **due
  date**, regardless of type.
- **Available request types depend on the OU** – an OU only sees its own types
  plus the global ones (see below).
- These rules are validated **both on the client** (immediate feedback) **and on
  the server** (authoritative), so the UI and backend never disagree.

### Dashboard / summary view

The dashboard (scoped to the current user's OU context) shows:

- **Open**, **Overdue**, and **Unassigned** request counts
- **Breakdown by status**
- **Breakdown by type**
- **Activity by Organization Unit**
- **Most active assignees**

---

## 2. Organization Units and the 3 roles

### Organization Unit hierarchy

The seeder creates this OU tree under the company root **T4Trust**:

```
T4Trust
├── Software Development
│   ├── UI/UX Team
│   ├── Backend Team
│   └── DevOps Team
└── Human Resources
    ├── Recruitment Team
    ├── Employee Relations Team
    └── Training Team
```

Each request type is attached to an OU (or is **global**). A user assigned to a
team sees the request types of that team **and** the global ones (*General
Request*, *Access Request*, *Document Request*).

### How visibility works with OUs (relevance-based)

Visibility is **derived from the user's own Organization Unit** – there is no
"see everything" bypass. A user can see a request if **any** of these is true:

1. The request belongs to the user's **OU subtree** (their deepest assigned OU
   and all of its descendants), **or**
2. The user **created** the request, **or**
3. The request is **assigned to** the user.

The subtree is resolved by walking the OU tree via `ParentId`, so a manager of
**Software Development** sees Software Development + UI/UX + Backend + DevOps,
but **not** Human Resources. Someone in the **UI/UX Team** only sees UI/UX
requests (plus their own / assigned ones). Assignment is also constrained: you
can only assign a request to a user inside the request's OU subtree.

### The 3 roles

Roles are permission-based. The recommended setup uses three roles, each granted
the matching permissions under **Internal Requests Management** (configured in
*Administration → Identity → Roles*):

| Role | Typical permissions | What they can do |
|------|--------------------|------------------|
| **Employee** | `Requests` (view), `Requests.Create` | Create requests and view the ones relevant to them (their OU subtree + own + assigned). Track status and read history. |
| **Manager** | `Requests` + `Create`, `Edit`, `ChangeStatus`, `Assign`, `Dashboard` | Everything an Employee can do, plus edit requests, move them through the lifecycle, assign people within their OU subtree, and use the dashboard for their OU. |
| **Administrator** | All of the above | Full access; combined with an assignment to the **root OU** an admin effectively sees the whole company. |

> Roles interact with OUs by **layering**: the **role** decides *which actions*
> a user may perform, while the user's **Organization Unit** decides *which
> requests* those actions apply to. A Manager in HR manages HR requests; a
> Manager in Software Development manages Software Development requests.

---

## 3. How to run the project

### Prerequisites

- [.NET 10.0+ SDK](https://dotnet.microsoft.com/download/dotnet)
- **SQL Server** (LocalDB, Express, or full) reachable from the connection string

### Step 1 – Configure the database connection

Check the `ConnectionStrings:Default` value in the `appsettings.json` of both:

- `src/InternalRequestsManagement.DbMigrator`
- `src/InternalRequestsManagement.Web`

Update it if your SQL Server instance or database name differs.

### Step 2 – Create the database and seed data

Run the migrator once. It applies all EF Core migrations and seeds the initial
data (admin user, OU hierarchy, and request types):

```bash
dotnet run --project src/InternalRequestsManagement.DbMigrator
```

> If the client-side libraries are missing, run `abp install-libs` in the
> solution root first. (Pre-installed in this repository, so usually not needed.)

### Step 3 – Run the web application

```bash
dotnet run --project src/InternalRequestsManagement.Web
```

Then browse to the URL shown in the console (e.g. `https://localhost:44397`).

### Step 4 – Log in and set up roles

Sign in with the default administrator account created by the seeder:

- **Username:** `admin`
- **Password:** `1q2w3E*`

Then, to exercise the OU/role behavior:

1. Go to **Administration → Identity → Roles** and create **Employee**,
   **Manager**, and **Administrator** roles, granting the permissions described
   above.
2. Go to **Administration → Identity → Organization Units** and assign users to
   the relevant OUs (e.g. a manager to *Software Development*, an employee to
   *UI/UX Team*).
3. Open **Requests** and **Dashboard** from the main menu and create a few
   requests as different users to see how OU scoping and role permissions apply.

---

## 4. Solution structure

A layered monolith:

| Project | Responsibility |
|---------|----------------|
| `InternalRequestsManagement.Domain.Shared` | Enums, constants, error codes, localization |
| `InternalRequestsManagement.Domain` | Entities, the `RequestManager` domain service, repository interfaces |
| `InternalRequestsManagement.Application.Contracts` | DTOs, service interfaces, permissions |
| `InternalRequestsManagement.Application` | Application services, validators, OU subtree resolver |
| `InternalRequestsManagement.EntityFrameworkCore` | DbContext, repositories, migrations, data seeders |
| `InternalRequestsManagement.Web` | Razor Pages UI (list, modals, dashboard), menus |
| `InternalRequestsManagement.DbMigrator` | Console app that migrates and seeds the database |

The `test` folder contains the `Domain.Tests`, `Application.Tests`, and
`EntityFrameworkCore.Tests` projects.

---

## 5. Notes for reviewers (deliverables)

### Understanding of the feature

Internal Requests Management lets users **create, manage, track, and review**
work requests inside the company. The heart of the feature is that a request is
not just a record with fields – it lives inside an **Organization Unit** and
moves through a **clear lifecycle**, and both of those drive real behavior:
which request types are available, who can be assigned, what extra information is
mandatory, what each user is allowed to see and do, and what the dashboard
reports. The implementation covers all seven business areas: creation, full
management (view/edit/status), a searchable & filterable list, OU-based behavior,
dynamic form rules, a summary dashboard, and user-context awareness (own /
assigned / my unit / unassigned / overdue scopes).

### Assumptions

- Roles and per-user OU assignments are configured through the standard ABP
  Identity UI (the sensible 3-role model in section 2 is the intended setup).
- A single tenant / single company is the primary scenario.
- A user's "home" OU is their **deepest** assigned Organization Unit; that
  defines the subtree of requests they can see.

### Important decisions

- **Relevance-based visibility** instead of a blanket "view all" permission, so
  users only see requests connected to their OU subtree, or that they created or
  were assigned. This keeps data isolation meaningful across departments.
- **Rich domain model** – status transitions, note/justification/due-date rules,
  and assignment constraints live in the `RequestManager` domain service and
  return explicit results, so the rules can't be bypassed from the UI.
- **Server + client validation parity** – the same rules run on both sides.
- The OU seeder **saves each unit individually** so ABP assigns a unique
  hierarchy code per sibling (the basis for subtree resolution).

### What I would improve given more time

- Seed sample roles, users, and example requests automatically for a one-command
  demo.
- Email / in-app notifications on assignment and status changes.
- Automated unit and integration tests around the lifecycle and OU scoping.
- Configurable, per-OU request type management from the UI.

---

## Additional resources

- ABP Framework documentation: https://abp.io/docs/latest
- Deployment guidance: https://abp.io/docs/latest/Deployment/Index
