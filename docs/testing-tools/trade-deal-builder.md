# TradeDealBuilder

Selects a trader, performs one trade action, and returns the result immediately.

## Setup

- `scenario.Trade(Pawn negotiator)`: start a trade builder for the negotiator pawn.
- `WithCaravanTrader(TraderKindDef traderKindDef)`: find an on-map caravan trader by kind.
- `WithOrbitalTrader(TraderKindDef traderKindDef)`: find a passing trade ship by kind.
- `WithSettlementTrader(Settlement settlement)`: trade with a settlement directly.

## Actions

- `Buy(Func<Tradeable, bool> filter = null, int count = -1)`: buy the first matching trader-held tradeable.
- `Sell(Func<Tradeable, bool> filter = null, int count = -1)`: sell the first matching colony-held tradeable.
- `Gift(Func<Tradeable, bool> filter = null, int count = -1)`: gift the first matching colony-held tradeable in gift mode.

## Result

- `TradeSessionResult.Bought`: things selected from the trader side.
- `TradeSessionResult.Sold`: things selected from the colony side.
- `TradeSessionResult.ActuallyTraded`: whether the underlying trade deal executed successfully.

There is no public `Execute()`. `Buy(...)`, `Sell(...)`, and `Gift(...)` each perform setup and execution.
