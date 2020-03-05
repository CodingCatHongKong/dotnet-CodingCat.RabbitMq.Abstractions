using CodingCat.Serializers.Interfaces;
using System;

namespace CodingCat.RabbitMq.Abstractions.Tests.Impls
{
    public class NotImplementedSerializer<T> : ISerializer<T>
    {
        public T FromBytes(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public byte[] ToBytes(T input)
        {
            throw new NotImplementedException();
        }
    }
}