# ACC.Application

The **Application** module coordinates workflows that do not naturally belong to a single bounded context.

It does not own core domain concepts. Instead, it composes bounded-context use cases into larger application workflows.

The current application workflow lets an already registered **User** complete onboarding by creating an **Accounting Subject** and receiving the **Owner** role for it.

## Ontology Diagram

```mermaid
flowchart LR

    CompleteOnboarding["CompleteOnboarding"]

    User["User"]
    AccountingSubject["AccountingSubject"]
    Owner["Owner"]

    CreateAccountingSubject["CreateAccountingSubject"]
    AssignRole["AssignRole"]

    CompleteOnboarding -->|for existing| User
    CompleteOnboarding -->|coordinates| CreateAccountingSubject
    CompleteOnboarding -->|coordinates| AssignRole

    CreateAccountingSubject -->|creates| AccountingSubject
    AssignRole -->|grants| Owner
    Owner -->|for| AccountingSubject
    Owner -->|to| User
```

## Aggregates

No aggregates are owned by this module.

## Use Cases

| Use Case | Description |
| --- | --- |
| CompleteOnboarding | Completes onboarding for an existing user by creating an accounting subject and assigning the Owner role. |

## Events

No application events have been introduced yet.

## Invariants

No application invariants have been introduced yet.
