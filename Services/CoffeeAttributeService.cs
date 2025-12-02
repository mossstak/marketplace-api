using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarketPlaceApi.Data;
using MarketPlaceApi.Models;

namespace MarketPlaceApi.Services
{
    public class CoffeeAttributeService : ICoffeeAttributeService
    {
        private readonly ApplicationDbContext _context;

        public CoffeeAttributeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RoastLevel> GetOrCreateRoastLevelAsync(string name)
        {
            var existing = await _context.RoastLevel
                .FirstOrDefaultAsync(x => x.Name == name);

            if (existing != null)
                return existing;

            var entity = new RoastLevel { Name = name };
            _context.RoastLevel.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<CoffeeProcess> GetOrCreateCoffeeProcessAsync(string name)
        {
            var existing = await _context.CoffeeProcess
                .FirstOrDefaultAsync(x => x.Name == name);

            if (existing != null)
                return existing;

            var entity = new CoffeeProcess { Name = name };
            _context.CoffeeProcess.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<CoffeeRegion> GetOrCreateRegionAsync(string name)
        {
            var existing = await _context.CoffeeRegion
                .FirstOrDefaultAsync(x => x.Name == name);

            if (existing != null)
                return existing;

            var entity = new CoffeeRegion { Name = name };
            _context.CoffeeRegion.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<CoffeeProducer> GetOrCreateProducerAsync(string name)
        {
            var existing = await _context.CoffeeProducer
                .FirstOrDefaultAsync(x => x.Name == name);

            if (existing != null)
                return existing;

            var entity = new CoffeeProducer { Name = name };
            _context.CoffeeProducer.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<CoffeeVarietal> GetOrCreateVarietalAsync(string name)
        {
            var existing = await _context.CoffeeVarietal
                .FirstOrDefaultAsync(x => x.Name == name);

            if (existing != null)
                return existing;

            var entity = new CoffeeVarietal { Name = name };
            _context.CoffeeVarietal.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<CoffeeAltitude> GetOrCreateAltitudeAsync(double valueInMasl)
        {
            var existing = await _context.CoffeeAltitude
                .FirstOrDefaultAsync(x => x.ValueInMasl == valueInMasl);

            if (existing != null)
                return existing;

            var entity = new CoffeeAltitude { ValueInMasl = valueInMasl };
            _context.CoffeeAltitude.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}