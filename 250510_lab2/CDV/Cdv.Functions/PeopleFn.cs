using Cdv.Domain.DbContext;
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

    public PeopleFn(ILogger<PeopleFn> logger, PeopleDbContext db)
    {
        _logger = logger;
        this.db = db;
    }

    [Function("PeopleFn")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function,
            "get", "post", "put", "delete")]
        HttpRequest req)
    {
        switch (req.Method)
        {
            case "POST":
                var person = CreatePerson(req);
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
                DeletePerson(req);
                return new OkResult();
            case "PUT":
                UpdatePerson(req);
                return new OkResult();
        }

        throw new NotImplementedException("Uknown method");
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

    private void UpdatePerson(HttpRequest req)
    {
        throw new NotImplementedException();
    }

    private void DeletePerson(HttpRequest req)
    {
        throw new NotImplementedException();
    }

    private PersonDto FindPerson(int personId)
    {
        var person = db.People.First(w=>w.Id==personId);
        return new PersonDto
        {
            Id = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName,
        };
    }

    private PersonDto CreatePerson(HttpRequest req)
    {
        throw new NotImplementedException();
    }
}