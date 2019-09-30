using Xunit;
using TinyMapper.Handler;
using System;
using FluentAssertions;
using System.Threading.Tasks;
using System.Linq;

namespace TinyMapper.UnitTests
{
    public class ConvertAsyncTests
    {
        [Fact]
        public async Task Can_convert_primitive_type()
        {
            // Arrange
            MappingHandler.AddMapping<int, bool>(source => source > 0);

            // Act
            var result = await MappingHandler.ConvertAsync<bool>(1);

            //Assert
            result.Should().Be(true);
        }

        [Fact]
        public async Task Can_add_optional_additional_async_conversion_after_convert()
        {
            // Arrange
            MappingHandler.AddMapping<int, int>(source => source + 2);

            // Act
            var result = await MappingHandler.ConvertAsync<int>(1, withTwoAdded => Task.FromResult(withTwoAdded * 3));

            //Assert
            result.Should().Be(9);
        }

        [Fact]
        public async Task Returns_null_given_null_despite_mapping_producing_value()
        {
            // Arrange
            MappingHandler.AddMapping<string, string>(source => "not null");

            // Act
            var result = await MappingHandler.ConvertAsync<string>(null);

            //Assert
            result.Should().Be(null);
        }

        [Fact]
        public void Throws_if_no_mapping_for_the_value_exists()
        {
            // Arrange
            MappingHandler.AddMapping<string, string>(source => "a string mapper");

            // Act
            Func<Task<bool>> convertAction = async () => await MappingHandler.ConvertAsync<bool>("a string value to a bool converter");

            //Assert
            convertAction.Should().Throw<Exception>("because bool has no mapping");
        }
    }

}