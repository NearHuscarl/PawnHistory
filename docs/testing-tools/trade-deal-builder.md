# TradeDealBuilder

Sets up a trade session and records what was bought, sold, or gifted.

## Constructor

- `TradeDealBuilder(Pawn negotiator)`: start a trade session for the given negotiator.

## Trader Selection

- `WithCaravanTrader(TraderKindDef traderKindDef)`: find a caravan trader pawn.
- `WithOrbitalTrader(TraderKindDef traderKindDef)`: find an orbital trader ship.
- `WithSettlementTrader(Settlement settlement)`: trade with a settlement.

## Trade Actions

- `Buy(Func<Tradeable, bool> filter = null, int count = -1)`: buy a tradeable by filter.
- `Sell(Func<Tradeable, bool> filter = null, int count = -1)`: sell a tradeable by filter.
- `Gift(Func<Tradeable, bool> filter = null, int count = -1)`: mark the session as gift mode and sell by filter.

## Execution

- `Execute()`: run the trade session and return a `TradeSessionResult`.

## Result

- `TradeSessionResult(Bought, Sold, ActuallyTraded)`: records the transaction outcome.
