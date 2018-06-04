using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using FluentAssertions;
using RequestHandlers;
using RequestHandlers.Http;
using Xunit;

namespace TypeScriptGeneration.RequestHandlers.Tests
{
    public class Test
    {
        [Fact]
        public void MyTest()
        {
            var engine = new ConvertContext();
            engine.Configuration.AddConverter(new HttpRequestInterfaceConverter());
            engine.Configuration.AddConverter(new RequestDispatcherConverter());
            engine.Configuration.AddConverter(new RequestConverter());
            engine.GenerateForTypes(typeof(TestRequest));
            
            

            var files = engine.GetFiles().ToDictionary(x => x.Key, x => x.Value);
            
            var expected = new Dictionary<string, string>
            {
                {"IHttpRequest.ts", @"export interface IHttpRequest<TResponse> {
    Method: string;
    Route: string;
    Body: string;
    QueryString: { [key: string]: string };
}
"},
                {"IRequestDispatcher.ts", @"import { IHttpRequest } from './IHttpRequest';

export interface IRequestDispatcher {
    execute<TResponse>(request: IHttpRequest<TResponse>);
}
"},
                {"TestRequest.ts", @"import { IHttpRequest } from './IHttpRequest';
import { TestResponse } from './TestResponse';
import { IRequestDispatcher } from './IRequestDispatcher';

export class TestRequest {
    constructor(
        public Id?: string) {
    }
    
    private __request = () => <IHttpRequest<TestResponse>>{
        Method: 'get',
        Route: 'api/values/{Id}'.replace('{Id}', this.Id ? this.Id.toString() : ''),
        Body: undefined,
        QueryString: {
        }
    };
    public execute = (dispatcher: IRequestDispatcher) => dispatcher.execute(this.__request());
}
"},
                {"TestResponse.ts", @"export class TestResponse {
    constructor(
        public Date?: Date) {
    }
}
"}
            };

            expected.All(x => files.ContainsKey(x.Key)).Should().BeTrue("Expected file is not found in actual");
            files.All(x => expected.ContainsKey(x.Key)).Should().BeTrue("Actual file is not found in expected");

            expected.ToList().ForEach(x => files[x.Key].Should().BeLike(x.Value));
        }
    }

    [GetRequest("api/values/{Id}")]
    public class TestRequest : IReturn<TestResponse>
    {
        public string Id { get; set; }
    }

    public class TestResponse
    {
        public DateTime Date { get; set; }
    }
}