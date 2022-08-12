


using Calcbench;

string[] companyIdentifers = { "MSFT", "ORCL" };
APIClient client = new();
var filings = await client.Filings(companyIdentifiers: companyIdentifers);
Console.WriteLine(string.Join(",", filings.Select(f => $"{f.entity_name} {f.fiscal_year} {f.fiscal_period} {f.filing_type}")));