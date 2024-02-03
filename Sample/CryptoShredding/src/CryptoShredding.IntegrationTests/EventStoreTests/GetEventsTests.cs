using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoShredding.Attributes;
using CryptoShredding.Contracts;
using CryptoShredding.IntegrationTests.TestSupport;
using CryptoShredding.Repository;
using CryptoShredding.Serialization;
using EventStore.Client;
using FluentAssertions;
using Xunit;

namespace CryptoShredding.IntegrationTests.EventStoreTests;

public static class GetEventsTests
{
    public class ContactAdded: IEvent
    {
        public Guid AggregateId { get; set; }

        [DataSubjectId]
        public Guid PersonId { get; set; }

        [PersonalData] public string Name { get; set; } = default!;

        [PersonalData]
        public DateTime Birthday { get; set; }

        public Address Address { get; set; } = new Address();
    }

    public class Address
    {
        [PersonalData]
        public string Street { get; set; } = default!;

        [PersonalData]
        public int Number { get; set; }

        public string CountryCode { get; set; } = default!;
    }

    public class ContactBookCreated: IEvent
    {
        public Guid AggregateId { get; set; }
    }

    public class Given_A_ContactBookCreated_And_Two_Events_With_Personal_Data_Stored_When_Getting_Events
        : Given_WhenAsync_Then_Test
    {
        private EventStore _sut = default!;
        private EventStoreClient _eventStoreClient = default!;
        private string _streamName = default!;
        private Guid _joeId;
        private Guid _janeId;
        private ContactAdded _expectedContactAddedOne = default!;
        private ContactAdded _expectedContactAddedTwo = default!;
        private IEnumerable<IEvent> _result = default!;

        protected override async Task Given()
        {
            const string connectionString = "esdb://localhost:2113?tls=false";
            _eventStoreClient = new EventStoreClient(EventStoreClientSettings.Create(connectionString));

            var supportedEvents =
                new List<Type>
                {
                    typeof(ContactBookCreated),
                    typeof(ContactAdded)
                };

            var cryptoRepository = new CryptoRepository();
            var encryptorDecryptor = new EncryptorDecryptor(cryptoRepository);
            var jsonSerializerSettingsFactory = new JsonSerializerSettingsFactory(encryptorDecryptor);
            var jsonSerializer = new JsonSerializer(jsonSerializerSettingsFactory, supportedEvents);
            var eventConverter = new EventConverter(jsonSerializer);

            var aggregateId = Guid.NewGuid();
            _streamName = $"ContactBook-{aggregateId.ToString().Replace("-", string.Empty)}";

            _sut = new EventStore(_eventStoreClient, eventConverter);

            var contactBookCreated =
                new ContactBookCreated
                {
                    AggregateId = aggregateId
                };

            _joeId = Guid.NewGuid();
            var contactAddedOne =
                new ContactAdded
                {
                    AggregateId = aggregateId,
                    Name = "Joe Bloggs",
                    Birthday = new DateTime(1984, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    PersonId = _joeId,
                    Address =
                        new Address
                        {
                            Street = "Blue Avenue",
                            Number = 23,
                            CountryCode = "ES"
                        }
                };

            _janeId = Guid.NewGuid();
            var contactAddedTwo =
                new ContactAdded
                {
                    AggregateId = aggregateId,
                    Name = "Jane Bloggs",
                    Birthday = new DateTime(1987, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    PersonId = _janeId,
                    Address =
                        new Address
                        {
                            Street = "Pink Avenue",
                            Number = 33,
                            CountryCode = "ES"
                        }
                };

            var eventsToPersist =
                new List<IEvent>
                {
                    contactBookCreated,
                    contactAddedOne,
                    contactAddedTwo
                };

            var aggregateVersion = eventsToPersist.Count;

            await _sut.PersistEvents(_streamName, aggregateVersion, eventsToPersist);

            _expectedContactAddedOne = contactAddedOne;
            _expectedContactAddedTwo = contactAddedTwo;
        }

        protected override async Task When()
        {
            _result = await _sut.GetEvents(_streamName);
        }

        [Fact]
        public void Then_It_Should_Retrieve_Three_Events()
        {
            _result.Should().HaveCount(3);
        }

        [Fact]
        public void Then_It_Should_Have_Decrypted_The_First_ContactAdded_Event()
        {
            _result.ElementAt(1).Should().BeEquivalentTo(_expectedContactAddedOne);
        }

        [Fact]
        public void Then_It_Should_Have_Decrypted_The_Second_ContactAdded_Event()
        {
            _result.ElementAt(2).Should().BeEquivalentTo(_expectedContactAddedTwo);
        }

        protected override void Cleanup()
        {
            _eventStoreClient.Dispose();
        }
    }

    public class Given_A_ContactBookCreated_And_Two_Events_With_Personal_Data_Stored_And_Encryption_Key_For_One_Is_Deleted_When_Getting_Events
        : Given_WhenAsync_Then_Test
    {
        private EventStore _sut = default!;
        private EventStoreClient _eventStoreClient = default!;
        private string _streamName = default!;
        private Guid _joeId;
        private Guid _janeId;
        private ContactAdded _expectedContactAddedOne = default!;
        private ContactAdded _expectedContactAddedTwo = default!;
        private IEnumerable<IEvent> _result = default!;

        protected override async Task Given()
        {
            const string connectionString = "esdb://localhost:2113?tls=false";
            _eventStoreClient = new EventStoreClient(EventStoreClientSettings.Create(connectionString));

            var supportedEvents =
                new List<Type>
                {
                    typeof(ContactBookCreated),
                    typeof(ContactAdded)
                };

            var cryptoRepository = new CryptoRepository();
            var encryptorDecryptor = new EncryptorDecryptor(cryptoRepository);
            var jsonSerializerSettingsFactory = new JsonSerializerSettingsFactory(encryptorDecryptor);
            var jsonSerializer = new JsonSerializer(jsonSerializerSettingsFactory, supportedEvents);
            var eventConverter = new EventConverter(jsonSerializer);

            var aggregateId = Guid.NewGuid();
            _streamName = $"ContactBook-{aggregateId.ToString().Replace("-", string.Empty)}";

            _sut = new EventStore(_eventStoreClient, eventConverter);

            var contactBookCreated =
                new ContactBookCreated
                {
                    AggregateId = aggregateId
                };

            _joeId = Guid.NewGuid();
            var contactAddedOne =
                new ContactAdded
                {
                    AggregateId = aggregateId,
                    Name = "Joe Bloggs",
                    Birthday = new DateTime(1984, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    PersonId = _joeId,
                    Address =
                        new Address
                        {
                            Street = "Blue Avenue",
                            Number = 23,
                            CountryCode = "ES"
                        }
                };

            _janeId = Guid.NewGuid();
            var contactAddedTwo =
                new ContactAdded
                {
                    AggregateId = aggregateId,
                    Name = "Jane Bloggs",
                    Birthday = new DateTime(1987, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    PersonId = _janeId,
                    Address =
                        new Address
                        {
                            Street = "Pink Avenue",
                            Number = 33,
                            CountryCode = "ES"
                        }
                };

            var eventsToPersist =
                new List<IEvent>
                {
                    contactBookCreated,
                    contactAddedOne,
                    contactAddedTwo
                };

            var aggregateVersion = eventsToPersist.Count;

            await _sut.PersistEvents(_streamName, aggregateVersion, eventsToPersist);
            cryptoRepository.DeleteEncryptionKey(_janeId.ToString());

            _expectedContactAddedOne = contactAddedOne;
            _expectedContactAddedTwo =
                new ContactAdded
                {
                    AggregateId = aggregateId,
                    Name = "***",
                    Birthday = default,
                    PersonId = _janeId,
                    Address =
                        new Address
                        {
                            Street = "***",
                            Number = default,
                            CountryCode = "ES"
                        }
                };
        }

        protected override async Task When()
        {
            _result = await _sut.GetEvents(_streamName);
        }

        [Fact]
        public void Then_It_Should_Retrieve_Three_Events()
        {
            _result.Should().HaveCount(3);
        }

        [Fact]
        public void Then_It_Should_Have_Decrypted_The_First_ContactAdded_Event()
        {
            _result.ElementAt(1).Should().BeEquivalentTo(_expectedContactAddedOne);
        }

        [Fact]
        public void Then_It_Should_Have_Decrypted_The_Second_ContactAdded_Event()
        {
            _result.ElementAt(2).Should().BeEquivalentTo(_expectedContactAddedTwo);
        }

        protected override void Cleanup()
        {
            _eventStoreClient.Dispose();
        }
    }
}
