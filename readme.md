# Calcbench API Client

A .net client for Calcbench's API.

Calcbench is an interface to the XBRL encoded 10-(K|Q) documents public companies file on the SEC's Edgar system.

    using Calcbench;
    string[] companyIdentifers = { "MSFT", "ORCL" };
    APIClient client = new();
    var filings = await client.Filings(companyIdentifiers: companyIdentifers);
    Console.WriteLine(string.Join(",", filings.Select(f => $"{f.entity_name} {f.fiscal_year} {f.fiscal_period} {f.filing_type}")));

## Support

andrew@calcbench.com