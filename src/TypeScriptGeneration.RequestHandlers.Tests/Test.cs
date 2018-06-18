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
            engine.Configuration.AddConverter(new RequestDispatcherConverter(DispatcherResponseType.Observable));
            engine.Configuration.AddConverter(new RequestConverter());
            engine.GenerateForTypes(typeof(TestRequest));
            
            

            var files = engine.GetFiles().ToDictionary(x => x.Key, x => x.Value);
            
            var expected = new Dictionary<string, string>
            {
                {"IHttpRequest.ts", @"export interface IHttpRequest<TResponse> {
    method: string;
    route: string;
    body: any;
    queryString: { [key: string]: string };
}"},
                {"IRequestDispatcher.ts", @"import { Observable } from 'rxjs/index';
import { IHttpRequest } from './IHttpRequest';

export interface IRequestDispatcher {
    execute<TResponse>(request: IHttpRequest<TResponse>): Observable<TResponse>;
}"},
                {"TestRequest.ts", @"import { IHttpRequest } from './IHttpRequest';
import { TestResponse } from './TestResponse';
import { IRequestDispatcher } from './IRequestDispatcher';

export class TestRequest {
    constructor(
        public id?: string) {
    }

    private __request = () => <IHttpRequest<TestResponse>>{
        method: 'get',
        route: 'api/values/{Id}'.replace('{Id}', this.id ? this.id.toString() : ''),
        body: undefined,
        queryString: {
        }
    }
    public execute = (dispatcher: IRequestDispatcher) => dispatcher.execute(this.__request());
}"},
                {"TestResponse.ts", @"export class TestResponse {
    constructor(
        public date?: Date) {
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