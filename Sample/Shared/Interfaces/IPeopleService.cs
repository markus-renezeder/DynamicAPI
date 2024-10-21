
using DynamicAPI.Attributes;
using Refit;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    [RequireAuthorization("user")]
    public interface IPeopleService
    {
        [Get("/people")]
        Task<IEnumerable<Person>> GetPeople();

        [Get("/people/search")]
        Task<IEnumerable<Person>> SearchPeople([Query] string? FirstName, [Query] string? LastName, [Query] string? Company);

        [Get("/people/company/{company}")]
        Task<IEnumerable<Person>> GetPeople([AliasAs("company")] string company);

        [Get("/people/person/{id}")]
        Task<Person> GetPerson([AliasAs("id")] string id);

        [Post("/people/person")]
        Task CreatePerson([Body] Person person);

        [RequireAuthorization("admin")]
        [Delete("/people/person/{id}")]
        Task DeletePerson([AliasAs("id")] string id);

        [Put("/people/person")]
        Task UpdatePerson([Body] Person person);

    }
}
