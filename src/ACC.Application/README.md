# ACC.Application

The **Application** module coordinates workflows that do not naturally belong to a single bounded context.

It does not own core domain concepts. Instead, it composes bounded-context use cases into larger application workflows.

The current application workflow lets an already registered **User** complete onboarding by creating an **Accounting Subject** and establishing the founding **Owner** role for it.

## Ontology Diagram

```mermaid
flowchart LR

    CompleteOnboarding["CompleteOnboarding"]

    User["User"]
    AccountingSubject["AccountingSubject"]
    Owner["Owner"]

    CreateAccountingSubject["CreateAccountingSubject"]
    EstablishInitialOwner["EstablishInitialOwner"]

    CompleteOnboarding -->|for existing| User
    CompleteOnboarding -->|coordinates| CreateAccountingSubject
    CompleteOnboarding -->|coordinates| EstablishInitialOwner

    CreateAccountingSubject -->|creates| AccountingSubject
    EstablishInitialOwner -->|establishes| Owner
    Owner -->|for| AccountingSubject
    Owner -->|to| User
```

## Aggregates

No aggregates are owned by this module.

## Use Cases

| Use Case | Description |
| --- | --- |
| CompleteOnboarding | Completes onboarding for an existing user by creating an accounting subject and establishing the founding Owner role. |

## Events

No application events have been introduced yet.

## Invariants

No application invariants have been introduced yet.
