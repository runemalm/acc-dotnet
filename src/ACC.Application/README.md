# ACC.Application

The **Application** module coordinates workflows that do not naturally belong to a single bounded context.

It does not own core domain concepts. Instead, it composes bounded-context use cases into larger application workflows.

The current application workflow lets an already registered **User** complete onboarding by creating an **Accounting Subject**, establishing the founding **Owner** role, and adopting a selected **Chart of Accounts** template.

## Ontology Diagram

```mermaid
flowchart LR

    CompleteOnboarding["CompleteOnboarding"]

    User["User"]
    AccountingSubject["AccountingSubject"]
    Owner["Owner"]
    ChartOfAccounts["ChartOfAccounts"]

    CreateAccountingSubject["CreateAccountingSubject"]
    EstablishInitialOwner["EstablishInitialOwner"]
    AdoptChartOfAccounts["AdoptChartOfAccounts"]

    CompleteOnboarding -->|for existing| User
    CompleteOnboarding -->|coordinates| CreateAccountingSubject
    CompleteOnboarding -->|coordinates| EstablishInitialOwner
    CompleteOnboarding -->|coordinates| AdoptChartOfAccounts

    CreateAccountingSubject -->|creates| AccountingSubject
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
| CompleteOnboarding | Completes onboarding by creating an accounting subject, establishing its founding Owner, and adopting the selected chart-of-accounts template. |

## Events

No application events have been introduced yet.

## Invariants

No application invariants have been introduced yet.
