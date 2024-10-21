using DynamicAPI.Models;
using Refit;
using Shared.Interfaces;
using Shared.Models;

namespace Server.Services
{
    public class PeopleService : IPeopleService
    {
        private List<Person> _people = new();

        public PeopleService()
        {
            CreatePerson(new Person() { FirstName = "Dagobert", LastName = "Duck", Company = "ACME" });
            CreatePerson(new Person() { FirstName = "Seppl", LastName = "Kasperls Freund", Company = "Kids Club" });
            CreatePerson(new Person() { FirstName = "Kasperl", LastName = "Seppls Freund", Company = "Kids Club" });
            CreatePerson(new Person() { FirstName = "Räuber", LastName = "Hotzenplotz", Company = "n.a." });
            CreatePerson(new Person() { FirstName = "Dimpfelmoser", LastName = "Wachtmeister", Company = "Polizei" });
            CreatePerson(new Person() { FirstName = "Inspector", LastName = "Gadget", Company = "Polizei" });
        }


        public async Task CreatePerson([Body] Person person)
        {
            if (string.IsNullOrEmpty(person.Id))
            {
                person.Id = Guid.NewGuid().ToString("N");
            }
            
            _people.Add(person);
        }

        public async Task DeletePerson([AliasAs("id")] string id)
        {
            
            var person = _people.FirstOrDefault(x => x.Id == id);

            if(person != null)
            {
                _people.Remove(person);
            }
            
        }

        public async Task DeletePersonNew([AliasAs("id")] string id)
        {
            await DeletePerson(id);
        }

        public async Task<IEnumerable<Person>> GetPeople()
        {
            return _people;
        }

        public async Task<IEnumerable<Person>> GetPeople([AliasAs("company")] string company)
        {
            return _people.Where(x => x.Company.Equals(company, StringComparison.InvariantCultureIgnoreCase));
        }

        public async Task<IEnumerable<Person>> GetPeopleNew()
        {
            var list = await GetPeople();

            return list.OrderBy(x => x.LastName);
        }

        public async Task<Person> GetPerson([AliasAs("id")] string id)
        {
            var person = _people.FirstOrDefault(x => x.Id == id);

            if(person == null)
            {
                throw new DynamicAPIException(System.Net.HttpStatusCode.NotFound, "Person not found!");
            }

            return person;
        }

        public async Task<IEnumerable<Person>> SearchPeople([Query] string? FirstName, [Query] string? LastName, [Query] string? Company)
        {
            var people = _people.Where(x => (string.IsNullOrEmpty(FirstName) || x.FirstName.Contains(FirstName, StringComparison.InvariantCultureIgnoreCase))
                                        && (string.IsNullOrEmpty(LastName) || x.LastName.Contains(LastName, StringComparison.InvariantCultureIgnoreCase))
                                        && (string.IsNullOrEmpty(Company) || x.Company.Contains(Company, StringComparison.InvariantCultureIgnoreCase)));

            return people;
                
        }

        public async Task UpdatePerson([Body] Person person)
        {
            await DeletePerson(person.Id);
            await CreatePerson(person);
        }
    }
}
