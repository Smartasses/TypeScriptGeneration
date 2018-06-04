using System;
using System.Collections.Generic;
using FluentAssertions;
using TypeScriptGeneration.TypeScriptTypes;
using Xunit;

namespace TypeScriptGeneration.Tests.TypeScriptTypes
{
    public class TypeScriptTypeTests
    {
        [Theory]
        [MemberData(nameof(TypeScriptTypeToTypeScriptTestsData))]
        public void TypeScriptTypeToTypeScriptTests(TypeScriptType typeScriptType, string expected)
        {
            typeScriptType.ToTypeScriptType().Should().Be(expected);
        }
        public static List<object[]> TypeScriptTypeToTypeScriptTestsData => new List<object[]>
        {
            new object[] {TypeScriptType.Any, "any"},
            new object[] {TypeScriptType.Date, "Date"},
            new object[] {TypeScriptType.Number, "number"},
            new object[] {TypeScriptType.Boolean, "boolean"},
            new object[] {TypeScriptType.Dictionary(TypeScriptType.String, TypeScriptType.Boolean), "{ [key: string]: boolean }"},
            new object[] {TypeScriptType.Array(TypeScriptType.String), "Array<string>"},
            new object[] {TypeScriptType.Generic(TypeScriptType.Boolean, TypeScriptType.Any), "boolean<any>"},
        };
        
        
        [Theory]
        [MemberData(nameof(TypeToTypeScriptTypeData))]
        public void TypeToTypeScriptType(Type type, TypeScriptType expected)
        {
            new LocalContext(new ConvertConfiguration(), null, type).GetTypeScriptType(type).Should().Be(expected);
        }
        public static List<object[]> TypeToTypeScriptTypeData => new List<object[]>
        {
            new object[] {typeof(object), TypeScriptType.Any},
            new object[] {typeof(DateTime), TypeScriptType.Date},
            new object[] {typeof(int), TypeScriptType.Number},
            new object[] {typeof(bool), TypeScriptType.Boolean},
            new object[] {typeof(Dictionary<string, bool>), TypeScriptType.Dictionary(TypeScriptType.String, TypeScriptType.Boolean)},
            new object[] {typeof(string[]), TypeScriptType.Array(TypeScriptType.String)}
        };
    }
}