# Protecting Sensitive Data in Event-Sourced Systems with Crypto Shredding

This sample is showing an example of using the Crypto Shredding pattern with [EventStoreDB](https://developers.eventstore.com). This can be a solution for handling e.g. [European General Data Protection Regulation](https://en.wikipedia.org/wiki/General_Data_Protection_Regulation).

Read more in the [Diego Martin](https://github.com/diegosasw) article ["Protecting Sensitive Data in Event-Sourced Systems with Crypto Shredding"](https://www.eventstore.com/blog/protecting-sensitive-data-in-event-sourced-systems-with-crypto-shredding-1);

## Prerequisities

- .NET 5.0 - https://dotnet.microsoft.com/download/dotnet/6.0.
- Visual Studio 2022, Jetbrains Rider or VSCode.
- Docker - https://docs.docker.com/docker-for-windows/install/.

## Running

1. Run: `docker-compose up`.
2. Wait until all Docker containers are up and running.
3. Check that you can access each started component the following URL:
     - EventStoreDB UI: http://localhost:2113/
4. Open, build and run [tests](./src/CryptoShredding.IntegrationTests/EventStoreTests/GetEventsTests.cs) in [CryptoShredding.sln](CryptoShredding.sln) solution.

## Overview

The general flow for using Crypto Shredding patern:

1. Identify sensitive data in an event. See:
    - [PersonalDataAttribute](./src/CryptoShredding/Attributes/PersonalDataAttribute.cs).
2. Associate sensitive data to a subject. See:
    - [DataSubjectIdAttribute ](./src/CryptoShredding/Attributes/DataSubjectIdAttribute.cs),
    - and the usage in [ContactAdded event](./src/CryptoShredding.IntegrationTests/EventStoreTests/GetEventsTests.cs#L274).
3. Store private encryption keys. See:
    - [EncryptionKey](./src/CryptoShredding/Repository/EncryptionKey.cs),
    - [CryptoRepository](./src/CryptoShredding/Repository/CryptoRepository.cs).
4. Get rid of the private encryption key when desired. See:
    - `DeleteEncryptionKey` method in [CryptoRepository](./src/CryptoShredding/Repository/CryptoRepository.cs).
5. Cryptographic algorithm to use when encrypting and decrypting. See:
    - [EncryptorDecryptor](./src/CryptoShredding/Serialization/EncryptorDecryptor.cs).
6. Encrypt text and other data types. See:
    - [FieldEncryptionDecryption](./src/CryptoShredding/Serialization/JsonConverters/FieldEncryptionDecryption.cs).
7. Upstream serialization with encryption mechanism. See:
    - [SerializationContractResolver](./src/CryptoShredding/Serialization/ContractResolvers/SerializationContractResolver.cs),
    - [EncryptionJsonConverter](./src/CryptoShredding/Serialization/JsonConverters/EncryptionJsonConverter.cs),
    - [JsonSerializerSettingsFactory](./src/CryptoShredding/Serialization/JsonSerializerSettingsFactory.cs),
    - [SerializedEvent](./src/CryptoShredding/Serialization/SerializedEvent.cs).
8. Decrypt text and masking mechanism when it cannot be decrypted. See:
    - [FieldEncryptionDecryption](./src/CryptoShredding/Serialization/JsonConverters/FieldEncryptionDecryption.cs).
9. Downstream deserialization with decryption mechanism. See:
    - [DeserializationContractResolver](./src/CryptoShredding/Serialization/ContractResolvers/DeserializationContractResolver.cs),
    - [DecryptionJsonConverter](./src/CryptoShredding/Serialization/JsonConverters/DecryptionJsonConverter.cs).
10. Wire up together with an [EventStoreDB](https://developers.eventstore.com) gRPC client. See:
    - [EventConverter](./src/CryptoShredding/EventConverter.cs),
    - [EventStore](./src/CryptoShredding/EventStore.cs).
11. Test everything with an [EventStoreDB](https://developers.eventstore.com). See:
    - [GetEventsTests](./src/CryptoShredding.IntegrationTests/EventStoreTests/GetEventsTests.cs).
