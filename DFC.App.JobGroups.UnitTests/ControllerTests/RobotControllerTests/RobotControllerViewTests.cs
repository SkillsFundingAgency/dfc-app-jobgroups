﻿using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Xunit;

namespace DFC.App.JobGroups.UnitTests.ControllerTests.RobotControllerTests
{
    [Trait("Category", "Robot Controller Unit Tests")]
    public class RobotControllerViewTests : BaseRobotControllerTests
    {
        [Fact]
        public void RobotControllerRobotViewReturnsSuccess()
        {
            // Arrange
            var controller = BuildRobotController();

            // Act
            var result = controller.RobotView();

            // Assert
            var contentResult = Assert.IsType<ContentResult>(result);

            contentResult.ContentType.Should().Be(MediaTypeNames.Text.Plain);

            controller.Dispose();
        }
    }
}
