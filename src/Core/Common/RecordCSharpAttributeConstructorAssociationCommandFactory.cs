namespace Paraminter.CSharp.Attributes.Constructor.Phrike.Common;

using Paraminter.Associators.Commands;
using Paraminter.Associators.Models;
using Paraminter.Parameters.Method.Models;

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
