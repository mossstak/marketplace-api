using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketPlaceApi.Models;

namespace MarketPlaceApi.Services
{
    public interface ICoffeeAttributeService
    {
        Task<RoastLevel> GetOrCreateRoastLevelAsync(string name);
        Task<CoffeeProcess> GetOrCreateCoffeeProcessAsync(string name);
        Task<CoffeeRegion> GetOrCreateRegionAsync(string name);
        Task<CoffeeProducer> GetOrCreateProducerAsync(string name);
        Task<CoffeeVarietal> GetOrCreateVarietalAsync(string name);
        Task<CoffeeAltitude> GetOrCreateAltitudeAsync(double valueInMasl);
    }
}