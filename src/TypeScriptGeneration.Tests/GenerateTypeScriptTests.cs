using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using FluentAssertions;
using TypeScriptGeneration.Tests.Converters;
using Xunit;

namespace TypeScriptGeneration.Tests
{
    public class GenerateTypeScriptTests
    {
        [Fact]
        public void GenerateTypeScript()
        {
            var engine = new ConvertContext();
            engine.Configuration.GetFileDirectory = type =>
                type == typeof(Person) ? "/dto/person/" : 
                type == typeof(Employee) ? "/dto/person/employee/" : 
                "/"; 
            engine.GenerateForTypes(
                typeof(Employee), 
                typeof(Employer));

            var files = engine.GetFiles().ToDictionary(x => x.Key, x => x.Value);
            
            var expected = new Dictionary<string, string>
            {
                {"dto/person/employee/Employee.ts", @"import { Person } from '../Person';
import { GenericInheritance } from '../../../GenericInheritance';
import { MyGenericTest } from '../../../MyGenericTest';

export class Employee extends Person {
    constructor(
        FirstName?: string,
        LastName?: string,
        public Salary?: number,
        public Hello?: GenericInheritance<MyGenericTest<string>, string>) {
        super(FirstName, LastName);
    }
}
"},
                {"Employer.ts", @"import { Person } from './dto/person/Person';

export class Employer extends Person {
    constructor(
        FirstName?: string,
        LastName?: string,
        public CompanyName?: string) {
        super(FirstName, LastName);
    }
}
"},
                {"GenericInheritance.ts", @"import { MyGenericTest } from './MyGenericTest';

export class GenericInheritance<T1, T2> extends MyGenericTest<T2> {
    constructor(
        GenericProperty?: T2,
        public AnotherProperty?: Array<T1>) {
        super(GenericProperty);
    }
}
"},
                {"MyGenericTest.ts", @"export class MyGenericTest<T> {
    constructor(
        public GenericProperty?: T) {
    }
}
"},
                {"dto/person/Person.ts", @"export abstract class Person {
    constructor(
        public FirstName?: string,
        public LastName?: string) {
    }
}
"},
            };

            expected.All(x => files.ContainsKey(x.Key)).Should().BeTrue("Expected file is not found in actual");
            files.All(x => expected.ContainsKey(x.Key)).Should().BeTrue("Actual file is not found in expected");

            expected.ToList().ForEach(x => files[x.Key].Should().BeLike(x.Value));
        }

        public abstract class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public class Employee : Person
        {
            public decimal Salary { get; set; }
            public GenericInheritance<MyGenericTest<string>, string> Hello { get; set; }
        }

        public class Employer : Person
        {
            public string CompanyName { get; set; }
        }

        public class MyGenericTest<T>
        {
            public T GenericProperty { get; set; }
        }

        public class GenericInheritance<T1, T2> : MyGenericTest<T2>
        {
            public ICollection<T1> AnotherProperty { get; set; }
        }
    }
}