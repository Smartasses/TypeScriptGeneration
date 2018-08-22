using System;
using System.Collections.Generic;
using System.Linq;
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
    queryString: {
        [key: string]: string | string[];
    };
}"},
                {"IRequestDispatcher.ts", @"import { Observable } from 'rxjs/index';
import { IHttpRequest } from './IHttpRequest';

export interface IRequestDispatcher {
    execute<TResponse>(request: IHttpRequest<TResponse>): Observable<TResponse>;
}"},
                {"TestRequest.ts", @"import { TestFilterDto } from './TestFilterDto';
import { IHttpRequest } from './IHttpRequest';
import { TestResponse } from './TestResponse';
import { IRequestDispatcher } from './IRequestDispatcher';

export class TestRequest {
    constructor(
        public id?: string,
        public numbers?: Array<number>,
        public filter?: TestFilterDto) {
    }

    public __name = 'TestRequest';
    private __request = () => {
        const req: IHttpRequest<TestResponse> = {
            method: 'get',
            route: 'api/values/{Id}'.replace('{Id}', this.id ? this.id.toString() : ''),
            body: undefined,
            queryString: {}
        };
        if (this.numbers) req.queryString['numbers'] = this.numbers.map(i => i ? i.toString() : null).filter(i => typeof i === 'string');
        if (this.filter) {
            if (this.filter.search) req.queryString['filter.search'] = this.filter.search;
            if (this.filter.page) {
                if (this.filter.page.size) req.queryString['filter.page.size'] = this.filter.page.size.toString();
                if (this.filter.page.index) req.queryString['filter.page.index'] = this.filter.page.index.toString();
            }
        }

        return req;
    }
    public execute = (dispatcher: IRequestDispatcher) => dispatcher.execute(this.__request());
}
"},
                {"TestResponse.ts", @"export class TestResponse {
    constructor(
        public date?: Date) {
    }
}
"},
                {"TestFilterDto.ts", @"import { PageDto } from './PageDto';
import { PagedFilterDto } from './PagedFilterDto';

export class TestFilterDto extends PagedFilterDto {
    constructor(
        page?: PageDto,
        public search?: string) {
        super(page);
    }
}
"},
                {"PagedFilterDto.ts", @"import { PageDto } from './PageDto';

export class PagedFilterDto {
    constructor(
        public page?: PageDto) {
    }
}
"},
                {"PageDto.ts", @"export class PageDto {
    constructor(
        public size?: number,
        public index?: number) {
    }
}
"}
            };

            expected.All(x => files.ContainsKey(x.Key)).Should().BeTrue("Expected file is not found in actual");
            files.All(x => expected.ContainsKey(x.Key)).Should().BeTrue("Actual file is not found in expected");

            foreach (var expectedFile in expected)
            {
                expectedFile.Value.Should().BeLike(files[expectedFile.Key], $"File content for {expectedFile.Key} does not match the expected value.");
            }
        }
    }

    [GetRequest("api/values/{Id}")]
    public class TestRequest : IReturn<TestResponse>
    {
        public string Id { get; set; }
        public List<int> Numbers { get; set; }
        public TestFilterDto Filter { get; set; }
    }

    public class TestResponse
    {
        public DateTime Date { get; set; }
    }

    public class TestFilterDto : PagedFilterDto
    {
        public string Search { get; set; }
    }

    public class PagedFilterDto
    {
        public PageDto Page { get; set; }
    }

    public class PageDto
    {
        public long Size { get; set; }
        public long Index { get; set; }
    }
}