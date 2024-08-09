namespace Paraminter.CSharp.Attributes.Constructor.Phrike.Commands;

using Paraminter.Arguments.Models;
using Paraminter.Parameters.Method.Models;
using Paraminter.Recorders.Commands;

internal static class RecordCSharpAttributeConstructorAssociationCommandFactory
{
    public static IRecordArgumentAssociationCommand<IMethodParameter, TArgumentData> Create<TArgumentData>(
        IMethodParameter parameter,
        TArgumentData argumentData)
        where TArgumentData : IArgumentData
    {
        return new RecordCSharpAttributeConstructorAssociationCommand<TArgumentData>(parameter, argumentData);
    }

    private sealed class RecordCSharpAttributeConstructorAssociationCommand<TArgumentData>
        : IRecordArgumentAssociationCommand<IMethodParameter, TArgumentData>
        where TArgumentData : IArgumentData
    {
        private readonly IMethodParameter Parameter;
        private readonly TArgumentData ArgumentData;

        public RecordCSharpAttributeConstructorAssociationCommand(
            IMethodParameter parameter,
            TArgumentData argumentData)
        {
            Parameter = parameter;
            ArgumentData = argumentData;
        }

        IMethodParameter IRecordArgumentAssociationCommand<IMethodParameter, TArgumentData>.Parameter => Parameter;
        TArgumentData IRecordArgumentAssociationCommand<IMethodParameter, TArgumentData>.ArgumentData => ArgumentData;
    }
}
