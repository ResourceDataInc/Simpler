﻿using System;
using Moq;
using NUnit.Framework;
using Simpler.Data.Tasks;
using System.Data;
using Simpler.Tests.Mocks;

namespace Simpler.Tests.Data.Tasks
{
    [TestFixture]
    public class BuildParametersTest
    {
        [Test]
        public void should_create_parameters_for_any_parameters_found_in_the_command_text_with_matching_properties_in_the_anonymous_object()
        {
            // Arrange
            var task = TaskFactory<BuildParameters>.Create();

            var mockDbCommand = new Mock<IDbCommand> { DefaultValue = DefaultValue.Mock };
            var mockDbDataParameter = new Mock<IDbDataParameter>();
            mockDbCommand.Setup(dbCommand => dbCommand.CreateParameter()).Returns(mockDbDataParameter.Object);
            mockDbCommand.Setup(dbCommand => dbCommand.CommandText).Returns("select blah");
            task.CommandWithParameters = mockDbCommand.Object;

            task.ObjectWithValues = new { Name = "John Doe", Age = 21 };

            var mockFindParameters = new Mock<FindParametersInCommandText>();
            mockFindParameters.Setup(findParams => findParams.ParameterNames).Returns(new string[] { "@Age" });
            task.FindParametersInCommandText = mockFindParameters.Object;

            // Act
            task.Execute();

            // Assert
            mockDbDataParameter.VerifySet(param => param.ParameterName = "@Age");
            mockDbDataParameter.VerifySet(param => param.Value = 21);
            mockDbCommand.Verify(dbCommand => dbCommand.Parameters.Add(mockDbDataParameter.Object), Times.Once());
        }

        [Test]
        public void should_set_parameter_to_the_value_found_in_the_matching_property_of_the_anonymous_object()
        {
            // Arrange
            var task = TaskFactory<BuildParameters>.Create();

            var mockDbCommand = new Mock<IDbCommand> { DefaultValue = DefaultValue.Mock };
            var mockDbDataParameter = new Mock<IDbDataParameter>();
            mockDbCommand.Setup(dbCommand => dbCommand.CreateParameter()).Returns(mockDbDataParameter.Object);
            mockDbCommand.Setup(dbCommand => dbCommand.CommandText).Returns("select blah");
            task.CommandWithParameters = mockDbCommand.Object;

            task.ObjectWithValues = new { Name = "John Doe", Age = 21 };

            var mockFindParameters = new Mock<FindParametersInCommandText>();
            mockFindParameters.Setup(findParams => findParams.ParameterNames).Returns(new string[] { "@Name"});
            task.FindParametersInCommandText = mockFindParameters.Object;

            // Act
            task.Execute();

            // Assert
            mockDbDataParameter.VerifySet(parameter => parameter.ParameterName = "@Name");
            mockDbDataParameter.VerifySet(parameter => parameter.Value = "John Doe");
            mockDbCommand.Verify(dbCommand => dbCommand.Parameters.Add(mockDbDataParameter.Object), Times.Once());
        }

        [Test]
        public void should_set_parameter_to_dbnull_if_a_matching_property_is_not_found_in_the_anonymous_object()
        {
            // Arrange
            var task = TaskFactory<BuildParameters>.Create();

            var mockDbCommand = new Mock<IDbCommand> { DefaultValue = DefaultValue.Mock };
            var mockDbDataParameter = new Mock<IDbDataParameter>();
            mockDbCommand.Setup(dbCommand => dbCommand.CreateParameter()).Returns(mockDbDataParameter.Object);
            mockDbCommand.Setup(dbCommand => dbCommand.CommandText).Returns("select blah");
            task.CommandWithParameters = mockDbCommand.Object;

            task.ObjectWithValues = new { NotAge = "A" };

            var mockFindParameters = new Mock<FindParametersInCommandText>();
            mockFindParameters.Setup(findParams => findParams.ParameterNames).Returns(new string[] { "@Age" });
            task.FindParametersInCommandText = mockFindParameters.Object;

            // Act
            task.Execute();

            // Assert
            mockDbDataParameter.VerifySet(parameter => parameter.ParameterName = "@Age");
            mockDbDataParameter.VerifySet(parameter => parameter.Value = DBNull.Value);
            mockDbCommand.Verify(dbCommand => dbCommand.Parameters.Add(mockDbDataParameter.Object), Times.Once());
        }

        [Test]
        public void should_support_setting_parameter_using_a_complex_object()
        {
            // Arrange
            var task = TaskFactory<BuildParameters>.Create();

            var mockDbCommand = new Mock<IDbCommand> { DefaultValue = DefaultValue.Mock };
            var mockDbDataParameter = new Mock<IDbDataParameter>();
            mockDbCommand.Setup(dbCommand => dbCommand.CreateParameter()).Returns(mockDbDataParameter.Object);
            mockDbCommand.Setup(dbCommand => dbCommand.CommandText).Returns("select something from table where @MockObject.Age = 21");
            task.CommandWithParameters = mockDbCommand.Object;

            task.ObjectWithValues = new { MockObject = new { Name = "John Doe", Age = 21 } };

            var mockFindParameters = new Mock<FindParametersInCommandText>();
            mockFindParameters.Setup(findParams => findParams.ParameterNames).Returns(new[] { "@MockObject.Age" });
            task.FindParametersInCommandText = mockFindParameters.Object;

            // Act
            task.Execute();

            // Assert
            mockDbDataParameter.VerifySet(param => param.ParameterName = "@MockObject_Age");
            mockDbDataParameter.VerifySet(param => param.Value = 21);
            mockDbCommand.Verify(dbCommand => dbCommand.Parameters.Add(mockDbDataParameter.Object), Times.Once());
            mockDbCommand.VerifySet(dbCommand => dbCommand.CommandText = "select something from table where @MockObject_Age = 21");
        }
    }
}