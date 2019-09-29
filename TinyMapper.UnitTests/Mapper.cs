﻿using System;
            //Arrange
            var fromObject = new FromObject()

            //Act
            var toObject = await fromObject.ConvertAsync<ToObject>();

            //Assert
            fromObject.Name.Should().Be(toObject.Name);
            //Arrange
            var fromObject = new FromObject()

            //Act
            var toObject = await fromObject.ConvertAsync<ToObject>();

            //Assert
            fromObject.WrappedValue.Should().Be(toObject.WrappedValue.Value);
            //Arrange
            MappingHandler.AddMapping<WrappedValue, string>((source) => source.Value);

            //Act
            var toObject = await fromObject.ConvertAsync<FromObject>();

            //Assert
            fromObject.WrappedValue.Value.Should().Be(toObject.WrappedValue);
            //Arrange
            MappingHandler.AddMapping<string, WrappedValue>((source) => new WrappedValue() { Value = source });

            //Act
            var toObject = await fromObject.ConvertAsync<ExtendedToObject>();

            //Assert
            using (new FluentAssertions.Execution.AssertionScope())
            //Arrange
            var fromObject = new FromObject()

            //Act
            var toObject = await fromObject.ConvertAsync<ToObject>();

            //Assert
            toObject.TestValue.Should().Be(TestEnum.Yes);
            //Arrange
            var fromObject = new ToObject()

            //Act
            var toObject = await fromObject.ConvertAsync<FromObject>();

            //Assert
            toObject.EnumValue.Should().Be("No");
            // Arrange
            MappingHandler.AddMapping(MappingHandler.AutoConverter<FromObject, ToObject>(MappingHandler.MappingPropertySource.Target, requireAllProperties: true));

            // Act
            Action shouldAddExisitingMapping = () => MappingHandler.AddMapping(MappingHandler.AutoConverter<FromObject, ToObject>());

            //Assert
            shouldAddExisitingMapping.Should().Throw<MapperAlreadyDefinedException>();
            //Arrange
            MappingHandler.AddMapping(MappingHandler.AutoConverter<FromObject, FailingToObject>(MappingHandler.MappingPropertySource.Source));

            //Act
            Func<Task> tryToConvert = async () => await fromObject.ConvertAsync<FailingToObject>();

            //Assert
            tryToConvert.Should().Throw<KeyNotFoundException>();