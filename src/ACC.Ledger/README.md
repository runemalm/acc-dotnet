# ACC.Ledger

The **Ledger** records accounting facts for an **Accounting Subject** within **Fiscal Periods**.

Accounting facts are recorded through the posting of **Journal Entries** to accounts recognized by the accounting subject's operative **Chart of Accounts**. The Ledger preserves accounting validity by enforcing accounting invariants and maintaining admissible accounting history over time.

The resulting ledger facts provide the foundation for reporting, taxation, and accounting decisions.

## Ontology Diagram

```mermaid
flowchart LR

    PostJournalEntry["PostJournalEntry"]
    OpenFiscalPeriod["OpenFiscalPeriod"]
    CloseFiscalPeriod["CloseFiscalPeriod"]
    JournalEntryPosted["JournalEntryPosted"]
    FiscalPeriodOpened["FiscalPeriodOpened"]
    FiscalPeriodClosed["FiscalPeriodClosed"]

    JournalEntry["JournalEntry"]
    FiscalPeriod["FiscalPeriod"]
    ChartOfAccounts["ChartOfAccounts"]

    JournalEntryMustBalance["JournalEntryMustBalance"]
    PostingMustOccurInOpenPeriod["PostingMustOccurInOpenPeriod"]
    PostingAccountMustBeRecognized["PostingAccountMustBeRecognized"]
    PostingAccountMustBeActive["PostingAccountMustBeActive"]
    FiscalPeriodMustBeOpenToClose["FiscalPeriodMustBeOpenToClose"]
    RequiredPower["Required Power"]

    PostJournalEntry -->|produces| JournalEntryPosted
    OpenFiscalPeriod -->|produces| FiscalPeriodOpened
    CloseFiscalPeriod -->|produces| FiscalPeriodClosed

    JournalEntryPosted -->|maintains| JournalEntry
    FiscalPeriodOpened -->|opens| FiscalPeriod
    FiscalPeriodClosed -->|closes| FiscalPeriod

    FiscalPeriod -->|governs| PostJournalEntry
    ChartOfAccounts -->|recognizes available accounts for| PostJournalEntry

    JournalEntryMustBalance -. constrains .-> PostJournalEntry
    PostingMustOccurInOpenPeriod -. constrains .-> PostJournalEntry
    PostingAccountMustBeRecognized -. constrains .-> PostJournalEntry
    PostingAccountMustBeActive -. constrains .-> PostJournalEntry
    FiscalPeriodMustBeOpenToClose -. constrains .-> CloseFiscalPeriod
    RequiredPower -. admits .-> OpenFiscalPeriod
    RequiredPower -. admits .-> CloseFiscalPeriod
    RequiredPower -. admits .-> PostJournalEntry
    RequiredPower -. admits .-> ViewJournalEntry
```

## Aggregates

| Aggregate    | Description                                                                    |
| ------------ | ------------------------------------------------------------------------------ |
| JournalEntry | Represents an accounting record belonging to an accounting subject and consisting of one or more journal entry lines. |
| FiscalPeriod | Governs whether accounting facts may be recorded for a period of time.         |

## Use Cases

| Use Case         | Description                                                                                                |
| ---------------- | ---------------------------------------------------------------------------------------------------------- |
| OpenFiscalPeriod | Opens a fiscal period for an accounting subject.                                                           |
| CloseFiscalPeriod | Closes an open fiscal period.                                                                             |
| PostJournalEntry | Records an accounting fact by posting a balanced journal entry to recognized, active accounts within an open fiscal period. |
| ViewJournalEntry | Returns a previously recorded journal entry to an actor empowered to view it for its accounting subject.    |

## Events

| Event              | Meaning                                          |
| ------------------ | ------------------------------------------------ |
| FiscalPeriodOpened | A fiscal period has been opened for posting.     |
| FiscalPeriodClosed | A fiscal period has been closed for posting.     |
| JournalEntryPosted | A journal entry has been recorded for an accounting subject in the ledger. |

## Invariants

The Ledger protects accounting validity through domain invariants.

| Invariant                    | Meaning                                                      |
| ---------------------------- | ------------------------------------------------------------ |
| JournalEntryMustBalance      | Total debits must equal total credits.                       |
| PostingMustOccurInOpenPeriod | Journal entries may only be posted in an open fiscal period. |
| PostingAccountMustBeRecognized | Every posting account must be recognized by the accounting subject's operative chart. |
| PostingAccountMustBeActive | Every posting account must be active when the journal entry is posted. |
| FiscalPeriodMustBeOpenToClose | A fiscal period may only be closed if it is open.            |
