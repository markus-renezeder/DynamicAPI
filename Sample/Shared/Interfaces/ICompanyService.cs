using DynamicAPI.Attributes;
using Refit;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    [Information(description: "API to access companies")]
    public interface ICompanyService
    {
        [Information(description: "Get companies", summary: "Get all companies")]
        [Get("/Companies")]
        Task<IEnumerable<Company>> GetCompanies();

        [Information(summary: "Get company by id")]
        [Get("/Companies/{id}")]
        Task<Company> GetCompany(string id);

        [Information(summary: "Create a new company")]
        [Post("/Companies")]
        Task CreateCompany(Company company);

        [Information(summary: "Delete an existing company")]
        [Delete("/Companies")]
        Task DeleteCompany(string id);

        [Information(summary: "Update an exsiting company")]
        [Put("/Companies")]
        Task UpdateCompany(Company company);
    }
}
