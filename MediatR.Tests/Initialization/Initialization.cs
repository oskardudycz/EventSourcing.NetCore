﻿using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Xunit;

namespace MediatR.Tests.Initialization;

public class Initialization
{
    private static class ServiceLocator
    {
        private static readonly Dictionary<Type, List<object>> Services = new Dictionary<Type, List<object>>();

        public static void Register(Type type, params object[] implementations)
            => Services.Add(type, implementations.ToList());

        public static List<object> Get(Type type)
        {
            return Services[type];
        }
    }

    [Fact]
    public void GivenProperParams_WhenObjectIsCreated_ThenIsCreatedProperly()
    {
        var ex = Record.Exception(() =>
        {
            //Given
            //When
            var mediator = new Mediator(type => ServiceLocator.Get(type).Single());

            mediator.Should().Not.Be.Null();
        });

        //Then
        ex.Should().Be.Null();
    }
}