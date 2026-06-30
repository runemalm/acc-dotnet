# ACC.Application

The **Application** module coordinates workflows that do not naturally belong to a single bounded context.

It does not own core domain concepts. Instead, it composes bounded-context use cases into larger application workflows.

The current application workflow lets an already registered **User** complete onboarding by establishing an **Accounting Subject** and its founding **Owner** role, then adopting a selected **Chart of Accounts** template.

## Ontology Diagram

```mermaid
flowchart LR

    CompleteOnboarding["CompleteOnboarding"]

    User["User"]
    AccountingSubject["AccountingSubject"]
    Owner["Owner"]
    ChartOfAccounts["ChartOfAccounts"]

    EstablishAccountingSubject["EstablishAccountingSubject"]
    EstablishInitialOwner["EstablishInitialOwner"]
    AdoptChartOfAccounts["AdoptChartOfAccounts"]

    CompleteOnboarding -->|for existing| User
    CompleteOnboarding -->|coordinates| EstablishAccountingSubject
    CompleteOnboarding -->|coordinates| EstablishInitialOwner
    CompleteOnboarding -->|coordinates| AdoptChartOfAccounts

    EstablishAccountingSubject -->|establishes| AccountingSubject
    EstablishInitialOwner -->|establishes| Owner
    Owner -->|for| AccountingSubject
    Owner -->|to| User
    AdoptChartOfAccounts -->|establishes operative| ChartOfAccounts
    ChartOfAccounts -->|for| AccountingSubject
```

## Aggregates

No aggregates are owned by this module.

## Use Cases

| Use Case | Description |
| --- | --- |
| CompleteOnboarding | Completes onboarding by establishing an accounting subject and its founding Owner, then adopting the selected chart-of-accounts template. |

## Events

No application events have been introduced yet.

## Invariants

No application invariants have been introduced yet.
