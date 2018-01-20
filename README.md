# Twilio Ethereum

Broadcast signed ethereum transactions from a queue. You have 14 days to broadcast the transaction or the transaction will be deleted. The broadcast may be triggered by SMS from an authorized phone number or by an authorized AWS IoT Button.

## Send Signed Transaction Hex to Twilio Endpoint

Send a transaction hex prefixed with `0x` up to 1,600 characters. A simple Ethereum signed transaction hex is only about 300 characters with only a timestamp as arbitrary data.

    0x000000000000000

Receive

    Inserted signed transaction hex into queue. Queue message md5 hash [MD5_HASH]


## Execute Queued Transaction

Send

    CONFIRM

Receive

    Broadcast [BATCH_COUNT] transactions to [ETHEREUM_NET] { jsonrpc: "2.0", id: 0, result: "[ETHEREUM_TRANSACTION_HEX]" }

## Check Number of Queued Transactions

Send

    COUNT

Receive

    [COUNT] unsent transactions.

## Clear Queued Transactions

Send

    PURGE

Receive

    OK - purged unsent transactions.

## Troubleshooting

### Transaction Fails to Process
TwilioEthereum will always use a `messageId` starting from 0 in the jsonrpc message container. When the incorrect `messgeId` is used, the transaction may be rejected. When using [TransferEther](https://github.com/timg456789/EtherTransfer) to create the signed transaction hex and the `messageId` is out of sync, the transaction will succeed about an hour after creation. The signed transaction hex can be re-sent by SMS/MMS any number of times.


### Transaction Hex is Truncated
**Send the signed transaction hex as an MMS by attaching an image.** SMS supports 160 characters, but some carriers and applications may allow messages greater than 160 characters to be received as a single message. Twilio supports up to 1,600 characters inbound and outbound by SMS.

This issue can be identified when executing the transaction with `CONFIRM` returns error code -32000 value size exceeds available input length.

### Can't setup AWS IoT Button

Use the AWS BTN Dev app to configure the AWS IoT Button automatically in AWS

https://github.com/ethereum/wiki/wiki/JSON-RPC