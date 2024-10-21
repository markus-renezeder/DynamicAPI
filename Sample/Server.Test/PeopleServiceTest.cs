using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Refit;
using Shared.Interfaces;

namespace Server.Test
{
    [TestClass]
    public class PeopleServiceTest
    {

        internal static WebApplicationFactory<Program>? webApplicationFactory;
        internal static HttpClient? httpClient;

        internal static IPeopleService? PeopleService;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext _)
        {

            webApplicationFactory = new WebApplicationFactory<Program>();

            httpClient = webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions()
            {
                BaseAddress = new Uri("https://localhost:7030")
            });

            PeopleService = RestService.For<IPeopleService>(httpClient);
        }

        [TestMethod]
        public async Task GetPeople()
        {
            var people = await PeopleService.GetPeople();

            Assert.IsTrue(people?.Any() == true);
        }

        [TestMethod]
        public async Task SearchPeople()
        {
            var people = await PeopleService.SearchPeople("a", null, null);

            Assert.IsTrue(people?.Any() == true);

            people = await PeopleService.SearchPeople(null, "a", null);

            Assert.IsTrue(people?.Any() == true);

            people = await PeopleService.SearchPeople(null, null, "a");

            Assert.IsTrue(people?.Any() == true);
        }

        [TestMethod]
        public async Task GetPeopleByCompany()
        {
            var people = await PeopleService.GetPeople("ACME");

            Assert.IsTrue(people?.Any() == true);
        }

        [TestMethod]
        public async Task GetPerson()
        {
            var people = await PeopleService.GetPeople();

            Assert.IsTrue(people?.Any() == true);

            var person = people?.FirstOrDefault();

            person = await PeopleService.GetPerson(person.Id);

            Assert.IsNotNull(person);
        }

        [TestMethod]
        public async Task CreatePerson()
        {
            var people = await PeopleService.GetPeople();

            Assert.IsTrue(people?.Any() == true);

            var count = people.Count();

            await PeopleService.CreatePerson(new() { FirstName = "User", LastName = "Test", Company = "TestCompany" });

            people = await PeopleService.GetPeople();

            Assert.IsTrue(people?.Any() == true);

            Assert.IsTrue(people.Count() == (count + 1));

            var person = people.FirstOrDefault(p => p.FirstName == "User" && p.LastName == "Test" && p.Company == "TestCompany");

            Assert.IsNotNull(person);

            person = await PeopleService.GetPerson(person.Id);

            Assert.IsNotNull(person);
        }

        [TestMethod]
        public async Task UpdatePerson()
        {
            var people = await PeopleService.GetPeople();

            Assert.IsTrue(people?.Any() == true);

            var person = people.FirstOrDefault();

            Assert.IsNotNull(person);

            person.FirstName = "TestUser";


            await PeopleService.UpdatePerson(person);

            person = await PeopleService.GetPerson(person.Id);

            Assert.IsNotNull(person);

            Assert.IsTrue(person.FirstName == "TestUser");
        }

        [TestMethod]
        public async Task DeletePerson()
        {
            var people = await PeopleService.GetPeople();

            Assert.IsTrue(people?.Any() == true, "No people found!");

            var person = people.FirstOrDefault();

            Assert.IsNotNull(person, "No first person");

            await PeopleService.DeletePerson(person.Id);

            people = await PeopleService.GetPeople();

            Assert.IsTrue(people?.Any() == true, "No people available!");

            bool exists = people.Any(p => p.Id == person.Id);

            Assert.IsTrue(exists == false, "Person was not deleted!");
        }

    }
}