using Xunit;
using TinyMapper.Handler;
using System;

namespace TinyMapper.UnitTests
{
    public class AddConvertersTests
    {
        // TODO: What is this supposed to do?
        [Fact]
        public static void Can_add_converters()
        {
            // Arrange
            var converter = new {
                Name = "Foo"
            };

            // Act
            MappingHandler.AddConverters(converter);

            //Assert
        }
    }

    class Foo
    {
        [TypeConverter]
        public string Name { get; set; }
    }
}