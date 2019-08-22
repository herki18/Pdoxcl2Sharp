﻿using System;
using System.IO;
using System.Text;
using Xunit;

namespace Pdoxcl2Sharp.Tests
{
    public class ScratchTest
    {
        class MyData
        {
            public string Hello { get; set; }
        }

        [Theory]
        [InlineData("hello=world")]
        [InlineData("hello = world")]
        [InlineData("\t\thello =\r\n world")]
        [InlineData("#1st\r\nhello #2nd\r\n= #3rd\r\n world #4th\r\n")]
        [InlineData("hello==world")]
        [InlineData("header hello = world")]
        [InlineData("a = 1.000\r\nhello=world\r\nc=d")]
        public async void TestOneProperty(string input)
        {
            var mem = new MemoryStream(Encoding.ASCII.GetBytes(input));
            var res = await Scratch.DeserializeAsync<MyData>(mem);
            Assert.Equal("world", res.Hello);
        }

        class MyTypes
        {
            public string Hello { get; set; }
            public int MyInt { get; set; }
        }

        [Fact]
        public async void TestManyTypes()
        {
            var input = @"
hello=world
my_int = 13
";
            var mem = new MemoryStream(Encoding.ASCII.GetBytes(input));
            var res = await Scratch.DeserializeAsync<MyTypes>(mem);
            Assert.Equal("world", res.Hello);
            Assert.Equal(13, res.MyInt);
        }


        class MyNestedObject
        {
            public string Hello { get; set; }
            public MyData Data { get; set; }
            public MyTypes Types { get; set; }
        }

        [Fact]
        public async void TestNestedObject()
        {
            var input = @"
hello=world
data={ hello=mars }
types = {
    hello=venus
    my_int = 13
}
";

            var mem = new MemoryStream(Encoding.ASCII.GetBytes(input));
            var res = await Scratch.DeserializeAsync<MyNestedObject>(mem);
            Assert.Equal("world", res.Hello);
            Assert.Equal("mars", res.Data.Hello);
            Assert.Equal("venus", res.Types.Hello);
            Assert.Equal(13, res.Types.MyInt);
        }
    }
}
