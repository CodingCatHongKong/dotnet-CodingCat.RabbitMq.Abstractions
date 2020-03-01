using CodingCat.Mq.Abstractions;
using System.Collections.Generic;

namespace CodingCat.RabbitMq.Abstractions.Tests.Impls
{
    public class SimpleProcessor<TInput> : Processor<TInput>
    {
        public List<TInput> ProcessedInputs { get; } = new List<TInput>();

        protected override void Process(TInput input)
        {
            this.ProcessedInputs.Add(input);
        }
    }
}