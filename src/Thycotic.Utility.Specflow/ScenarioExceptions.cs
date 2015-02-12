﻿using FluentAssertions;
using TechTalk.SpecFlow;

namespace Thycotic.Utility.Specflow
{
    [Binding]
    public class ScenarioExceptions
    {

        [Then(@"there should have been a exception thrown with message ""(.*)""")]
        public void ThenThereShouldHaveBeenAExceptionThrownWithMessage(string exceptionMessage)
        {
            var message = (string)ScenarioContext.Current[ScenarioCommon.ScenarioException];
            message.Should().Be(exceptionMessage);
        }
    }
}