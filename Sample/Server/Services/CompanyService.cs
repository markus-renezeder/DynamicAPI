using DynamicAPI.Models;
using Shared.Interfaces;
using Shared.Models;
using System;

namespace Server.Services
{
    public class CompanyService : ICompanyService
    {
        private List<Company> _companyList = new();

        public CompanyService()
        {
            CreateCompany(new() { Name = "ACME" });
            CreateCompany(new() { Name = "Kids Club" });
            CreateCompany(new() { Name = "Polizei" });
        }

        public async Task CreateCompany(Company company)
        {
            if(string.IsNullOrEmpty(company.Id))
            {
                company.Id = Guid.NewGuid().ToString("N");
            }
            
            _companyList.Add(company);
        }

        public async Task DeleteCompany(string id)
        {
            var company = _companyList.FirstOrDefault(x => x.Id == id);

            if (company != null)
            {
                _companyList.Remove(company);
            }
        }

        public async Task<IEnumerable<Company>> GetCompanies()
        {
            return _companyList;
        }

        public async Task<Company> GetCompany(string id)
        {
            var company = _companyList.FirstOrDefault(x => x.Id == id);

            if (company != null)
            {
                return company;
            }

            throw new DynamicAPIException(System.Net.HttpStatusCode.NotFound, "Company not found");
        }

        public async Task UpdateCompany(Company company)
        {
            await DeleteCompany(company.Id);
            await CreateCompany(company);
        }
    }
}
