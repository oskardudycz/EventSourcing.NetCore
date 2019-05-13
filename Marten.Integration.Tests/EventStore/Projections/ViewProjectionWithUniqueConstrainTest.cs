using System;
using System.Linq;
using System.Reflection;
using Baseline;
using Marten.Events.Projections;
using Marten.Integration.Tests.TestsInfrasructure;
using Marten.Schema;
using Marten.Storage;
using Marten.Util;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Projections
{
    public class ViewProjectionWithUniqueConstrainTest: MartenTest
    {
        public class UniqueIndex: IIndexDefinition
        {
            private readonly MemberInfo[][] _members;
            private readonly string _locator = string.Empty;
            private readonly DbObjectName _table;
            private string _indexName;

            public UniqueIndex(DocumentMapping mapping, MemberInfo[][] members)
            {
                _members = members;

                _locator = members
                    .Select(m =>
                    {
                        var sql = mapping.FieldFor(m).SqlLocator.Replace("d.", "");
                        switch (Casing)
                        {
                            case Casings.Upper:
                                return $" upper({sql})";

                            case Casings.Lower:
                                return $" lower({sql})";

                            default:
                                return $" ({sql})";
                        }
                    })
                    .Join(",");

                _locator = $" ({_locator})";

                _table = mapping.Table;
            }

            /// <summary>
            /// Creates the index as UNIQUE
            /// </summary>
            public bool IsUnique { get; set; }

            /// <summary>
            /// Specifies the index should be created in the background and not block/lock
            /// </summary>
            public bool IsConcurrent { get; set; }

            /// <summary>
            /// Specify the name of the index explicity
            /// </summary>
            public string IndexName
            {
                get
                {
                    if (_indexName.IsNotEmpty())
                    {
                        return DocumentMapping.MartenPrefix + _indexName;
                    }

                    return GenerateIndexName();
                }
                set { _indexName = value; }
            }

            /// <summary>
            /// Allows you to specify a where clause on the index
            /// </summary>
            public string Where { get; set; }

            /// <summary>
            /// Marks the column value as upper/lower casing
            /// </summary>
            public Casings Casing { get; set; }

            /// <summary>
            /// Specifies the type of index to create
            /// </summary>
            public IndexMethod Method { get; set; } = IndexMethod.btree;

            public string ToDDL()
            {
                var index = IsUnique ? "CREATE UNIQUE INDEX" : "CREATE INDEX";

                if (IsConcurrent)
                {
                    index += " CONCURRENTLY";
                }

                index += $" {IndexName} ON {_table.QualifiedName}";

                if (Method != IndexMethod.btree)
                {
                    index += $" USING {Method}";
                }

                index += _locator;

                if (Where.IsNotEmpty())
                {
                    index += $" WHERE ({Where})";
                }

                return index + ";";
            }

            private string GenerateIndexName()
            {
                var name = _table.Name;

                name += IsUnique ? "_uidx_" : "_idx_";

                name += _members.First().ToTableAlias();

                return name;
            }

            public bool Matches(ActualIndex index)
            {
                return index != null;
            }

            public enum Casings
            {
                /// <summary>
                /// Leave the casing as is (default)
                /// </summary>
                Default,

                /// <summary>
                /// Change the casing to uppercase
                /// </summary>
                Upper,

                /// <summary>
                /// Change the casing to lowercase
                /// </summary>
                Lower
            }
        }

        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        public class UniqueIndexAttribute: MartenAttribute
        {
            public override void Modify(DocumentMapping mapping, MemberInfo member)
            {
                var membersGroupedByIndexName = member.DeclaringType.GetMembers()
                    .Where(mi => mi.GetCustomAttributes<UniqueIndexAttribute>().Any())
                    .Select(mi => new
                    {
                        Member = mi,
                        IndexInformation = mi.GetCustomAttributes<UniqueIndexAttribute>().First()
                    })
                    .GroupBy(m => m.IndexInformation.IndexName ?? m.Member.Name)
                    .Where(mg => mg.Any(m => m.Member == member))
                    .Single();

                //var indexDefinition = new ComputedIndex(mapping, new[] { member })
                var indexDefinition = new UniqueIndex(mapping, membersGroupedByIndexName.Select(m => new[] { m.Member }).ToArray())
                {
                    Method = IndexMethod
                };

                if (IndexName.IsNotEmpty())
                    indexDefinition.IndexName = IndexName;

                indexDefinition.IsUnique = true;

                if (!mapping.Indexes.Any(ind => ind.IndexName == indexDefinition.IndexName))
                    mapping.Indexes.Add(indexDefinition);
            }

            /// <summary>
            /// Use to override the Postgresql database column type of this searchable field
            /// </summary>
            public string PgType { get; set; } = null;

            /// <summary>
            /// Specifies the type of index to create
            /// </summary>
            public IndexMethod IndexMethod { get; set; } = IndexMethod.btree;

            /// <summary>
            /// Specify the name of the index explicity
            /// </summary>
            public string IndexName { get; set; } = null;
        }

        private class UserCreated
        {
            public Guid UserId { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string Surname { get; set; }
        }

        private class UserView
        {
            public Guid Id { get; set; }

            [UniqueIndex(IndexName = "test")]
            public string UserName { get; set; }

            [UniqueIndex(IndexName = "test")]
            public string Email { get; set; }

            public string FullName { get; set; }
        }

        private class UserViewProjection: ViewProjection<UserView, Guid>
        {
            public UserViewProjection()
            {
                ProjectEvent<UserCreated>(Apply);
            }

            private void Apply(UserView view, UserCreated @event)
            {
                view.Id = @event.UserId;
                view.Email = @event.Email;
                view.FullName = $"{@event.FirstName} {@event.Surname}";

                view.UserName = @event.Email;
            }
        }

        public IDocumentSession CreateSession()
        {
            return base.CreateSession(options =>
            {
                options.Events.AddEventTypes(new[] { typeof(UserCreated) });
                options.Events.InlineProjections.Add(new UserViewProjection());
            });
        }

        [Fact]
        public void GivenTwoEventsWithSameValueForUniqueField_WhenInlineTransformationIsApplied_ThenThrowsExceptions()
        {
            //1. Create Events
            const string email = "john.smith@mail.com";
            var firstEvent = new UserCreated { UserId = Guid.NewGuid(), Email = email, FirstName = "John", Surname = "Smith" };
            var secondEvent = new UserCreated { UserId = Guid.NewGuid(), Email = email, FirstName = "John", Surname = "Smith" };

            using (var session = CreateSession())
            {
                //2. Publish Events
                session.Events.Append(firstEvent.UserId, firstEvent);
                session.Events.Append(secondEvent.UserId, secondEvent);

                Assert.Throws<MartenCommandException>(() =>
                {
                    session.SaveChanges();
                });
            }
        }
    }
}
