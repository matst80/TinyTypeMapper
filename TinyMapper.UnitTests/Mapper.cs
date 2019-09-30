using System;
using System.Collections.Generic;
using Xunit;
using TinyMapper.Handler;
using FluentAssertions;
using System.Threading.Tasks;


[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace TinyMapper.UnitTests
{
    public class Mapper : IDisposable
    {

        [Fact]
        public async Task Mapping_Should_Work_Reversed()
        {
            //Arrange
            MappingHandler.AddMapping(MappingHandler.AutoConverter<FromObject, ToObject>(MappingPropertySource.Target, requireAllProperties: true));
            var fromObject = new FromObject()
            {
                Name = "Testsson",
                Age = 22
            };

            //Act
            var toObject = await fromObject.ConvertAsync<ToObject>();

            //Assert
            fromObject.Name.Should().Be(toObject.Name);
        }

        [Fact]
        public async Task Mapping_Should_Work_With_Wrapped_Elements()
        {
            //Arrange
            MappingHandler.AddMapping(MappingHandler.AutoConverter<FromObject, ToObject>(MappingPropertySource.Target, requireAllProperties: true));
            MappingHandler.AddMapping<string, WrappedValue>((source) => new WrappedValue() { Value = source });
            var fromObject = new FromObject()
            {
                Name = "Testsson",
                WrappedValue = "inner value",
                Age = 22
            };

            //Act
            var toObject = await fromObject.ConvertAsync<ToObject>();

            //Assert
            fromObject.WrappedValue.Should().Be(toObject.WrappedValue.Value);
        }

        [Fact]
        public async Task Mapping_Should_Work_With_Unwrapped_Elements()
        {
            //Arrange
            MappingHandler.AddMapping<WrappedValue, string>((source) => source.Value);
            MappingHandler.AddMapping(MappingHandler.AutoConverter<ToObject, FromObject>(MappingPropertySource.Target, requireAllProperties: true));

            var fromObject = new ToObject()
            {
                Name = "Testsson",
                WrappedValue = new WrappedValue()
                {
                    Value = "sklep"
                },
                Age = 22
            };

            //Act
            var toObject = await fromObject.ConvertAsync<FromObject>();

            //Assert
            fromObject.WrappedValue.Value.Should().Be(toObject.WrappedValue);
        }

        [Fact]
        public async Task Mapping_Should_Work_With_Manual_Addon_for_automapper_Elements()
        {
            //Arrange
            MappingHandler.AddMapping<string, WrappedValue>((source) => new WrappedValue() { Value = source });
            MappingHandler.AddMapping(MappingHandler.AutoConverter<ExtendedFromObject, ExtendedToObject>(async (from, to) =>
            {
                await Task.Delay(100);
                to.WrappedValue.OtherValue = from.OtherWrappedValue;
                return to;
            }, MappingPropertySource.Target));


            var fromObject = new ExtendedFromObject()
            {
                Name = "Testsson",
                WrappedValue = "inner value",
                OtherWrappedValue = "second value",
                Age = 22
            };

            //Act
            var toObject = await fromObject.ConvertAsync<ExtendedToObject>();

            //Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                fromObject.WrappedValue.Should().Be(toObject.WrappedValue.Value);
                fromObject.OtherWrappedValue.Should().Be(toObject.WrappedValue.OtherValue);
            }
        }

        [Fact]
        public async Task Automapping_should_work_with_enums()
        {
            //Arrange
            MappingHandler.AddMapping(MappingHandler.AutoConverter<FromObject, ToObject>(MappingPropertySource.Target, requireAllProperties: true));
            var fromObject = new FromObject()
            {
                Name = "Testsson",
                EnumValue = "Yes",
                Age = 22
            };

            //Act
            var toObject = await fromObject.ConvertAsync<ToObject>();

            //Assert
            toObject.TestValue.Should().Be(TestEnum.Yes);
        }

         [Fact]
        public async Task Automapping_should_work_with_enumerable()
        {
            //Arrange
            MappingHandler.AddMapping(MappingHandler.AutoConverter<FromObject, ToObject>(MappingPropertySource.Target, requireAllProperties: true));
            var fromObject = new FromObject()
            {
                Numbers = new [] { 1, 2, 3}
            };

            //Act
            var toObject = await fromObject.ConvertAsync<ToObject>();

            //Assert
            toObject.Numbers.Should().Equal(new [] { 1, 2, 3});
        }

        [Fact]
        public async Task Automapping_should_work_with_dictionaries()
        {
            //Arrange
            MappingHandler.AddMapping(MappingHandler.AutoConverter<FromObject, ToObject>(MappingPropertySource.Target, requireAllProperties: true));
            var fromObject = new FromObject()
            {
                Table = new Dictionary<int, string> {
                    {1, "1"},
                    {2, "2"},
                    {3, "3"}
                }
            };

            //Act
            var toObject = await fromObject.ConvertAsync<ToObject>();

            //Assert
            toObject.Table.Should().Equal(new Dictionary<int, string> {
                    {1, "1"},
                    {2, "2"},
                    {3, "3"}
            });
        }


        [Fact]
        public async Task Automapping_should_work_with_enums_tostring()
        {
            //Arrange
            MappingHandler.AddMapping(MappingHandler.AutoConverter<ToObject, FromObject>(MappingPropertySource.Target, requireAllProperties: true));
            var fromObject = new ToObject()
            {
                Name = "Testsson",
                TestValue = TestEnum.No,
                Age = 22
            };

            //Act
            var toObject = await fromObject.ConvertAsync<FromObject>();

            //Assert
            toObject.EnumValue.Should().Be("No");

        }

        [Fact]
        public async Task Allow_mapperoverwrite()
        {
            // Arrange
            MappingHandler.AddMapping(MappingHandler.AutoConverter<FromObject, ToObject>(MappingPropertySource.Target, requireAllProperties: true));

            MappingHandler.OnMappingOverwrite += (form, to) =>
            {
                return true;
            };
            // Act
            //Action shouldAddExisitingMapping = () =>
            MappingHandler.AddMapping<FromObject, ToObject>(async (source) => new ToObject()
            {
                Name = "NewMapper!"
            });
            //); ;

            var result = await new FromObject().ConvertAsync<ToObject>();

            //Assert
            Assert.Equal("NewMapper!", result.Name);
        }

        [Fact]
        public async Task Disallow_mapperoverwrite()
        {
            // Arrange
            MappingHandler.AddMapping(MappingHandler.AutoConverter<FromObject, ToObject>(MappingPropertySource.Target, requireAllProperties: true));

            MappingHandler.OnMappingOverwrite += (form, to) =>
            {
                return false;
            };
            // Act
            Action shouldAddExisitingMapping = () =>
            {
                MappingHandler.AddMapping<FromObject, ToObject>(async (source) => new ToObject()
                {
                    Name = "NewMapper!"
                });
            };


            var result = await new FromObject().ConvertAsync<ToObject>();

            //Assert
            shouldAddExisitingMapping.Should().Throw<MapperAlreadyDefinedException>();
        }

        [Fact]
        public void Mapping_should_throw_if_fields_are_missing()
        {
            //Arrange
            MappingHandler.AddMapping(MappingHandler.AutoConverter<FromObject, FailingToObject>(MappingPropertySource.Source));

            var fromObject = new FromObject()
            {
                Name = "Testsson",
                Age = 22
            };

            //Act
            Func<Task> tryToConvert = async () => await fromObject.ConvertAsync<FailingToObject>();

            //Assert
            tryToConvert.Should().Throw<KeyNotFoundException>();
        }

        public void Dispose() => MappingHandler.Reset();
    }

    public class FailingToObject
    {
        public string Name { get; set; }
    }

    public enum TestEnum
    {
        Error,
        Yes,
        No,
        Maybe
    }

    public class ExtendedToObject : ToObject
    {

    }

    public class ExtendedFromObject : FromObject
    {
        public string OtherWrappedValue { get; set; }
    }

    public class ToObject
    {
        public WrappedValue WrappedValue { get; set; }
        public string Name { get; set; }
        [MapTo(nameof(FromObject.EnumValue))]
        public TestEnum TestValue { get; set; }
        public int Age { get; set; }
        public IEnumerable<int> Numbers { get; set; }
        public Dictionary<int, string> Table { get; set; }
    }

    public class WrappedValue
    {
        public string Value { get; set; }
        public string OtherValue { get; set; }
    }

    public class FromObject
    {
        public string WrappedValue { get; set; }
        public string Name { get; set; }
        [MapTo(nameof(ToObject.TestValue))]
        public string EnumValue { get; set; }
        public int Age { get; set; }
        public IEnumerable<int> Numbers { get; set; }
        public Dictionary<int, string> Table { get; set; }
    }
}
