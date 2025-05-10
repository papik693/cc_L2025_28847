using System.Text.Json;
using System.Text.Json.Serialization;
using Cdv.Domain.DbContext;
using Cdv.Domain.Entities;
using Cdv.Functions.Dto;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cdv.Functions;

public class PeopleFn
{
    private readonly ILogger<PeopleFn> _logger;
    private readonly PeopleDbContext db;
    private readonly JsonSerializerOptions _jsonOptions;

    public PeopleFn(ILogger<PeopleFn> logger, PeopleDbContext db)
    {
        _logger = logger;
        this.db = db;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() },
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
    }

    [Function("PeopleFn")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function,
            "get", "post", "put", "delete")]
        HttpRequest req)
    {
        try
        {
            switch (req.Method)
            {
                case "POST":
                    var person = await CreatePersonAsync(req);
                    return new OkObjectResult(person);
                case "GET":
                    var idExist = req.Query.Any(w => w.Key == "id");

                    if (idExist)
                    {
                        var personId = req.Query.First(w => w.Key == "id").Value;
                        int id = Int32.Parse(personId.First());
                        return new OkObjectResult(FindPerson(id));
                    }

                    var people = GetPeople(req);
                    return new OkObjectResult(people);
                case "DELETE":
                    await DeletePersonAsync(req);
                    return new OkResult();
                case "PUT":
                    await UpdatePersonAsync(req);
                    return new OkResult();
            }

            return new BadRequestObjectResult("Unknown method");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request");
            return new BadRequestObjectResult(ex.Message);
        }
    }

    private List<PersonDto> GetPeople(HttpRequest req)
    {
        var people = db.People.ToList();
        return people.Select(s => new PersonDto
        {
            Id = s.Id,
            FirstName = s.FirstName,
            LastName = s.LastName,
        }).ToList();
    }

    private async Task UpdatePersonAsync(HttpRequest req)
    {
        var personDto = await JsonSerializer.DeserializeAsync<PersonDto>(req.Body, _jsonOptions);
        var existingPerson = db.People.First(p => p.Id == personDto.Id);
        existingPerson.FirstName = personDto.FirstName;
        existingPerson.LastName = personDto.LastName;
        await db.SaveChangesAsync();
    }

    private async Task DeletePersonAsync(HttpRequest req)
    {
        int id;
    
        // First try to get ID from query string
        if (req.Query.TryGetValue("id", out var idValues) && idValues.Any())
        {
            if (!int.TryParse(idValues.First(), out id))
            {
                throw new ArgumentException("Invalid ID format in query string");
            }
        }
        // If not in query string, try to get from body
        else
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var deleteRequest = JsonSerializer.Deserialize<DeleteRequest>(requestBody, _jsonOptions);
        
            if (deleteRequest == null || deleteRequest.Id <= 0)
            {
                throw new ArgumentException("ID is required in request body");
            }
            id = deleteRequest.Id;
        }

        var personToDelete = db.People.FirstOrDefault(p => p.Id == id);
        if (personToDelete == null)
        {
            throw new KeyNotFoundException($"Person with ID {id} not found");
        }

        db.People.Remove(personToDelete);
        await db.SaveChangesAsync();
    }
    
    public class DeleteRequest
    {
        public int Id { get; set; }
    }

    private PersonDto FindPerson(int personId)
    {
        var person = db.People.First(w => w.Id == personId);
        return new PersonDto
        {
            Id = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName,
        };
    }

    private async Task<PersonDto> CreatePersonAsync(HttpRequest req)
    {
        var personDto = await JsonSerializer.DeserializeAsync<PersonDto>(req.Body, _jsonOptions);
        var person = new PersonEntity
        {
            FirstName = personDto.FirstName,
            LastName = personDto.LastName
        };
        db.People.Add(person);
        await db.SaveChangesAsync();
        personDto.Id = person.Id;
        return personDto;
    }
}