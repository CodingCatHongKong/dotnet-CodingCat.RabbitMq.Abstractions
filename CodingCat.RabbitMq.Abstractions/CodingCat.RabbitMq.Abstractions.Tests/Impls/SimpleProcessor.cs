using CodingCat.Mq.Abstractions;
using System.Collections.Generic;

namespace CodingCat.RabbitMq.Abstractions.Tests.Impls
{
    public class SimpleProcessor<TInput> : BaseProcessor<TInput>
    {
        public List<TInput> ProcessedInputs { get; } = new List<TInput>();

        protected override void Process(TInput input)
        {
            this.ProcessedInputs.Add(input);
        }
    }

    public class SimpleProcessor<TInput, TOutput>
        : BaseDelegatedProcessor<TInput, TOutput>
    {
        #region Constructor(s)

        public SimpleProcessor(ProcessDelegate delegatedProcess)
            : base(delegatedProcess)
        {
        }

        #endregion Constructor(s)
    }
}