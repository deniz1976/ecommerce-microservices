using System.Reflection;
using System.Text.Json;
using ECommerce.BuildingBlocks.Contracts.Messaging;

namespace ECommerce.ContractTests;

public sealed class MessageContractCompatibilityTests
{
    private static readonly (string Name, Type Type)[] RequiredMetadata =
    [
        (nameof(IMessageContract.MessageId), typeof(Guid)),
        (nameof(IMessageContract.CorrelationId), typeof(Guid)),
        (nameof(IMessageContract.CausationId), typeof(Guid?)),
        (nameof(IMessageContract.OccurredAt), typeof(DateTimeOffset)),
        (nameof(IMessageContract.Version), typeof(int))
    ];

    public static TheoryData<Type> MessageContractTypes
    {
        get
        {
            TheoryData<Type> types = [];
            foreach (Type type in typeof(IMessageContract).Assembly.GetTypes()
                         .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                             typeof(IMessageContract).IsAssignableFrom(type))
                         .OrderBy(type => type.FullName, StringComparer.Ordinal))
            {
                types.Add(type);
            }

            return types;
        }
    }

    [Theory]
    [MemberData(nameof(MessageContractTypes))]
    public void Every_message_keeps_the_shared_metadata_prefix(Type contractType)
    {
        ConstructorInfo constructor = Assert.Single(contractType.GetConstructors());
        ParameterInfo[] parameters = constructor.GetParameters();

        Assert.True(parameters.Length >= RequiredMetadata.Length);
        for (int index = 0; index < RequiredMetadata.Length; index++)
        {
            Assert.Equal(RequiredMetadata[index].Name, contractType.GetProperties()[index].Name);
            Assert.Equal(RequiredMetadata[index].Type, contractType.GetProperties()[index].PropertyType);
            Assert.Equal(RequiredMetadata[index].Type, parameters[index].ParameterType);
        }
    }

    [Theory]
    [MemberData(nameof(MessageContractTypes))]
    public void Current_contract_reader_tolerates_unknown_future_fields(Type contractType)
    {
        const int version = 1;
        string json = $$"""
            {
              "MessageId": "{{Guid.NewGuid()}}",
              "CorrelationId": "{{Guid.NewGuid()}}",
              "CausationId": null,
              "OccurredAt": "2026-08-20T12:00:00+00:00",
              "Version": {{version}},
              "FutureOptionalField": "ignored"
            }
            """;

        IMessageContract? deserialized = JsonSerializer.Deserialize(json, contractType) as IMessageContract;

        Assert.NotNull(deserialized);
        Assert.Equal(version, deserialized.Version);
        Assert.Equal(TimeSpan.Zero, deserialized.OccurredAt.Offset);
    }
}
