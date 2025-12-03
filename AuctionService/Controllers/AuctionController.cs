using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [Route("api/auctions")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private AuctionDbContext _context { get; }
        private  IMapper _mapper { get; }
        private IPublishEndpoint _publishEndpoint { get; }

        public AuctionController(AuctionDbContext context,IMapper mapper,IPublishEndpoint publishEndpoint) 
        {
            _context = context;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
        {

            var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

            if (!string.IsNullOrEmpty(date))
            {
               
                    query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
           
            }   

            return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync( x=> x.Id == id);

            if(auction == null)
            {
                return NotFound();
            }   

            return _mapper.Map<AuctionDto>(auction);
        }

        [HttpPost]
        public  async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            if(auctionDto != null)
            {
                var auction = _mapper.Map<Auction>(auctionDto);

                //TODO : ADD CURRENT USER CHECKING

                if(auction != null)
                {
                    _context.Auctions.Add(auction);

                    var newAuction = _mapper.Map<AuctionDto>(auction);  

                    await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));


                    var result = await _context.SaveChangesAsync() > 0;


                    if (!result) return BadRequest("Could not save changes to the DB");

                    return CreatedAtAction(nameof(GetAuctionById), new {auction.Id}, _mapper.Map<AuctionDto>(auction));
                }
                return BadRequest("Request Wrong");
            }

            return BadRequest("Request Wrong");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, [FromBody]  UpdateAuctionDto updateAuctionDto)
        {
            if (updateAuctionDto != null )
            {
                var auction = await _context.Auctions.Include(x => x.Item)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (auction != null)
                {

                    // TODO : CHECK USER SELLER == USERNAME 

                    auction.Item.Make = updateAuctionDto?.Make ?? auction.Item.Make;
                    auction.Item.Year = updateAuctionDto?.Year ?? auction.Item.Year;
                    auction.Item.Model = updateAuctionDto?.Model ?? auction.Item.Model;
                    auction.Item.Color = updateAuctionDto?.Color ?? auction.Item.Color;
                    auction.Item.Mileage = updateAuctionDto?.Mileage ?? auction.Item.Mileage;

                    await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

                    var result = await _context.SaveChangesAsync() > 0;

                    if (!result) return BadRequest("Could not save changes to the DB");

                    return Ok("Auction updated successfully");
                }
                return BadRequest("Request Wrong");
            }

            return BadRequest("Request Wrong");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            if (id != Guid.Empty)
            {
                var auction = await _context.Auctions.FindAsync(id);
                

                if (auction != null)
                {
                    // TODO : CHECK USER SELLER == USERNAME 


                    _context.Auctions.Remove(auction);

                    await _publishEndpoint.Publish<AuctionDeleted>( new {Id = auction.Id.ToString()});

                    var result = await _context.SaveChangesAsync() > 0;

                    if (!result) return BadRequest("Could not delete the auction");

                    return Ok("Auction Deleted successfully");
                }
                return BadRequest("Could not find the item with this id");
            }

            return BadRequest("Request Wrong");
        }
    }
}
