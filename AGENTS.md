# AGENTS.md

## Purpose

ACC.NET is a domain-driven accounting application implemented as a modular monolith.

The project uses Domain-Driven Design (DDD) as its primary software design methodology.

In addition to DDD, the project explores how semantic modeling, ontology, institutional theory, and systems thinking can support domain discovery and architectural decision-making.

The accounting domain is the primary business domain currently being implemented.

---

## Development Philosophy

The project distinguishes between:

```text
Semantic Architecture
    ↓ informs

Software Architecture
    ↓ realized as

Implementation
```

Semantic architecture exists to improve understanding of the domain.

Software architecture exists to implement the domain.

Do not assume that semantic structures must map one-to-one to software modules, projects, services, or deployment boundaries.

---

## Domain Discovery

Domain discovery is considered an ongoing activity.

Before introducing new modules, abstractions, services, or workflows, first attempt to understand the underlying domain concepts.

When a new feature is requested, ask:

1. Which domain or subdomain does this belong to?
2. Which bounded context owns the language?
3. Which semantic center appears to own the act?
4. Is a new concept being discovered?

Favor discovery over premature design.

---

## Semantic Centers

The project maintains a semantic context map that serves as an orientation aid during discovery.

Current working map:

```text
Institutional Foundation
├─ Identity
├─ Authority
└─ Accountability

Accounting
├─ AccountingSubject
├─ Ledger
├─ Evidence
├─ Reporting
├─ Receivables
└─ Payables

Taxation
├─ VAT
└─ Tax Declarations

Employment
└─ Payroll

Commerce
├─ Sales
└─ Purchasing
```

This map is exploratory and expected to evolve.

---

## Semantic Ownership

Acts should be owned by the domain concept to which they most naturally belong.

Examples:

```text
Post Journal Entry
    -> Ledger

Declare VAT
    -> VAT

Register Identity
    -> Identity

Close Fiscal Period
    -> Ledger
```

When implementing a use case:

* First attempt to identify a semantic owner.
* Let the owner coordinate the act when reasonable.
* Avoid introducing orchestration layers prematurely.

---

## Application Services

Application services are considered coordination mechanisms, not business concepts.

Before introducing an application service, consider:

* Does a semantic owner already exist?
* Is the context map incomplete?
* Is a missing domain concept being discovered?

Only introduce an application service when no convincing semantic owner can be identified or when explicit workflow coordination provides clear value.

---

## Events

Events are not the default integration mechanism.

Prefer:

```text
Explicit collaboration
```

over:

```text
Event-driven collaboration
```

unless a concrete need exists.

Examples of valid reasons:

* Multiple independent consumers
* External integrations
* Long-running workflows
* Operational decoupling

Avoid introducing events solely because the architecture is modular.

---

## Modular Monolith

ACC.NET starts as a modular monolith.

Modules are logical boundaries, not deployment boundaries.

Do not introduce microservices, distributed messaging, or distributed consistency concerns unless explicitly required.

Favor simplicity.

---

## Code Organization

Each major semantic center should be implemented as a top-level namespace under:

```text
ACC.<SemanticCenter>
```

Examples:

```text
ACC.Ledger
ACC.Evidence
ACC.Reporting
ACC.VAT
ACC.Identity
ACC.Authority
```

Tests should be placed under:

```text
ACC.<SemanticCenter>.Tests
```

within the `tests/` directory.

---

## Module READMEs

Each bounded context module may include a `README.md` at its project root.

A module README is a semantic catalog indexed by DDD/CQRS building blocks. It should help developers move between domain understanding and the code that implements it.

Use software architecture terms for the catalog structure, and semantic or institutional language for the descriptions inside each entry.

Recommended structure:

```text
Purpose
Ontology Diagram
Aggregates
Use Cases
Events
Invariants
```

These sections describe the meaning behind the code, not the mechanics of the code.

They should answer:

```text
Which concepts exist in this module?
What do they mean?
How do they relate?
Where should a developer look in the code?
```

---

## Naming

Prefer business language over technical language.

Prefer:

```text
JournalEntry
FiscalPeriod
VATDeclaration
```

over:

```text
JournalEntryEntity
FiscalPeriodRecord
VATDeclarationModel
```

Names should reflect the ubiquitous language of the domain.

---

## Architectural Preference Order

When faced with multiple design alternatives, prefer:

1. Semantic clarity
2. Domain alignment
3. Simplicity
4. Evolvability
5. Technical flexibility

Avoid introducing abstractions that do not currently solve a domain problem.

---

## Important Principle

The primary goal is not to produce a perfect architecture.

The primary goal is to continuously improve understanding of the domain while building a useful accounting application.

The architecture should evolve as understanding evolves.
