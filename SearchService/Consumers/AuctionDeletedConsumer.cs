using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers
{
    public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
    {
        public async Task Consume(ConsumeContext<AuctionDeleted> context)
        {
            Console.WriteLine("--> Consuming auction created: " + context.Message.Id);
            var result = await DB.DeleteAsync<Item>(context.Message.Id);

            if (result.DeletedCount > 0)
            {
                Console.WriteLine("--> Auction deleted from search index: " + context.Message.Id);
            }
            else
            {
                Console.WriteLine("--> Auction not found in search index: " + context.Message.Id);
            }

        }
    }
}
