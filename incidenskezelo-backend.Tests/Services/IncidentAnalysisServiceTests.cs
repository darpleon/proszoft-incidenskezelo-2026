using System.Net;
using System.Text;
using System.Text.Json;
using incidenskezelo_backend.Configuration;
using incidenskezelo_backend.Data;
using incidenskezelo_backend.Models;
using incidenskezelo_backend.Models.AI;
using incidenskezelo_backend.Models.Incident;
using incidenskezelo_backend.Services;
using incidenskezelo_backend.Services.AI;
using incidenskezelo_backend.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace incidenskezelo_backend.Tests.Services;

[TestClass]
public class IncidentAnalysisServiceTests
{
    [TestMethod]
    public async Task AnalyzeAsync_ExceedsToolCallLimit_DoesNotExecuteExtraTools()
    {
        using var dbContext = TestDbContextFactory.Create();

        var user = new User
        {
            Name = "Test Operator",
            Email = "operator@example.com",
            Role = UserRole.Operator,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var firstService = new Service
        {
            Code = "orders-api",
            Name = "Orders API",
            Kind = ServiceKind.Api,
            IsActive = true
        };

        var secondService = new Service
        {
            Code = "payments-api",
            Name = "Payments API",
            Kind = ServiceKind.Api,
            IsActive = true
        };

        dbContext.Users.Add(user);
        dbContext.Services.AddRange(firstService, secondService);
        await dbContext.SaveChangesAsync();

        var incidentService = new IncidentService(
            dbContext,
            TimeProvider.System);

        var incident = await incidentService.CreateAsync(
            new CreateIncidentRequest(
                "Orders API returns errors",
                "POST /orders returns HTTP 500.",
                IncidentPriority.High),
            user.UserId,
            CancellationToken.None);

        var toolCallResponse = JsonSerializer.Serialize(new
        {
            message = new
            {
                role = "assistant",
                content = "",
                tool_calls = new object[]
                {
                new
                {
                    type = "function",
                    function = new
                    {
                        name = "getRelatedEvents",
                        arguments = new { }
                    }
                },
                new
                {
                    type = "function",
                    function = new
                    {
                        name = "getIncidentHistory",
                        arguments = new { }
                    }
                },
                new
                {
                    type = "function",
                    function = new
                    {
                        name = "getRecentServiceEvents",
                        arguments = new
                        {
                            serviceCode = "orders-api",
                            limit = 1
                        }
                    }
                },
                new
                {
                    type = "function",
                    function = new
                    {
                        name = "searchPreviousIncidents",
                        arguments = new
                        {
                            query = "Orders",
                            limit = 1
                        }
                    }
                },
                new
                {
                    type = "function",
                    function = new
                    {
                        name = "getRecentServiceEvents",
                        arguments = new
                        {
                            serviceCode = "payments-api",
                            limit = 1
                        }
                    }
                }
                }
            }
        });

        var finalAiJson = JsonSerializer.Serialize(new
        {
            summary = "The incident concerns errors from the Orders API.",
            probableCauses = Array.Empty<object>(),
            suggestedActions = Array.Empty<string>(),
            relevantEventIds = Array.Empty<long>(),
            limitations = Array.Empty<string>()
        });

        var finalResponse = JsonSerializer.Serialize(new
        {
            message = new
            {
                role = "assistant",
                content = finalAiJson
            }
        });

        var handler = new StubHttpMessageHandler(
            toolCallResponse,
            finalResponse);

        using var httpClient = new HttpClient(handler);

        var localAiService = new LocalAiService(
            httpClient,
            Options.Create(new LocalAiOptions
            {
                BaseUrl = "http://localhost:11434",
                ApiKey = "test-api-key",
                Model = "qwen3:8b"
            }));

        var geminiService = new GeminiService(
            httpClient,
            Options.Create(new GeminiOptions
            {
                ApiKey = "",
                Model = ""
            }));

        var resolver = new AiServiceResolver(
            geminiService,
            localAiService);

        var eventService = new EventService(
            dbContext,
            NullLogger<EventService>.Instance);

        var analysisService = new IncidentAnalysisService(
            resolver,
            incidentService,
            eventService,
            dbContext,
            TimeProvider.System);

        var result = await analysisService.AnalyzeAsync(
            incident.Id,
            AiProvider.Qwen3,
            CancellationToken.None);

        Assert.AreEqual(2, handler.RequestCount);

        Assert.AreEqual(6, result.ToolCalls.Count);
        Assert.AreEqual(5, result.ToolCalls.Count(call => call.Succeeded));
        Assert.AreEqual(1, result.ToolCalls.Count(call => !call.Succeeded));

        Assert.AreEqual("getIncidentDetails", result.ToolCalls[0].Name);
        Assert.IsTrue(result.ToolCalls[0].Succeeded);

        Assert.AreEqual("getRecentServiceEvents", result.ToolCalls[5].Name);
        Assert.IsFalse(result.ToolCalls[5].Succeeded);
    }

    [TestMethod]
    public async Task AnalyzeAsync_UnverifiedEventIds_RemovesUnknownIds()
    {
        using var dbContext = TestDbContextFactory.Create();

        var user = new User
        {
            Name = "Test Operator",
            Email = "operator@example.com",
            Role = UserRole.Operator,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var service = new Service
        {
            Code = "orders-api",
            Name = "Orders API",
            Kind = ServiceKind.Api,
            IsActive = true
        };

        dbContext.Users.Add(user);
        dbContext.Services.Add(service);
        await dbContext.SaveChangesAsync();

        var incidentService = new IncidentService(
            dbContext,
            TimeProvider.System);

        var incidentEvent = new Event
        {
            SourceEventId = Guid.NewGuid().ToString(),
            ServiceId = service.ServiceId,
            EventType = EventType.HighErrorRate,
            Severity = Severity.Error,
            OccurredAtUtc = DateTime.UtcNow,
            ReceivedAtUtc = DateTime.UtcNow,
            Summary = "Orders API error rate increased."
        };

        dbContext.Events.Add(incidentEvent);
        await dbContext.SaveChangesAsync();

        var incident = await incidentService.CreateAsync(
            new CreateIncidentRequest(
                "Orders API returns errors",
                "POST /orders returns HTTP 500.",
                IncidentPriority.High,
                EventIds: [incidentEvent.EventId]),
            user.UserId,
            CancellationToken.None);

        var unknownEventId = incidentEvent.EventId + 999999;

        const string toolCallResponse = """
        {
          "message": {
            "role": "assistant",
            "content": "",
            "tool_calls": [
              {
                "type": "function",
                "function": {
                  "name": "getRelatedEvents",
                  "arguments": {}
                }
              }
            ]
          }
        }
        """;

        var finalAiJson = JsonSerializer.Serialize(new
        {
            summary = "The Orders API has an increased error rate.",
            probableCauses = new[]
            {
            new
            {
                description = "A recent change may have caused errors.",
                confidence = "Medium"
            }
        },
            suggestedActions = new[]
            {
            "Review the Orders API logs."
        },
            relevantEventIds = new[]
            {
            incidentEvent.EventId,
            unknownEventId
        },
            limitations = Array.Empty<string>()
        });

        var finalResponse = JsonSerializer.Serialize(new
        {
            message = new
            {
                role = "assistant",
                content = finalAiJson
            }
        });

        var handler = new StubHttpMessageHandler(
            toolCallResponse,
            finalResponse);

        using var httpClient = new HttpClient(handler);

        var localAiService = new LocalAiService(
            httpClient,
            Options.Create(new LocalAiOptions
            {
                BaseUrl = "http://localhost:11434",
                ApiKey = "test-api-key",
                Model = "qwen3:8b"
            }));

        var geminiService = new GeminiService(
            httpClient,
            Options.Create(new GeminiOptions
            {
                ApiKey = "",
                Model = ""
            }));

        var resolver = new AiServiceResolver(
            geminiService,
            localAiService);

        var eventService = new EventService(
            dbContext,
            NullLogger<EventService>.Instance);

        var analysisService = new IncidentAnalysisService(
            resolver,
            incidentService,
            eventService,
            dbContext,
            TimeProvider.System);

        var result = await analysisService.AnalyzeAsync(
            incident.Id,
            AiProvider.Qwen3,
            CancellationToken.None);

        Assert.AreEqual(2, handler.RequestCount);
        Assert.AreEqual(1, result.RelevantEventIds.Count);
        Assert.AreEqual(
            incidentEvent.EventId,
            result.RelevantEventIds[0]);
        Assert.IsFalse(
            result.RelevantEventIds.Contains(unknownEventId));
    }

    [TestMethod]
    public async Task AnalyzeAsync_InvalidCauseConfidence_ThrowsAiInvalidResponseException()
    {
        using var dbContext = TestDbContextFactory.Create();

        var user = new User
        {
            Name = "Test Operator",
            Email = "operator@example.com",
            Role = UserRole.Operator,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var incidentService = new IncidentService(
            dbContext,
            TimeProvider.System);

        var incident = await incidentService.CreateAsync(
            new CreateIncidentRequest(
                "Orders API returns errors",
                "POST /orders returns HTTP 500.",
                IncidentPriority.High),
            user.UserId,
            CancellationToken.None);

        const string aiJson = """
        {
          "summary": "The Orders API is returning HTTP 500 errors.",
          "probableCauses": [
            {
              "description": "A database connection problem may be affecting order creation.",
              "confidence": "Certain"
            }
          ],
          "suggestedActions": [],
          "relevantEventIds": [],
          "limitations": []
        }
        """;

        var responseBody = JsonSerializer.Serialize(new
        {
            message = new
            {
                role = "assistant",
                content = aiJson
            }
        });

        var handler = new StubHttpMessageHandler(responseBody);
        using var httpClient = new HttpClient(handler);

        var localAiService = new LocalAiService(
            httpClient,
            Options.Create(new LocalAiOptions
            {
                BaseUrl = "http://localhost:11434",
                ApiKey = "test-api-key",
                Model = "qwen3:8b"
            }));

        var geminiService = new GeminiService(
            httpClient,
            Options.Create(new GeminiOptions
            {
                ApiKey = "",
                Model = ""
            }));

        var resolver = new AiServiceResolver(
            geminiService,
            localAiService);

        var eventService = new EventService(
            dbContext,
            NullLogger<EventService>.Instance);

        var analysisService = new IncidentAnalysisService(
            resolver,
            incidentService,
            eventService,
            dbContext,
            TimeProvider.System);

        var exception = await Assert.ThrowsExactlyAsync<AiInvalidResponseException>(
            () => analysisService.AnalyzeAsync(
                incident.Id,
                AiProvider.Qwen3,
                CancellationToken.None));

        Assert.AreEqual(1, handler.RequestCount);
        StringAssert.Contains(exception.Message, "confidence");
    }

    [TestMethod]
    public async Task AnalyzeAsync_InvalidJson_ThrowsAiInvalidResponseException()
    {
        using var dbContext = TestDbContextFactory.Create();

        var user = new User
        {
            Name = "Test Operator",
            Email = "operator@example.com",
            Role = UserRole.Operator,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var incidentService = new IncidentService(
            dbContext,
            TimeProvider.System);

        var incident = await incidentService.CreateAsync(
            new CreateIncidentRequest(
                "Orders API returns errors",
                "POST /orders returns HTTP 500.",
                IncidentPriority.High),
            user.UserId,
            CancellationToken.None);

        const string responseBody = """
        {
          "message": {
            "role": "assistant",
            "content": "This is not valid analysis JSON."
          }
        }
        """;

        var handler = new StubHttpMessageHandler(responseBody);
        using var httpClient = new HttpClient(handler);

        var localAiService = new LocalAiService(
            httpClient,
            Options.Create(new LocalAiOptions
            {
                BaseUrl = "http://localhost:11434",
                ApiKey = "test-api-key",
                Model = "qwen3:8b"
            }));

        var geminiService = new GeminiService(
            httpClient,
            Options.Create(new GeminiOptions
            {
                ApiKey = "",
                Model = ""
            }));

        var resolver = new AiServiceResolver(
            geminiService,
            localAiService);

        var eventService = new EventService(
            dbContext,
            NullLogger<EventService>.Instance);

        var analysisService = new IncidentAnalysisService(
            resolver,
            incidentService,
            eventService,
            dbContext,
            TimeProvider.System);

        var exception = await Assert.ThrowsExactlyAsync<AiInvalidResponseException>(
            () => analysisService.AnalyzeAsync(
                incident.Id,
                AiProvider.Qwen3,
                CancellationToken.None));

        Assert.AreEqual(1, handler.RequestCount);
        StringAssert.Contains(exception.Message, "invalid analysis JSON");
        Assert.AreEqual(
            0,
            await dbContext.StoredIncidentAnalyses.CountAsync());
    }

    [TestMethod]
    public async Task AnalyzeAsync_UnknownIncident_DoesNotCallAi()
    {

        using var dbContext = TestDbContextFactory.Create();

        var incidentService = new IncidentService(
            dbContext,
            TimeProvider.System);

        const string responseBody = """
    {
      "message": {
        "role": "assistant",
        "content": "{}"
      }
    }
    """;

        var handler = new StubHttpMessageHandler(responseBody);

        using var httpClient = new HttpClient(handler);

        var localAiService = new LocalAiService(
            httpClient,
            Options.Create(new LocalAiOptions
            {
                BaseUrl = "http://localhost:11434",
                ApiKey = "test-api-key",
                Model = "qwen3:8b"
            }));

        var geminiService = new GeminiService(
            httpClient,
            Options.Create(new GeminiOptions
            {
                ApiKey = "",
                Model = ""
            }));

        var resolver = new AiServiceResolver(
            geminiService,
            localAiService);

        var eventService = new EventService(
            dbContext,
            NullLogger<EventService>.Instance);

        var analysisService = new IncidentAnalysisService(
            resolver,
            incidentService,
            eventService,
            dbContext,
            TimeProvider.System);

        var missingIncidentId = Guid.NewGuid();

        var result = await analysisService.AnalyzeAsync(
            missingIncidentId,
            AiProvider.Qwen3,
            CancellationToken.None);

        Assert.AreEqual(0, handler.RequestCount);
        Assert.AreEqual("Qwen3", result.Model);
        StringAssert.Contains(result.Summary, "could not be found");

        Assert.AreEqual(0, result.ProbableCauses.Count);
        Assert.AreEqual(0, result.SuggestedActions.Count);
        Assert.AreEqual(0, result.RelevantEventIds.Count);

        Assert.AreEqual(1, result.ToolCalls.Count);
        Assert.AreEqual(
            "getIncidentDetails",
            result.ToolCalls[0].Name);
        Assert.IsTrue(result.ToolCalls[0].Succeeded);


    }

    [TestMethod]
    public async Task AnalyzeAsync_ValidIncident_ReturnsAndStoresStructuredAnalysis()
    {
        using var dbContext = TestDbContextFactory.Create();

        var user = new User
        {
            Name = "Test Operator",
            Email = "operator@example.com",
            Role = UserRole.Operator,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var incidentService = new IncidentService(dbContext, TimeProvider.System);

        var incident = await incidentService.CreateAsync(
            new CreateIncidentRequest(
                "Orders API returns errors",
                "POST /orders returns HTTP 500.",
                IncidentPriority.High),
            user.UserId,
            CancellationToken.None);

        const string aiJson = """
        {
          "summary": "The Orders API is returning HTTP 500 errors.",
          "probableCauses": [
            {
              "description": "A database connection problem may be affecting order creation.",
              "confidence": "Medium"
            }
          ],
          "suggestedActions": [
            "Check the Orders API logs."
          ],
          "relevantEventIds": [],
          "limitations": [
            "No related events were checked."
          ]
        }
        """;

        var responseBody = JsonSerializer.Serialize(new
        {
            message = new
            {
                role = "assistant",
                content = aiJson
            }
        });

        var handler = new StubHttpMessageHandler(responseBody);

        using var httpClient = new HttpClient(handler);

        var localAiService = new LocalAiService(
            httpClient,
            Options.Create(new LocalAiOptions
            {
                BaseUrl = "http://localhost:11434",
                ApiKey = "test-api-key",
                Model = "qwen3:8b"
            }));

        var geminiService = new GeminiService(
            httpClient,
            Options.Create(new GeminiOptions
            {
                ApiKey = "",
                Model = ""
            }));

        var resolver = new AiServiceResolver(geminiService, localAiService);
        var eventService = new EventService(
            dbContext,
            NullLogger<EventService>.Instance);

        var analysisService = new IncidentAnalysisService(
            resolver,
            incidentService,
            eventService,
            dbContext,
            TimeProvider.System);

        var startedAtUtc = DateTime.UtcNow;

        var result = await analysisService.AnalyzeAsync(
            incident.Id,
            AiProvider.Qwen3,
            CancellationToken.None);

        var finishedAtUtc = DateTime.UtcNow;

        Assert.AreEqual("Qwen3", result.Model);
        Assert.AreEqual(
            "The Orders API is returning HTTP 500 errors.",
            result.Summary);

        Assert.AreEqual(1, result.ProbableCauses.Count);
        Assert.AreEqual(
            "A database connection problem may be affecting order creation.",
            result.ProbableCauses[0].Description);
        Assert.AreEqual("Medium", result.ProbableCauses[0].Confidence);

        Assert.AreEqual(1, result.SuggestedActions.Count);
        Assert.AreEqual("Check the Orders API logs.", result.SuggestedActions[0]);

        Assert.AreEqual(0, result.RelevantEventIds.Count);
        Assert.AreEqual(1, result.ToolCalls.Count);
        Assert.AreEqual(
            "getIncidentDetails",
            result.ToolCalls[0].Name);
        Assert.IsTrue(result.ToolCalls[0].Succeeded);

        using var requestJson = JsonDocument.Parse(handler.LastRequestBody!);
        var prompt = requestJson.RootElement
            .GetProperty("messages")[0]
            .GetProperty("content")
            .GetString();

        StringAssert.Contains(prompt!, "Orders API returns errors");
        StringAssert.Contains(prompt!, "POST /orders returns HTTP 500.");

        // Ne a context által követett objektumból ellenőrizzünk.
        dbContext.ChangeTracker.Clear();

        var stored = await dbContext.StoredIncidentAnalyses
            .AsNoTracking()
            .SingleAsync();

        var internalIncidentId = await dbContext.Incidents
            .Where(item => item.PublicId == incident.Id)
            .Select(item => item.IncidentId)
            .SingleAsync();

        // A mentett elemzés a megfelelő incidenshez tartozik.
        Assert.AreEqual(internalIncidentId, stored.IncidentId);
        Assert.IsTrue(stored.AnalysisId > 0);

        // A teljes visszaadott eredmény került mentésre.
        var expectedJson = JsonSerializer.Serialize(
            result,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.AreEqual(expectedJson, stored.ResultJson);

        // A mentési időpont az elemzés futása alatt keletkezett.
        Assert.IsTrue(stored.AnalyzedAtUtc >= startedAtUtc);
        Assert.IsTrue(stored.AnalyzedAtUtc <= finishedAtUtc);
    }

}

internal sealed class StubHttpMessageHandler(params string[] responseBodies)
    : HttpMessageHandler
{
    public int RequestCount { get; private set; }
    public string? LastRequestBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        RequestCount++;
        var responseIndex = Math.Min(
            RequestCount - 1,
            responseBodies.Length - 1);

        LastRequestBody = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken);

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                responseBodies[responseIndex],
                Encoding.UTF8,
                "application/json")
        };
    }
}