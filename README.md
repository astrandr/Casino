# Casino

## Task description:

Build a small casino platform in .NET made of two independently running
services: a Wallet and a Game. How the responsibilities are split between them,
and how they talk to each other, is your decision. Be ready to defend it.
Pick whatever components you are comfortable with. We are not grading the
tooling, we are grading the design of the two services and the way they work
together.
Requirements
A new player starts with a balance of $0. After every operation the new balance
is shown.
The player can deposit and withdraw funds.
The player can bet between $1 and $10 per round.
The game plays out as follows:
50% of the bets lose
40% of the bets win up to x2 the bet amount
10% of the bets win between x2 and x10 the bet amount
After every round the player balance is:

	new balance = old balance - bet amount + win amount

## Prerequisites

- .NET 8 SDK
- Visual Studio 2022 if running from the IDE

## Architecture

The solution consists of the following projects/units:

- Casino.Wallet 
- Casino.Game 
- Casino.UI 
- Casino.Core 
- Casino.Common
- Casino.Test

### Wallet

service running as .NET Core WEB API rest endpoints. Handles deposit, withdraw and settle bet operations. For the sake of the demo and simplicity the storage is implemented just as in-memory property. The in-memory balance resets to $0 when Wallet restarts.

### Game

Service running as .NET Core WEB API rest endpoints. Handles Bet operation. The outcome of the play round is implemented in GameService. Handling of the bet is in BetService:PlaceBetAsync. The method first check for available balance against the amount of the bet. Gets play outcome, calculates the payout and calls Wallet to apply the bet debit and payout atomically.

### Casino.UI

Implements console application which includes parser of entered command and uses Casino.Core

### Casino.Core

Implements CasinoService  library to make calls to Wallet and Game services.

### Casino.Common

Definition of common data structures and helper utilities used in the solution projects.

#### Special notes about the implementation and design
For the purpose of the demo there is no required authentication and authorization in the REST APIs but only one API Key involved in the call to Wallet:SettleBet. To ease handling the task that key is provided in the appsettings.json file but not in secrets or key vault.
Since the task specification is not showing user login for the sake of simplicity there is no per user registration, authentication and handling.
No SQL or other persistent storage is used but the services follow the Controller->Service->Repository pattern.
Under assumption that multiple console app may be running there is some locking mechanism in Wallet services to handle concurency at level only for demonstration.
Again for the purpose of the demo and simplicity there is no Retry and Idempotent consumer patterns involved in the services.
No logging history of operations is implemented for the goal of audits and so on ...


The solution uses Refit library for http calls.

## How to run

In the solution configuration set the following projects to start simultanously: 

- Casino.UI
- Casino.Wallet
- Casino.Game

## How to play

```text
deposit 10
bet 5
withdraw 2
exit
```


## Tests

- CasinoServiceTests - covers basic test for deposit and withdrawal operations in Casino.Core CasinoService
- BetServiceTests - covers basic tests for placing bet under different account balance.
- WalletTests - verifies that concurrent wallet operations preserve the correct balance.


Tests were implemented with the help of GitHub Copilot Agent GPT-6 Luna