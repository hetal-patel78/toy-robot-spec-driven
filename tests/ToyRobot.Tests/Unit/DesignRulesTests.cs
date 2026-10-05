using System.Reflection;

namespace ToyRobot.Tests.Unit;

// NFR-07, NFR-07a to NFR-07j; DEC-14, DEC-26, DEC-27, DEC-34, DEC-38; SC-006.
// These tests check code that already exists and guard against later drift.
public class DesignRulesTests
{
    private static readonly Assembly AppAssembly = typeof(Table).Assembly;

    private static readonly BindingFlags AllMembers =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private static readonly Type[] CommandTypes =
    {
        typeof(PlaceCommand), typeof(MoveCommand), typeof(LeftCommand), typeof(RightCommand), typeof(ReportCommand),
    };

    [Fact]
    public void The_public_top_level_types_are_exactly_the_components_on_page_6()
    {
        var expected = new[]
        {
            "CommandParser", "Direction", "IRobotCommand", "IRobotOperator", "LeftCommand", "MoveCommand",
            "Outcome", "ParseResult", "PlaceCommand", "ReportCommand", "RightCommand", "Robot", "RobotOperator", "Runner",
            "RobotState", "Table",
        };

        var actual = AppAssembly.GetTypes()
            .Where(t => t.IsPublic && !t.IsNested)
            .Select(t => t.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expected.OrderBy(name => name, StringComparer.Ordinal), actual);
    }

    [Fact]
    public void Each_public_type_is_in_the_namespace_of_its_folder()
    {
        var expected = new Dictionary<string, string>
        {
            ["Direction"] = "ToyRobot.Domain", ["IRobotOperator"] = "ToyRobot.Domain", ["Robot"] = "ToyRobot.Domain",
            ["RobotOperator"] = "ToyRobot.Domain", ["RobotState"] = "ToyRobot.Domain", ["Table"] = "ToyRobot.Domain",
            ["IRobotCommand"] = "ToyRobot.Commands", ["LeftCommand"] = "ToyRobot.Commands", ["MoveCommand"] = "ToyRobot.Commands",
            ["Outcome"] = "ToyRobot.Commands", ["PlaceCommand"] = "ToyRobot.Commands", ["ReportCommand"] = "ToyRobot.Commands",
            ["RightCommand"] = "ToyRobot.Commands",
            ["CommandParser"] = "ToyRobot.Parsing", ["ParseResult"] = "ToyRobot.Parsing",
            ["Runner"] = "ToyRobot.Session",
        };

        foreach (var type in AppAssembly.GetTypes().Where(t => t.IsPublic && !t.IsNested))
        {
            Assert.True(expected.TryGetValue(type.Name, out var ns), $"{type.Name} has no folder in this test.");
            Assert.Equal(ns, type.Namespace);
        }
    }

    [Fact]
    public void IRobotCommand_declares_exactly_Name_NeedsRobotOnTable_and_Execute()
    {
        var members = typeof(IRobotCommand).GetMembers(AllMembers).Select(Describe).OrderBy(x => x, StringComparer.Ordinal);

        Assert.Equal(
            new[] { "get_Name", "get_NeedsRobotOnTable", "Execute", "Name", "NeedsRobotOnTable" }.OrderBy(x => x, StringComparer.Ordinal),
            members);
    }

    [Fact]
    public void IRobotOperator_declares_exactly_CurrentState_and_TryMoveTo()
    {
        var members = typeof(IRobotOperator).GetMembers(AllMembers).Select(Describe).OrderBy(x => x, StringComparer.Ordinal);

        Assert.Equal(
            new[] { "get_CurrentState", "CurrentState", "TryMoveTo" }.OrderBy(x => x, StringComparer.Ordinal),
            members);
    }

    [Fact]
    public void The_five_command_classes_are_sealed_and_implement_IRobotCommand()
    {
        foreach (var type in CommandTypes)
        {
            Assert.True(type.IsSealed, $"{type.Name} must be sealed.");
            Assert.True(typeof(IRobotCommand).IsAssignableFrom(type), $"{type.Name} must implement IRobotCommand.");
        }
    }

    [Fact]
    public void Outcome_has_exactly_three_nested_public_cases()
    {
        Assert.Equal(
            new[] { "Done", "PositionText", "Warning" },
            NestedPublicNames(typeof(Outcome)));
    }

    [Fact]
    public void ParseResult_has_exactly_three_nested_public_cases()
    {
        Assert.Equal(
            new[] { "CommandObject", "Exit", "Failure" },
            NestedPublicNames(typeof(ParseResult)));
    }

    [Fact]
    public void Only_the_private_field_of_RobotOperator_holds_the_Robot()
    {
        var owners = new[] { typeof(RobotOperator), typeof(Table) }.Concat(CommandTypes);

        foreach (var type in owners)
        {
            foreach (var member in type.GetMembers(AllMembers))
            {
                if (!IsPrivate(member) && MentionsRobot(member))
                {
                    Assert.Fail($"{type.Name}.{member.Name} exposes the Robot.");
                }
            }
        }

        var field = typeof(RobotOperator).GetFields(AllMembers).Single(f => f.FieldType == typeof(Robot));
        Assert.True(field.IsPrivate);
        Assert.True(field.IsInitOnly);
    }

    [Fact]
    public void No_type_has_a_static_field_of_type_Robot()
    {
        foreach (var type in AppAssembly.GetTypes())
        {
            var staticRobotFields = type.GetFields(AllMembers).Where(f => f.IsStatic && f.FieldType == typeof(Robot));
            Assert.Empty(staticRobotFields);
        }
    }

    [Fact]
    public void CommandParser_has_no_instance_fields()
    {
        Assert.Empty(typeof(CommandParser).GetFields(AllMembers).Where(f => !f.IsStatic));
    }

    [Fact]
    public void Robot_Placement_has_no_public_setter()
    {
        var setter = typeof(Robot).GetProperty(nameof(Robot.Placement))!.SetMethod;

        Assert.True(setter is null || !setter.IsPublic);
    }

    private static string[] NestedPublicNames(Type type) =>
        type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
            .Where(t => t.IsNestedPublic)
            .Select(t => t.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

    private static string Describe(MemberInfo member) => member.Name;

    private static bool IsPrivate(MemberInfo member) => member switch
    {
        FieldInfo field => field.IsPrivate,
        MethodBase method => method.IsPrivate,
        PropertyInfo property => (property.GetMethod ?? property.SetMethod)!.IsPrivate,
        Type nested => nested.IsNestedPrivate,
        _ => false,
    };

    private static bool MentionsRobot(MemberInfo member) => member switch
    {
        FieldInfo field => field.FieldType == typeof(Robot),
        PropertyInfo property => property.PropertyType == typeof(Robot),
        MethodInfo method => method.ReturnType == typeof(Robot) || method.GetParameters().Any(p => p.ParameterType == typeof(Robot)),
        ConstructorInfo constructor => constructor.GetParameters().Any(p => p.ParameterType == typeof(Robot)),
        _ => false,
    };
}
