namespace BaldurBillsApp.Services;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using BaldurBillsApp.Models;
using Microsoft.EntityFrameworkCore;

public class NbpRateService
{
    private readonly HttpClient _httpClient;
    private readonly BaldurBillsDbContext _dbContext;
    private readonly string _nbpApiUrl = "https://static.nbp.pl/dane/kursy/xml/LastA.xml";

    public NbpRateService(HttpClient httpClient, BaldurBillsDbContext dbContext)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task FetchAndSaveRatesAsync()
    {
        HttpResponseMessage response = await _httpClient.GetAsync(_nbpApiUrl);
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var xmlContent = await response.Content.ReadAsStringAsync();
                var xDocument = XDocument.Parse(xmlContent);


                var publicationDate = DateOnly.ParseExact(xDocument.Descendants("data_publikacji").First().Value, "yyyy-MM-dd");
                var tableNumber = xDocument.Descendants("numer_tabeli").First().Value;
                var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

                var positions = xDocument.Descendants("pozycja");

                foreach (var position in positions)
                {
                    var currencyCode = position.Element("kod_waluty").Value;
                    var averageRate = Convert.ToDecimal(position.Element("kurs_sredni").Value);

                    // Assuming you want to update the rate if it already exists
                    var existingRate = await _dbContext.ToPlnRates
                        .Where(r => r.RateDate == publicationDate && r.RateCurrency == currencyCode)
                        .SingleOrDefaultAsync();

                    if (existingRate != null)
                    {
                        existingRate.RateValue = averageRate;
                        existingRate.NbpTableName = tableNumber;
                    }
                    else
                    {
                        var newRate = new ToPlnRate
                        {
                            RateDate = publicationDate,
                            RateCurrency = currencyCode,
                            RateValue = Convert.ToDecimal(averageRate),
                            NbpTableName = tableNumber
                        };
                        _dbContext.ToPlnRates.Add(newRate);
                    }
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception details: " + e.InnerException.Message);
                    var inner = e.InnerException;
                    while (inner.InnerException != null)
                    {
                        inner = inner.InnerException;
                        Console.WriteLine(inner.Message);
                    }
                }
            }
        }
        else
        {
            // Handle the response error
            throw new HttpRequestException($"Failed to fetch data: {response.ReasonPhrase}");
        }
    }
}

