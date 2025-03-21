using System;
using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;

namespace NForza.Cyrus.Templating
{
    public static class CyrusFilters
    {
        public static ValueTask<FluidValue> ContractName(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.Type == FluidValues.String)
            {
                var text = input.ToStringValue();
                if (!string.IsNullOrEmpty(text))
                {
                    text += "Contract";
                }
                return new ValueTask<FluidValue>(new StringValue(text));
            }

            return new ValueTask<FluidValue>(input);
        }
    }
}