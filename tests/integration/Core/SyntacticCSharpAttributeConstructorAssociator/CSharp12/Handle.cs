namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Associators.Commands;
using Paraminter.Commands.Handlers;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Models;
using Paraminter.Parameters.Method.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void AttributeUsage_NormalArguments_RecordsAll()
    {
        var source = """
            using System;

            [Test(1, "", false)]
            public class Foo { }

            public class TestAttribute : Attribute
            {
                public TestAttribute(int a, string b, bool c) { }
            }
            """;

        var compilation = CompilationFactory.Create(source);

        var type = compilation.GetTypeByMetadataName("Foo")!;
        var parameters = type.GetAttributes()[0].AttributeConstructor!.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];

        var attributeSyntax = syntaxTree.GetRoot().DescendantNodes().OfType<AttributeSyntax>().Single();
        var syntacticArguments = attributeSyntax.ArgumentList!.Arguments;

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns(parameters);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.NormalRecorderMock.Verify(RecordNormalExpression(parameters[0], syntacticArguments[0]), Times.Once());
        Fixture.NormalRecorderMock.Verify(RecordNormalExpression(parameters[1], syntacticArguments[1]), Times.Once());
        Fixture.NormalRecorderMock.Verify(RecordNormalExpression(parameters[2], syntacticArguments[2]), Times.Once());
        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Exactly(3));

        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Never());
    }

    [Fact]
    public void AttributeUsage_ParamsArguments_RecordsAll()
    {
        var source = """
            using System;

            [Test(1, 2, 3)]
            public class Foo { }

            public class TestAttribute : Attribute
            {
                public TestAttribute(params int[] a) { }
            }
            """;

        var compilation = CompilationFactory.Create(source);

        var type = compilation.GetTypeByMetadataName("Foo")!;
        var parameters = type.GetAttributes()[0].AttributeConstructor!.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];

        var attributeSyntax = syntaxTree.GetRoot().DescendantNodes().OfType<AttributeSyntax>().Single();
        var syntacticArguments = attributeSyntax.ArgumentList!.Arguments;

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns(parameters);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.ParamsRecorderMock.Verify(RecordParamsExpression(parameters[0], syntacticArguments), Times.Once());
        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Exactly(1));

        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Never());
    }

    [Fact]
    public void AttributeUsage_DefaultArgument_RecordsAll()
    {
        var source = """
            using System;

            [Test]
            public class Foo { }

            public class TestAttribute : Attribute
            {
                public TestAttribute(int a = 3) { }
            }
            """;

        var compilation = CompilationFactory.Create(source);

        var type = compilation.GetTypeByMetadataName("Foo")!;
        var parameters = type.GetAttributes()[0].AttributeConstructor!.Parameters;

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> commandMock = new();

        commandMock.Setup((query) => query.Data.Parameters).Returns(parameters);
        commandMock.Setup((query) => query.Data.SyntacticArguments).Returns([]);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.DefaultRecorderMock.Verify(RecordDefaultExpression(parameters[0]), Times.Once());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Exactly(1));

        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Never());
    }

    private static Expression<Action<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>> RecordNormalExpression(
        IParameterSymbol parameterSymbol,
        AttributeArgumentSyntax syntacticArgument)
    {
        return (recorder) => recorder.Handle(It.Is(MatchRecordNormalCommand(parameterSymbol, syntacticArgument)));
    }

    private static Expression<Func<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>, bool>> MatchRecordNormalCommand(
        IParameterSymbol parameterSymbol,
        AttributeArgumentSyntax syntacticArgument)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchNormalArgumentData(syntacticArgument, command.ArgumentData);
    }

    private static Expression<Action<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>> RecordParamsExpression(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
    {
        return (recorder) => recorder.Handle(It.Is(MatchRecordParamsCommand(parameterSymbol, syntacticArguments)));
    }

    private static Expression<Func<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>, bool>> MatchRecordParamsCommand(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchParamsArgumentData(syntacticArguments, command.ArgumentData);
    }

    private static Expression<Action<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>> RecordDefaultExpression(
        IParameterSymbol parameterSymbol)
    {
        return (recorder) => recorder.Handle(It.Is(MatchRecordDefaultCommand(parameterSymbol)));
    }

    private static Expression<Func<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>, bool>> MatchRecordDefaultCommand(
        IParameterSymbol parameterSymbol)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter);
    }

    private static bool MatchParameter(
        IParameterSymbol parameterSymbol,
        IMethodParameter parameter)
    {
        return ReferenceEquals(parameterSymbol, parameter.Symbol);
    }

    private static bool MatchNormalArgumentData(
        AttributeArgumentSyntax syntacticArgument,
        INormalCSharpAttributeConstructorArgumentData argumentData)
    {
        return ReferenceEquals(syntacticArgument, argumentData.SyntacticArgument);
    }

    private static bool MatchParamsArgumentData(
        IReadOnlyList<AttributeArgumentSyntax> syntacticArguments,
        IParamsCSharpAttributeConstructorArgumentData argumentData)
    {
        return Enumerable.SequenceEqual(syntacticArguments, argumentData.SyntacticArguments);
    }

    private void Target(
        IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData> command)
    {
        Fixture.Sut.Handle(command);
    }
}
