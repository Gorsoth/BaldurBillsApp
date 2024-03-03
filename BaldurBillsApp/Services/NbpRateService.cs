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
            var xmlContent = await response.Content.ReadAsStringAsync();
            var xDocument = XDocument.Parse(xmlContent);

            var ns = xDocument.Root.GetDefaultNamespace();
            var publicationDate = DateOnly.ParseExact(xDocument.Descendants(ns + "data_publikacji").First().Value, "yyyy-MM-dd");
            var tableNumber = xDocument.Descendants(ns + "numer_tabeli").First().Value;
            var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

            // Ensure we are adding rates for yesterday only
            //if (publicationDate == yesterday)
            //{
                var positions = xDocument.Descendants(ns + "pozycja");

                foreach (var position in positions)
                {
                    var currencyCode = position.Element(ns + "kod_waluty").Value;
                    var averageRate = decimal.Parse(position.Element(ns + "kurs_sredni").Value, CultureInfo.InvariantCulture);

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
                            RateValue = averageRate,
                            NbpTableName = tableNumber
                        };
                        _dbContext.ToPlnRates.Add(newRate);
                    }
                }

                await _dbContext.SaveChangesAsync();
            //}
        }
        else
        {
            // Handle the response error
            throw new HttpRequestException($"Failed to fetch data: {response.ReasonPhrase}");
        }
    }
}

