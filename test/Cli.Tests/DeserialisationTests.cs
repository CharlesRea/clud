using System.Collections.Generic;
using System.IO;
using Clud.Cli.Config;
using Xunit;

namespace Cli.Tests
{
    public class DeserialisationTests
    {
        [Fact]
        public void DeserialisesAppConfig()
        {
            var configYaml = @"
name: weather
entryPoint: web
owner: CharlesRea
description: An example app
repository: https://github.com/CharlesRea/repo
services:
  - name: web
    dockerfile: ./Web/dockerfile
    httpPort: 80
    replicas: 2
    environmentVariables:
      ASPNETCORE__ENVIRONMENT: Production
      FOO: bar
    secrets:
      - redisConnectionString
  - name: redis
    dockerImage: redis
    tcpPorts:
      - 6379
    secrets:
      - username
      - password
";

            var result = ApplicationConfiguration.Parse(new StringReader(configYaml));

            Assert.Empty(result.Errors);
            Assert.Empty(result.Warnings);

            var config = result.Result;
            Assert.Equal("weather", config.Name);
            Assert.Equal("web", config.EntryPoint);
            Assert.Equal("CharlesRea", config.Owner);
            Assert.Equal("An example app", config.Description);
            Assert.Equal("https://github.com/CharlesRea/repo", config.Repository);

            var web = config.Services[0];
            Assert.Equal("web", web.Name);
            Assert.Equal("./Web/dockerfile", web.Dockerfile);
            Assert.Equal(80, web.HttpPort);
            Assert.Empty(web.TcpPorts);
            Assert.Equal(2, web.Replicas);
            Assert.Equal(new Dictionary<string, string> {{ "ASPNETCORE__ENVIRONMENT", "Production" }, { "FOO", "bar" }}, web.EnvironmentVariables);
            Assert.Equal(new[] { "redisConnectionString" }, web.Secrets);

            var redis = config.Services[1];
            Assert.Equal("redis", redis.Name);
            Assert.Equal("redis", redis.DockerImage);
            Assert.Null(redis.HttpPort);
            Assert.Equal(new[] { 6379 }, redis.TcpPorts);
            Assert.Equal(1, redis.Replicas);
            Assert.Empty(redis.EnvironmentVariables);
            Assert.Equal(new[] { "username", "password" }, redis.Secrets);
        }

        [Fact]
        public void InvalidAppName_Fails()
        {
            var configYaml = @"
name: weather with a space
repository: https://github.com/CharlesRea/repo
services:
  - name: web
    dockerfile: ./Web/dockerfile
    httpPort: 80
";

            var result = ApplicationConfiguration.Parse(new StringReader(configYaml));

            Assert.NotEmpty(result.Errors);
            Assert.Contains("Name must be a valid DNS subdomain name: it should contain alphanumeric characters and dashes (-) only", result.Errors);
        }
    }
}
